using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xe.BinaryMapper;

namespace OpenKh.Kh2.Jiminy
{
    public class Mini
    {
        public const int MagicCode = 0x474D4D4A;

        [Data] public ushort World { get; set; }
        [Data] public ushort Title { get; set; }
        [Data] public ushort HighscoreText { get; set; }
        [Data] public ushort Unk06 { get; set; }

        public List<Mini> Read(Stream stream) => BaseJiminy<Mini>.Read(stream).Items;
        public void Write(Stream stream, int version, IEnumerable<Mini> items) => BaseJiminy<Mini>.Write(stream, MagicCode, version, items.ToList());
    }
}
