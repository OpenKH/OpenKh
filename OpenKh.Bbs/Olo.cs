using System.Collections.Generic;
using System.IO;
using Xe.BinaryMapper;

namespace OpenKh.Bbs
{
    public class Olo
    {
        public Dictionary<string, string> SpawnObjectList = new Dictionary<string, string>()
        {
            {"g01ex00", "Savepoint"},
            {"g02ex00", "Examine Actor (To place on static object)"},
            {"g03ex00", "Crown (Puzzle)"},

            {"p01ex00", "Ventus (PC)"},
            {"p02ex00", "Aqua (PC)"},
            {"p03ex00", "Terra (PC)"},
            {"p11ex00", "Ventus Armor (PC)"},
            {"p12ex00", "Aqua Armor (PC)"},
            {"p13ex00", "Terra Armor (PC)"},
            {"p41ex00", "Ventus Armor Helmetless (PC)"},
            {"p42ex00", "Aqua Armor Helmetless (PC)"},
            {"p43ex00", "Terra Armor Helmetless (PC)"},

            {"n10ex00", "Moogle Shop 1"},
            {"n11ex00", "Moogle Shop 2"},
            {"n12ex00", "Moogle Shop 3"},
            {"n13ex00", "Moogle Shop 4"},
        };

        private const int MagicCode = 0x4F4C4F40; // Always @OLO

        private struct UnkStruct
        {
            [Data] public int unk1 { get; set; } // 0x1 for bosses - 0x2 execute special code. SHOP-SAVE-TALK NECESSARY FOR OBJECTS TO APPEAR
            [Data] public int ExtraDataOffset { get; set; }
        }

        private struct OLOInfo
        {

        }

        // Y-Axis is UP.
        private struct SpawnStruct
        {
            [Data] public int UnknownVal1 { get; set; }
            [Data] public float PositionX { get; set; }
            [Data] public float PositionY { get; set; }
            [Data] public float PositionZ { get; set; }
            [Data] public float RotationX { get; set; }
            [Data] public float RotationY { get; set; }
            [Data] public float RotationZ { get; set; } // Yaw
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
        }

        [Data] public List<string> ObjectsLoaded { get; set; }

        private Olo()
        {

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
