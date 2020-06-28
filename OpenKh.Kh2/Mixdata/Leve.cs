using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xe.BinaryMapper;

namespace OpenKh.Kh2.Mixdata
{
    public class Leve
    {
        private const int MagicCode = 0x564C494D;

        [Data] public ushort Id { get; set; }
        [Data] public ushort Unk2 { get; set; }
        [Data] public ushort Unk4 { get; set; }
        [Data] public ushort Unk6 { get; set; }
        [Data] public int Exp { get; set; }

        public List<Leve> Read(Stream stream) => BaseMixdata<Leve>.Read(stream).Items;
        public void Write(Stream stream, int version, IEnumerable<Leve> items) => BaseMixdata<Leve>.Write(stream, MagicCode, version, items.ToList());
    }
}
