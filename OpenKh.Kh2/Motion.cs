using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using Xe.BinaryMapper;

namespace OpenKh.Kh2
{
    public class Motion
    {
        private class Header
        {
            [Data] public int Version { get; set; }
            [Data] public int Unk04 { get; set; }
            [Data] public int ByteCount { get; set; }
            [Data] public int Unk0c { get; set; }
        }

        public class RawMotion
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

        public bool IsRaw { get; }

        public RawMotion Raw { get; }

        private Motion(Stream stream)
        {
            stream.Position += 0x90;

            var header = BinaryMapping.ReadObject<Header>(stream);
            IsRaw = header.Version == 1;

            if (IsRaw)
            {
                Raw = BinaryMapping.ReadObject<RawMotion>(stream);
                Raw.Matrices = new List<Matrix4x4[]>(Raw.TotalFrameCount);

                var reader = new BinaryReader(stream);
                for (var i = 0; i < Raw.TotalFrameCount; i++)
                {
                    var matrices = new Matrix4x4[Raw.BoneCount];
                    for (var j = 0; j < Raw.BoneCount; j++)
                        matrices[j] = ReadMatrix(reader);

                    Raw.Matrices.Add(matrices);
                }
            }
            else
                throw new NotImplementedException();
        }

        public static Motion Read(Stream stream) =>
            new Motion(stream);

        public static void Write(Stream stream, Motion motion)
        {
            if (motion.IsRaw)
                Write(stream, motion.Raw);
            else
                throw new NotImplementedException();
        }

        public static void Write(Stream stream, RawMotion rawMotion)
        {
            const int HeaderSize = 0x60;
            const int Matrix4x4Size = 0x40;

            stream.Write(new byte[0x90], 0, 0x90);
            BinaryMapping.WriteObject(stream, new Header
            {
                Version = 1,
                Unk04 = 0,
                ByteCount = HeaderSize + rawMotion.BoneCount *
                    rawMotion.TotalFrameCount * Matrix4x4Size,
                Unk0c = 0,
            });

            rawMotion.TotalFrameCount = rawMotion.Matrices.Count;
            rawMotion.Unk1c = 0;
            rawMotion.Unk20 = (rawMotion.TotalFrameCount - 1) * 2;
            rawMotion.FrameCount = rawMotion.TotalFrameCount - 1;
            BinaryMapping.WriteObject(stream, rawMotion);

            var writer = new BinaryWriter(stream);
            foreach (var block in rawMotion.Matrices)
                for (int i = 0; i < block.Length; i++)
                    WriteMatrix(writer, block[i]);
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
