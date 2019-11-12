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
        [Data] public ushort DropItem1 { get; set; }
        [Data] public short DropItem1Percentage { get; set; }
        [Data] public ushort DropItem2 { get; set; }
        [Data] public short DropItem2Percentage { get; set; }
        [Data] public ushort DropItem3 { get; set; }
        [Data] public short DropItem3Percentage { get; set; }

        public static BaseBattle<Przt> Read(Stream stream) => BaseBattle<Przt>.Read(stream);
    }
}
