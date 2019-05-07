using System.Collections.ObjectModel;
using System.IO;

namespace kh.kh2
{
    public class Lvup
    {
        private byte[] Header { get; }

        public ObservableCollection<PlayableCharacter> Characters { get; set; }

        public Lvup(Stream stream)
        {
            if (!stream.CanRead || !stream.CanSeek)
                throw new InvalidDataException($"Read or seek must be supported.");

            var reader = new BinaryReader(stream);
            Header = reader.ReadBytes(0x40);
            for (int i = 0; i < 13; i++)
            {
                reader.BaseStream.Seek(4, SeekOrigin.Current);
                Characters.Add(new PlayableCharacter(stream, i));
            }
        }

        public void Save(Stream stream)
        {
            if (!stream.CanRead || !stream.CanSeek)
                throw new InvalidDataException($"Read or seek must be supported.");

            var writer = new BinaryWriter(stream);
            writer.Write(Header);

            foreach(var character in Characters)
            {
                writer.Write((int)0);
                foreach(var lvl in character.Levels)
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

    public class PlayableCharacter
    {
        public PlayableCharacterType Type { get; }
        public ObservableCollection<LevelUpEntry> Levels { get; set; }
        public string Name
        {
            get => Type.ToString();
        }

        public PlayableCharacter(Stream stream, int index)
        {
            if (!stream.CanRead || !stream.CanSeek)
                throw new InvalidDataException($"Read or seek must be supported.");

            Type = (PlayableCharacterType)index;
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
        public short Padding { get; }

        public string Name
        {
            get => $"Level {Level}";
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
        Biest,
        Jack,
        Simba,
        Tron,
        Riku
    }
}