using System.IO;
using Xe.BinaryMapper;

namespace OpenKh.Kh2.Battle
{
    public class Przt
    {
        [Data] public byte Index { get; set; }
        [Data] public byte Unknown01 { get; set; }
        [Data] public byte Unknown02 { get; set; }
        [Data] public byte Unknown03 { get; set; }
        [Data] public byte Unknown04 { get; set; }
        [Data] public byte Unknown05 { get; set; }
        [Data] public byte Unknown06 { get; set; }
        [Data] public byte Unknown07 { get; set; }
        [Data] public byte Unknown08 { get; set; }
        [Data] public byte Unknown09 { get; set; }
        [Data] public byte Unknown0a { get; set; }
        [Data] public byte Unknown0b { get; set; }
        [Data] public short DropItem1 { get; set; }
        [Data] public short DropItem1Percentage { get; set; }
        [Data] public short DropItem2 { get; set; }
        [Data] public short DropItem2Percentage { get; set; }
        [Data] public short DropItem3 { get; set; }
        [Data] public short DropItem3Percentage { get; set; }

        public static BaseBattle<Przt> Read(Stream stream) => BaseBattle<Przt>.Read(stream);
    }
}
