using System.Collections.Generic;
using System.IO;
using Xe.BinaryMapper;

namespace OpenKh.Kh2.Battle
{
    public class Btlv
    {
        [Data] public int Id { get; set; }
        [Data] public int ProgressFlag { get; set; }
        [Data] public byte WorldZZ { get; set; }
        [Data] public byte WorldOfDarkness { get; set; }
        [Data] public byte TwilightTown { get; set; }
        [Data] public byte DestinyIslands { get; set; }
        [Data] public byte HollowBastion { get; set; }
        [Data] public byte BeastCastle { get; set; }
        [Data] public byte OlympusColiseum { get; set; }
        [Data] public byte Agrabah { get; set; }
        [Data] public byte LandOfDragons { get; set; }
        [Data] public byte HundredAcreWoods { get; set; }
        [Data] public byte PrideLands { get; set; }
        [Data] public byte Atlantica { get; set; }
        [Data] public byte DisneyCastle { get; set; }
        [Data] public byte TimelessRiver { get; set; }
        [Data] public byte HalloweenTown { get; set; }
        [Data] public byte WorldMap { get; set; }
        [Data] public byte PortRoyal { get; set; }
        [Data] public byte SpaceParanoids { get; set; }
        [Data] public byte TheWorldThatNeverWas { get; set; }
        [Data(Count = 5)] public byte[] Padding { get; set; }

        public static List<Btlv> Read(Stream stream) => BaseTable<Btlv>.Read(stream);

        public static void Write(Stream stream, IEnumerable<Btlv> items) =>
            BaseTable<Btlv>.Write(stream, 1, items);
    }
}
