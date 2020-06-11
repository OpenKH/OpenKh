using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xe.BinaryMapper;

namespace OpenKh.Kh2.Jiminy
{
    public class Puzz
    {
        public const int MagicCode = 0x5A504D4A;

        public enum Rotation : byte
        {
            Fixed = 0x0,
            Rotatable = 0x1,
        }

        public enum PuzzleSize : byte
        {
            Twelve = 0x0,
            FourtyEight = 0x1,
        }

        [Data] public byte Id { get; set; }
        [Data] public byte Unk1 { get; set; }
        [Data] public ushort Name { get; set; }
        [Data] public ushort RewardItem { get; set; }
        [Data(Count = 0xA)] public string FileName { get; set; }

        public int PieceRotation
        {
            get => Unk1 >> 4;
            set => Unk1 = (byte)((Unk1 & 0x0F) | (value << 4));
        }

        public int Size
        {
            get => Unk1 & 0xF;
            set => Unk1 = (byte)((Unk1 & 0xF0) | (value & 0xF));
        }

        public static List<Puzz> Read(Stream stream) => BaseJiminy<Puzz>.Read(stream).Items;
        public static void Write(Stream stream, int version, IEnumerable<Puzz> items) => BaseJiminy<Puzz>.Write(stream, MagicCode, version, items.ToList());
    }
}
