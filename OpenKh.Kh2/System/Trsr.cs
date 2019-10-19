using System.IO;
using Xe.BinaryMapper;

namespace OpenKh.Kh2.System
{
    public class Trsr
    {
        public enum TrsrType : byte
        {
            Chest,
            Event
        }
        public enum TrsrWorld : byte
        {
            Undefined,
            DarkRealm,
            TwilightTown,
            Unknown,
            HollowBastion,
            BeastCastle,
            TheUnderworld,
            Agrabah,
            LandofDragons,
            HundredAcreWood,
            PrideLands,
            Atlantica,
            DisneyCastle,
            TimelessRiver,
            HalloweenTown,
            WorldMap,
            PortRoyal,
            SpaceParanoids,
            FinalWorld
        }

        [Data] public short Identifier { get; set; }
        [Data] public short Item { get; set; }
        [Data] public TrsrType Type { get; set; }
        [Data] public TrsrWorld World { get; set; }
        [Data] public byte Room { get; set; }
        [Data] public byte RoomChestIndex { get; set; }
        [Data] public short EventId { get; set; }
        [Data] public short OverallChestIndex { get; set; }

        public static BaseSystem<Trsr> Read(Stream stream) => BaseSystem<Trsr>.Read(stream);
    }
}
