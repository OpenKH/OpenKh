using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xe.BinaryMapper;

namespace OpenKh.Kh2.Battle
{
    /// <summary>
    /// Unfinished
    /// </summary>
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

        public static List<Plrp> Read(Stream stream) => BaseTable<Plrp>.Read(stream).Items;

        public static void Write(Stream stream, IEnumerable<Plrp> items) =>
            new BaseTable<Plrp>
            {
                Id = 2,
                Items = items.ToList()
            }.Write(stream);
    }
}
