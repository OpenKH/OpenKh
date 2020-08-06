using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OpenKh.Kh2.Ard
{
    public static partial class SpawnScriptParser
    {
        private static readonly string[] PARTY = new string[]
        {
            "NO_FRIEND",
            "DEFAULT",
            "W_FRIEND",
            "W_FRIEND_IN",
            "W_FRIEND_FIX",
            "W_FRIEND_ONLY",
            "DONALD_ONLY",
        };

        public static string Decompile(IEnumerable<SpawnScript> scripts)
        {
            var sb = new StringBuilder();
            foreach (var script in scripts)
            {
                foreach (var line in AsText(script))
                {
                    sb.Append(line);
                    sb.Append('\n');
                }

                sb.Append('\n');
            }

            return sb.ToString();
        }

        public static IEnumerable<string> AsText(SpawnScript script)
        {
            yield return $"Program 0x{script.ProgramId:X02}";
            foreach (var f in script.Functions)
                yield return AsText(f);
        }

        public static string AsText(SpawnScript.Function function)
        {
            var p = function.Parameters;
            switch (function.Opcode)
            {
                case SpawnScript.Operation.Spawn:
                    return $"Spawn \"{ReadString(p[0])}\"";
                case SpawnScript.Operation.MapOcclusion:
                    return $"MapOcclusion 0x{p[0]:x08} 0x{p[1]:x08}";
                case SpawnScript.Operation.MultipleSpawn:
                    var spawns = function.Parameters.Select(ReadString).Select(s => $"\"{s}\"");
                    return $"MultipleSpawn {string.Join(" ", spawns)}";
                case (SpawnScript.Operation)3:
                    return $"Unk03 {p[0]} \"{ReadString(p[1])}\"";
                case (SpawnScript.Operation)4:
                    return $"Unk04 {p[0]}";
                case (SpawnScript.Operation)5:
                    return $"Unk05 {p[0]}";
                case (SpawnScript.Operation)6:
                    return $"Unk06 {p[0]}";
                case (SpawnScript.Operation)7:
                    return $"Unk07 {p[0]}";
                case (SpawnScript.Operation)9:
                    return $"Unk09 \"{ReadString(p[0])}\"";
                //case SpawnScript.Operation.Run: // One day...
                //    return RunAsText(function.Parameters);
                case SpawnScript.Operation.Party:
                    return $"Party {PARTY[p[0]]}";
                case SpawnScript.Operation.Bgm:
                    if (p[0] == 0)
                        return "BgmDefault";
                    return $"Bgm {p[0] & 0xffff} {(p[0] >> 16) & 0xffff}";
                case (SpawnScript.Operation)0x14:
                    return $"Unk14";
                case SpawnScript.Operation.Mission:
                    return $"Mission 0x{p[0]:x} \"" + string.Join(string.Empty,
                        ReadString(p[1]),
                        ReadString(p[2]),
                        ReadString(p[3]),
                        ReadString(p[4]),
                        ReadString(p[5]),
                        ReadString(p[6]),
                        ReadString(p[7]),
                        ReadString(p[8]) + "\"");
                case SpawnScript.Operation.Layout:
                    return $"Layout \"" + string.Join(string.Empty,
                        ReadString(p[0]),
                        ReadString(p[1]),
                        ReadString(p[2]),
                        ReadString(p[3]),
                        ReadString(p[4]),
                        ReadString(p[5]),
                        ReadString(p[6]),
                        ReadString(p[7]) + "\"");
                case (SpawnScript.Operation)0x17:
                    return $"Unk17";
                case SpawnScript.Operation.BattleLevel:
                    return $"BattleLevel {p[0]}";
                case (SpawnScript.Operation)0x1f:
                    return $"Unk1f \"{ReadString(p[0])}\"";
                default:
                    return $"Unk{(int)(function.Opcode):x02} {string.Join(" ", p.Select(x => $"0x{x:x}"))}";
            }
        }

        private static string RunAsText(List<int> parameters) // 00182280
        {
            var code = new List<string>();

            var i = 0;
            var a0 = parameters[i] & 0xffff;
            var a1 = parameters[i] >> 16;
            // 00198c28
            var head = $"Run 0x{a0:x} 0x{a1:x}";
            i++;

            while (i < parameters.Count)
            {
                var op = parameters[i] & 0xffff;
                var extraParam = parameters[i] >> 16;
                i++;
                switch (op)
                {
                    case 0:
                        code.Add($"Cutscene \"{ReadString(parameters[i++])}\" {parameters[i++]}");
                        break;
                    //case 1:
                    //    code.Add($"Unk01 0x{parameters[i++]:x} 0x{parameters[i++]:x} 0x{parameters[i++]:x}");
                    //    break;
                    case 2:
                        code.Add($"Op02 P:{extraParam} 0x{parameters[i++]:x} 0x{parameters[i++]:x} 0x{parameters[i++]:x}");
                        break;
                    case 4:
                        code.Add($"Op04 P:{extraParam} 0x{parameters[i++]:x} 0x{parameters[i++]:x}");
                        break;
                    case 5:
                        code.Add($"Op05 P:{extraParam} 0x{parameters[i++]:x} 0x{parameters[i++]:x}");
                        break;
                    case 6:
                        var itemCount = extraParam & 7;
                        var itemList = parameters.Skip(i).Take(extraParam).Select(x => $"{x}");
                        code.Add($"GetItem {string.Join(" ", itemList)}");
                        i += itemCount;
                        break;
                    case 7:
                        code.Add("End");
                        break;
                    case 8:
                        code.Add($"Op08 P:{extraParam} 0x{parameters[i++]:x}");
                        i++;
                        break;
                    default:
                        code.Add($"UnkRun{op:x02} DUMP {string.Join(" ", parameters.Select(p => $"0x{p:x}"))}");
                        i = int.MaxValue;
                        break;
                }
            }

            return $"{head}\n\t{string.Join("\n\t", code)}";
        }

        private static void sub_00198c28(int a0, int a1)
        {
        }

        private static string ReadString(int parameter)
        {
            var data = new byte[]
            {
                (byte)((parameter >> 0) & 0xff),
                (byte)((parameter >> 8) & 0xff),
                (byte)((parameter >> 16) & 0xff),
                (byte)((parameter >> 24) & 0xff),
            };

            var length = 0;
            while (length < data.Length)
            {
                if (data[length] == 0)
                    break;
                length++;
            }

            return Encoding.ASCII.GetString(data, 0, length);
        }
    }
}
