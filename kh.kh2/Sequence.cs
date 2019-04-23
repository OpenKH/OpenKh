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
            [Data] public int Unknown04 { get; set; }
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

        public int Unknown04 { get; set; }
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

            Unknown04 = header.Unknown04;
            Q1Items = stream.ReadList<Q1>(header.Q1.Offset, header.Q1.Count);
            Q2Items = stream.ReadList<Q2>(header.Q2.Offset, header.Q2.Count);
            Q3Items = stream.ReadList<Q3>(header.Q3.Offset, header.Q3.Count);
            Q4Items = stream.ReadList<Q4>(header.Q4.Offset, header.Q4.Count);
            Q5Items = stream.ReadList<Q5>(header.Q5.Offset, header.Q5.Count);
        }

        public void Write(Stream stream)
        {
            if (!stream.CanWrite || !stream.CanSeek)
                throw new InvalidDataException($"Write and seek must be supported.");

            var header = new Header
            {
                MagicCode = MagicCodeValidator,
                Unknown04 = Unknown04,
                Q1 = new Section() { Count = Q1Items.Count },
                Q2 = new Section() { Count = Q2Items.Count },
                Q3 = new Section() { Count = Q3Items.Count },
                Q4 = new Section() { Count = Q4Items.Count },
                Q5 = new Section() { Count = Q5Items.Count },
            };

            var basePosition = stream.Position;
            stream.Position = basePosition + MinimumLength;
            header.Q1.Offset = (int)(stream.Position - basePosition);
            header.Q2.Offset = stream.WriteList(Q1Items) + header.Q1.Offset;
            header.Q3.Offset = stream.WriteList(Q2Items) + header.Q2.Offset;
            header.Q4.Offset = stream.WriteList(Q3Items) + header.Q3.Offset;
            header.Q5.Offset = stream.WriteList(Q4Items) + header.Q4.Offset;
            stream.WriteList(Q5Items);

            var endPosition = stream.Position;
            stream.Position = basePosition;
            BinaryMapping.WriteObject(stream, header);
            stream.Position = endPosition;
        }

        public static Sequence Read(Stream stream) =>
            new Sequence(stream);

        public static bool IsValid(Stream stream) =>
            stream.Length >= MinimumLength && new BinaryReader(stream).PeekInt32() == MagicCodeValidator;
    }
}
