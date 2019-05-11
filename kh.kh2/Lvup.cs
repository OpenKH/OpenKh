using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;

namespace kh.kh2
{
    public static class Lvup
    {
        private static byte[] _header;

        public static List<PlayableCharacter> Open(Stream stream)
        {
            if (!stream.CanRead || !stream.CanSeek)
                throw new InvalidDataException($"Read or seek must be supported.");

            var reader = new BinaryReader(stream);
            _header = reader.ReadBytes(0x40);

            return Enumerable.Range(0, 13)
                .Select(x =>
                {
                    reader.BaseStream.Seek(4, SeekOrigin.Current);
                    return new PlayableCharacter(stream, x);
                })
                .ToList();
        }

        public static void Save(Stream stream, IEnumerable<PlayableCharacter> characters)
        {
            if (!stream.CanRead || !stream.CanSeek)
                throw new InvalidDataException($"Read or seek must be supported.");

            if (CanSave())
            {
                var writer = new BinaryWriter(stream);
                writer.Write(_header);

                foreach (var character in characters)
                {
                    writer.Write(0);
                    foreach (var lvl in character.Levels)
                    {
                        writer.Write(lvl.EXP);
                        writer.Write(lvl.Strength);
                        writer.Write(lvl.Magic);
                        writer.Write(lvl.Defense);
                        writer.Write(lvl.AP);

                        writer.Write(lvl.SwordAbility);
                        writer.Write(lvl.ShieldAbility);
                        writer.Write(lvl.StaffAbility);
                        writer.Write((short)0);
                    }
                }
            }
        }

        public static bool CanSave() => _header?.Length > 0;

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
            Biest,
            Jack,
            Simba,
            Tron,
            Riku
        }

        public class PlayableCharacter
        {
            public Lvup.PlayableCharacterType Type { get; }
            public ObservableCollection<LevelUpEntry> Levels { get; set; }
            public string Name
            {
                get => Type.ToString();
            }

            public PlayableCharacter(Stream stream, int index)
            {
                if (!stream.CanRead || !stream.CanSeek)
                    throw new InvalidDataException($"Read or seek must be supported.");

                Type = (Lvup.PlayableCharacterType)index;
                Levels = new ObservableCollection<LevelUpEntry>();

                var reader = new BinaryReader(stream);
                for (int j = 0; j < 99; j++)
                {
                    Levels.Add(new LevelUpEntry
                    {
                        Level = j + 1,
                        EXP = reader.ReadInt32(),
                        Strength = reader.ReadByte(),
                        Magic = reader.ReadByte(),
                        Defense = reader.ReadByte(),
                        AP = reader.ReadByte(),

                        SwordAbility = reader.ReadInt16(),
                        ShieldAbility = reader.ReadInt16(),
                        StaffAbility = reader.ReadInt16()
                    });
                    reader.BaseStream.Seek(2, SeekOrigin.Current);
                }
            }
        }

        public class LevelUpEntry
        {
            public int Level { get; set; }
            public int EXP { get; set; }

            public byte Strength { get; set; }
            public byte Magic { get; set; }
            public byte Defense { get; set; }
            public byte AP { get; set; }

            public short SwordAbility { get; set; }
            public short ShieldAbility { get; set; }
            public short StaffAbility { get; set; }

            public string Name
            {
                get => $"Level {Level}";
            }
        }
    }
}