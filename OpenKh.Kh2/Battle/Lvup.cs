using System;
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
        public List<PlayableCharacter> Characters { get; set; }

        public class PlayableCharacter
        {
            [Data] public int NumLevels { get; set; }
            public List<Level> Levels { get; set; }

            public class Level
            {
                [Data] public int Exp { get; set; }
                [Data] public byte Strength { get; set; }
                [Data] public byte Magic { get; set; }
                [Data] public byte Defense { get; set; }
                [Data] public byte Ap { get; set; }
                [Data] public ushort SwordAbility { get; set; }
                [Data] public ushort ShieldAbility { get; set; }
                [Data] public ushort StaffAbility { get; set; }
                [Data] public ushort Padding { get; set; }
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


        private static int ReadInt32(Stream stream)
        {
            var buffer = new byte[4];
            int read = stream.Read(buffer, 0, 4);

            if (read != 4)
                throw new EndOfStreamException();

            return BitConverter.ToInt32(buffer, 0);
        }

        private static void WriteInt32(Stream stream, int value)
        {
            var buffer = BitConverter.GetBytes(value);
            stream.Write(buffer, 0, 4);
        }

        private static byte[] ReadBytes(Stream stream, int count)
        {
            var buffer = new byte[count];
            int read = stream.Read(buffer, 0, count);

            if (read != count)
                throw new EndOfStreamException();

            return buffer;
        }

        private static PlayableCharacter ReadCharacter(Stream stream)
        {
            var ch = new PlayableCharacter();

            ch.NumLevels = ReadInt32(stream);
            ch.Levels = new List<PlayableCharacter.Level>();

            for (int i = 0; i < ch.NumLevels; i++)
                ch.Levels.Add(BinaryMapping.ReadObject<PlayableCharacter.Level>(stream));

            return ch;
        }

        public static Lvup Read(Stream stream)
        {
            var lvup = new Lvup();

            lvup.MagicCode = ReadInt32(stream);
            lvup.Count = ReadInt32(stream);
            lvup.Unknown08 = ReadBytes(stream, 0x38);

            lvup.Characters = new List<PlayableCharacter>();

            for (int i = 0; i < 13; i++)
                lvup.Characters.Add(ReadCharacter(stream));

            return lvup;
        }



        private static void WriteCharacter(Stream stream, PlayableCharacter ch)
        {
            ch.NumLevels = ch.Levels?.Count ?? 0;

            WriteInt32(stream, ch.NumLevels);

            foreach (var level in ch.Levels)
                BinaryMapping.WriteObject(stream, level);
        }


        public void Write(Stream stream)
        {
            long fileStart = stream.Position;

            WriteInt32(stream, MagicCode);
            WriteInt32(stream, Count);

            long offsetTablePos = stream.Position;

            stream.Write(new byte[0x38], 0, 0x38);

            long characterBase = stream.Position;

            var offsets = new List<int>();

            offsets.Add(0); //Index 0 is a "fake" entry. It doesn't have any pointed data, seems to moreso exist as a type of "Padding" to ensure that an objects Part value corresponds with their LVUP table index.

            foreach (var ch in Characters)
            {
                long charStart = stream.Position;

                offsets.Add((int)((charStart - fileStart) / 4));

                WriteCharacter(stream, ch);
            }

            long endPos = stream.Position;

            
            stream.Position = offsetTablePos;

            foreach (var offset in offsets)
                WriteInt32(stream, offset);

            stream.Position = endPos;
        }
    }
}
