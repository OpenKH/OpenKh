using OpenKh.Command.Bdxio.Models;
using OpenKh.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xe.BinaryMapper;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NodeTypeResolvers;
using static OpenKh.Command.Bdxio.Models.BdxHeader;

namespace OpenKh.Command.Bdxio.Utils
{
    public class BdxDecoder
    {
        public BdxHeader Header { get; }

        public IDictionary<int, IAnnotation> AddressAnnotated { get; }
        public IDictionary<int, string> AddressLabels { get; }
        public IDictionary<int, string> AddressComments { get; }
        public IDictionary<int, string> AddressCodeComments { get; }
        public IDictionary<int, string> WorkLabels { get; }
        public string TopComment { get; set; }

        public BdxDecoder(Stream read, int[]? codeLabels = null, bool codeRevealer = false, bool codeRevealerLabeling = false)
        {
            if (read.Length == 0)
            {
                Header = new BdxHeader { IsEmpty = true };
                AddressAnnotated = new Dictionary<int, IAnnotation>();
                AddressLabels = new Dictionary<int, string>();
                AddressComments = new Dictionary<int, string>();
                AddressCodeComments = new Dictionary<int, string>();
                WorkLabels = new Dictionary<int, string>();
                TopComment = "";
                return;
            }

            Header = BinaryMapping.ReadObject<BdxHeader>(read);

            var triggers = new List<BdxHeader.IntTrigger>();
            while (true)
            {
                var one = BinaryMapping.ReadObject<BdxHeader.IntTrigger>(read);
                if (one.Key == 0 && one.Addr == 0)
                {
                    break;
                }
                triggers.Add(one);
            }

            int firstAddr = (12 + 8 * (triggers.Count + 1)) / 2;

            var addrAnnotated = new SortedDictionary<int, IAnnotation>();
            var addrLabels = new SortedDictionary<int, string>();
            var addrComments = new SortedDictionary<int, string>();
            var addrCodeComments = new SortedDictionary<int, string>();
            var workLabels = new SortedDictionary<int, string>();
            var topComment = "";

            void AddAddr(int entry, string label)
            {
                if (addrLabels.TryGetValue(entry, out string? existing) && existing != null)
                {
                    addrLabels[entry] = string.Join(",", existing.Split(',').Append(label).Distinct());
                }
                else
                {
                    addrLabels[entry] = label;
                }
            }

            void Walk(int entry)
            {
                var topAddr = entry;

                while (true)
                {
                    var needClearance = false;

                    if (addrAnnotated.TryGetValue(entry, out var annotHere))
                    {
                        if (annotHere is IndeterminateContent)
                        {
                            needClearance = true;
                        }
                        else
                        {
                            return;
                        }
                    }

                    var newPos = 16 + 2 * entry;
                    if (read.Length <= newPos)
                    {
                        // bug?
                        break;
                    }

                    read.Position = newPos;
                    var codeWord = read.ReadUInt16();

                    var desc = BdxInstructionDescs.FindByCode(codeWord);
                    if (desc == null)
                    {
                        break;
                    }

                    var code = new CodeContent(desc);

                    if (needClearance)
                    {
                        var overrideDetected = false;
                        int x = 1;
                        for (; x < desc.CodeSize; x++)
                        {
                            if (true
                                && addrAnnotated.TryGetValue(entry + x, out var codePartHit)
                                && codePartHit is not IndeterminateContent
                            )
                            {
                                overrideDetected = true;
                                break;
                            }

                            if (addrLabels.TryGetValue(entry + x, out var codePartLabels))
                            {
                                overrideDetected = true;
                                break;
                            }
                        }
                        if (overrideDetected)
                        {
                            addrComments[entry] = $"Although L{entry + x} is in IndeterminateContent area (L{entry} to L{entry + desc.CodeSize - 1}), there is a collision with text data? Maybe codeRevealer problem. Possibly need more better bottom text pool detection.";
                            break;
                        }
                    }

                    addrAnnotated[entry] = code;

                    if (desc.NeverReturn || desc.IsGosubRet)
                    {
                        break;
                    }

                    if (desc.Args != null)
                    {
                        var parsedArgs = new List<ParsedArg>();

                        foreach (var arg in desc.Args)
                        {
                            var parsedArg = new ParsedArg();

                            int argValue;
                            if (arg.Type == BdxInstructionDesc.ArgType.Imm16)
                            {
                                argValue = read.ReadInt16();
                            }
                            else if (arg.Type == BdxInstructionDesc.ArgType.Imm32)
                            {
                                argValue = read.ReadInt32();
                            }
                            else if (arg.Type == BdxInstructionDesc.ArgType.Float32)
                            {
                                argValue = read.ReadInt32();
                                parsedArg.PreferFloat32 = true;
                            }
                            else
                            {
                                argValue = new BdxInstruction(codeWord).Ssub;
                            }


                            if (arg.IsRelative)
                            {
                                var nextEntry = (ushort)(entry + (desc.CodeSize) + argValue);

                                parsedArg.Value = nextEntry;
                                parsedArg.AddrRef = nextEntry;

                                if (desc.IsJump || desc.IsGosub)
                                {
                                    var newLabel = $"L{nextEntry}";
                                    parsedArg.Label = newLabel;
                                    AddAddr(nextEntry, newLabel);
                                    Walk(nextEntry);
                                }
                            }
                            else if (arg.AiPos)
                            {
                                parsedArg.Value = (ushort)argValue;

                                var newLabel = $"L{(ushort)argValue}";
                                parsedArg.Label = newLabel;
                                AddAddr((ushort)argValue, newLabel);
                            }
                            else if (arg.WorkPos)
                            {
                                parsedArg.Value = (ushort)argValue;

                                var newLabel = $"W{(ushort)argValue}";
                                parsedArg.Label = newLabel;
                                workLabels[(ushort)argValue] = newLabel;
                            }
                            else
                            {
                                parsedArg.Value = argValue;
                            }

                            parsedArgs.Add(parsedArg);
                        }

                        if (desc.IsSyscall)
                        {
                            var tableIdx = new BdxInstruction(codeWord).Ssub;
                            var trapIdx = parsedArgs[1].Value;
                            var trap = BdxTraps.GetTraps()
                                .FirstOrDefault(it => it.TableIndex == tableIdx && it.TrapIndex == trapIdx);
                            addrCodeComments[entry] = (trap != null)
                                ? $"{trap.Name} ({trap.Flags & 65535} in, {(((trap.Flags & 0x40000000) != 0) ? 1 : 0)} out)"
                                : "?";
                        }

                        code.Args = parsedArgs.ToArray();
                    }

                    for (int x = 1; x < desc.CodeSize; x++)
                    {
                        addrAnnotated[entry + x] = CodeArg.Value;
                    }

                    if (desc.IsJump && !desc.IsJumpConditional)
                    {
                        break;
                    }

                    entry += desc.CodeSize;
                }
            }

            var knownItems = BdxTrigger.GetKnownItems()
                .ToDictionary(
                    one => one.Key,
                    one => one.Label
                );

            string GetTriggerLabelFor(int key)
            {
                if (knownItems.TryGetValue(key, out string? label) && label != null)
                {
                    return label;
                }
                else
                {
                    return $"TR{key}";
                }
            }

            foreach (var trigger in triggers)
            {
                AddAddr(trigger.Addr, GetTriggerLabelFor(trigger.Key));
                Walk(trigger.Addr);
            }

            foreach (var codeLabel in codeLabels ?? new int[0])
            {
                AddAddr(codeLabel, $"L{codeLabel}");
                Walk(codeLabel);
            }

            Header.Triggers = triggers
                .Select(
                    it => new Trigger
                    {
                        Addr = it.Addr.ToString(),
                        Key = it.Key,
                    }
                )
                .ToArray();

            var revealed = new List<int>();

            for (int addr = firstAddr; addr < read.Length / 2 - 8; addr++)
            {
                if (addrAnnotated.ContainsKey(addr))
                {
                    continue;
                }

                read.Position = 16 + 2 * addr;
                var dataWord = read.ReadUInt16();

                addrAnnotated[addr] = new IndeterminateContent(dataWord);

                if (addrAnnotated.TryGetValue(addr - 1, out var prevAnnot) && prevAnnot is not IndeterminateContent prevWord)
                {
                    AddAddr(addr, $"D{addr}");
                }
            }

            var firstTextPoolAddr = FindFirstTextPoolAddr(addrAnnotated);

            {
                // resolve strings

                var dataList = addrAnnotated
                    .Where(pair => pair.Value is IndeterminateContent)
                    .ToList();

                while (dataList.Any())
                {
                    var baseAddr = dataList[0].Key;

                    if (firstTextPoolAddr <= baseAddr)
                    {
                        int numContinuous = 1;
                        while (true)
                        {
                            if (true
                                && numContinuous < dataList.Count
                                && dataList[numContinuous].Value is IndeterminateContent
                                && dataList[numContinuous].Key == baseAddr + numContinuous
                            )
                            {
                                numContinuous += 1;
                                continue;
                            }
                            else
                            {
                                break;
                            }
                        }

                        if (numContinuous >= 2)
                        {
                            var bytes = new byte[2 * numContinuous];
                            for (int x = 0; x < numContinuous; x++)
                            {
                                var word = ((IndeterminateContent)dataList[x].Value).Word;
                                bytes[2 * x + 0] = (byte)(word);
                                bytes[2 * x + 1] = (byte)(word >> 8);
                            }

                            static (int numValidWords, IAnnotation? annot) TryToDecode(byte[] bytes)
                            {
                                for (int x = 0; x < bytes.Length; x++)
                                {
                                    if (bytes[x] == 0)
                                    {
                                        if (x < 1)
                                        {
                                            // The length is not enough to decide this is a text.
                                            break;
                                        }
                                        if ((x & 1) == 0)
                                        {
                                            x++;
                                        }

                                        // This is a valid text
                                        return (
                                            (x + 1) / 2,
                                            new TextContent(Encoding.ASCII.GetString(bytes, 0, x + 1))
                                        );
                                    }
                                    if (IsValidChar(bytes[x]))
                                    {
                                        // A valid char
                                        continue;
                                    }
                                    else
                                    {
                                        // An invalid char
                                        break;
                                    }
                                }

                                // No valid text found
                                return (0, null);
                            }

                            var (numValidWords, annot) = TryToDecode(bytes);

                            if (1 <= numValidWords && annot != null)
                            {
                                dataList.RemoveRange(0, numValidWords);
                                for (int x = 0; x < numValidWords; x++)
                                {
                                    if (x == 0)
                                    {
                                        AddAddr(baseAddr + x, $"TXT{baseAddr + x}");
                                        addrAnnotated[baseAddr + x] = annot;
                                    }
                                    else
                                    {
                                        addrAnnotated[baseAddr + x] = TextPart.Value;
                                    }
                                }
                                continue;
                            }
                        }
                    }

                    dataList.RemoveAt(0);
                }
            }

            if (codeRevealer)
            {
                while (true)
                {
                    var firstIndeterminate = addrAnnotated
                        .FirstOrDefault(
                            pair => true
                                && pair.Key < firstTextPoolAddr
                                && pair.Value is IndeterminateContent
                        );

                    // KeyValuePair is struct (value) and it cannot be null. While Value is null-able instead.

                    if (firstIndeterminate.Value == null)
                    {
                        break;
                    }

                    var addr = firstIndeterminate.Key;

                    addrAnnotated.Remove(firstIndeterminate.Key);

                    AddAddr(addr, $"L{addr}");
                    Walk(addr);

                    revealed.Add(addr);
                }
            }

            if (codeRevealerLabeling)
            {
                foreach (var pair in addrAnnotated.ToArray())
                {
                    if (pair.Value is CodeContent content)
                    {
                        if (content.Desc.Name == "pushImm"
                            && content.Args.Length == 1
                            && content.Args[0] is ParsedArg arg
                            && arg.Label == null
                            && arg.Value != 0
                        )
                        {
                            if (revealed.Contains(arg.Value))
                            {
                                content.Args[0].Label = $"L{arg.Value}";
                                content.Args[0].AddrRef = arg.Value;
                            }
                        }
                    }
                }
            }

            {
                foreach (var pair in addrAnnotated.ToArray())
                {
                    if (pair.Value is IndeterminateContent indeterminate)
                    {
                        addrAnnotated[pair.Key] = new WordContent(indeterminate.Word);
                    }
                }
            }

            topComment = string.Join("\n"
                , $"{nameof(codeLabels)}: {((codeLabels != null) ? string.Join(" ", codeLabels.Select(it => $"-l {it}")) : "")}"
                , $"{nameof(codeRevealer)}: {((revealed != null) ? string.Join(" ", revealed.Select(it => $"-l {it}")) : "")}"
            );

            AddressAnnotated = addrAnnotated;
            AddressLabels = addrLabels;
            AddressComments = addrComments;
            AddressCodeComments = addrCodeComments;
            WorkLabels = workLabels;
            TopComment = topComment;
        }

        private static int FindFirstTextPoolAddr(IDictionary<int, IAnnotation> addrAnnotated)
        {
            var firstTextPoolAddr = int.MaxValue;

            var dataPairs = addrAnnotated
                .OrderByDescending(pair => pair.Key)
                .Where(pair => pair.Value is IndeterminateContent);

            if (dataPairs.Any())
            {
                foreach (var pair in dataPairs)
                {
                    var content = (IndeterminateContent)pair.Value;
                    var bytes = new byte[] { (byte)(content.Word), (byte)(content.Word >> 8) };
                    if (true
                        && (bytes[0] == 0 || IsValidChar(bytes[0]))
                        && (bytes[1] == 0 || IsValidChar(bytes[1]))
                    )
                    {
                        firstTextPoolAddr = pair.Key;
                        continue;
                    }
                    else
                    {
                        break;
                    }
                }
            }

            return firstTextPoolAddr;
        }

        private static bool IsValidChar(byte v)
        {
            var chr = (char)v;
            return 32 <= chr && chr <= 126;
        }

        public interface IAnnotation
        {

        }

        public class ParsedArg
        {
            public string? Label { get; set; }
            public int Value { get; set; }
            public int? AddrRef { get; set; }
            public bool PreferFloat32 { get; set; }
        }

        public sealed class CodeContent : IAnnotation
        {
            public CodeContent(BdxInstructionDesc desc)
            {
                Desc = desc;
            }

            public BdxInstructionDesc Desc { get; set; }
            public ParsedArg[] Args { get; set; } = new ParsedArg[0];

            public override string ToString() => Desc?.ToString() ?? "?";
        }

        public sealed class CodeArg : IAnnotation
        {
            private CodeArg()
            {

            }

            public override string ToString() => $"CodeArg";

            public static readonly CodeArg Value = new CodeArg();
        }

        public sealed class IndeterminateContent : IAnnotation
        {
            public ushort Word { get; set; }

            public IndeterminateContent(ushort word)
            {
                Word = word;
            }

            public override string ToString() => $"Indeterminate";
        }

        public sealed class WordContent : IAnnotation
        {
            public ushort Word { get; set; }

            public WordContent(ushort word)
            {
                Word = word;
            }

            public override string ToString() => $"Word";
        }

        public sealed class TextContent : IAnnotation
        {
            public string Text { get; set; }

            public TextContent(string text)
            {
                Text = text;
            }

            public override string ToString() => $"Text: {Text}";
        }

        public sealed class TextPart : IAnnotation
        {
            private TextPart()
            {

            }

            public override string ToString() => $"TextPart";

            public static readonly TextPart Value = new TextPart();
        }

        public static class TextFormatter
        {
            private static ISerializer ser = new SerializerBuilder()
                .Build();

            public static string Format(BdxDecoder decoder)
            {
                if (decoder.Header.IsEmpty)
                {
                    var empty = new StringWriter();
                    empty.WriteLine("---");
                    empty.WriteLine("---");
                    return empty.ToString();
                }

                var writer = new StringWriter();

                writer.WriteLine("---");
                writer.WriteLine(
                    ser
                        .Serialize(
                            new BdxHeader
                            {
                                Name = decoder.Header.Name,
                                WorkSize = decoder.Header.WorkSize,
                                StackSize = decoder.Header.StackSize,
                                TempSize = decoder.Header.TempSize,
                                Triggers = decoder.Header.Triggers
                                    .Select(
                                        it =>
                                        {
                                            if (int.TryParse(it.Addr, out int addr))
                                            {
                                                decoder.AddressLabels.TryGetValue(addr, out string? labels);

                                                return new Trigger
                                                {
                                                    Key = it.Key,
                                                    Addr = ((labels != null) ? labels.Split(',')[0] : it.Addr),
                                                };
                                            }
                                            else
                                            {
                                                return new Trigger
                                                {
                                                    Key = it.Key,
                                                    Addr = it.Addr,
                                                };
                                            }
                                        }
                                    )
                                    .ToArray(),
                            }
                        )
                );
                writer.WriteLine("---");

                {
                    if (!string.IsNullOrEmpty(decoder.TopComment))
                    {
                        foreach (var comment in decoder.TopComment.Split('\n'))
                        {
                            writer.WriteLine($"; {comment}");
                        }
                    }
                }

                writer.WriteLine(" section .text");

                foreach (var line in decoder.AddressAnnotated)
                {
                    if (decoder.AddressComments.TryGetValue(line.Key, out var commentBody) && commentBody != null)
                    {
                        foreach (var comment in commentBody.Replace("\r\n", "\n").Split('\n'))
                        {
                            writer.WriteLine($" ; {comment}");
                        }
                    }

                    if (decoder.AddressLabels.TryGetValue(line.Key, out string? labels) && labels != null)
                    {
                        foreach (var label in labels.Split(','))
                        {
                            writer.WriteLine($"{label}:");
                        }
                    }

                    if (line.Value is CodeContent code)
                    {
                        if (decoder.AddressCodeComments.TryGetValue(line.Key, out string? commentPart))
                        {
                            commentPart = " ; " + commentPart;
                        }

                        writer.WriteLine($" {code.Desc.Name} {string.Join(", ", code.Args.Select(FormatArg))}{commentPart}");
                    }
                    else if (line.Value is WordContent data)
                    {
                        writer.WriteLine($" dw 0x{data.Word:X4}");
                    }
                    else if (line.Value is TextContent text)
                    {
                        writer.WriteLine($" db {MakeDbBody(text.Text)}");
                    }
                }

                if (decoder.Header.WorkSize != 0)
                {
                    writer.WriteLine();
                    writer.WriteLine(" section .bss");
                    if (decoder.WorkLabels.Any())
                    {
                        int lastPos = 0;
                        foreach (var workLabel in decoder.WorkLabels)
                        {
                            var thisPos = workLabel.Key;
                            if (lastPos < thisPos)
                            {
                                writer.WriteLine($" resb {thisPos - lastPos}");
                                lastPos = thisPos;
                            }
                            writer.WriteLine($"{workLabel.Value}:");
                        }
                        {
                            var thisPos = decoder.Header.WorkSize;
                            if (lastPos < thisPos)
                            {
                                writer.WriteLine($" resb {thisPos - lastPos}");
                            }
                        }
                    }
                    else
                    {
                        writer.WriteLine($" resb {decoder.Header.WorkSize}");
                    }
                }

                return writer.ToString();
            }

            private static string FormatArg(ParsedArg arg)
            {
                if (arg.Label != null)
                {
                    return $"{arg.Label}"; //offset
                }
                else if (arg.PreferFloat32)
                {
                    return BitConverter.ToSingle(BitConverter.GetBytes(arg.Value)).ToString("R", invariantNumberFormat);
                }
                else
                {
                    return arg.Value.ToString();
                }
            }

            private static string MakeDbBody(string text)
            {
                var bytes = Encoding.ASCII.GetBytes(text);

                bool IsPrint(byte b) => 0x20 <= b && b <= 0x7E && b != 0x27 && b != 0x5C;

                var tokens = new List<string>();

                for (int x = 0, cx = bytes.Length; x < cx;)
                {
                    {
                        int from = x;
                        while (x < cx && IsPrint(bytes[x]))
                        {
                            x++;
                        }
                        if (from != x)
                        {
                            tokens.Add("'" + Encoding.ASCII.GetString(bytes, from, x - from) + "'");
                        }
                    }

                    {
                        int from = x;
                        while (x < cx && !IsPrint(bytes[x]))
                        {
                            x++;
                        }
                        if (from != x)
                        {
                            for (; from < x; from++)
                            {
                                tokens.Add(bytes[from].ToString());
                            }
                        }
                    }
                }

                return string.Join(",", tokens);
            }
        }

        private static readonly IFormatProvider invariantNumberFormat = CultureInfo.InvariantCulture.NumberFormat;
    }
}
