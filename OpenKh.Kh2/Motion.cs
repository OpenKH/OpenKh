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
        private const int ReservedSize = 0x90;
        private const int Matrix4x4Size = 0x40;

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
            [Data] public int Unk04 { get; set; }
            [Data] public int SecondaryBoneOffset { get; set; }
            [Data] public int JointIndicesOffset { get; set; }
            [Data] public int FrameTimeCount { get; set; }
            [Data] public int InitialPoseTableOffset { get; set; }
            [Data] public int InitialPoseTableCount { get; set; }
            [Data] public int FooterOffset { get; set; }
            [Data] public int AnimationBonePrimaryTableOffset { get; set; }
            [Data] public int AnimationBonePrimaryTableCount { get; set; }
            [Data] public int AnimationBoneSecondaryTableOffset { get; set; }
            [Data] public int AnimationBoneSecondaryTableCount { get; set; }
            [Data] public int TimelineTableOffset { get; set; }
            [Data] public int FrameTimeOffset { get; set; }
            [Data] public int KeyFramesOffset { get; set; }
            [Data] public int TangentValueTableOffset { get; set; }
            [Data] public int InverseKinematicTableOffset { get; set; }
            [Data] public int InverseKinematicTableCount { get; set; }
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
            [Data] public int UnkA8 { get; set; }
            [Data] public int UnkAc { get; set; }
        }

        public class InterpolatedMotion
        {
            public short BoneCount { get; set; }
            public int Unk04 { get; set; }
            public int Unk48 { get; set; }
            public int Table8Offset { get; set; }
            public int Table7Offset { get; set; }
            public int Table7Count { get; set; }
            public int Table6Offset { get; set; }
            public int Table6Count { get; set; }
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
            public int UnkA8 { get; set; }
            public int UnkAc { get; set; }

            public List<InitialPoseTable> InitialPose { get; set; }
            public List<AnimationBoneTable> AnimationBonePrimary { get; set; }
            public List<AnimationBoneTable> AnimationBoneSecondary { get; set; }
            public List<TimelineTable> Timeline { get; set; }
            public List<float> FrameTimes { get; set; }
            public List<InverseKinematicTable> InverseKinematic { get; set; }
            public List<SecondaryBoneTable> SecondaryBones { get; set; }
            public List<int> JointIndices { get; set; }
            public FooterTable Footer { get; internal set; }
        }

        public class InitialPoseTable
        {
            [Data] public short JointIndex { get; set; }
            [Data] public short Transform { get; set; }
            [Data] public float Value { get; set; }

            public override string ToString() =>
                $"{JointIndex:X4} {Transform:X4} {Value}";
        }

        public class AnimationBoneTable
        {
            [Data] public short JointIndex { get; set; }
            [Data] public byte Channel { get; set; }
            [Data] public byte TimelineCount { get; set; }
            [Data] public short TimelineStartIndex { get; set; }
        }

        private class TimelineTableInternal
        {
            [Data] public short Time { get; set; }
            [Data] public short KeyFrameIndex { get; set; }
            [Data] public short TangentStartIndex { get; set; }
            [Data] public short TangentEndIndex { get; set; }
        }

        public class TimelineTable
        {
            public Interpolation Interpolation { get; set; }
            public int FrameTimeIndex { get; set; }
            public float KeyFrame { get; set; }
            public float[] Tangents { get; set; }
        }

        public class InverseKinematicTable
        {
            [Data] public byte Unk00 { get; set; }
            [Data] public byte Unk01 { get; set; }
            [Data] public short Unk02 { get; set; }
            [Data] public short Unk04 { get; set; }
            [Data] public short Unk06 { get; set; }
            [Data] public int Unk08 { get; set; }
        }

        public class SecondaryBoneTable
        {
            [Data] public int BoneIndex { get; set; }
            [Data] public int ParentBoneIndex { get; set; }
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
                    Unk04 = motion.Unk04,
                    Unk48 = motion.Unk48,
                    Table8Offset = motion.Table8Offset,
                    Table7Offset = motion.Table7Offset,
                    Table7Count = motion.Table7Count,
                    Table6Offset = motion.Table6Offset,
                    Table6Count = motion.Table6Count,
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
                    UnkA8 = motion.UnkA8,
                    UnkAc = motion.UnkAc,
                };

                stream.Position = ReservedSize + motion.InitialPoseTableOffset;
                Interpolated.InitialPose = Enumerable
                    .Range(0, motion.InitialPoseTableCount)
                    .Select(x => BinaryMapping.ReadObject<InitialPoseTable>(stream))
                    .ToList();

                stream.Position = ReservedSize + motion.AnimationBonePrimaryTableOffset;
                Interpolated.AnimationBonePrimary = Enumerable
                    .Range(0, motion.AnimationBonePrimaryTableCount)
                    .Select(x => BinaryMapping.ReadObject<AnimationBoneTable>(stream))
                    .ToList();

                stream.Position = ReservedSize + motion.AnimationBoneSecondaryTableOffset;
                Interpolated.AnimationBoneSecondary = Enumerable
                    .Range(0, motion.AnimationBoneSecondaryTableCount)
                    .Select(x => BinaryMapping.ReadObject<AnimationBoneTable>(stream))
                    .ToList();

                stream.Position = ReservedSize + motion.TimelineTableOffset;
                var rawTimeline = Enumerable
                    .Range(0, (motion.FrameTimeOffset - motion.TimelineTableOffset) / 8)
                    .Select(x => BinaryMapping.ReadObject<TimelineTableInternal>(stream))
                    .ToList();

                stream.Position = ReservedSize + motion.FrameTimeOffset;
                Interpolated.FrameTimes = Enumerable
                    .Range(0, motion.FrameTimeCount)
                    .Select(x => reader.ReadSingle())
                    .ToList();

                stream.Position = ReservedSize + motion.KeyFramesOffset;
                var keyFrames = Enumerable
                    .Range(0, (motion.TangentValueTableOffset - motion.KeyFramesOffset) / 4)
                    .Select(x => reader.ReadSingle())
                    .ToList();

                stream.Position = ReservedSize + motion.TangentValueTableOffset;
                var tangentValues = Enumerable
                    .Range(0, (motion.InverseKinematicTableOffset - motion.TangentValueTableOffset) / 4)
                    .Select(x => reader.ReadSingle())
                    .ToList();

                stream.Position = ReservedSize + motion.InverseKinematicTableOffset;
                Interpolated.InverseKinematic = Enumerable
                    .Range(0, motion.InverseKinematicTableCount)
                    .Select(x => BinaryMapping.ReadObject<InverseKinematicTable>(stream))
                    .ToList();

                stream.Position = ReservedSize + motion.SecondaryBoneOffset;
                Interpolated.SecondaryBones = Enumerable
                    .Range(0, motion.TotalBoneCount - motion.BoneCount)
                    .Select(x => BinaryMapping.ReadObject<SecondaryBoneTable>(stream))
                    .ToList();

                stream.Position = ReservedSize + motion.JointIndicesOffset;
                Interpolated.JointIndices = Enumerable
                    .Range(0, motion.TotalBoneCount + 1)
                    .Select(x => reader.ReadInt32())
                    .ToList();

                Interpolated.Timeline = rawTimeline
                    .Select(x => new TimelineTable
                    {
                        Interpolation = (Interpolation)(x.Time & 3),
                        FrameTimeIndex = x.Time >> 2,
                        KeyFrame = keyFrames[x.KeyFrameIndex],
                        Tangents = Enumerable
                            .Range(0, x.TangentEndIndex - x.TangentStartIndex + 1)
                            .Select(i => tangentValues[x.TangentStartIndex + i])
                            .ToArray(),
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
            var keyFrameDictionary = new Dictionary<float, short>();
            var keyFrames = new List<float>();
            var tangentDictionary = new Dictionary<float, short>();
            var tangents = new List<float>();

            var rawTimeline = new List<TimelineTableInternal>(motion.Timeline.Count);
            foreach (var item in motion.Timeline)
            {
                var rawItem = new TimelineTableInternal
                {
                    Time = (short)(((int)item.Interpolation & 3) | (item.FrameTimeIndex << 2)),
                };

                if (!keyFrameDictionary.TryGetValue(item.KeyFrame, out var keyFrameIndex))
                {
                    rawItem.KeyFrameIndex = (short)keyFrames.Count;
                    keyFrameDictionary.Add(item.KeyFrame, rawItem.KeyFrameIndex);
                    keyFrames.Add(item.KeyFrame);
                }
                else
                    rawItem.KeyFrameIndex = keyFrameIndex;

                if (!tangentDictionary.TryGetValue(item.Tangents[0], out var tangentIndex))
                {
                    rawItem.TangentStartIndex = (short)tangents.Count;
                    tangentDictionary.Add(item.Tangents[0], (short)tangents.Count);
                    tangents.AddRange(item.Tangents);
                }
                else
                    rawItem.TangentStartIndex = tangentIndex;

                rawItem.TangentEndIndex = (short)(rawItem.TangentStartIndex + item.Tangents.Length - 1);
                rawTimeline.Add(rawItem);
            }

            var writer = new BinaryWriter(stream);
            var header = new InterpolatedMotionInternal
            {
                BoneCount = motion.BoneCount,
                Unk04 = motion.Unk04,
                Unk48 = motion.Unk48,
                Table8Offset = motion.Table8Offset,
                Table7Offset = motion.Table7Offset,
                Table7Count = motion.Table7Count,
                Table6Offset = motion.Table6Offset,
                Table6Count = motion.Table6Count,
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
                UnkA8 = motion.UnkA8,
                UnkAc = motion.UnkAc,
            };

            stream.Write(new byte[ReservedSize], 0, ReservedSize);
            BinaryMapping.WriteObject(stream, new Header { });
            BinaryMapping.WriteObject(stream, new InterpolatedMotionInternal { });

            header.InitialPoseTableCount = motion.InitialPose.Count;
            header.InitialPoseTableOffset = (int)(stream.Position - ReservedSize);
            foreach (var item in motion.InitialPose)
                BinaryMapping.WriteObject(stream, item);

            header.AnimationBonePrimaryTableCount = motion.AnimationBonePrimary.Count;
            header.AnimationBonePrimaryTableOffset = (int)(stream.Position - ReservedSize);
            foreach (var item in motion.AnimationBonePrimary)
                BinaryMapping.WriteObject(stream, item);

            header.AnimationBoneSecondaryTableCount = motion.AnimationBoneSecondary.Count;
            header.AnimationBoneSecondaryTableOffset = (int)(stream.Position - ReservedSize);
            foreach (var item in motion.AnimationBoneSecondary)
                BinaryMapping.WriteObject(stream, item);

            header.TimelineTableOffset = (int)(stream.Position - ReservedSize);
            foreach (var item in rawTimeline)
                BinaryMapping.WriteObject(stream, item);

            header.FrameTimeCount = motion.FrameTimes.Count;
            header.FrameTimeOffset = (int)(stream.Position - ReservedSize);
            foreach (var item in motion.FrameTimes)
                writer.Write(item);

            header.KeyFramesOffset = (int)(stream.Position - ReservedSize);
            foreach (var item in keyFrames)
                writer.Write(item);

            header.TangentValueTableOffset = (int)(stream.Position - ReservedSize);
            foreach (var item in tangents)
                writer.Write(item);

            header.InverseKinematicTableCount = motion.InverseKinematic.Count;
            header.InverseKinematicTableOffset = (int)(stream.Position - ReservedSize);
            foreach (var item in motion.InverseKinematic)
                BinaryMapping.WriteObject(stream, item);

            stream.AlignPosition(0x10);
            header.TotalBoneCount = (short)(motion.BoneCount + motion.SecondaryBones.Count);
            header.SecondaryBoneOffset = (int)(stream.Position - ReservedSize);
            foreach (var item in motion.SecondaryBones)
                BinaryMapping.WriteObject(stream, item);

            header.JointIndicesOffset = (int)(stream.Position - ReservedSize);
            foreach (var item in motion.JointIndices)
                writer.Write(item);

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
    }
}
