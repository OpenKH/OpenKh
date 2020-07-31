using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OpenKh.Kh2.Ard
{
    public static class SpawnScriptParser
    {
        public static IEnumerable<string> AsText(SpawnScript script)
        {
            yield return $"Program 0x{script.ProgramId}";
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
                case (SpawnScript.Operation)5: // 0034ecd0
                    return $"Set05 {p[0]}";
                case (SpawnScript.Operation)6: // 0034ecd8
                    return $"Set06 {p[0]}";
                case (SpawnScript.Operation)7: // 0034ecdc
                    return $"Set07 {p[0]}";
                case SpawnScript.Operation.MultipleSpawn:
                    var spawns = function.Parameters.Select(ReadString).Select(s => $"\"{s}\"");
                    return $"MultipleSpawn {string.Join(" ", spawns)}";
                case SpawnScript.Operation.Run:
                    return RunAsText(function.Parameters);
                case SpawnScript.Operation.Bgm:
                    if (p[0] == 0)
                        return "BgmDefault";
                    return $"Bgm {p[0] & 0xffff} {(p[0] >> 16) & 0xffff}";
                default:
                    return $"{(int)(function.Opcode):x02} {string.Join(" ", p.Select(x => $"0x{x:x}"))}";
            }
        }

        private static string ReadString(int parameter) =>
            Encoding.ASCII.GetString(new byte[]
            {
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
