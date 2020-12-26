using OpenKh.Common;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using Xe.BinaryMapper;

namespace OpenKh.Kh2
{
    public class Motion
    {
        private class PoolValues<T>
        {
            private readonly Dictionary<T, int> _valuePool = new Dictionary<T, int>();
            private readonly List<T> _valueList = new List<T>();

            public List<T> Values => _valueList;

            public PoolValues()
            {

            }

            public PoolValues(IEnumerable<T> values)
            {
                var index = 0;
                foreach (var value in values)
                {
                    _valuePool.Add(value, index++);
                    _valueList.Add(value);
                }
            }

            public int GetIndex(T value)
            {
                if (_valuePool.TryGetValue(value, out var index))
                    return index;

                index = _valueList.Count;
                _valuePool.Add(value, index);
                _valueList.Add(value);

                return index;
            }
        }

        private const int ReservedSize = 0x90;
        private const int Matrix4x4Size = 0x40;
        private static readonly string[] Transforms = new[]
        {
            "Scale.X", "Scale.Y", "Scale.Z",
            "Rotation.X", "Rotation.Y", "Rotation.Z",
            "Translation.X", "Translation.Y", "Translation.Z",
        };

        public enum Interpolation
        {
            Nearest,
            Linear,
            Hermite,
            Zero,
        }

        private class Header
        {
            [Data] public int Version { get; set; }
            [Data] public int Unk04 { get; set; }
            [Data] public int ByteCount { get; set; }
            [Data] public int Unk0c { get; set; }
        }

        private class RawMotionInternal
        {
            [Data] public int BoneCount { get; set; }
            [Data] public int Unk14 { get; set; }
            [Data] public int Unk18 { get; set; }
            [Data] public int Unk1c { get; set; }
            [Data] public int FrameCountPerLoop { get; set; }
            [Data] public int TotalFrameCount { get; set; }
            [Data] public int Unk28 { get; set; }
            [Data] public int Unk2c { get; set; }
            [Data] public float BoundingBoxMinX { get; set; }
            [Data] public float BoundingBoxMinY { get; set; }
            [Data] public float BoundingBoxMinZ { get; set; }
            [Data] public float BoundingBoxMinW { get; set; }
            [Data] public float BoundingBoxMaxX { get; set; }
            [Data] public float BoundingBoxMaxY { get; set; }
            [Data] public float BoundingBoxMaxZ { get; set; }
            [Data] public float BoundingBoxMaxW { get; set; }
            [Data] public float FrameLoop { get; set; }
            [Data] public float FrameEnd { get; set; }
            [Data] public float FramePerSecond { get; set; }
            [Data] public float FrameCount { get; set; }

            public List<Matrix4x4[]> Matrices { get; set; }
        }

        public class RawMotion
        {
            public int BoneCount { get; set; }
            public int Unk28 { get; set; }
            public int Unk2c { get; set; }
            public float BoundingBoxMinX { get; set; }
            public float BoundingBoxMinY { get; set; }
            public float BoundingBoxMinZ { get; set; }
            public float BoundingBoxMinW { get; set; }
            public float BoundingBoxMaxX { get; set; }
            public float BoundingBoxMaxY { get; set; }
            public float BoundingBoxMaxZ { get; set; }
            public float BoundingBoxMaxW { get; set; }
            public float FrameLoop { get; set; }
            public float FrameEnd { get; set; }
            public float FramePerSecond { get; set; }
            public float FrameCount { get; set; }

            public List<Matrix4x4[]> Matrices { get; set; }
            public Matrix4x4[] Matrices2 { get; set; }
        }

        private class InterpolatedMotionInternal
        {
            [Data] public short BoneCount { get; set; }
            [Data] public short TotalBoneCount { get; set; }
            [Data] public int TotalFrameCount { get; set; }
            [Data] public int IKHelperOffset { get; set; }
            [Data] public int JointIndexOffset { get; set; }
            [Data] public int KeyFrameCount { get; set; }
            [Data] public int StaticPoseOffset { get; set; }
            [Data] public int StaticPoseCount { get; set; }
            [Data] public int FooterOffset { get; set; }
            [Data] public int ModelBoneAnimationOffset { get; set; }
            [Data] public int ModelBoneAnimationCount { get; set; }
            [Data] public int IKHelperAnimationOffset { get; set; }
            [Data] public int IKHelperAnimationCount { get; set; }
            [Data] public int TimelineOffset { get; set; }
            [Data] public int KeyFrameOffset { get; set; }
            [Data] public int TransformationValueOffset { get; set; }
            [Data] public int TangentOffset { get; set; }
            [Data] public int IKChainOffset { get; set; }
            [Data] public int IKChainCount { get; set; }
            [Data] public int Unk48 { get; set; }
            [Data] public int Table8Offset { get; set; }
            [Data] public int Table7Offset { get; set; }
            [Data] public int Table7Count { get; set; }
            [Data] public int Table6Offset { get; set; }
            [Data] public int Table6Count { get; set; }
            [Data] public float BoundingBoxMinX { get; set; }
            [Data] public float BoundingBoxMinY { get; set; }
            [Data] public float BoundingBoxMinZ { get; set; }
            [Data] public float BoundingBoxMinW { get; set; }
            [Data] public float BoundingBoxMaxX { get; set; }
            [Data] public float BoundingBoxMaxY { get; set; }
            [Data] public float BoundingBoxMaxZ { get; set; }
            [Data] public float BoundingBoxMaxW { get; set; }
            [Data] public float FrameLoop { get; set; }
            [Data] public float FrameEnd { get; set; }
            [Data] public float FramePerSecond { get; set; }
            [Data] public float FrameCount { get; set; }
            [Data] public int UnknownTable1Offset { get; set; }
            [Data] public int UnknownTable1Count { get; set; }
            [Data] public int Unk98 { get; set; }
            [Data] public int Unk9c { get; set; }
        }

        public class InterpolatedMotion
        {
            public short BoneCount { get; set; }
            public int TotalFrameCount { get; set; }
            public int Unk48 { get; set; }
            public float BoundingBoxMinX { get; set; }
            public float BoundingBoxMinY { get; set; }
            public float BoundingBoxMinZ { get; set; }
            public float BoundingBoxMinW { get; set; }
            public float BoundingBoxMaxX { get; set; }
            public float BoundingBoxMaxY { get; set; }
            public float BoundingBoxMaxZ { get; set; }
            public float BoundingBoxMaxW { get; set; }
            public float FrameLoop { get; set; }
            public float FrameEnd { get; set; }
            public float FramePerSecond { get; set; }
            public float FrameCount { get; set; }
            public int Unk98 { get; set; }
            public int Unk9c { get; set; }

            public List<StaticPoseTable> StaticPose { get; set; }
            public List<BoneAnimationTable> ModelBoneAnimation { get; set; }
            public List<BoneAnimationTable> IKHelperAnimation { get; set; }
            public List<TimelineTable> Timeline { get; set; }
            public List<IKChainTable> IKChains { get; set; }
            public List<UnknownTable8> Table8 { get; set; }
            public List<UnknownTable7> Table7 { get; set; }
            public List<UnknownTable6> Table6 { get; set; }
            public List<IKHelperTable> IKHelpers { get; set; }
            public List<JointTable> Joints { get; set; }
            public FooterTable Footer { get; set; }
        }

        public class StaticPoseTable
        {
            [Data] public short BoneIndex { get; set; }
            [Data] public short Channel { get; set; }
            [Data] public float Value { get; set; }

            public override string ToString() =>
                $"{BoneIndex} {Transforms[Channel]} {Value}";
        }

        public class BoneAnimationTableInternal
        {
            [Data] public short JointIndex { get; set; }
            [Data] public byte Channel { get; set; }
            [Data] public byte TimelineCount { get; set; }
            [Data] public short TimelineStartIndex { get; set; }

            public override string ToString() =>
                $"{JointIndex} {Transforms[Channel]} ({TimelineStartIndex},{TimelineCount})";
        }

        public class BoneAnimationTable
        {
            public short JointIndex { get; set; }
            public byte Channel { get; set; }
            public byte Pre { get; set; }
            public byte Post { get; set; }
            public byte TimelineCount { get; set; }
            public short TimelineStartIndex { get; set; }

            public override string ToString() =>
                $"{JointIndex} {Transforms[Channel]} ({Pre},{Post}) ({TimelineStartIndex},{TimelineCount})";
        }

        private class TimelineTableInternal
        {
            [Data] public short Time { get; set; }
            [Data] public short ValueIndex { get; set; }
            [Data] public short TangentIndexEaseIn { get; set; }
            [Data] public short TangentIndexEaseOut { get; set; }
        }

        public class TimelineTable
        {
            public Interpolation Interpolation { get; set; }
            public float KeyFrame { get; set; }
            public float Value { get; set; }
            public float TangentEaseIn { get; set; }
            public float TangentEaseOut { get; set; }

            public override string ToString() =>
                $"{KeyFrame} {Value} {Interpolation}:({TangentEaseIn},{TangentEaseOut})";
        }

        public class IKChainTable
        {
            [Data] public byte Unk00 { get; set; }
            [Data] public byte Unk01 { get; set; }
            [Data] public short ModelBoneIndex { get; set; }
            [Data] public short IKHelperIndex { get; set; }
            [Data] public short Table8Index { get; set; }
            [Data] public int Unk08 { get; set; }
        }

        public class UnknownTable8
        {
            [Data] public int Unk00 { get; set; }
            [Data] public float Unk04 { get; set; }
            [Data] public float Unk08 { get; set; }
            [Data] public float Unk0c { get; set; }
            [Data] public float Unk10 { get; set; }
            [Data] public float Unk14 { get; set; }
            [Data] public float Unk18 { get; set; }
            [Data] public float Unk1c { get; set; }
            [Data] public float Unk20 { get; set; }
            [Data] public float Unk24 { get; set; }
            [Data] public float Unk28 { get; set; }
            [Data] public float Unk2c { get; set; }
        }

        public class UnknownTable7
        {
            [Data] public short Unk00 { get; set; }
            [Data] public short Unk02 { get; set; }
            [Data] public float Unk04 { get; set; }
        }

        public class UnknownTable6
        {
            [Data] public short Unk00 { get; set; }
            [Data] public short Unk02 { get; set; }
            [Data] public float Unk04 { get; set; }
            [Data] public short Unk08 { get; set; }
            [Data] public short Unk0a { get; set; }
        }

        public class JointTable
        {
            [Data] public short JointIndex { get; set; }
            [Data] public short Flag { get; set; }
        }

        public class IKHelperTable
        {
            [Data] public int Index { get; set; }
            [Data] public int ParentIndex { get; set; }
            [Data] public int Unk08 { get; set; }
            [Data] public int Unk0c { get; set; }
            [Data] public float ScaleX { get; set; }
            [Data] public float ScaleY { get; set; }
            [Data] public float ScaleZ { get; set; }
            [Data] public float ScaleW { get; set; }
            [Data] public float RotateX { get; set; }
            [Data] public float RotateY { get; set; }
            [Data] public float RotateZ { get; set; }
            [Data] public float RotateW { get; set; }
            [Data] public float TranslateX { get; set; }
            [Data] public float TranslateY { get; set; }
            [Data] public float TranslateZ { get; set; }
            [Data] public float TranslateW { get; set; }
        }

        public class FooterTable
        {
            [Data] public float ScaleX { get; set; }
            [Data] public float ScaleY { get; set; }
            [Data] public float ScaleZ { get; set; }
            [Data] public float ScaleW { get; set; }
            [Data] public float RotateX { get; set; }
            [Data] public float RotateY { get; set; }
            [Data] public float RotateZ { get; set; }
            [Data] public float RotateW { get; set; }
            [Data] public float TranslateX { get; set; }
            [Data] public float TranslateY { get; set; }
            [Data] public float TranslateZ { get; set; }
            [Data] public float TranslateW { get; set; }
            [Data(Count = 9)] public int[] Unknown { get; set; }
        }


        public bool UnkFlag { get; set; }
        public bool IsRaw { get; }

        public RawMotion Raw { get; }
        public InterpolatedMotion Interpolated { get; }

        private Motion(Stream stream)
        {
            stream.Position += ReservedSize;

            var header = BinaryMapping.ReadObject<Header>(stream);
            IsRaw = header.Version == 1;
            UnkFlag = header.Unk04 != 0;

            if (IsRaw)
            {
                var raw = BinaryMapping.ReadObject<RawMotionInternal>(stream);
                Raw = new RawMotion
                {
                    BoneCount = raw.BoneCount,
                    Unk28 = raw.Unk28,
                    Unk2c = raw.Unk2c,
                    BoundingBoxMinX = raw.BoundingBoxMinX,
                    BoundingBoxMinY = raw.BoundingBoxMinY,
                    BoundingBoxMinZ = raw.BoundingBoxMinZ,
                    BoundingBoxMinW = raw.BoundingBoxMinW,
                    BoundingBoxMaxX = raw.BoundingBoxMaxX,
                    BoundingBoxMaxY = raw.BoundingBoxMaxY,
                    BoundingBoxMaxZ = raw.BoundingBoxMaxZ,
                    BoundingBoxMaxW = raw.BoundingBoxMaxW,
                    FrameLoop = raw.FrameLoop,
                    FrameEnd = raw.FrameEnd,
                    FramePerSecond = raw.FramePerSecond,
                    FrameCount = raw.FrameCount
                };

                var reader = new BinaryReader(stream);
                Raw.Matrices = new List<Matrix4x4[]>(raw.TotalFrameCount);
                for (var i = 0; i < raw.TotalFrameCount; i++)
                {
                    var matrices = new Matrix4x4[Raw.BoneCount];
                    for (var j = 0; j < Raw.BoneCount; j++)
                        matrices[j] = ReadMatrix(reader);

                    Raw.Matrices.Add(matrices);
                }

                if (raw.Unk2c > 0)
                {
                    stream.Position = ReservedSize + raw.Unk2c;
                    Raw.Matrices2 = new Matrix4x4[raw.TotalFrameCount];
                    for (var j = 0; j < Raw.Matrices2.Length; j++)
                        Raw.Matrices2[j] = ReadMatrix(reader);
                }
                else
                    Raw.Matrices2 = new Matrix4x4[0];
            }
            else
            {
                var reader = new BinaryReader(stream);
                var motion = BinaryMapping.ReadObject<InterpolatedMotionInternal>(stream);
                Interpolated = new InterpolatedMotion
                {
                    BoneCount = motion.BoneCount,
                    TotalFrameCount = motion.TotalFrameCount,
                    Unk48 = motion.Unk48,
                    BoundingBoxMinX = motion.BoundingBoxMinX,
                    BoundingBoxMinY = motion.BoundingBoxMinY,
                    BoundingBoxMinZ = motion.BoundingBoxMinZ,
                    BoundingBoxMinW = motion.BoundingBoxMinW,
                    BoundingBoxMaxX = motion.BoundingBoxMaxX,
                    BoundingBoxMaxY = motion.BoundingBoxMaxY,
                    BoundingBoxMaxZ = motion.BoundingBoxMaxZ,
                    BoundingBoxMaxW = motion.BoundingBoxMaxW,
                    FrameLoop = motion.FrameLoop,
                    FrameEnd = motion.FrameEnd,
                    FramePerSecond = motion.FramePerSecond,
                    FrameCount = motion.FrameCount,
                    Unk98 = motion.Unk98,
                    Unk9c = motion.Unk9c,
                };

                stream.Position = ReservedSize + motion.StaticPoseOffset;
                Interpolated.StaticPose = Enumerable
                    .Range(0, motion.StaticPoseCount)
                    .Select(x => BinaryMapping.ReadObject<StaticPoseTable>(stream))
                    .ToList();

                stream.Position = ReservedSize + motion.ModelBoneAnimationOffset;
                Interpolated.ModelBoneAnimation = Enumerable
                    .Range(0, motion.ModelBoneAnimationCount)
                    .Select(x => BinaryMapping.ReadObject<BoneAnimationTableInternal>(stream))
                    .Select(Map)
                    .ToList();

                stream.Position = ReservedSize + motion.IKHelperAnimationOffset;
                Interpolated.IKHelperAnimation = Enumerable
                    .Range(0, motion.IKHelperAnimationCount)
                    .Select(x => BinaryMapping.ReadObject<BoneAnimationTableInternal>(stream))
                    .Select(Map)
                    .ToList();

                stream.Position = ReservedSize + motion.TimelineOffset;
                var estimatedTimelineCount = (motion.KeyFrameOffset - motion.TimelineOffset) / 8;
                var rawTimeline = Enumerable
                    .Range(0, estimatedTimelineCount)
                    .Select(x => BinaryMapping.ReadObject<TimelineTableInternal>(stream))
                    .ToList();

                stream.Position = ReservedSize + motion.KeyFrameOffset;
                var keyFrames = Enumerable
                    .Range(0, motion.KeyFrameCount)
                    .Select(x => reader.ReadSingle())
                    .ToList();

                stream.Position = ReservedSize + motion.TransformationValueOffset;
                var estimatedKeyFrameCount = (motion.TangentOffset - motion.TransformationValueOffset) / 4;
                var transformationValues = Enumerable
                    .Range(0, estimatedKeyFrameCount)
                    .Select(x => reader.ReadSingle())
                    .ToList();

                stream.Position = ReservedSize + motion.TangentOffset;
                var estimatedTangentCount = (motion.IKChainOffset - motion.TangentOffset) / 4;
                var tangentValues = Enumerable
                    .Range(0, estimatedTangentCount)
                    .Select(x => reader.ReadSingle())
                    .ToList();

                stream.Position = ReservedSize + motion.IKChainOffset;
                Interpolated.IKChains = Enumerable
                    .Range(0, motion.IKChainCount)
                    .Select(x => BinaryMapping.ReadObject<IKChainTable>(stream))
                    .ToList();

                stream.Position = ReservedSize + motion.Table8Offset;
                var estimatedTable8Count = (motion.Table7Offset - motion.Table8Offset) / 0x30;
                Interpolated.Table8 = Enumerable
                    .Range(0, estimatedTable8Count)
                    .Select(x => BinaryMapping.ReadObject<UnknownTable8>(stream))
                    .ToList();

                stream.Position = ReservedSize + motion.Table7Offset;
                Interpolated.Table7 = Enumerable
                    .Range(0, motion.Table7Count)
                    .Select(x => BinaryMapping.ReadObject<UnknownTable7>(stream))
                    .ToList();

                stream.Position = ReservedSize + motion.Table6Offset;
                Interpolated.Table6 = Enumerable
                    .Range(0, motion.Table6Count)
                    .Select(x => BinaryMapping.ReadObject<UnknownTable6>(stream))
                    .ToList();

                stream.Position = ReservedSize + motion.IKHelperOffset;
                Interpolated.IKHelpers = Enumerable
                    .Range(0, motion.TotalBoneCount - motion.BoneCount)
                    .Select(x => BinaryMapping.ReadObject<IKHelperTable>(stream))
                    .ToList();

                stream.Position = ReservedSize + motion.JointIndexOffset;
                Interpolated.Joints = Enumerable
                    .Range(0, motion.TotalBoneCount + 1)
                    .Select(x => BinaryMapping.ReadObject<JointTable>(stream))
                    .ToList();

                Interpolated.Timeline = rawTimeline
                    .Select(x => new TimelineTable
                    {
                        Interpolation = (Interpolation)(x.Time & 3),
                        KeyFrame = keyFrames[x.Time >> 2],
                        Value = transformationValues[x.ValueIndex],
                        TangentEaseIn = tangentValues[x.TangentIndexEaseIn],
                        TangentEaseOut = tangentValues[x.TangentIndexEaseOut],
                    })
                    .ToList();

                stream.Position = ReservedSize + motion.FooterOffset;
                Interpolated.Footer = BinaryMapping.ReadObject<FooterTable>(stream);

                stream.Position = ReservedSize + motion.UnknownTable1Offset;
            }
        }

        public static Motion Read(Stream stream) =>
            new Motion(stream);

        public static void Write(Stream stream, Motion motion)
        {
            if (motion.IsRaw)
                Write(stream, motion.Raw, motion.UnkFlag);
            else
                Write(stream, motion.Interpolated, motion.UnkFlag);
        }

        private static void Write(Stream stream, RawMotion rawMotion, bool unkFlag)
        {
            const int HeaderSize = 0x60;

            stream.Write(new byte[ReservedSize], 0, ReservedSize);
            BinaryMapping.WriteObject(stream, new Header
            {
                Version = 1,
                Unk04 = unkFlag ? 1 : 0,
                ByteCount = HeaderSize +
                    rawMotion.BoneCount * rawMotion.Matrices.Count * Matrix4x4Size +
                    rawMotion.Matrices2.Length * Matrix4x4Size,
                Unk0c = 0,
            });

            BinaryMapping.WriteObject(stream, new RawMotionInternal
            {
                BoneCount = rawMotion.BoneCount,
                Unk14 = 0,
                Unk18 = 0,
                Unk1c = 0,
                FrameCountPerLoop = (int)(rawMotion.FrameEnd - rawMotion.FrameLoop) * 2,
                TotalFrameCount = rawMotion.Matrices.Count,
                Unk28 = rawMotion.Unk28,
                Unk2c = rawMotion.Matrices2.Length > 0 ? HeaderSize +
                    rawMotion.BoneCount * rawMotion.Matrices.Count * Matrix4x4Size : 0,
                BoundingBoxMinX = rawMotion.BoundingBoxMinX,
                BoundingBoxMinY = rawMotion.BoundingBoxMinY,
                BoundingBoxMinZ = rawMotion.BoundingBoxMinZ,
                BoundingBoxMinW = rawMotion.BoundingBoxMinW,
                BoundingBoxMaxX = rawMotion.BoundingBoxMaxX,
                BoundingBoxMaxY = rawMotion.BoundingBoxMaxY,
                BoundingBoxMaxZ = rawMotion.BoundingBoxMaxZ,
                BoundingBoxMaxW = rawMotion.BoundingBoxMaxW,
                FrameLoop = rawMotion.FrameLoop,
                FrameEnd = rawMotion.FrameEnd,
                FramePerSecond = rawMotion.FramePerSecond,
                FrameCount = rawMotion.FrameCount
            });

            var writer = new BinaryWriter(stream);
            foreach (var block in rawMotion.Matrices)
                for (int i = 0; i < block.Length; i++)
                    WriteMatrix(writer, block[i]);

            for (int i = 0; i < rawMotion.Matrices2.Length; i++)
                WriteMatrix(writer, rawMotion.Matrices2[i]);

            writer.Flush();
        }

        private static void Write(Stream stream, InterpolatedMotion motion, bool unkFlag)
        {
            var valuePool = new PoolValues<float>();
            var keyFramePool = new PoolValues<float>(motion.Timeline.Select(x => x.KeyFrame).Distinct().OrderBy(x => x));
            var tangentPool = new PoolValues<float>();
            var rawTimeline = new List<TimelineTableInternal>(motion.Timeline.Count);
            foreach (var item in motion.Timeline)
            {
                rawTimeline.Add(new TimelineTableInternal
                {
                    Time = (short)(((int)item.Interpolation & 3) | (keyFramePool.GetIndex(item.KeyFrame) << 2)),
                    ValueIndex = (short)valuePool.GetIndex(item.Value),
                    TangentIndexEaseIn = (short)tangentPool.GetIndex(item.TangentEaseIn),
                    TangentIndexEaseOut = (short)tangentPool.GetIndex(item.TangentEaseOut),
                });
            }

            var writer = new BinaryWriter(stream);
            var header = new InterpolatedMotionInternal
            {
                BoneCount = motion.BoneCount,
                TotalFrameCount = motion.TotalFrameCount,
                Unk48 = motion.Unk48,
                BoundingBoxMinX = motion.BoundingBoxMinX,
                BoundingBoxMinY = motion.BoundingBoxMinY,
                BoundingBoxMinZ = motion.BoundingBoxMinZ,
                BoundingBoxMinW = motion.BoundingBoxMinW,
                BoundingBoxMaxX = motion.BoundingBoxMaxX,
                BoundingBoxMaxY = motion.BoundingBoxMaxY,
                BoundingBoxMaxZ = motion.BoundingBoxMaxZ,
                BoundingBoxMaxW = motion.BoundingBoxMaxW,
                FrameLoop = motion.FrameLoop,
                FrameEnd = motion.FrameEnd,
                FramePerSecond = motion.FramePerSecond,
                FrameCount = motion.FrameCount,
                Unk98 = motion.Unk98,
                Unk9c = motion.Unk9c,
            };

            stream.Write(new byte[ReservedSize], 0, ReservedSize);
            BinaryMapping.WriteObject(stream, new Header { });
            BinaryMapping.WriteObject(stream, new InterpolatedMotionInternal { });

            header.StaticPoseCount = motion.StaticPose.Count;
            header.StaticPoseOffset = (int)(stream.Position - ReservedSize);
            foreach (var item in motion.StaticPose)
                BinaryMapping.WriteObject(stream, item);

            header.ModelBoneAnimationCount = motion.ModelBoneAnimation.Count;
            header.ModelBoneAnimationOffset = (int)(stream.Position - ReservedSize);
            foreach (var item in motion.ModelBoneAnimation.Select(Map))
                BinaryMapping.WriteObject(stream, item);

            header.IKHelperAnimationCount = motion.IKHelperAnimation.Count;
            header.IKHelperAnimationOffset = (int)(stream.Position - ReservedSize);
            foreach (var item in motion.IKHelperAnimation.Select(Map))
                BinaryMapping.WriteObject(stream, item);

            header.TimelineOffset = (int)(stream.Position - ReservedSize);
            foreach (var item in rawTimeline)
                BinaryMapping.WriteObject(stream, item);

            stream.AlignPosition(4);
            header.KeyFrameCount = keyFramePool.Values.Count;
            header.KeyFrameOffset = (int)(stream.Position - ReservedSize);
            foreach (var item in keyFramePool.Values)
                writer.Write(item);

            header.TransformationValueOffset = (int)(stream.Position - ReservedSize);
            foreach (var item in valuePool.Values)
                writer.Write(item);

            header.TangentOffset = (int)(stream.Position - ReservedSize);
            foreach (var item in tangentPool.Values)
                writer.Write(item);

            header.IKChainCount = motion.IKChains.Count;
            header.IKChainOffset = (int)(stream.Position - ReservedSize);
            foreach (var item in motion.IKChains)
                BinaryMapping.WriteObject(stream, item);

            header.Unk48 = (int)(stream.Position - ReservedSize);

            stream.AlignPosition(0x10);
            header.Table8Offset = (int)(stream.Position - ReservedSize);
            foreach (var item in motion.Table8)
                BinaryMapping.WriteObject(stream, item);

            stream.AlignPosition(0x10);
            header.Table7Count = motion.Table7.Count;
            header.Table7Offset = (int)(stream.Position - ReservedSize);
            foreach (var item in motion.Table7)
                BinaryMapping.WriteObject(stream, item);

            header.Table6Count = motion.Table6.Count;
            header.Table6Offset = (int)(stream.Position - ReservedSize);
            foreach (var item in motion.Table6)
                BinaryMapping.WriteObject(stream, item);

            stream.AlignPosition(0x10);
            header.TotalBoneCount = (short)(motion.BoneCount + motion.IKHelpers.Count);
            header.IKHelperOffset = (int)(stream.Position - ReservedSize);
            foreach (var item in motion.IKHelpers)
                BinaryMapping.WriteObject(stream, item);

            header.JointIndexOffset = (int)(stream.Position - ReservedSize);
            foreach (var item in motion.Joints)
                BinaryMapping.WriteObject(stream, item);

            stream.AlignPosition(0x10);
            header.FooterOffset = (int)(stream.Position - ReservedSize);
            BinaryMapping.WriteObject(stream, motion.Footer);

            header.UnknownTable1Offset = (int)(stream.Position - ReservedSize);
            header.UnknownTable1Count = 0;

            stream.AlignPosition(0x10);
            stream.SetLength(stream.Position);

            stream.SetPosition(ReservedSize);
            BinaryMapping.WriteObject(stream, new Header
            {
                Version = 0,
                Unk04 = unkFlag ? 1 : 0,
                ByteCount = (int)(stream.Length - ReservedSize),
                Unk0c = 0,
            });
            BinaryMapping.WriteObject(stream, header);
        }

        private static BoneAnimationTable Map(BoneAnimationTableInternal obj) => new BoneAnimationTable
        {
            JointIndex = obj.JointIndex,
            Channel = (byte)(obj.Channel & 0xF),
            Pre = (byte)((obj.Channel >> 4) & 3),
            Post = (byte)((obj.Channel >> 6) & 3),
            TimelineStartIndex = obj.TimelineStartIndex,
            TimelineCount = obj.TimelineCount,
        };

        private static BoneAnimationTableInternal Map(BoneAnimationTable obj) => new BoneAnimationTableInternal
        {
            JointIndex = obj.JointIndex,
            Channel = (byte)((obj.Channel & 0xF) | ((obj.Pre & 3) << 4) | ((obj.Post & 3) << 6)),
            TimelineStartIndex = obj.TimelineStartIndex,
            TimelineCount = obj.TimelineCount,
        };

        private static Matrix4x4 ReadMatrix(BinaryReader reader) => new Matrix4x4(
            reader.ReadSingle(),
            reader.ReadSingle(),
            reader.ReadSingle(),
            reader.ReadSingle(),
            reader.ReadSingle(),
            reader.ReadSingle(),
            reader.ReadSingle(),
            reader.ReadSingle(),
            reader.ReadSingle(),
            reader.ReadSingle(),
            reader.ReadSingle(),
            reader.ReadSingle(),
            reader.ReadSingle(),
            reader.ReadSingle(),
            reader.ReadSingle(),
            reader.ReadSingle());

        private static void WriteMatrix(BinaryWriter writer, Matrix4x4 matrix)
        {
            writer.Write(matrix.M11);
            writer.Write(matrix.M12);
            writer.Write(matrix.M13);
            writer.Write(matrix.M14);
            writer.Write(matrix.M21);
            writer.Write(matrix.M22);
            writer.Write(matrix.M23);
            writer.Write(matrix.M24);
            writer.Write(matrix.M31);
            writer.Write(matrix.M32);
            writer.Write(matrix.M33);
            writer.Write(matrix.M34);
            writer.Write(matrix.M41);
            writer.Write(matrix.M42);
            writer.Write(matrix.M43);
            writer.Write(matrix.M44);
        }

        private Motion(bool isRaw)
        {
            IsRaw = isRaw;
            if (isRaw)
            {
                Raw = new RawMotion
                {
                    Matrices = new List<Matrix4x4[]>(),
                };
            }
            else
            {
                Interpolated = new InterpolatedMotion
                {
                    Footer = new FooterTable(),
                    IKChains = new List<IKChainTable>(),
                    IKHelperAnimation = new List<BoneAnimationTable>(),
                    IKHelpers = new List<IKHelperTable>(),
                    Joints = new List<JointTable>(),
                    ModelBoneAnimation = new List<BoneAnimationTable>(),
                    StaticPose = new List<StaticPoseTable>(),
                    Table6 = new List<UnknownTable6>(),
                    Table7 = new List<UnknownTable7>(),
                    Table8 = new List<UnknownTable8>(),
                    Timeline = new List<TimelineTable>(),
                };
            }
        }

        public static Motion CreateInterpolatedFromScratch() => new Motion(false);
    }
}
