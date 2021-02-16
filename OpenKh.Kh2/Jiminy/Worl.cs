using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xe.BinaryMapper;

namespace OpenKh.Kh2.Jiminy
{
    public class Worl
    {
        public const int MagicCode = 0x4F574D4A;

        [Data] public byte Id { get; set; } //also responsible for world icon in journal, menu/<region>/jm_world.2ld
        [Data(Count = 2)] public string Name { get; set; }
        [Data] public byte Padding { get; set; }
        [Data] public ushort TextTitle { get; set; }
        [Data] public ushort TextSubmenu { get; set; }
        [Data] public ushort StoryFlag { get; set; } //this same flag is used in multiple subfiles, probably to determine a world name switch??
        [Data] public ushort TextTitle2 { get; set; } //these 3 fields are only used by hollow bastion, to switch to radiant garden later
        [Data] public ushort TextSubmenu2 { get; set; }
        [Data] public ushort Unk0E { get; set; }

        public List<Worl> Read(Stream stream) => BaseJiminy<Worl>.Read(stream).Items;
        public void Write(Stream stream, int version, IEnumerable<Worl> items) => BaseJiminy<Worl>.Write(stream, MagicCode, version, items.ToList());
    }
}
