using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xe.BinaryMapper;

namespace OpenKh.Kh2.Jiminy
{
    public class Anse
    {
        public const int MagicCode = 0x4E414D4A;

        [Data] public ushort Id { get; set; } //03system -> item
        [Data] public ushort Title { get; set; }
        [Data] public ushort Text { get; set; }
        [Data] public ushort Padding { get; set; }

        public List<Anse> Read(Stream stream) => BaseJiminy<Anse>.Read(stream).Items;
        public void Write(Stream stream, int version, IEnumerable<Anse> items) => BaseJiminy<Anse>.Write(stream, MagicCode, version, items.ToList());
    }
}
