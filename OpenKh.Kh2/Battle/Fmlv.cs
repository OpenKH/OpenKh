using System.IO;
using Xe.BinaryMapper;

namespace OpenKh.Kh2.Battle
{
    public class Fmlv
    {
        [Data] public byte FormIdFormLevel { get; set; }
        [Data] public byte LevelOfMovementAbility { get; set; }
        [Data] public short AbilityId { get; set; }
        [Data] public int Exp { get; set; }

        public int FormId
        {
            get => FormIdFormLevel >> 4;
            set => FormIdFormLevel = (byte)((FormIdFormLevel & 0x0F) | (value << 4));
        }

        public int FormLevel
        {
            get => FormIdFormLevel & 0xF;
            set => FormIdFormLevel = (byte)((FormIdFormLevel & 0xF0) | (value & 0xF));
        }

        public static BaseBattle<Fmlv> Read(Stream stream) => BaseBattle<Fmlv>.Read(stream);
    }
}
