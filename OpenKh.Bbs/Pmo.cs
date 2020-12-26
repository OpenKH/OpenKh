using System;
using System.Collections.Generic;
using System.IO;
using OpenKh.Common;
using Xe.BinaryMapper;

namespace OpenKh.Bbs
{
    public class Pmo
    {
        private const UInt32 MagicCode = 0x4F4D50;

        public class Header
        {
            [Data] public UInt32 MagicCode { get; set; }
            [Data] public byte Unk1 { get; set; }
            [Data] public byte Unk2 { get; set; }
            [Data] public byte Unk3 { get; set; }
            [Data] public byte Unk4 { get; set; }
            [Data] public ushort TextureCount { get; set; }
            [Data] public ushort Unk5 { get; set; }
            [Data] public UInt32 SkeletonOffset { get; set; }
            [Data] public UInt32 MeshOffset0 { get; set; }
            [Data] public ushort TriangleCount { get; set; }
            [Data] public ushort VertexCount { get; set; }
            [Data] public float ModelScale { get; set; }
            [Data] public UInt32 MeshOffset1 { get; set; }
            [Data(Count = 32)] public float BoundingBox { get; set; }
        }

        public class TextureInfo
        {
            [Data] public UInt32 TextureOffset { get; set; }
            [Data(Count = 12)] public string TextureName { get; set; }
            [Data] public UInt32 Unk1 { get; set; }
        }

        public class MeshSection
        {
            [Data] public ushort VertexCount { get; set; }
            [Data] public byte TextureID { get; set; }
            [Data] public byte VertexSize { get; set; } // In bytes.
            [Data] public UInt32 VertexFlags { get; set; }
            [Data] public byte Unknown1 { get; set; }
            [Data] public byte TriangleStripCount { get; set; }
            [Data(Count = 2)] public byte Unknown2 { get; set; }
        }

        public class MeshSectionOptional1
        {
            [Data(Count = 8)] public byte SectionBoneIndices { get; set; } // Only present if header.SkeletonOffset != 0
        }
        public class MeshSectionOptional2
        {
            [Data] public UInt32 DiffuseColor { get; set; } // Only present if vertexFlags.DiffuseColor & 1
        }

        public enum CoordinateFormat
        {
            NO_VERTEX,
            NORMALIZED_8_BITS,
            NORMALIZED_16_BITS,
            FLOAT_32_BITS
        }

        public enum ColorFormat
        {
            NO_COLOR,
            BGR_5650_16BITS = 4,
            ABGR_5551_16BITS,
            ABGR_4444_16BITS,
            ABGR_8888_32BITS,
        }

        public enum PrimitiveType
        {
            PRIMITIVE_POINT,
            PRIMITIVE_LINE,
            PRIMITIVE_LINE_STRIP,
            PRIMITIVE_TRIANGLE,
            PRIMITIVE_TRIANGLE_STRIP,
            PRIMITIVE_TRIANGLE_FAN,
            PRIMITIVE_QUAD
        }

        public class VertexFlags
        {
            public CoordinateFormat TextureCoordinateFormat;
            public ColorFormat ColorFormat;
            public CoordinateFormat NormalFormat; // Unused
            public CoordinateFormat PositionFormat;
            public CoordinateFormat WeightFormat;
            public byte IndicesFormat; // Unused
            public bool Unused1;
            public byte SkinningWeightsCount;
            public bool Unused2;
            public byte MorphWeightsCount; // Unused
            public byte Unused3;
            public bool SkipTransformPipeline; // Unused
            public bool UniformDiffuseFlag;
            public byte Unknown1;
            public PrimitiveType Primitive;
        }

        public static UInt32 GetBitFieldRange(UInt32 value, int start = 0, int length = 1)
        {
            return ( (value << start) >> (32 - length) );
        }

        public Header header { get; set; }
        public TextureInfo[] textureInfo { get; set; }
        public List<MeshSection> meshSection = new List<MeshSection>();
        public List<MeshSectionOptional1> meshSectionOpt1 = new List<MeshSectionOptional1>();
        public List<MeshSectionOptional2> meshSectionOpt2 = new List<MeshSectionOptional2>();
        public List<float> jointWeights = new List<float>();
        public List<float> textureCoordinates = new List<float>();
        public List<float> vertices = new List<float>();

        public static Pmo Read(Stream stream)
        {
            Pmo pmo = new Pmo
            {
                header = BinaryMapping.ReadObject<Header>(stream.SetPosition(0))
            };

            pmo.textureInfo = new TextureInfo[pmo.header.TextureCount];
            for (ushort i = 0; i < pmo.header.TextureCount; i++) pmo.textureInfo[i] = BinaryMapping.ReadObject<TextureInfo>(stream.SetPosition(0xA0 + i * 0x20));

            // Get Mesh Sections.
            UInt16 VertCnt = 0xFFFF;

            while(VertCnt != 0)
            {
                MeshSection meshSec = new MeshSection();
                MeshSectionOptional1 meshSecOpt1 = new MeshSectionOptional1();
                MeshSectionOptional2 meshSecOpt2 = new MeshSectionOptional2();

                meshSec = BinaryMapping.ReadObject<MeshSection>(stream);

                if (pmo.header.SkeletonOffset != 0) meshSecOpt1 = BinaryMapping.ReadObject<MeshSectionOptional1>(stream);
                if (GetBitFieldRange(meshSec.VertexFlags, 24, 1)  == 1) meshSecOpt2 = BinaryMapping.ReadObject<MeshSectionOptional2>(stream);

                pmo.meshSection.Add(meshSec);
                pmo.meshSectionOpt1.Add(meshSecOpt1);
                pmo.meshSectionOpt2.Add(meshSecOpt2);
                VertCnt = meshSec.VertexCount;
            }

            // Get Vertices
            for(ushort v = 0; v < pmo.meshSection.Count; v++)
            {
                CoordinateFormat TexCoordFormat = (CoordinateFormat)GetBitFieldRange(pmo.meshSection[v].VertexFlags, 0, 2);
                CoordinateFormat VertexPositionFormat = (CoordinateFormat)GetBitFieldRange(pmo.meshSection[v].VertexFlags, 7, 2);
                CoordinateFormat WeightFormat = (CoordinateFormat)GetBitFieldRange(pmo.meshSection[v].VertexFlags, 9, 2);
                UInt32 SkinningWeightsCount = GetBitFieldRange(pmo.meshSection[v].VertexFlags, 14, 3);

                // Read all skin weights.
                for (int i = 0; i < (SkinningWeightsCount + 1); i++)
                {
                    switch (WeightFormat)
                    {

                        case CoordinateFormat.NO_VERTEX:
                            break;
                        case CoordinateFormat.NORMALIZED_8_BITS:
                            pmo.textureCoordinates.Add((float)stream.ReadByte() / 127.0f);
                            pmo.textureCoordinates.Add((float)stream.ReadByte() / 127.0f);
                            break;
                        case CoordinateFormat.NORMALIZED_16_BITS:
                            pmo.textureCoordinates.Add((float)stream.ReadUInt16() / 32767.0f);
                            pmo.textureCoordinates.Add((float)stream.ReadUInt16() / 32767.0f);
                            break;
                        case CoordinateFormat.FLOAT_32_BITS:
                            pmo.textureCoordinates.Add(stream.ReadFloat());
                            pmo.textureCoordinates.Add(stream.ReadFloat());
                            break;

                    }
                }

                switch (TexCoordFormat)
                {
                    case CoordinateFormat.NO_VERTEX:
                        break;
                    case CoordinateFormat.NORMALIZED_8_BITS:
                        pmo.textureCoordinates.Add((float)stream.ReadByte() / 127.0f);
                        pmo.textureCoordinates.Add((float)stream.ReadByte() / 127.0f);
                        break;
                    case CoordinateFormat.NORMALIZED_16_BITS:
                        pmo.textureCoordinates.Add((float)stream.ReadUInt16() / 32767.0f);
                        pmo.textureCoordinates.Add((float)stream.ReadUInt16() / 32767.0f);
                        break;
                    case CoordinateFormat.FLOAT_32_BITS:
                        pmo.textureCoordinates.Add(stream.ReadFloat());
                        pmo.textureCoordinates.Add(stream.ReadFloat());
                        break;
                }

                switch (VertexPositionFormat)
                {
                    case CoordinateFormat.NO_VERTEX:
                        break;
                    case CoordinateFormat.NORMALIZED_8_BITS:
                        pmo.vertices.Add((float)Convert.ToSByte(stream.ReadByte()) / 127.0f);
                        pmo.vertices.Add((float)Convert.ToSByte(stream.ReadByte()) / 127.0f);
                        pmo.vertices.Add((float)Convert.ToSByte(stream.ReadByte()) / 127.0f);
                        break;
                    case CoordinateFormat.NORMALIZED_16_BITS:
                        pmo.vertices.Add((float)stream.ReadInt16() / 32767.0f);
                        pmo.vertices.Add((float)stream.ReadInt16() / 32767.0f);
                        pmo.vertices.Add((float)stream.ReadInt16() / 32767.0f);
                        break;
                    case CoordinateFormat.FLOAT_32_BITS:
                        pmo.vertices.Add(stream.ReadFloat());
                        pmo.vertices.Add(stream.ReadFloat());
                        pmo.vertices.Add(stream.ReadFloat());
                        break;
                }
            }

            return pmo;
        }

        public static bool IsValid(Stream stream) =>
            stream.Length >= 4 &&
            new BinaryReader(stream.SetPosition(0)).ReadUInt32() == MagicCode;
    }
}
