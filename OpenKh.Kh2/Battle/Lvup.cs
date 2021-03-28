using System.Collections.Generic;
using System.IO;
using Xe.BinaryMapper;

namespace OpenKh.Kh2.Battle
{
    public class Lvup
    {
        [Data] public int MagicCode { get; set; }
        [Data] public int Count { get; set; }
        [Data(Count = 0x38)] public byte[] Unknown08 { get; set; }
        [Data(Count = 13)] public List<PlayableCharacter> Characters { get; set; }

        public class PlayableCharacter
        {
            [Data] public int NumLevels { get; set; }
            [Data(Count = 99)] public List<Level> Levels { get; set; }

            public class Level
            {
                [Data] public int Exp { get; set; }
                [Data] public byte Strength { get; set; }
                [Data] public byte Magic { get; set; }
                [Data] public byte Defense { get; set; }
                [Data] public byte Ap { get; set; }
                [Data] public short SwordAbility { get; set; }
                [Data] public short ShieldAbility { get; set; }
                [Data] public short StaffAbility { get; set; }
                [Data] public short Padding { get; set; }
            }
        }

        public enum PlayableCharacterType
        {
            Sora,
            Donald,
            Goofy,
            Mickey,
            Auron,
            PingMulan,
            Aladdin,
            Sparrow,
            Beast,
            Jack,
            Simba,
            Tron,
            Riku
        }

        public static Lvup Read(Stream stream) => BinaryMapping.ReadObject<Lvup>(stream);

        public void Write(Stream stream) => BinaryMapping.WriteObject(stream, this);
    }
}
