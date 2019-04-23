using kh.common;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xe.BinaryMapper;
using Xe.IO;

namespace kh.kh2
{
    public class Layout
    {
        private const uint MagicCodeValidator = 0x4459414CU;
        private const int SupportedVersion = 0x100;
        private static readonly long MinimumLength = 32L;

        private class Header
        {
            [Data] public uint MagicCode { get; set; }
            [Data] public int Version { get; set; }
            [Data] public int L1Count { get; set; }
            [Data] public int L1Offset { get; set; }
            [Data] public int L2Count { get; set; }
            [Data] public int L2Offset { get; set; }
            [Data] public int SequenceCount { get; set; }
            [Data] public int SequenceOffset { get; set; }
        }

        public class L1
        {
            [Data] public int TextureIndex { get; set; }
            [Data] public int SequenceIndex { get; set; }
            [Data] public int AnimationGroup { get; set; }
            [Data] public int ShowAtFrame { get; set; }
            [Data] public int PositionX { get; set; }
            [Data] public int PositionY { get; set; }
        }

        public class L2
        {
            [Data] public short L1Index { get; set; }
            [Data] public short L1Count { get; set; }
            [Data] public int Unknown04 { get; set; }
            [Data] public int Unknown08 { get; set; }
            [Data] public int Unknown0c { get; set; }
            [Data] public int Unknown10 { get; set; }
        }

        public List<L1> L1Items { get; set; }
        public List<L2> L2Items { get; set; }
        public List<Sequence> SequenceItems { get; set; }

        internal Layout(Stream stream)
        {
            if (!stream.CanRead || !stream.CanSeek)
                throw new InvalidDataException($"Read or seek must be supported.");

            if (stream.Length < MinimumLength)
                throw new InvalidDataException("Invalid header length");

            var header = BinaryMapping.ReadObject<Header>(stream);
            if (header.MagicCode != MagicCodeValidator)
                throw new InvalidDataException("Invalid header");
            if (header.Version != SupportedVersion)
                throw new InvalidDataException($"Unsupported version {header.Version}");

            L1Items = stream.ReadList<L1>(header.L1Offset, header.L1Count);
            L2Items = stream.ReadList<L2>(header.L2Offset, header.L2Count);
            SequenceItems = new List<Sequence>();

            var sequenceOffsets = stream.ReadInt32List(header.SequenceOffset, header.SequenceCount);
            sequenceOffsets.Add((int)stream.Length);
            for (var i = 0; i < sequenceOffsets.Count - 1; i++)
            {
                // TODO Assuming that the sequence files are stored in order...
                int offset = sequenceOffsets[i];
                var length = sequenceOffsets[i + 1] - offset;
                var sequenceStream = new SubStream(stream, offset, length);
                SequenceItems.Add(Sequence.Read(sequenceStream));
            }
        }

        public static bool IsValid(Stream stream) =>
            stream.Length >= MinimumLength && new BinaryReader(stream).PeekInt32() == MagicCodeValidator;

        public static Layout Read(Stream stream) => new Layout(stream);

    }
}
