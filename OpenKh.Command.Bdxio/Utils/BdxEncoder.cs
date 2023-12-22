using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using NLog;
using OpenKh.Command.Bdxio.Models;
using System.Globalization;
using System.Text;
using Xe.BinaryMapper;
using static BdxScriptParser;
using static OpenKh.Command.Bdxio.Models.BdxHeader;
using static OpenKh.Command.Bdxio.Models.BdxInstructionDesc;

namespace OpenKh.Command.Bdxio.Utils
{
    public class BdxEncoder
    {
        public byte[] Content { get; } = new byte[0];

        public BdxEncoder(
            BdxHeader header,
            string script,
            string scriptName,
            Func<string, string> loadScript
        )
        {
            var logger = LogManager.GetLogger("BdxEncoder");

            if (string.IsNullOrWhiteSpace(script) && header.IsEmpty)
            {
                return;
            }

            var labels = new SortedDictionary<string, ILabel>();

            var instructionDict = new (string, BdxInstructionDesc)[0]
                .Concat(
                    BdxInstructionDescs.GetDescs()
                        .Select(desc => (desc.Name, desc))
                )
                .Concat(
                    BdxInstructionDescs.GetDescs()
                        .SelectMany(
                            desc => desc.OldNames
                                .Select(
                                    oldName => (oldName, desc)
                                )
                        )
                )
                .GroupBy(pair => pair.Item1)
                .Select(it => it.First())
                .ToDictionary(it => it.Item1, it => it.Item2);

            Func<ArgContext, CompilePass, int> ResolveInt32Factory(
                Func<NumberdataContext, int> stringToInt32,
                Func<string, Func<IToken>, int> labelToInt32
            )
            {
                return (ArgContext argContext, CompilePass pass) =>
                {
                    if (pass.First)
                    {
                        return 0;
                    }
                    else
                    {
                        if (argContext.numberdata() is NumberdataContext numberdata)
                        {
                            return stringToInt32(numberdata);
                        }
                        else if (argContext.id() is IdContext id)
                        {
                            var name = id.GetText();
                            return labelToInt32(name, () => id.Start);
                        }
                        else
                        {
                            throw new InvalidDataException($"{FormatLineColumn(argContext.Start)} Unknown arg!");
                        }
                    }
                };
            }

            int LabelToInt32Converter(string name, Func<IToken> getStart)
            {
                if (!labels.TryGetValue(name, out var any) || any == null)
                {
                    throw new ArgumentException(FormatErrorMessage(getStart(), $"Label {name} not found"));
                }

                if (any is CodeLabel label)
                {
                    return label.PC;
                }
                else if (any is ConstLabel constLabel)
                {
                    return constLabel.Value;
                }
                else if (any is BssLabel bssLabel)
                {
                    return bssLabel.ByteOffset;
                }

                throw new ArgumentException(FormatErrorMessage(getStart(), $"Label {name} cannot be used here"));
            }

            float LabelToFloat32Converter(string name, Func<IToken> getStart)
            {
                if (!labels.TryGetValue(name, out var any) || any == null)
                {
                    throw new ArgumentException(FormatErrorMessage(getStart(), $"Label {name} not found"));
                }

                if (any is ConstLabel constLabel)
                {
                    return constLabel.Value;
                }

                throw new ArgumentException(FormatErrorMessage(getStart(), $"Label {name} cannot be used here"));
            }

            Func<ArgContext, CompilePass, float> ResolveFloat32Factory(
                Func<string, Func<IToken>, float> labelToFloat32
            )
            {
                return (ArgContext argContext, CompilePass pass) =>
                {
                    if (pass.First)
                    {
                        return 0;
                    }
                    else
                    {
                        if (argContext.numberdata() is NumberdataContext numberdata)
                        {
                            return float.Parse(numberdata.GetText(), invariantNumberFormat);
                        }
                        else if (argContext.id() is IdContext id)
                        {
                            var name = id.GetText();
                            return labelToFloat32(name, () => id.Start);
                        }
                        else
                        {
                            throw new InvalidDataException($"{FormatLineColumn(argContext.Start)} Unknown arg!");
                        }
                    }
                };
            }

            var resolveInt32ForIntTrigger = ResolveInt32Factory(
                stringToInt32: Helper.NumberdataContextToInt32ConverterFactory(allowFloat: false),
                labelToInt32: LabelToInt32Converter
            );

            var resolveInt32ForSsub = ResolveInt32Factory(
                stringToInt32: Helper.AllowedRangeFactory(
                    factory: Helper.NumberdataContextToInt32ConverterFactory(allowFloat: false),
                    min: -2048,
                    max: 4095
                ),
                labelToInt32: LabelToInt32Converter
            );

            var resolveInt32ForRelativeImm16 = ResolveInt32Factory(
                stringToInt32: Helper.AllowedRangeFactory(
                    factory: Helper.NumberdataContextToInt32ConverterFactory(allowFloat: false),
                    min: short.MinValue,
                    max: short.MaxValue
                ),
                labelToInt32: LabelToInt32Converter
            );

            var resolveInt32ForValueImm16 = ResolveInt32Factory(
                stringToInt32: Helper.AllowedRangeFactory(
                    factory: Helper.NumberdataContextToInt32ConverterFactory(allowFloat: false),
                    min: short.MinValue,
                    max: ushort.MaxValue
                ),
                labelToInt32: LabelToInt32Converter
            );

            var resolveInt32ForRelativeImm32 = ResolveInt32Factory(
                stringToInt32: Helper.AllowedRangeFactory(
                    factory: Helper.NumberdataContextToInt32ConverterFactory(allowFloat: false),
                    min: int.MinValue,
                    max: int.MaxValue
                ),
                labelToInt32: LabelToInt32Converter
            );

            var resolveInt32ForValueImm32 = ResolveInt32Factory(
                stringToInt32: Helper.AllowedRangeFactory(
                    factory: Helper.NumberdataContextToInt32ConverterFactory(allowFloat: true),
                    min: int.MinValue,
                    max: int.MaxValue
                ),
                labelToInt32: LabelToInt32Converter
            );

            var resolveFloat32ForValueFloat32 = ResolveFloat32Factory(
                labelToFloat32: LabelToFloat32Converter
            );

            int lastBssBytes = 0;

            var compilePasses = new CompilePass[] {
                new CompilePass(Index: 0, First: true, Last: false), // resolve code placement and labels offset
                new CompilePass(Index: 1, First: false, Last: false), // resolve equ (const)
                new CompilePass(Index: 2, First: false, Last: true), // compile
            };

            foreach (var pass in compilePasses)
            {
                var words = new MemoryStream();

                void WordAlignment()
                {
                    if ((words.Length & 1) != 0)
                    {
                        words.WriteByte(0);
                    }
                }

                BinaryMapping.WriteObject(words, header);

                if (!pass.First)
                {
                    if (header.WorkSize < lastBssBytes)
                    {
                        logger.Warn($"WorkSize {header.WorkSize} is extended to {lastBssBytes}");

                        words.Position = 16;
                        words.Write(BitConverter.GetBytes(lastBssBytes));
                        words.Seek(0, SeekOrigin.End);
                    }
                }

                foreach (var intTrigger in header.Triggers
                    .Select(
                        item =>
                        {
                            return new IntTrigger
                            {
                                Key = item.Key,
                                Addr = resolveInt32ForIntTrigger(Helper.ParseAsArg(item.Addr), pass),
                            };
                        }
                    )
                )
                {
                    BinaryMapping.WriteObject(words, intTrigger);
                }

                BinaryMapping.WriteObject(words, new IntTrigger { Addr = 0, Key = 0, });

                var codeSeg = new CodeSegment(words);
                var bssSeg = new BssSegment();
                ISegment segment = codeSeg;

                void AddLabel(string labelName, ILabel labelValue, Func<ParserRuleContext> getTokenPos)
                {
                    if (pass.First)
                    {
                        if (!labels.TryAdd(labelName, labelValue))
                        {
                            throw new InvalidDataException($"{FormatLineColumn(getTokenPos().Start)} Redefine of label {labelName}");
                        }
                    }
                    else
                    {
                        labels[labelName] = labelValue;
                    }
                }

                void WalkStatements(ProgContext root)
                {
                    foreach (var st in root.statement())
                    {
                        //var pc = (int)(words.Length / 2 - 8);

                        if (st.section() is SectionContext section)
                        {
                            var id = section.section_id();
                            var sectionName = id.GetText();
                            if (sectionName == ".text")
                            {
                                segment = codeSeg;
                            }
                            else if (sectionName == ".bss")
                            {
                                segment = bssSeg;
                            }
                            else
                            {
                                throw new InvalidDataException($"{FormatLineColumn(id.Start)} Unknown section name {sectionName}");
                            }
                            continue;
                        }
                        else if (st.equ() is EquContext equ)
                        {
                            var intValue = Helper.NumberdataContextToInt32ConverterFactory(allowFloat: false).Invoke(equ.numberdata());

                            AddLabel(
                                equ.name.GetText(),
                                new ConstLabel { Value = intValue, },
                                () => equ.name
                            );
                        }

                        if (st.label() is LabelContext label)
                        {
                            WordAlignment();
                            var id = label.id();
                            AddLabel(
                                id.GetText(),
                                segment.GetLabel(),
                                () => id
                            );
                        }

                        if (st.order() is OrderContext order)
                        {
                            if (order.db() is DbContext db)
                            {
                                foreach (var byteData in db.bytedata())
                                {
                                    if (byteData.numberdata() is NumberdataContext numberData)
                                    {
                                        segment.WriteByte(Helper.GetAsByte(numberData));
                                    }
                                    else if (byteData.StringData() is ITerminalNode stringData)
                                    {
                                        var text = stringData.GetText();
                                        var bytes = Encoding.ASCII.GetBytes(text.Substring(1, text.Length - 2));
                                        segment.Write(bytes);
                                    }
                                    else if (byteData.id() is IdContext id)
                                    {
                                        var value = pass.First ? 0 : LabelToInt32Converter(id.GetText(), () => id.Start);
                                        segment.WriteByte((byte)value);
                                    }
                                    else
                                    {
                                        throw new InvalidDataException($"{FormatLineColumn(byteData.Start)} Unknown byte data!");
                                    }
                                }
                            }
                            else if (order.resb() is ResbContext resb)
                            {
                                if (resb.numberdata() is NumberdataContext numberdata)
                                {
                                    var numBytes = Helper.GetAsInt32(numberdata);
                                    segment.AllocateBytes(numBytes);
                                }
                                else if (resb.id() is IdContext id)
                                {
                                    var numBytes = LimitInt32Range(
                                        pass.First
                                            ? 0
                                            : LabelToInt32Converter(id.GetText(), () => id.Start),
                                        min: int.MinValue,
                                        max: int.MaxValue,
                                        getStart: () => id.Start
                                    );
                                    segment.AllocateBytes(numBytes);
                                }
                            }
                            else if (order.dw() is DwContext dw)
                            {
                                WordAlignment();
                                foreach (var wordData in dw.worddata())
                                {
                                    if (wordData.numberdata() is NumberdataContext numberdata)
                                    {
                                        segment.Write(BitConverter.GetBytes(Helper.GetAsUInt16(numberdata)));
                                    }
                                    else if (wordData.id() is IdContext id)
                                    {
                                        var value = (ushort)LimitInt32Range(pass.First ? 0 : LabelToInt32Converter(id.GetText(), () => id.Start), min: -127, max: 255, () => id.Start);
                                        segment.Write(BitConverter.GetBytes(value));
                                    }
                                    else
                                    {
                                        throw new InvalidDataException($"{FormatLineColumn(wordData.Start)} Unknown word data!");
                                    }
                                }
                            }
                            else if (order.resw() is ReswContext resw)
                            {
                                if (resw.numberdata() is NumberdataContext numberdata)
                                {
                                    var numBytes = 2 * Helper.GetAsInt32(numberdata);
                                    segment.AllocateBytes(numBytes);
                                }
                                else if (resw.id() is IdContext id)
                                {
                                    var numBytes = 2 * LimitInt32Range(pass.First ? 0 : LabelToInt32Converter(id.GetText(), () => id.Start), min: int.MinValue, max: int.MaxValue, () => id.Start);
                                    segment.AllocateBytes(numBytes);
                                }
                            }
                            else if (order.instruction() is InstructionContext instr)
                            {
                                var name = instr.id().GetText();
                                if (!instructionDict.TryGetValue(name, out var desc))
                                {
                                    throw new InvalidDataException($"{FormatLineColumn(instr.Start)} Unknown instruction '{name}'!");
                                }

                                if (segment is not CodeSegment thisSegment)
                                {
                                    throw new InvalidDataException($"{FormatLineColumn(instr.Start)} This must be placed at `section .text`!");
                                }

                                var pc = thisSegment.PC;

                                var word = new BdxInstruction(desc.Code);

                                var args = desc.Args ?? new Arg[0];

                                var instrArgs = instr.arg();

                                for (int x = 0; x < args.Length; x++)
                                {
                                    if (args[x].Type == ArgType.Ssub)
                                    {
                                        word.Ssub = (ushort)resolveInt32ForSsub(instrArgs[x], pass);
                                    }
                                }

                                segment.Write(BitConverter.GetBytes(word.Code));

                                for (int x = 0; x < args.Length; x++)
                                {
                                    if (args[x].Type == ArgType.Imm16)
                                    {
                                        if (args[x].IsRelative)
                                        {
                                            var baseOffset = -pc - desc.CodeSize;

                                            segment.Write(BitConverter.GetBytes((ushort)(baseOffset + resolveInt32ForRelativeImm16(instrArgs[x], pass))));
                                        }
                                        else
                                        {
                                            segment.Write(BitConverter.GetBytes((ushort)(resolveInt32ForValueImm16(instrArgs[x], pass))));
                                        }
                                    }
                                    else if (args[x].Type == ArgType.Imm32)
                                    {
                                        if (args[x].IsRelative)
                                        {
                                            var baseOffset = -pc - desc.CodeSize;

                                            segment.Write(BitConverter.GetBytes((uint)(baseOffset + resolveInt32ForRelativeImm32(instrArgs[x], pass))));
                                        }
                                        else
                                        {
                                            segment.Write(BitConverter.GetBytes((uint)(resolveInt32ForValueImm32(instrArgs[x], pass))));
                                        }
                                    }
                                    else if (args[x].Type == ArgType.Float32)
                                    {
                                        {
                                            segment.Write(BitConverter.GetBytes((float)(resolveFloat32ForValueFloat32(instrArgs[x], pass))));
                                        }
                                    }
                                }
                            }
                            else if (order.include() is IncludeContext include)
                            {
                                var scriptName = include.FileName().GetText().Trim('"');
                                var subScript = loadScript(scriptName);

                                var stream = FromString(subScript, scriptName);
                                var lexer = new BdxScriptLexer(stream);
                                var tokens = new CommonTokenStream(lexer);
                                var parser = new BdxScriptParser(tokens);
                                var subRoot = parser.prog();

                                WalkStatements(subRoot);
                            }
                            else
                            {
                                //throw new InvalidDataException("Unknown statement!");
                                // maybe NL
                            }
                        }
                    }
                }

                {
                    var stream = FromString(script, scriptName);
                    var lexer = new BdxScriptLexer(stream);
                    var tokens = new CommonTokenStream(lexer);
                    var parser = new BdxScriptParser(tokens);
                    var root = parser.prog();

                    WalkStatements(root);
                }

                WordAlignment();

                lastBssBytes = bssSeg.numBytes;

                if (pass.Last)
                {
                    Content = words.ToArray();
                }
            }
        }

        private int LimitInt32Range(int value, int min, int max, Func<IToken> getStart)
        {
            if (min <= value && value <= max)
            {
                return value;
            }

            throw new ArgumentOutOfRangeException(FormatErrorMessage(getStart(), $"Integer must be in range of from {min} to {max}"));
        }

        private static ICharStream FromString(string script, string sourceName)
        {
            var stream = CharStreams.fromString(script);
            if (stream is CodePointCharStream charStream)
            {
                charStream.name = sourceName;
            }
            return stream;
        }

        private static string FormatLineColumn(IToken start) => $"{start.InputStream.SourceName} line {start.Line}:{start.Column}";

        private static string FormatErrorMessage(IToken start, IFormattable text) => $"{start.InputStream.SourceName} line {start.Line}:{start.Column} {text}";
        private static string FormatErrorMessage(IToken start, string text) => $"{start.InputStream.SourceName} line {start.Line}:{start.Column} {text}";

        private static class Helper
        {
            internal static int GetAsInt32(NumberdataContext numberData)
            {
                var decimalNumber = numberData.decimalnumber;
                if (decimalNumber != null)
                {
                    var body = decimalNumber.Text;

                    if (uint.TryParse(body, out uint uintVal))
                    {
                        return (int)uintVal;
                    }
                    else if (int.TryParse(body, out int intVal))
                    {
                        return intVal;
                    }
                    else
                    {
                        throw new InvalidDataException(FormatErrorMessage(numberData.Start, "Must be int32 here"));
                    }
                }
                else if (numberData.hexnumber is IToken hexNumber)
                {
                    var body = hexNumber.Text;

                    if (uint.TryParse(body, NumberStyles.HexNumber, null, out uint uintVal))
                    {
                        return (int)uintVal;
                    }
                    else
                    {
                        throw new InvalidDataException(FormatErrorMessage(numberData.Start, "Must be int32 here"));
                    }
                }
                else if (numberData.floatnumber is IToken)
                {
                    throw new InvalidDataException(FormatErrorMessage(numberData.Start, "Float number is not allowed here"));
                }
                else
                {
                    throw new InvalidDataException($"{FormatLineColumn(numberData.Start)} Unknown number data!");
                }
            }

            internal static ArgContext ParseAsArg(string addr)
            {
                var stream = CharStreams.fromString(addr);
                var lexer = new BdxScriptLexer(stream);
                var tokens = new CommonTokenStream(lexer);
                var parser = new BdxScriptParser(tokens);

                return parser.arg();
            }

            internal static Func<NumberdataContext, object> NumberdataContextToInt32OrFloat32ConverterFactory()
            {
                return (NumberdataContext numberData) =>
                {
                    var decimalNumber = numberData.decimalnumber;
                    if (decimalNumber != null)
                    {
                        var body = decimalNumber.Text;

                        if (uint.TryParse(body, out uint uintVal))
                        {
                            return (int)uintVal;
                        }
                        else if (int.TryParse(body, out int intVal))
                        {
                            return intVal;
                        }
                        else
                        {
                            throw new InvalidDataException(FormatErrorMessage(numberData.Start, "Must be int32 here"));
                        }
                    }
                    else if (numberData.hexnumber is IToken hexNumber)
                    {
                        var body = hexNumber.Text;

                        if (uint.TryParse(body, NumberStyles.HexNumber, null, out uint uintVal))
                        {
                            return (int)uintVal;
                        }
                        else
                        {
                            throw new InvalidDataException(FormatErrorMessage(numberData.Start, "Must be int32 here"));
                        }
                    }
                    else if (numberData.floatnumber is IToken floatnumber)
                    {
                        var body = floatnumber.Text;

                        if (float.TryParse(body, NumberStyles.HexNumber, invariantNumberFormat, out float floatVal))
                        {
                            return (float)floatVal;
                        }
                        else
                        {
                            throw new InvalidDataException(FormatErrorMessage(numberData.Start, "Must be float32 here"));
                        }
                    }
                    else
                    {
                        throw new InvalidDataException(FormatErrorMessage(numberData.Start, "Unknown number data!"));
                    }
                };
            }

            internal static Func<NumberdataContext, int> NumberdataContextToInt32ConverterFactory(bool allowFloat)
            {
                var func = NumberdataContextToInt32OrFloat32ConverterFactory();

                return (NumberdataContext numberData) =>
                {
                    var value = func(numberData);

                    if (value is int intValue)
                    {
                        return intValue;
                    }
                    else if (value is float floatValue)
                    {
                        if (!allowFloat)
                        {
                            throw new InvalidDataException(FormatErrorMessage(numberData.Start, "Float number is not allowed here"));
                        }
                        return BitConverter.ToInt32(BitConverter.GetBytes(floatValue));
                    }
                    else
                    {
                        throw new InvalidDataException(FormatErrorMessage(numberData.Start, "Numeric converter bug"));
                    }
                };
            }

            internal static Func<NumberdataContext, int> AllowedRangeFactory(Func<NumberdataContext, int> factory, int min, int max)
            {
                return (NumberdataContext numberData) =>
                {
                    var value = factory(numberData);
                    if (min <= value && value <= max)
                    {
                        return value;
                    }

                    throw new ArgumentOutOfRangeException(FormatErrorMessage(numberData.Start, $"The value must be in range of from {min} to {max}!"));
                };
            }

            internal static byte GetAsByte(NumberdataContext numberData)
            {
                var decimalNumber = numberData.decimalnumber;
                if (decimalNumber != null)
                {
                    var body = decimalNumber.Text;

                    if (byte.TryParse(body, out byte byteVal))
                    {
                        return byteVal;
                    }
                    else if (sbyte.TryParse(body, out sbyte sbyteVal))
                    {
                        return (byte)sbyteVal;
                    }
                    else
                    {
                        throw new InvalidDataException(FormatErrorMessage(numberData.Start, "Must be byte here"));
                    }
                }
                else if (numberData.hexnumber is IToken hexNumber)
                {
                    var body = hexNumber.Text;

                    if (byte.TryParse(body, NumberStyles.HexNumber, null, out byte byteVal))
                    {
                        return byteVal;
                    }
                    else
                    {
                        throw new InvalidDataException(FormatErrorMessage(numberData.Start, "Must be byte here"));
                    }
                }
                else if (numberData.floatnumber is IToken)
                {
                    throw new InvalidDataException(FormatErrorMessage(numberData.Start, "Float number is not allowed here"));
                }
                else
                {
                    throw new InvalidDataException(FormatErrorMessage(numberData.Start, "Unknown number data!"));
                }
            }

            internal static ushort GetAsUInt16(NumberdataContext numberData)
            {
                var decimalNumber = numberData.decimalnumber;
                if (decimalNumber != null)
                {
                    var body = decimalNumber.Text;

                    if (ushort.TryParse(body, out ushort ushortVal))
                    {
                        return ushortVal;
                    }
                    else if (short.TryParse(body, out short shortVal))
                    {
                        return (byte)shortVal;
                    }
                    else
                    {
                        throw new InvalidDataException(FormatErrorMessage(numberData.Start, "Must be int16 here"));
                    }
                }
                else if (numberData.hexnumber is IToken hexNumber)
                {
                    var body = hexNumber.Text;

                    if (ushort.TryParse(body, NumberStyles.HexNumber, null, out ushort ushortVal))
                    {
                        return ushortVal;
                    }
                    else
                    {
                        throw new InvalidDataException(FormatErrorMessage(numberData.Start, "Must be int16 here"));
                    }
                }
                else if (numberData.floatnumber is IToken)
                {
                    throw new InvalidDataException(FormatErrorMessage(numberData.Start, "Float number is not allowed here"));
                }
                else
                {
                    throw new InvalidDataException(FormatErrorMessage(numberData.Start, "Unknown number data!"));
                }
            }
        }

        private interface ISegment
        {
            void Write(byte[] bytes);
            void WriteByte(byte value);
            void AllocateBytes(int num);
            ILabel GetLabel();
        }

        private class BssSegment : ISegment
        {
            public int numBytes = 0;

            public void AllocateBytes(int num)
            {
                numBytes += num;
            }

            public ILabel GetLabel()
            {
                return new BssLabel { ByteOffset = numBytes };
            }

            public void Write(byte[] bytes)
            {
                throw new InvalidOperationException("`section .bss` cannot place static data. Use `resb 4` or `resw 2` or such.");
            }

            public void WriteByte(byte value)
            {
                throw new InvalidOperationException("`section .bss` cannot place static data. Use `resb 4` or `resw 2` or such.");
            }
        }

        private class CodeSegment : ISegment
        {
            private readonly MemoryStream _words;

            public CodeSegment(MemoryStream words)
            {
                _words = words;
            }

            public int PC => (int)(_words.Length / 2 - 8);

            public void AllocateBytes(int num)
            {
                _words.Write(new byte[num]);
            }

            public ILabel GetLabel()
            {
                return new CodeLabel { PC = PC };
            }

            public void Write(byte[] buffer)
            {
                _words.Write(buffer);
            }

            public void WriteByte(byte value)
            {
                _words.WriteByte(value);
            }
        }

        private interface ILabel
        {

        }

        private class CodeLabel : ILabel
        {
            public int PC { get; set; }
        }

        private class BssLabel : ILabel
        {
            public int ByteOffset { get; set; }
        }

        private class ConstLabel : ILabel
        {
            public int Value { get; set; }
        }

        private record CompilePass(
            int Index,
            bool First,
            bool Last
        );

        private static readonly IFormatProvider invariantNumberFormat = CultureInfo.InvariantCulture.NumberFormat;
    }
}
