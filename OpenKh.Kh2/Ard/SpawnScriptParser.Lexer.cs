using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace OpenKh.Kh2.Ard
{
    public class SpawnScriptParserException : Exception
    {
        public SpawnScriptParserException(int line, string message) :
            base($"Line {line}: {message}")
        { }
    }

    public class SpawnScriptMissingProgramException : SpawnScriptParserException
    {
        public SpawnScriptMissingProgramException(int line) :
            base(line, "Expected \"Program\" statement.")
        { }
    }

    public class SpawnScriptNotANumberException : SpawnScriptParserException
    {
        public SpawnScriptNotANumberException(int line, string token) :
            base(line, $"Expected an integer value, but got '{token}'.")
        { }
    }

    public class SpawnScriptShortException : SpawnScriptParserException
    {
        public SpawnScriptShortException(int line, int value) :
            base(line, $"The value {value} is too big. Please chose a value between {0} and {short.MaxValue}.")
        { }
    }

    public class SpawnScriptStringTooLongException : SpawnScriptParserException
    {
        public SpawnScriptStringTooLongException(int line, string value, int maxLength) :
            base(line, $"The value \"{value}\" is {value.Length} characters long, but the maximum allowed is {maxLength}.")
        { }
    }

    public class SpawnScriptInvalidEnumException : SpawnScriptParserException
    {
        public SpawnScriptInvalidEnumException(int line, string token, IEnumerable<string> allowed) :
            base(line, $"The token '{token}' is not recognized as only {string.Join(",", allowed.Select(x => $"'{x}'"))} are allowed.")
        { }
    }



    public static partial class SpawnScriptParser
    {
        enum LexState
        {
            Init,
            Code,
        }

        public static IEnumerable<SpawnScript> Compile(string text)
        {
            const char Comment = '#';

            var script = NewScript();
            var state = LexState.Init;
            var lines = text
                .Replace("\n\r", "\n")
                .Replace("\r\n", "\n")
                .Replace("\r", "\n")
                .Split('\n');
            var row = 0;

            foreach (var line in lines)
            {
                row++;
                var cleanLine = line.Split(Comment);
                var tokens = Tokenize(row, cleanLine[0]).ToList();
                if (tokens.Count == 0)
                    continue;

                if (state == LexState.Init && tokens[0] != "Program")
                    throw new SpawnScriptMissingProgramException(row);

                SpawnScript.Function func;
                switch (tokens[0])
                {
                    case "Program":
                        var programId = ParseAsShort(row, GetToken(row, tokens, 1));
                        if (state == LexState.Code)
                        {
                            yield return script;
                            script = NewScript();
                        }

                        state = LexState.Code;
                        script.ProgramId = programId;
                        break;
                    case "Spawn":
                        func = NewFunction(SpawnScript.Operation.Spawn);
                        AddString(row, func, GetToken(row, tokens, 1), 4);
                        script.Functions.Add(func);
                        break;
                    case "MapOcclusion":
                        script.Functions.Add(NewFunction(SpawnScript.Operation.MapOcclusion,
                            ParseAsInt(row, GetToken(row, tokens, 1)),
                            ParseAsInt(row, GetToken(row, tokens, 2))));
                        break;
                    case "MultipleSpawn":
                        func = NewFunction(SpawnScript.Operation.MultipleSpawn);
                        for (var i = 1; i < tokens.Count; i++)
                            AddString(row, func, GetToken(row, tokens, i), 4);
                        script.Functions.Add(func);
                        break;
                    case "Unk03":
                        func = NewFunction((SpawnScript.Operation)0x03,
                            ParseAsInt(row, GetToken(row, tokens, 1)));
                        AddString(row, func, GetToken(row, tokens, 2), 4);
                        script.Functions.Add(func);
                        break;
                    case "Unk04":
                        script.Functions.Add(NewFunction((SpawnScript.Operation)0x04,
                            ParseAsInt(row, GetToken(row, tokens, 1))));
                        break;
                    case "Unk05":
                        script.Functions.Add(NewFunction((SpawnScript.Operation)0x05,
                            ParseAsInt(row, GetToken(row, tokens, 1))));
                        break;
                    case "Unk06":
                        script.Functions.Add(NewFunction((SpawnScript.Operation)0x06,
                            ParseAsInt(row, GetToken(row, tokens, 1))));
                        break;
                    case "Unk07":
                        script.Functions.Add(NewFunction((SpawnScript.Operation)0x07,
                            ParseAsInt(row, GetToken(row, tokens, 1))));
                        break;
                    case "Unk09":
                        func = NewFunction((SpawnScript.Operation)0x09);
                        AddString(row, func, GetToken(row, tokens, 1), 4);
                        script.Functions.Add(func);
                        break;
                    case "Party":
                        script.Functions.Add(NewFunction(SpawnScript.Operation.Party,
                            ParseAsEnum(row, GetToken(row, tokens, 1), PARTY)));
                        break;
                    case "Bgm":
                        script.Functions.Add(NewFunction(SpawnScript.Operation.Bgm,
                            ParseAsInt(row, GetToken(row, tokens, 1)) |
                            ParseAsInt(row, GetToken(row, tokens, 2)) << 16));
                        break;
                    case "BgmDefault":
                        script.Functions.Add(NewFunction(SpawnScript.Operation.Bgm, 0));
                        break;
                    case "Unk14":
                        script.Functions.Add(NewFunction((SpawnScript.Operation)0x14));
                        break;
                    case "Mission":
                        func = NewFunction(SpawnScript.Operation.Mission);
                        func.Parameters.Add(ParseAsInt(row, GetToken(row, tokens, 1)));
                        AddString(row, func, GetToken(row, tokens, 2), 32);
                        script.Functions.Add(func);
                        break;
                    case "Layout":
                        func = NewFunction(SpawnScript.Operation.Layout);
                        AddString(row, func, GetToken(row, tokens, 1), 32);
                        script.Functions.Add(func);
                        break;
                    case "Unk17":
                        script.Functions.Add(NewFunction((SpawnScript.Operation)0x17));
                        break;
                    case "BattleLevel":
                        script.Functions.Add(NewFunction(SpawnScript.Operation.BattleLevel,
                            ParseAsInt(row, GetToken(row, tokens, 1))));
                        break;
                    case "Unk1f":
                        func = NewFunction((SpawnScript.Operation)0x1f);
                        AddString(row, func, GetToken(row, tokens, 1), 4);
                        script.Functions.Add(func);
                        break;
                    default:
                        // Fallback for unimplemented opcodes with variable parameter length.
                        if (tokens[0].Length == 5)
                        {
                            var strOpcodeId = tokens[0].Substring(3);
                            if (int.TryParse(strOpcodeId, NumberStyles.HexNumber, null, out var opcodeId))
                            {
                                func = NewFunction((SpawnScript.Operation)opcodeId);
                                for (var i = 1; i < tokens.Count; i++)
                                    func.Parameters.Add(ParseAsInt(row, GetToken(row, tokens, i)));
                                script.Functions.Add(func);
                                break;
                            }
                        }
                        throw new SpawnScriptParserException(row, $"Unrecognized token '{tokens[0]}'.");
                }
            }

            if (state == LexState.Code)
                yield return script;
            yield break;
        }

        private static IEnumerable<string> Tokenize(int row, string line)
        {
            var sb = new StringBuilder();
            for (var i = 0; i < line.Length; i++)
            {
                var ch = line[i];
                if (char.IsWhiteSpace(ch))
                {
                    if (sb.Length > 0)
                        yield return sb.ToString();
                    sb.Clear();
                    continue;
                }
                else if (ch == '"')
                {
                    sb.Append(ch);
                    do
                    {
                        i++;
                        if (i >= line.Length)
                            throw new SpawnScriptParserException(row, $"Missing '\"'.");
                        sb.Append(ch = line[i]);
                    } while (ch != '"');
                }
                else
                    sb.Append(ch);
            }

            if (sb.Length > 0)
                yield return sb.ToString();
        }

        private static SpawnScript NewScript() => new SpawnScript
        {
            Functions = new List<SpawnScript.Function>()
        };

        private static SpawnScript.Function NewFunction(SpawnScript.Operation opcode, params int[] parameters) =>
            new SpawnScript.Function
            {
                Opcode = opcode,
                Parameters = parameters.ToList()
            };

        private static string GetToken(int row, List<string> tokens, int tokenIndex)
        {
            return tokens[tokenIndex];
        }

        private static short ParseAsShort(int row, string text)
        {
            var value = ParseAsInt(row, text);
            if (value < 0 || value > short.MaxValue)
                throw new SpawnScriptShortException(row, value);
            return (short)value;
        }

        private static int ParseAsInt(int row, string token)
        {
            var isHexadecimal = token.Length > 2 &&
                token[0] == '0' && token[1] == 'x';
            var nStyle = isHexadecimal ? NumberStyles.HexNumber : NumberStyles.Integer;
            var strValue = isHexadecimal ? token.Substring(2) : token;

            if (!int.TryParse(strValue, nStyle, null, out var value))
                throw new SpawnScriptNotANumberException(row, token);
            return value;
        }

        private static int ParseAsEnum(int row, string token, string[] allowValues)
        {
            for (var i = 0; i < allowValues.Length; i++)
            {
                if (token == allowValues[i])
                    return i;
            }

            throw new SpawnScriptInvalidEnumException(row, token, allowValues);
        }

        private static void AddString(int row, SpawnScript.Function func, string str, int maxLength)
        {
            if (str.Length < 2)
                throw new SpawnScriptParserException(row, $"Expected a string but got '{str}'");
            if (str[0] != '"' || str[str.Length - 1] != '"')
                throw new SpawnScriptParserException(row, $"Expected a string but got '{str}' with probably wrong double-quotes.");
            str = str.Substring(1, str.Length - 2);

            if (str.Length > maxLength)
                throw new SpawnScriptStringTooLongException(row, str, maxLength);
            for (var i = 0; i < maxLength; i += 4)
                func.Parameters.Add(GetStringAsInt(str, i));
        }

        private static int GetStringAsInt(string str, int startIndex)
        {
            if (startIndex >= str.Length)
                return 0;

            var value = 0;
            for (var i = 0; i < 4; i++)
            {
                if (startIndex + i < str.Length)
                    value |= ((byte)str[startIndex + i] << i * 8);
            }

            return value;
        }
    }
}
