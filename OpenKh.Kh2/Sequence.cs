﻿using OpenKh.Common;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xe.BinaryMapper;
using Xe.IO;

namespace OpenKh.Kh2
{
    public class Sequence
    {
        public static readonly uint MagicCodeValidator = 0x44514553U;
        private static readonly long MinimumLength = 48L;

        public const int LinearInterpolationFlag = 0x00000001;
        public const int AttachTextFlag = 0x00000002;
        public const int BouncingFlag = 0x00000010;
        public const int RotationFlag = 0x00000020;
        public const int ScalingFlag = 0x00000040;
        public const int ColorInterpolationFlag = 0x00000080;
        public const int ColorMaskingFlag = 0x00000400;
        public const int TranslationFlag = 0x00001000;
        public const int PivotFlag = 0x00004000;

        private class Section
        {
            [Data] public int Count { get; set; }
            [Data] public int Offset { get; set; }
        }

        private class Header
        {
            [Data] public uint MagicCode { get; set; }
            [Data] public int Unknown04 { get; set; }
            [Data] public Section SpriteDesc { get; set; }
            [Data] public Section SpritePartDesc { get; set; }
            [Data] public Section SpriteGroupDesc { get; set; }
            [Data] public Section AnimationDesc { get; set; }
            [Data] public Section AnimationGroupDesc { get; set; }
        }

        public class RawSprite
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

        public class RawSpriteGroup
        {
            [Data] public short Start { get; set; }
            [Data] public short Count { get; set; }
        }

        public class RawAnimationGroup
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

        public class Sprite
        {
            public int Left { get; set; }
            public int Top { get; set; }
            public int Right { get; set; }
            public int Bottom { get; set; }
            public float UTranslation { get; set; }
            public float VTranslation { get; set; }
            public uint ColorLeft { get; set; }
            public uint ColorTop { get; set; }
            public uint ColorRight { get; set; }
            public uint ColorBottom { get; set; }
        }

        public class SpritePart
        {
            [Data] public int Left { get; set; }
            [Data] public int Top { get; set; }
            [Data] public int Right { get; set; }
            [Data] public int Bottom { get; set; }
            [Data] public int SpriteIndex { get; set; }
        }

        public class Animation
        {
            [Data] public int Flags { get; set; }
            [Data] public int SpriteGroupIndex { get; set; }
            [Data] public int FrameStart { get; set; }
            [Data] public int FrameEnd { get; set; }
            [Data] public int TranslateXStart { get; set; }
            [Data] public int TranslateXEnd { get; set; }
            [Data] public int TranslateYStart { get; set; }
            [Data] public int TranslateYEnd { get; set; }
            [Data] public int PivotXStart { get; set; }
            [Data] public int PivotXEnd { get; set; }
            [Data] public int PivotYStart { get; set; }
            [Data] public int PivotYEnd { get; set; }
            [Data] public float RotationXStart { get; set; }
            [Data] public float RotationXEnd { get; set; }
            [Data] public float RotationYStart { get; set; }
            [Data] public float RotationYEnd { get; set; }
            [Data] public float RotationZStart { get; set; }
            [Data] public float RotationZEnd { get; set; }
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
            [Data] public float BounceXStart { get; set; }
            [Data] public float BounceXEnd { get; set; }
            [Data] public float BounceYStart { get; set; }
            [Data] public float BounceYEnd { get; set; }
            [Data] public short BounceXSpeed { get; set; }
            [Data] public short BounceYSpeed { get; set; }
            [Data] public int ColorBlend { get; set; }
            [Data] public uint ColorStart { get; set; }
            [Data] public uint ColorEnd { get; set; }
        }

        public class AnimationGroup
        {
            public List<Animation> Animations { get; set; }
            public short DoNotLoop { get; set; }
            public short Unknown06 { get; set; }
            public int LoopStart { get; set; }
            public int LoopEnd { get; set; }
            public int LightPositionX { get; set; }
            public int TextPositionY { get; set; }
            public int TextScale { get; set; }
            public int UiPadding { get; set; }
            public int TextPositionX { get; set; }
        }

        public int Unknown04 { get; set; }
        public List<Sprite> Sprites { get; set; }
        public List<List<SpritePart>> SpriteGroups { get; set; }
        public List<AnimationGroup> AnimationGroups { get; set; }

        public Sequence()
        {
            Unknown04 = 0x100; // assuming that this value is constant
        }

        private Sequence(Stream inputStream)
        {
            inputStream.MustReadAndSeek();
            if (inputStream.Length < MinimumLength)
                throw new InvalidDataException("Invalid header length");

            var stream = new SubStream(inputStream, inputStream.Position, inputStream.Length - inputStream.Position);
            var header = BinaryMapping.ReadObject<Header>(stream);
            if (header.MagicCode != MagicCodeValidator)
                throw new InvalidDataException("Invalid header");

            Unknown04 = header.Unknown04;
            Sprites = stream.ReadList<RawSprite>(header.SpriteDesc.Offset, header.SpriteDesc.Count)
                .Select(x => new Sprite
                {
                    Left = x.Left,
                    Top = x.Top,
                    Right = x.Right,
                    Bottom = x.Bottom,
                    UTranslation = x.UTranslation,
                    VTranslation = x.VTranslation,
                    ColorLeft = x.ColorLeft,
                    ColorTop = x.ColorTop,
                    ColorRight = x.ColorRight,
                    ColorBottom = x.ColorBottom,
                }).ToList();

            var spritePart = stream.ReadList<SpritePart>(header.SpritePartDesc.Offset, header.SpritePartDesc.Count);
            SpriteGroups = stream.ReadList<RawSpriteGroup>(header.SpriteGroupDesc.Offset, header.SpriteGroupDesc.Count)
                .Select(x => spritePart.Skip(x.Start).Take(x.Count).ToList()).ToList();

            var animations = stream.ReadList<Animation>(header.AnimationDesc.Offset, header.AnimationDesc.Count);
            AnimationGroups = stream.ReadList<RawAnimationGroup>(header.AnimationGroupDesc.Offset, header.AnimationGroupDesc.Count)
                .Select(x => new AnimationGroup
                {
                    Animations = animations.Skip(x.AnimationIndex).Take(x.Count).ToList(),
                    DoNotLoop = x.DoNotLoop,
                    Unknown06 = x.Unknown06,
                    LoopStart = x.LoopStart,
                    LoopEnd = x.LoopEnd,
                    LightPositionX = x.Unknown10,
                    TextPositionY = x.Unknown14,
                    TextScale = x.Unknown18,
                    UiPadding = x.Unknown1C,
                    TextPositionX = x.Unknown20,
                }).ToList();
        }

        public void Write(Stream stream)
        {
            stream.MustWriteAndSeek();

            var header = new Header
            {
                MagicCode = MagicCodeValidator,
                Unknown04 = Unknown04,
                SpriteDesc = new Section() { Count = Sprites.Count },
                SpritePartDesc = new Section() { Count = SpriteGroups.Sum(x => x.Count) },
                SpriteGroupDesc = new Section() { Count = SpriteGroups.Count },
                AnimationDesc = new Section() { Count = AnimationGroups.Sum(x => x.Animations.Count) },
                AnimationGroupDesc = new Section() { Count = AnimationGroups.Count },
            };

            var index = 0;
            var basePosition = stream.Position;

            stream.Position = basePosition + MinimumLength;
            header.SpriteDesc.Offset = (int)(stream.Position - basePosition);

            header.SpritePartDesc.Offset = stream.WriteList(Sprites.Select(x => new RawSprite
            {
                Left = x.Left,
                Top = x.Top,
                Right = x.Right,
                Bottom = x.Bottom,
                UTranslation = x.UTranslation,
                VTranslation = x.VTranslation,
                ColorLeft = x.ColorLeft,
                ColorTop = x.ColorTop,
                ColorRight = x.ColorRight,
                ColorBottom = x.ColorBottom,
            })) + header.SpriteDesc.Offset;
            header.SpriteGroupDesc.Offset = stream.WriteList(SpriteGroups.SelectMany(x => x)) + header.SpritePartDesc.Offset;

            index = 0;
            foreach (var spriteGroup in SpriteGroups)
            {
                BinaryMapping.WriteObject(stream, new RawSpriteGroup
                {
                    Start = (short)index,
                    Count = (short)spriteGroup.Count
                });

                index += spriteGroup.Count;
            }
            header.AnimationDesc.Offset = SpriteGroups.Count * 4 + header.SpriteGroupDesc.Offset;
            header.AnimationGroupDesc.Offset = stream.WriteList(AnimationGroups.SelectMany(x => x.Animations)) + header.AnimationDesc.Offset;
            index = 0;
            foreach (var animGroup in AnimationGroups)
            {
                BinaryMapping.WriteObject(stream, new RawAnimationGroup
                {
                    AnimationIndex = (short)index,
                    Count = (short)animGroup.Animations.Count,
                    DoNotLoop = animGroup.DoNotLoop,
                    Unknown06 = animGroup.Unknown06,
                    LoopStart = animGroup.LoopStart,
                    LoopEnd = animGroup.LoopEnd,
                    Unknown10 = animGroup.LightPositionX,
                    Unknown14 = animGroup.TextPositionY,
                    Unknown18 = animGroup.TextScale,
                    Unknown1C = animGroup.UiPadding,
                    Unknown20 = animGroup.TextPositionX,
                });

                index += animGroup.Animations.Count;
            }

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
