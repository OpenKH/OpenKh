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
            [Data] public int Unk20 { get; set; }
            [Data] public int TotalFrameCount { get; set; }
            [Data] public int Unk28 { get; set; }
            [Data] public int Unk2c { get; set; }
            [Data] public float Unk30 { get; set; }
            [Data] public float Unk34 { get; set; }
            [Data] public float Unk38 { get; set; }
            [Data] public float Unk3c { get; set; }
            [Data] public float Unk40 { get; set; }
            [Data] public float Unk44 { get; set; }
            [Data] public float Unk48 { get; set; }
            [Data] public float Unk4c { get; set; }
            [Data] public float FrameLoop { get; set; }
            [Data] public float FrameEnd { get; set; }
            [Data] public float FramePerSecond { get; set; }
            [Data] public float FrameCount { get; set; }

            public List<Matrix4x4[]> Matrices { get; set; }
        }

        public class RawMotion
        {
            public int BoneCount { get; set; }
            public int Unk14 { get; set; }
            public int Unk18 { get; set; }
            public int Unk1c { get; set; }
            public int Unk28 { get; set; }
            public int Unk2c { get; set; }
            public float Unk30 { get; set; }
            public float Unk34 { get; set; }
            public float Unk38 { get; set; }
            public float Unk3c { get; set; }
            public float Unk40 { get; set; }
            public float Unk44 { get; set; }
            public float Unk48 { get; set; }
            public float Unk4c { get; set; }
            public float FrameLoop { get; set; }
            public float FrameEnd { get; set; }
            public float FramePerSecond { get; set; }
            public float FrameCount { get; set; }

            public List<Matrix4x4[]> Matrices { get; set; }
            public Matrix4x4[] Matrices2 { get; set; }
        }

        private int _04;
        public bool IsRaw { get; }

        public RawMotion Raw { get; }

        private Motion(Stream stream)
        {
            stream.Position += ReservedSize;

            var header = BinaryMapping.ReadObject<Header>(stream);
            IsRaw = header.Version == 1;
            _04 = header.Unk04;

            if (IsRaw)
            {
                var raw = BinaryMapping.ReadObject<RawMotionInternal>(stream);
                Raw = new RawMotion
                {
                    BoneCount = raw.BoneCount,
                    Unk14 = raw.Unk14,
                    Unk18 = raw.Unk18,
                    Unk1c = raw.Unk1c,
                    Unk28 = raw.Unk28,
                    Unk2c = raw.Unk2c,
                    Unk30 = raw.Unk30,
                    Unk34 = raw.Unk34,
                    Unk38 = raw.Unk38,
                    Unk3c = raw.Unk3c,
                    Unk40 = raw.Unk40,
                    Unk44 = raw.Unk44,
                    Unk48 = raw.Unk48,
                    Unk4c = raw.Unk4c,
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
                Write(stream, motion.Raw, motion._04);
            else
                throw new NotImplementedException();
        }

        private static void Write(Stream stream, RawMotion rawMotion, int _04)
        {
            const int HeaderSize = 0x60;
            const int Matrix4x4Size = 0x40;

            stream.Write(new byte[ReservedSize], 0, ReservedSize);
            BinaryMapping.WriteObject(stream, new Header
            {
                Version = 1,
                Unk04 = _04,
                ByteCount = HeaderSize +
                    rawMotion.BoneCount * rawMotion.Matrices.Count * Matrix4x4Size +
                    rawMotion.Matrices2.Length * Matrix4x4Size,
                Unk0c = 0,
            });

            BinaryMapping.WriteObject(stream, new RawMotionInternal
            {
                BoneCount = rawMotion.BoneCount,
                Unk14 = rawMotion.Unk14,
                Unk18 = rawMotion.Unk18,
                Unk1c = rawMotion.Unk1c, // Maybe always 0????
                Unk20 = (rawMotion.Matrices.Count - 1) * 2,
                TotalFrameCount = rawMotion.Matrices.Count,
                Unk28 = rawMotion.Unk28,
                Unk2c = rawMotion.Matrices2.Length > 0 ? HeaderSize +
                    rawMotion.BoneCount * rawMotion.Matrices.Count * Matrix4x4Size : 0,
                Unk30 = rawMotion.Unk30,
                Unk34 = rawMotion.Unk34,
                Unk38 = rawMotion.Unk38,
                Unk3c = rawMotion.Unk3c,
                Unk40 = rawMotion.Unk40,
                Unk44 = rawMotion.Unk44,
                Unk48 = rawMotion.Unk48,
                Unk4c = rawMotion.Unk4c,
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
