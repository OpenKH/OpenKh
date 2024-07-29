using System;
using System.Collections.Generic;
using System.IO;
using Xe.BinaryMapper;

namespace OpenKh.Kh2.Battle
{
    public class Limt
    {


        [Flags]
        public enum Characters : byte
        {
            None = 0,
            Sora = 1,
            Donald = 2,
            Goofy = 3,
            Mickey = 4,
            Auron = 5,
            Mulan = 6,
            Aladdin = 7,
            JackSparrow = 8,
            Beast = 9,
            JackSkellington = 10,
            Simba = 11,
            Tron = 12,
            Riku = 13,
            Roxas = 14,
            Ping = 15,
            Stitch = 200,
            Genie = 201,
            PeterPan = 202,
            ChickenLittle = 204,
        }



        [Data] public byte Id { get; set; }
        [Data] public Characters Character { get; set; }
        [Data] public Characters Summon { get; set; }
        [Data] public byte Group { get; set; }
        [Data(Count = 32)] public string FileName { get; set; }
        [Data] public uint SpawnId { get; set; }
        [Data] public ushort Command { get; set; }
        [Data] public ushort Limit { get; set; }
        [Data] public ushort World { get; set; }
        [Data(Count = 18)] public byte[] Padding { get; set; }

        public override string ToString()
        {
            return FileName;
        }
        public static List<Limt> Read(Stream stream) => BaseTable<Limt>.Read(stream);

        public static void Write(Stream stream, IEnumerable<Limt> items) =>
            BaseTable<Limt>.Write(stream, 2, items);
    }
}
