using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xe.BinaryMapper;

namespace OpenKh.Kh2.Jiminy
{
    public class Albu
    {
        public const int MagicCode = 0x4C414D4A;

        [Data] public byte World { get; set; } //Worl -> Id
        [Data(Count = 2)] public string Number { get; set; } //menu/<region>/jm_photo/<world_id><number>.bin
        [Data] public byte Padding { get; set; }
        [Data] public ushort Unk04 { get; set; } // Padding?, always 00
        [Data] public ushort StoryFlag { get; set; }
        [Data] public ushort Title { get; set; }
        [Data] public ushort Text { get; set; }

        public List<Albu> Read(Stream stream) => BaseJiminy<Albu>.Read(stream).Items;
        public void Write(Stream stream, int version, IEnumerable<Albu> items) => BaseJiminy<Albu>.Write(stream, MagicCode, version, items.ToList());
    }
}
