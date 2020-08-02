using OpenKh.Common;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Xe.BinaryMapper;

namespace OpenKh.Kh2.Ard
{
    public class SpawnScript
    {
        private class Header
        {
            [Data] public short Id { get; set; }
            [Data] public short Length { get; set; }
        }

        public enum Operation
        {
            Spawn,
            MapOcclusion,
            MultipleSpawn,
            Run = 0xc,
            Party = 0x0f,
            Bgm = 0x10,
            Mission = 0x15,
            Layout = 0x16,
            BattleLevel = 0x1e,
        }

        public class Function
        {
            public Operation Opcode { get; set; }
            public List<int> Parameters { get; set; }

            public string AsString(int index) => ReadString(Parameters[index]);

            public override string ToString()
            {
                switch ((int)Opcode)
                {
                    case 0x00:
                        return $"Spawn({ReadString(Parameters[0])})";
                    case 0x01:
                        // Stores the data to qword_34ECC0
                        return $"MapOcclusion({Parameters[1]:X08}{Parameters[0]:X08})";
                    //case 0x02: throw new NotImplementedException($"OPCODE {Opcode:X02} not implemented.");
                    //case 0x03: throw new NotImplementedException($"OPCODE {Opcode:X02} not implemented.");
                    //case 0x04: throw new NotImplementedException($"OPCODE {Opcode:X02} not implemented.");
                    //case 0x05: throw new NotImplementedException($"OPCODE {Opcode:X02} not implemented.");
                    //case 0x06: throw new NotImplementedException($"OPCODE {Opcode:X02} not implemented.");
                    //case 0x07: throw new NotImplementedException($"OPCODE {Opcode:X02} not implemented.");
                    //case 0x08: throw new NotImplementedException($"OPCODE {Opcode:X02} not implemented.");
                    //case 0x09: throw new NotImplementedException($"OPCODE {Opcode:X02} not implemented.");
                    //case 0x0a: throw new NotImplementedException($"OPCODE {Opcode:X02} not implemented.");
                    //case 0x0b: throw new NotImplementedException($"OPCODE {Opcode:X02} not implemented.");
                    //case 0x0c: throw new NotImplementedException($"OPCODE {Opcode:X02} not implemented.");
                    //case 0x0e: throw new NotImplementedException($"OPCODE {Opcode:X02} not implemented.");
                    //case 0x0f: throw new NotImplementedException($"OPCODE {Opcode:X02} not implemented.");
                    //case 0x10: throw new NotImplementedException($"OPCODE {Opcode:X02} not implemented.");
                    //case 0x11: throw new NotImplementedException($"OPCODE {Opcode:X02} not implemented.");
                    //case 0x12: throw new NotImplementedException($"OPCODE {Opcode:X02} not implemented.");
                    //case 0x13: throw new NotImplementedException($"OPCODE {Opcode:X02} not implemented.");
                    //case 0x14: throw new NotImplementedException($"OPCODE {Opcode:X02} not implemented.");
                    //case 0x15: throw new NotImplementedException($"OPCODE {Opcode:X02} not implemented.");
                    //case 0x16: throw new NotImplementedException($"OPCODE {Opcode:X02} not implemented.");
                    //case 0x17: throw new NotImplementedException($"OPCODE {Opcode:X02} not implemented.");
                    //case 0x18: throw new NotImplementedException($"OPCODE {Opcode:X02} not implemented.");
                    //case 0x19: throw new NotImplementedException($"OPCODE {Opcode:X02} not implemented.");
                    //case 0x1a: throw new NotImplementedException($"OPCODE {Opcode:X02} not implemented.");
                    //case 0x1b: throw new NotImplementedException($"OPCODE {Opcode:X02} not implemented.");
                    //case 0x1c: throw new NotImplementedException($"OPCODE {Opcode:X02} not implemented.");
                    //case 0x1d: throw new NotImplementedException($"OPCODE {Opcode:X02} not implemented.");
                    //case 0x1e: throw new NotImplementedException($"OPCODE {Opcode:X02} not implemented.");
                    //case 0x1f: throw new NotImplementedException($"OPCODE {Opcode:X02} not implemented.");
                    default:
                        // If the OPCODE is not between 00 and 1f, the game just ignores it.
                        return $"{Opcode:X02}({string.Join(", ", Parameters.Select(p => $"{p:X}"))})";
                }
            }

            private static string ReadString(int parameter) =>
                Encoding.ASCII.GetString(new byte[]
                {
                    (byte)((parameter >> 0) & 0xff),
                    (byte)((parameter >> 8) & 0xff),
                    (byte)((parameter >> 16) & 0xff),
                    (byte)((parameter >> 24) & 0xff),
                });
        }

        public short ProgramId { get; set; }
        public List<Function> Functions { get; set; }

        public override string ToString() =>
            $"Program {ProgramId:X}\n{string.Join("\n", Functions.Select(x => x.ToString()))}";

        public static List<SpawnScript> Read(Stream stream) =>
            ReadAll(stream).ToList();

        public static void Write(Stream stream, IEnumerable<SpawnScript> items)
        {
            foreach (var script in items)
            {
                var scriptLength = (script.Functions.Sum(x => x.Parameters.Count) +
                    script.Functions.Count) * 4 + 4;
                stream.Write(script.ProgramId);
                stream.Write((short)(scriptLength));
                foreach (var function in script.Functions)
                {
                    stream.Write((ushort)function.Opcode);
                    stream.Write((ushort)function.Parameters.Count);
                    foreach (var parameter in function.Parameters)
                        stream.Write(parameter);
                }
            }
            stream.Write(0x0000FFFF);
        }

        private static IEnumerable<SpawnScript> ReadAll(Stream stream)
        {
            while (true)
            {
                var header = BinaryMapping.ReadObject<Header>(stream);
                if (header.Id == -1 && header.Length == 0)
                    yield break;

                var bytecode = stream.ReadBytes(header.Length - 4);
                yield return new SpawnScript()
                {
                    ProgramId = header.Id,
                    Functions= ParseScript(bytecode)
                };
            }
        }

        private static List<Function> ParseScript(byte[] data)
        {
            var functions = new List<Function>();
            for (int pc = 0; pc < data.Length; )
            {
                var opcode = (ushort)(data[pc++] | (data[pc++] << 8));
                var parameterCount = (ushort)(data[pc++] | (data[pc++] << 8));
                var parameters = new List<int>(parameterCount);
                for (var i = 0; i < parameterCount; i++)
                {
                    var parameter = data[pc++] |
                        (data[pc++] << 8) |
                        (data[pc++] << 16) |
                        (data[pc++] << 24);
                    parameters.Add(parameter);
                }

                functions.Add(new Function
                {
                    Opcode = (Operation)opcode,
                    Parameters = parameters
                });
            }

            return functions;
        }
    }
}
