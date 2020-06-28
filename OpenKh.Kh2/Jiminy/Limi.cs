using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xe.BinaryMapper;

namespace OpenKh.Kh2.Jiminy
{
    public class Limi
    {
        public const int MagicCode = 0x494C4D4A;

        [Data] public ushort Unk00 { get; set; }
        [Data] public ushort Title { get; set; }
        [Data] public ushort Description { get; set; }
        [Data] public ushort Padding { get; set; }

        public List<Limi> Read(Stream stream) => BaseJiminy<Limi>.Read(stream).Items;
        public void Write(Stream stream, int version, IEnumerable<Limi> items) => BaseJiminy<Limi>.Write(stream, MagicCode, version, items.ToList());
    }
}
