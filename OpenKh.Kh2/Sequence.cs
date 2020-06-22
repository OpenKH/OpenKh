using OpenKh.Common;
using System.Collections.Generic;
using System.IO;
using Xe.BinaryMapper;

namespace OpenKh.Kh2
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
            [Data] public Section AnimationGroupsDesc { get; set; }
            [Data] public Section AnimationsDesc { get; set; }
            [Data] public Section FrameGroupsDesc { get; set; }
            [Data] public Section FramesExDesc { get; set; }
            [Data] public Section FramesDesc { get; set; }
        }

        public class Frame
        {
            [Data] public int Unknown00 { get; set; }
            [Data] public int Left { get; set; }
            [Data] public int Top { get; set; }
            [Data] public int Right { get; set; }
            [Data] public int Bottom { get; set; }
            [Data] public float UTranslation { get; set; }
            [Data] public float VTranslation { get; set; }
            [Data] public uint ColorLeft { get; set; }
            [Data] public uint ColorTop { get; set; }
            [Data] public uint ColorRight { get; set; }
            [Data] public uint ColorBottom { get; set; }
        }

        public class FrameEx
        {
            [Data] public int Left { get; set; }
            [Data] public int Top { get; set; }
            [Data] public int Right { get; set; }
            [Data] public int Bottom { get; set; }
            [Data] public int FrameIndex { get; set; }
        }

        public class FrameGroup
        {
            [Data] public short Start { get; set; }
            [Data] public short Count { get; set; }
        }

        public class Animation
        {
            [Data] public int Flags { get; set; }
            [Data] public int FrameGroupIndex { get; set; }
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
            [Data] public float Unknown60 { get; set; }
            [Data] public float Unknown64 { get; set; }
            [Data] public float Unknown68 { get; set; }
            [Data] public float Unknown6c { get; set; }
            [Data] public int BounceXStart { get; set; }
            [Data] public int BounceXEnd { get; set; }
            [Data] public int BounceYStart { get; set; }
            [Data] public int BounceYEnd { get; set; }
            [Data] public int Unknwon80 { get; set; }
            [Data] public int ColorBlend { get; set; }
            [Data] public uint ColorStart { get; set; }
            [Data] public uint ColorEnd { get; set; }
        }

        public class AnimationGroup
        {
            [Data] public short AnimationIndex { get; set; }
            [Data] public short Count { get; set; }
            [Data] public short DoNotLoop { get; set; }
            [Data] public short Unknown06 { get; set; }
            [Data] public int LoopStart { get; set; }
            [Data] public int LoopEnd { get; set; }
            [Data] public int Unknown10 { get; set; }
            [Data] public int Unknown14 { get; set; }
            [Data] public int Unknown18 { get; set; }
            [Data] public int Unknown1C { get; set; }
            [Data] public int Unknown20 { get; set; }
        }

        public int Unknown04 { get; set; }
        public List<Frame> Frames { get; set; }
        public List<FrameEx> FramesEx { get; set; }
        public List<FrameGroup> FrameGroups { get; set; }
        public List<Animation> Animations { get; set; }
        public List<AnimationGroup> AnimationGroups { get; set; }

        public Sequence()
        {
            Unknown04 = 0x100; // assuming that this value is constant
        }

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
            Frames = stream.ReadList<Frame>(header.AnimationGroupsDesc.Offset, header.AnimationGroupsDesc.Count);
            FramesEx = stream.ReadList<FrameEx>(header.AnimationsDesc.Offset, header.AnimationsDesc.Count);
            FrameGroups = stream.ReadList<FrameGroup>(header.FrameGroupsDesc.Offset, header.FrameGroupsDesc.Count);
            Animations = stream.ReadList<Animation>(header.FramesExDesc.Offset, header.FramesExDesc.Count);
            AnimationGroups = stream.ReadList<AnimationGroup>(header.FramesDesc.Offset, header.FramesDesc.Count);
        }

        public void Write(Stream stream)
        {
            if (!stream.CanWrite || !stream.CanSeek)
                throw new InvalidDataException($"Write and seek must be supported.");

            var header = new Header
            {
                MagicCode = MagicCodeValidator,
                Unknown04 = Unknown04,
                AnimationGroupsDesc = new Section() { Count = Frames.Count },
                AnimationsDesc = new Section() { Count = FramesEx.Count },
                FrameGroupsDesc = new Section() { Count = FrameGroups.Count },
                FramesExDesc = new Section() { Count = Animations.Count },
                FramesDesc = new Section() { Count = AnimationGroups.Count },
            };

            var basePosition = stream.Position;
            stream.Position = basePosition + MinimumLength;
            header.AnimationGroupsDesc.Offset = (int)(stream.Position - basePosition);
            header.AnimationsDesc.Offset = stream.WriteList(Frames) + header.AnimationGroupsDesc.Offset;
            header.FrameGroupsDesc.Offset = stream.WriteList(FramesEx) + header.AnimationsDesc.Offset;
            header.FramesExDesc.Offset = stream.WriteList(FrameGroups) + header.FrameGroupsDesc.Offset;
            header.FramesDesc.Offset = stream.WriteList(Animations) + header.FramesExDesc.Offset;
            stream.WriteList(AnimationGroups);

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
