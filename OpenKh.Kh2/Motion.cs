using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using Xe.BinaryMapper;

namespace OpenKh.Kh2
{
    public class Motion
    {
        private const int ReservedSize = 0x90;

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

        public bool UnkFlag { get; set; }
        public bool IsRaw { get; }

        public RawMotion Raw { get; }

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
                throw new NotImplementedException();
        }

        public static Motion Read(Stream stream) =>
            new Motion(stream);

        public static void Write(Stream stream, Motion motion)
        {
            if (motion.IsRaw)
                Write(stream, motion.Raw, motion.UnkFlag);
            else
                throw new NotImplementedException();
        }

        private static void Write(Stream stream, RawMotion rawMotion, bool unkFlag)
        {
            const int HeaderSize = 0x60;
            const int Matrix4x4Size = 0x40;

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
