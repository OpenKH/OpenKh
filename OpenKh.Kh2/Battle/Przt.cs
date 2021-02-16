using System.Collections.Generic;
using System.IO;
using Xe.BinaryMapper;

namespace OpenKh.Kh2.Battle
{
    public class Przt
    {
        [Data] public ushort Id { get; set; }
        [Data] public byte SmallHpOrbs { get; set; }
        [Data] public byte BigHpOrbs { get; set; }
        [Data] public byte BigMoneyOrbs { get; set; }
        [Data] public byte MediumMoneyOrbs { get; set; }
        [Data] public byte SmallMoneyOrbs { get; set; }
        [Data] public byte SmallMpOrbs { get; set; }
        [Data] public byte BigMpOrbs { get; set; }
        [Data] public byte SmallDriveOrbs { get; set; }
        [Data] public byte BigDriveOrbs { get; set; }
        [Data] public byte Unknown0a { get; set; } // Padding?
        [Data] public ushort Item1 { get; set; }
        [Data] public short Item1Percentage { get; set; }
        [Data] public ushort Item2 { get; set; }
        [Data] public short Item2Percentage { get; set; }
        [Data] public ushort Item3 { get; set; }
        [Data] public short Item3Percentage { get; set; }

        public static List<Przt> Read(Stream stream) => BaseTable<Przt>.Read(stream);

        public static void Write(Stream stream, IEnumerable<Przt> items) =>
            BaseTable<Przt>.Write(stream, 2, items);
    }
}
