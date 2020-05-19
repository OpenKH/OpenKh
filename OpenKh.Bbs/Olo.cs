using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xe.BinaryMapper;

namespace OpenKh.Bbs
{
    public class Olo
    {
        public Dictionary<string, string> SpawnObjectList = new Dictionary<string, string>()
        {
            {"p01ex00", "Terra (PC)"},
            {"p02ex00", "Ventus (PC)"},
            {"p03ex00", "Aqua (PC)"},
            {"p11ex00", "Terra Armor (PC)"},
            {"p12ex00", "Ventus Armor (PC)"},
            {"p13ex00", "Aqua Armor (PC)"},
        };

        private const int MagicCode = 0x4F4C4F40; // Always @OLO

        private struct UnkStruct
        {
            [Data] public int unk1 { get; set; }
            [Data] public int ExtraDataOffset { get; set; } // Indicates offset where other info such as pos/rot/scale are defined.
        }

        private readonly Header _header;

        private class Header
        {
            [Data] public int MagicCode { get; set; }
            [Data] public short unkStructNumber1 { get; set; } // Pretty much always 5.
            [Data] public short unkStructNumber2 { get; set; }
            [Data] public int NumObjectsToSpawn { get; set; }
            [Data] public int HeaderLength { get; set; }
            [Data] public List<UnkStruct> UnkData { get; set; } // Repeats by unkStructNumber1.
            [Data] public int padding { get; set; }

            static Header()
            {
                BinaryMapping.SetMemberLengthMapping<Header>(nameof(UnkData), (o, m) => o.unkStructNumber1);
            }
        }

        [Data] public List<string> ObjectsLoaded { get; set; }

        static Olo()
        {
            BinaryMapping.SetMemberLengthMapping<Olo>(nameof(ObjectsLoaded), (o, m) => o._header.NumObjectsToSpawn);
        }

        public static bool IsValid(Stream stream)
        {
            var prevPosition = stream.Position;
            var magicCode = new BinaryReader(stream).ReadInt32();
            stream.Position = prevPosition;

            return magicCode == MagicCode;
        }

        public static List<string> Read(Stream stream) =>
            BinaryMapping.ReadObject<Olo>(stream).ObjectsLoaded;
    }
}
