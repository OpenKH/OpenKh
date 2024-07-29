using System.Collections.Generic;
using System.IO;
using Xe.BinaryMapper;

namespace OpenKh.Kh2
{
    public class Jigsaw
    {
        public enum PictureName : byte
        {
            Awakening = 0,
            Heart = 1,
            Duality = 2,
            Frontier = 3,
            Daylight = 4,
            Sunset = 5
        }

        public enum WorldList : byte
        {
            ZZ = 0,
            EndofSea = 1,
            TwilightTown = 2,
            DestinyIsland = 3,
            HollowBastion = 4,
            BeastsCastle = 5,
            OlympusColiseum = 6,
            Agrabah = 7,
            TheLandOfDragons = 8,
            HundredAcreWood = 9,
            PrideLand = 10,
            Atlantica = 11,
            DisneyCastle = 12,
            TimelessRiver = 13,
            HalloweenTown = 14,
            WorldMap = 15,
            PortRoyal = 16,
            SpaceParanoids = 17,
            TheWorldThatNeverWas = 18,
        }

        [Data] public PictureName Picture { get; set; }
        [Data] public byte Part { get; set; }
        [Data] public ushort Text { get; set; } //z_un_002a2de8, binary addition 0x8000
        [Data] public WorldList World { get; set; }
        [Data] public byte Room { get; set; }
        [Data] public byte JigsawIdWorld { get; set; }
        [Data] public byte Unk07 { get; set; } //has also something to do with pos and orientation
        [Data] public ushort Unk08 { get; set; } //z_un_001d9d88, starting pos and orientation

        public static List<Jigsaw> Read(Stream stream) => BaseTable<Jigsaw>.Read(stream);
        public static void Write(Stream stream, IEnumerable<Jigsaw> items) =>
            BaseTable<Jigsaw>.Write(stream, 2, items);
    }
}
