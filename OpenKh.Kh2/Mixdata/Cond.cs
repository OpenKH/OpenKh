using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xe.BinaryMapper;

namespace OpenKh.Kh2.Mixdata
{
    public class Cond
    {
        private const int MagicCode = 0x4F43494D;

        [Data] public ushort Id { get; set; }
        [Data] public short Reward { get; set; } //either item from 03system or shop upgrades
        [Data] public byte Unk4 { get; set; }
        [Data] public byte Unk5 { get; set; }
        [Data] public byte Unk6 { get; set; }
        [Data] public byte Unk7 { get; set; }
        [Data] public short Count { get; set; }
        [Data] public short UnkA { get; set; }

        public List<Cond> Read(Stream stream) => BaseMixdata<Cond>.Read(stream).Items;
        public void Write(Stream stream, int version, IEnumerable<Cond> items) => BaseMixdata<Cond>.Write(stream, MagicCode, version, items.ToList());
    }
}
