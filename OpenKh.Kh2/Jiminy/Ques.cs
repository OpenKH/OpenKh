using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xe.BinaryMapper;

namespace OpenKh.Kh2.Jiminy
{
    public class Ques
    {
        public const int MagicCode = 0x55514D4A;

        [Data] public ushort World { get; set; }
        [Data] public ushort CategoryText { get; set; }
        [Data] public ushort Title { get; set; }
        [Data] public ushort Unk06 { get; set; } //z_un_002a99c8
        [Data] public ushort StoryFlag { get; set; }
        [Data] public ushort Unk0A { get; set; }
        [Data] public ushort Unk0C { get; set; }
        [Data] public ushort Unk0E { get; set; }

        public List<Ques> Read(Stream stream) => BaseJiminy<Ques>.Read(stream).Items;
        public void Write(Stream stream, int version, IEnumerable<Ques> items) => BaseJiminy<Ques>.Write(stream, MagicCode, version, items.ToList());
    }
}
