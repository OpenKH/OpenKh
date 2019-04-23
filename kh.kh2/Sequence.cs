using kh.common;
using System.Collections.Generic;
using System.IO;
using Xe.BinaryMapper;

namespace kh.kh2
{
    public class Sequence
    {
        private static readonly uint MagicCodeValidator = 0x44514553U;
        private static readonly long MinimumLength = 48L;

        private class Section
        {
            [Data] public int Count { get; set; }
            [Data] public int Offset { get; set; }
        }

        private class Header
        {
            [Data] public uint MagicCode { get; set; }
            [Data] public int Unknown { get; set; }
            [Data] public Section Q1 { get; set; }
            [Data] public Section Q2 { get; set; }
            [Data] public Section Q3 { get; set; }
            [Data] public Section Q4 { get; set; }
            [Data] public Section Q5 { get; set; }
        }

        public class Q1
        {
            [Data] public int Unknown00 { get; set; }
            [Data] public int Left { get; set; }
            [Data] public int Top { get; set; }
            [Data] public int Right { get; set; }
            [Data] public int Bottom { get; set; }
            [Data] public float Unknown10 { get; set; }
            [Data] public float Unknown14 { get; set; }
            [Data] public int ColorLeft { get; set; }
            [Data] public int ColorTop { get; set; }
            [Data] public int ColorRight { get; set; }
            [Data] public int ColorBottom { get; set; }
        }

        public class Q2
        {
            [Data] public int Left { get; set; }
            [Data] public int Top { get; set; }
            [Data] public int Right { get; set; }
            [Data] public int Bottom { get; set; }
            [Data] public int Q1Index { get; set; }
        }

        public class Q3
        {
            [Data] public short Start { get; set; }
            [Data] public short Count { get; set; }
        }

        public class Q4
        {
            [Data] public int Flags { get; set; }
            [Data] public int Q3Index { get; set; }
            [Data] public int FrameStart { get; set; }
            [Data] public int FrameEnd { get; set; }
            [Data] public int Xa0 { get; set; }
            [Data] public int Xa1 { get; set; }
            [Data] public int Ya0 { get; set; }
            [Data] public int Ya1 { get; set; }
            [Data] public int Xb0 { get; set; }
            [Data] public int Xb1 { get; set; }
            [Data] public int Yb0 { get; set; }
            [Data] public int Yb1 { get; set; }
            [Data] public int Unknown30 { get; set; }
            [Data] public int Unknown34 { get; set; }
            [Data] public int Unknown38 { get; set; }
            [Data] public int Unknown3c { get; set; }
            [Data] public float RotationStart { get; set; }
            [Data] public float RotationEnd { get; set; }
            [Data] public float ScaleStart { get; set; }
            [Data] public float ScaleEnd { get; set; }
            [Data] public float ScaleXStart { get; set; }
            [Data] public float ScaleXEnd { get; set; }
            [Data] public float ScaleYStart { get; set; }
            [Data] public float ScaleYEnd { get; set; }
            [Data] public int Unknown60 { get; set; }
            [Data] public int Unknown64 { get; set; }
            [Data] public int Unknown68 { get; set; }
            [Data] public int Unknown6c { get; set; }
            [Data] public int BounceXStart { get; set; }
            [Data] public int BounceXEnd { get; set; }
            [Data] public int BounceYStart { get; set; }
            [Data] public int BounceYEnd { get; set; }
            [Data] public int Unknwon80 { get; set; }
            [Data] public int ColorBlend { get; set; }
            [Data] public int ColorStart { get; set; }
            [Data] public int ColorEnd { get; set; }
        }

        public class Q5
        {
            [Data] public short Q4Index { get; set; }
            [Data] public short Count { get; set; }
            [Data] public short Unknown04 { get; set; }
            [Data] public short Unknown06 { get; set; }
            [Data] public int Tick1 { get; set; }
            [Data] public int Tick2 { get; set; }
            [Data] public int Unknown10 { get; set; }
            [Data] public int Unknown14 { get; set; }
            [Data] public int Unknown18 { get; set; }
            [Data] public int Unknown1C { get; set; }
            [Data] public int Unknown20 { get; set; }
        }

        public List<Q1> Q1Items { get; set; }
        public List<Q2> Q2Items { get; set; }
        public List<Q3> Q3Items { get; set; }
        public List<Q4> Q4Items { get; set; }
        public List<Q5> Q5Items { get; set; }

        private Sequence(Stream stream)
        {
            if (!stream.CanRead || !stream.CanSeek)
                throw new InvalidDataException($"Read or seek must be supported.");

            if (stream.Length < MinimumLength)
                throw new InvalidDataException("Invalid header length");

            var header = BinaryMapping.ReadObject<Header>(stream);
            if (header.MagicCode != MagicCodeValidator)
                throw new InvalidDataException("Invalid header");

            Q1Items = stream.ReadList<Q1>(header.Q1.Offset, header.Q1.Count);
            Q2Items = stream.ReadList<Q2>(header.Q2.Offset, header.Q2.Count);
            Q3Items = stream.ReadList<Q3>(header.Q3.Offset, header.Q3.Count);
            Q4Items = stream.ReadList<Q4>(header.Q4.Offset, header.Q4.Count);
            Q5Items = stream.ReadList<Q5>(header.Q5.Offset, header.Q5.Count);
        }

        public static Sequence Read(Stream stream) =>
            new Sequence(stream);

        public static bool IsValid(Stream stream) =>
            stream.Length >= MinimumLength && new BinaryReader(stream).PeekInt32() == MagicCodeValidator;
    }
}
