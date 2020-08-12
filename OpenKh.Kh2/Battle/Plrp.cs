using System.Collections.Generic;
using System.IO;
using Xe.BinaryMapper;

namespace OpenKh.Kh2.Battle
{
    public class Plrp
    {
        [Data] public short Difficulty { get; set; } //???
        [Data] public byte Character { get; set; } //???
        [Data] public byte Hp { get; set; }
        [Data] public byte Mp { get; set; }
        [Data] public byte Ap { get; set; }
        [Data] public short Unknown06 { get; set; } //Padding?
        [Data] public short Unknown08 { get; set; }
        [Data] public short Unknown0a { get; set; }
        [Data(Count = 58)] public List<short> Objects { get; set; }

        public static BaseTable<Plrp> Read(Stream stream) => BaseTable<Plrp>.Read(stream);
    }
}
