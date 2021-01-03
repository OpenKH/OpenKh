using OpenKh.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
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
            [Data(Count = 32)] public float[] BoundingBox { get; set; }
        }

        public class TextureInfo
        {
            [Data] public UInt32 TextureOffset { get; set; }
            [Data(Count = 12)] public string TextureName { get; set; }
            [Data(Count = 4)] public UInt32[] Unkown { get; set; }
        }

        public class MeshSection
        {
            [Data] public ushort VertexCount { get; set; }
            [Data] public byte TextureID { get; set; }
            [Data] public byte VertexSize { get; set; } // In bytes.
            [Data] public UInt32 VertexFlags { get; set; }
            [Data] public byte Unknown1 { get; set; }
            [Data] public byte TriangleStripCount { get; set; }
            [Data(Count = 2)] public byte[] Unknown2 { get; set; }
        }

        // Fields starting with _ have a temporary name.
        public class SkeletonHeader
        {
            [Data] public UInt32 MagicValue { get; set; }
            [Data] public UInt32 Unknown { get; set; }
            [Data] public UInt32 JointCount { get; set; }
            [Data] public ushort _SkinnedJoints { get; set; }
            [Data] public ushort _SkinningStartIndex { get; set; }
        }

        public class JointData
        {
            [Data] public ushort JointIndex { get; set; }
            [Data] public ushort Padding { get; set; }
            [Data] public ushort ParentJointIndex { get; set; }
            [Data] public ushort Padding2 { get; set; }
            [Data] public UInt32 _SkinningIndex { get; set; }
            [Data] public UInt32 Padding3 { get; set; }
            [Data(Count = 16)] public string JointName { get; set; }
            [Data(Count = 16)] public float[] Transform { get; set; }
            [Data(Count = 16)] public float[] InverseTransform { get; set; }
        }

        public class MeshSectionOptional1
        {
            [Data(Count = 8)] public byte[] SectionBoneIndices { get; set; } // Only present if header.SkeletonOffset != 0
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

        public static VertexFlags GetFlags(MeshSection meshSec)
        {
            VertexFlags flags = new VertexFlags();
            flags.TextureCoordinateFormat = (CoordinateFormat)GetBitFieldRange(meshSec.VertexFlags, 0, 2);
            flags.ColorFormat = (ColorFormat)GetBitFieldRange(meshSec.VertexFlags, 2, 3);
            flags.NormalFormat = (CoordinateFormat)GetBitFieldRange(meshSec.VertexFlags, 5, 2);
            flags.PositionFormat = (CoordinateFormat)GetBitFieldRange(meshSec.VertexFlags, 7, 2);
            flags.WeightFormat = (CoordinateFormat)GetBitFieldRange(meshSec.VertexFlags, 9, 2);
            flags.IndicesFormat = (byte)GetBitFieldRange(meshSec.VertexFlags, 11, 2);
            flags.Unused1 = (byte)GetBitFieldRange(meshSec.VertexFlags, 13, 1) == 1;
            flags.SkinningWeightsCount = (byte)GetBitFieldRange(meshSec.VertexFlags, 14, 3);
            flags.Unused2 = (byte)GetBitFieldRange(meshSec.VertexFlags, 17, 1) == 1;
            flags.MorphWeightsCount = (byte)GetBitFieldRange(meshSec.VertexFlags, 18, 3);
            flags.Unused3 = (byte)GetBitFieldRange(meshSec.VertexFlags, 21, 2);
            flags.SkipTransformPipeline = (byte)GetBitFieldRange(meshSec.VertexFlags, 23, 1) == 1;
            flags.UniformDiffuseFlag = (byte)GetBitFieldRange(meshSec.VertexFlags, 24, 1) == 1;
            flags.Unknown1 = (byte)GetBitFieldRange(meshSec.VertexFlags, 25, 3);
            flags.Primitive = (PrimitiveType)GetBitFieldRange(meshSec.VertexFlags, 28, 4);

            return flags;
        }

        public static UInt32 GetBitFieldRange(UInt32 value, int start = 0, int length = 1)
        {
            UInt32 bit = value << 32 - (start+length);
            bit >>= 32 - length;
            return bit;
        }

        public static int GetVertexSize(CoordinateFormat TexCoordFormat, CoordinateFormat VertexPositionFormat, CoordinateFormat WeightFormat, ColorFormat ColorFormat, int numWeights = 0)
        {
            int vertexSize = 0;

            switch (TexCoordFormat)
            {
                case CoordinateFormat.NORMALIZED_8_BITS:
                    vertexSize += 2;
                    break;
                case CoordinateFormat.NORMALIZED_16_BITS:
                    vertexSize += 4;
                    break;
                case CoordinateFormat.FLOAT_32_BITS:
                    vertexSize += 8;
                    break;
            }

            switch (VertexPositionFormat)
            {
                case CoordinateFormat.NORMALIZED_8_BITS:
                    vertexSize += 3;
                    break;
                case CoordinateFormat.NORMALIZED_16_BITS:
                    vertexSize += 6;
                    break;
                case CoordinateFormat.FLOAT_32_BITS:
                    vertexSize += 12;
                    break;
            }

            switch (WeightFormat)
            {
                case CoordinateFormat.NORMALIZED_8_BITS:
                        vertexSize += (numWeights + 1);
                    break;
                case CoordinateFormat.NORMALIZED_16_BITS:
                        vertexSize += (numWeights + 1) * 2;
                    break;
                case CoordinateFormat.FLOAT_32_BITS:
                        vertexSize += (numWeights + 1) * 4;
                    break;
            }

            switch (ColorFormat)
            {
                case ColorFormat.BGR_5650_16BITS:
                    vertexSize += 2;
                    break;
                case ColorFormat.ABGR_5551_16BITS:
                    vertexSize += 2;
                    break;
                case ColorFormat.ABGR_4444_16BITS:
                    vertexSize += 2;
                    break;
                case ColorFormat.ABGR_8888_32BITS:
                    vertexSize += 4;
                    break;
            }

            return vertexSize;
        }

        public class MeshChunks
        {
            public int TextureID;
            public List<Vector3> vertices;
            public List<int> Indices;
            public List<Vector4> colors;
            public List<Vector2> textureCoordinates;

            public MeshChunks()
            {
                TextureID = 0;
                vertices = new List<Vector3>();
                Indices = new List<int>();
                colors = new List<Vector4>();
                textureCoordinates = new List<Vector2>();
            }
        }

        public Header header { get; set; }
        public TextureInfo[] textureInfo { get; set; }
        public List<MeshSection> meshSection = new List<MeshSection>();
        public List<MeshSectionOptional1> meshSectionOpt1 = new List<MeshSectionOptional1>();
        public List<MeshSectionOptional2> meshSectionOpt2 = new List<MeshSectionOptional2>();
        public List<float> jointWeights = new List<float>();
        public List<Vector2> textureCoordinates = new List<Vector2>();
        public List<Vector4> colors = new List<Vector4>();
        public List<Vector3> vertices = new List<Vector3>();
        public List<int> Indices = new List<int>();
        public List<byte[]> Textures = new List<byte[]>();
        public List<byte> TextureIndices = new List<byte>();
        public SkeletonHeader skeletonHeader { get; set; }
        public JointData[] jointList;

        public List<MeshChunks> Meshes = new List<MeshChunks>();

        public static Pmo Read(Stream stream)
        {
            Pmo pmo = new Pmo
            {
                header = BinaryMapping.ReadObject<Header>(stream.SetPosition(0))
            };

            pmo.textureInfo = new TextureInfo[pmo.header.TextureCount];
            for (ushort i = 0; i < pmo.header.TextureCount; i++) pmo.textureInfo[i] = BinaryMapping.ReadObject<TextureInfo>(stream);

            // Read all data
            UInt16 VertCnt = 0xFFFF;
            int VertNumber = 0;
            int meshSecCount = 0;

            // Mesh body 1.
            while(VertCnt > 0)
            {
                MeshChunks meshChunk = new MeshChunks();
                MeshSection meshSec = new MeshSection();
                MeshSectionOptional1 meshSecOpt1 = new MeshSectionOptional1();
                MeshSectionOptional2 meshSecOpt2 = new MeshSectionOptional2();
                UInt16[] TriangleStripValues = new UInt16[0];
                meshSecCount++;

                long positionBeforeHeader = stream.Position;

                meshSec = BinaryMapping.ReadObject<MeshSection>(stream);
                meshChunk.TextureID = meshSec.TextureID;
                VertexFlags flags = GetFlags(meshSec);

                if (meshSec.VertexCount <= 0) break;
                bool isColorFlagRisen = flags.UniformDiffuseFlag;

                if (pmo.header.SkeletonOffset != 0) meshSecOpt1 = BinaryMapping.ReadObject<MeshSectionOptional1>(stream);
                if (isColorFlagRisen) meshSecOpt2 = BinaryMapping.ReadObject<MeshSectionOptional2>(stream);
                if (meshSec.TriangleStripCount > 0)
                {
                    TriangleStripValues = new UInt16[meshSec.TriangleStripCount];
                    for(int i = 0; i < meshSec.TriangleStripCount; i++)
                    {
                        TriangleStripValues[i] = stream.ReadUInt16();
                    }
                }

                pmo.meshSection.Add(meshSec);
                pmo.meshSectionOpt1.Add(meshSecOpt1);
                pmo.meshSectionOpt2.Add(meshSecOpt2);

                // Get Vertices
                CoordinateFormat TexCoordFormat = flags.TextureCoordinateFormat;
                CoordinateFormat VertexPositionFormat = flags.PositionFormat;
                CoordinateFormat WeightFormat = flags.WeightFormat;
                ColorFormat ColorFormat = flags.ColorFormat;
                UInt32 SkinningWeightsCount = flags.SkinningWeightsCount;
                BinaryReader r = new BinaryReader(stream);

                int currentVertexSize = GetVertexSize(TexCoordFormat, VertexPositionFormat, WeightFormat, ColorFormat, (int)SkinningWeightsCount);
                int vertexSizeDifference = meshSec.VertexSize - currentVertexSize;
                long positionAfterHeader = stream.Position;

                if (meshSec.TriangleStripCount > 0 || flags.Primitive == PrimitiveType.PRIMITIVE_TRIANGLE_STRIP)
                {
                    int vertInd = 0;
                    for(int p = 0; p < meshSec.TriangleStripCount; p++)
                    {
                        for(int s = 0; s < (TriangleStripValues[p] - 2); s++)
                        {
                            if(s % 2 == 0)
                            {
                                meshChunk.Indices.Add(vertInd + s + 0);
                                meshChunk.Indices.Add(vertInd + s + 1);
                                meshChunk.Indices.Add(vertInd + s + 2);
                            }
                            else
                            {
                                meshChunk.Indices.Add(vertInd + s + 1);
                                meshChunk.Indices.Add(vertInd + s + 0);
                                meshChunk.Indices.Add(vertInd + s + 2);
                            }
                        }

                        VertNumber += TriangleStripValues[p];
                        vertInd += TriangleStripValues[p];
                    }
                }

                for (int v = 0; v < meshSec.VertexCount; v++)
                {
                    int vertDiff = vertexSizeDifference;
                    long vertexStartPos = stream.Position;
                    int vertexIncreaseAmount = 0;

                    for (int i = 0; i < (SkinningWeightsCount + 1); i++)
                    {
                        switch (WeightFormat)
                        {
                            case CoordinateFormat.NORMALIZED_8_BITS:
                                pmo.jointWeights.Add(stream.ReadByte() / 127.0f);
                                break;
                            case CoordinateFormat.NORMALIZED_16_BITS:
                                pmo.jointWeights.Add(stream.ReadUInt16() / 32767.0f);
                                break;
                            case CoordinateFormat.FLOAT_32_BITS:
                                pmo.jointWeights.Add(stream.ReadFloat());
                                break;
                        }
                    }

                    Vector2 currentTexCoord;

                    switch (TexCoordFormat)
                    {
                        case CoordinateFormat.NORMALIZED_8_BITS:
                            currentTexCoord.X = stream.ReadByte() / 127.0f;
                            currentTexCoord.Y = stream.ReadByte() / 127.0f;
                            meshChunk.textureCoordinates.Add(currentTexCoord);
                            break;
                        case CoordinateFormat.NORMALIZED_16_BITS:
                            vertexIncreaseAmount = ((0x2 - (Convert.ToInt32(stream.Position - vertexStartPos) & 0x1)) & 0x1);
                            stream.Seek(vertexIncreaseAmount, SeekOrigin.Current);
                            vertDiff -= vertexIncreaseAmount;

                            currentTexCoord.X = stream.ReadUInt16() / 32767.0f;
                            currentTexCoord.Y = stream.ReadUInt16() / 32767.0f;
                            meshChunk.textureCoordinates.Add(currentTexCoord);
                            break;
                        case CoordinateFormat.FLOAT_32_BITS:
                            vertexIncreaseAmount = ((0x4 - (Convert.ToInt32(stream.Position - vertexStartPos) & 0x3)) & 0x3);
                            stream.Seek(vertexIncreaseAmount, SeekOrigin.Current);
                            vertDiff -= vertexIncreaseAmount;

                            currentTexCoord.X = stream.ReadFloat();
                            currentTexCoord.Y = stream.ReadFloat();
                            meshChunk.textureCoordinates.Add(currentTexCoord);
                            break;
                    }

                    switch (ColorFormat)
                    {
                        case Pmo.ColorFormat.NO_COLOR:
                            meshChunk.colors.Add(new Vector4(0, 0, 0, 0));
                            break;
                        case Pmo.ColorFormat.BGR_5650_16BITS:
                            stream.ReadUInt16();
                            break;
                        case Pmo.ColorFormat.ABGR_5551_16BITS:
                            stream.ReadUInt16();
                            break;
                        case Pmo.ColorFormat.ABGR_4444_16BITS:
                            stream.ReadUInt16();
                            break;
                        case Pmo.ColorFormat.ABGR_8888_32BITS:
                            vertexIncreaseAmount = ((0x4 - (Convert.ToInt32(stream.Position - vertexStartPos) & 0x3)) & 0x3);
                            stream.Seek(vertexIncreaseAmount, SeekOrigin.Current);
                            vertDiff -= vertexIncreaseAmount;

                            meshChunk.colors.Add(new Vector4(stream.ReadByte(), stream.ReadByte(), stream.ReadByte(), stream.ReadByte()));
                            break;
                    }

                    Vector3 currentVertex;

                    // Handle triangles and triangle strips.
                    switch (VertexPositionFormat)
                    {
                        case CoordinateFormat.NORMALIZED_8_BITS:
                            currentVertex.X = r.ReadSByte() / 127.0f * pmo.header.ModelScale;
                            currentVertex.Y = r.ReadSByte() / 127.0f * pmo.header.ModelScale;
                            currentVertex.Z = r.ReadSByte() / 127.0f * pmo.header.ModelScale;
                            meshChunk.vertices.Add(currentVertex);
                            break;
                        case CoordinateFormat.NORMALIZED_16_BITS:
                            vertexIncreaseAmount = ((0x2 - (Convert.ToInt32(stream.Position - vertexStartPos) & 0x1)) & 0x1);
                            stream.Seek(vertexIncreaseAmount, SeekOrigin.Current);
                            vertDiff -= vertexIncreaseAmount;

                            currentVertex.X = (float)stream.ReadInt16() / 32767.0f * pmo.header.ModelScale;
                            currentVertex.Y = (float)stream.ReadInt16() / 32767.0f * pmo.header.ModelScale;
                            currentVertex.Z = (float)stream.ReadInt16() / 32767.0f * pmo.header.ModelScale;
                            meshChunk.vertices.Add(currentVertex);
                            break;
                        case CoordinateFormat.FLOAT_32_BITS:
                            vertexIncreaseAmount = ((0x4 - (Convert.ToInt32(stream.Position - vertexStartPos) & 0x3)) & 0x3);
                            stream.Seek(vertexIncreaseAmount, SeekOrigin.Current);
                            vertDiff -= vertexIncreaseAmount;

                            currentVertex.X = stream.ReadFloat() * pmo.header.ModelScale;
                            currentVertex.Y = stream.ReadFloat() * pmo.header.ModelScale;
                            currentVertex.Z = stream.ReadFloat() * pmo.header.ModelScale;
                            meshChunk.vertices.Add(currentVertex);
                            break;
                    }

                    stream.Seek(vertDiff, SeekOrigin.Current);

                    pmo.TextureIndices.Add(meshSec.TextureID);

                    if (meshSec.TriangleStripCount <= 0)
                    {
                        meshChunk.Indices.Add(v);
                        VertNumber++;
                    }
                }

                uint remainder = ((uint)stream.Position % 4);

                if (remainder != 0)
                {
                    stream.Seek(remainder, SeekOrigin.Current);
                }

                VertCnt = meshSec.VertexCount;
                pmo.Meshes.Add(meshChunk);
            }

            stream.Seek(pmo.header.MeshOffset1, SeekOrigin.Begin);
            VertCnt = 0xFFFF;

            // Mesh body 2.
            while (VertCnt > 0)
            {
                MeshChunks meshChunk = new MeshChunks();
                MeshSection meshSec = new MeshSection();
                MeshSectionOptional1 meshSecOpt1 = new MeshSectionOptional1();
                MeshSectionOptional2 meshSecOpt2 = new MeshSectionOptional2();
                UInt16[] TriangleStripValues = new UInt16[0];
                meshSecCount++;

                long positionBeforeHeader = stream.Position;

                meshSec = BinaryMapping.ReadObject<MeshSection>(stream);
                meshChunk.TextureID = meshSec.TextureID;
                VertexFlags flags = GetFlags(meshSec);

                if (meshSec.VertexCount <= 0)
                    break;
                bool isColorFlagRisen = flags.UniformDiffuseFlag;

                if (pmo.header.SkeletonOffset != 0)
                    meshSecOpt1 = BinaryMapping.ReadObject<MeshSectionOptional1>(stream);
                if (isColorFlagRisen)
                    meshSecOpt2 = BinaryMapping.ReadObject<MeshSectionOptional2>(stream);
                if (meshSec.TriangleStripCount > 0)
                {
                    TriangleStripValues = new UInt16[meshSec.TriangleStripCount];
                    for (int i = 0; i < meshSec.TriangleStripCount; i++)
                    {
                        TriangleStripValues[i] = stream.ReadUInt16();
                    }
                }

                pmo.meshSection.Add(meshSec);
                pmo.meshSectionOpt1.Add(meshSecOpt1);
                pmo.meshSectionOpt2.Add(meshSecOpt2);

                // Get Vertices
                CoordinateFormat TexCoordFormat = flags.TextureCoordinateFormat;
                CoordinateFormat VertexPositionFormat = flags.PositionFormat;
                CoordinateFormat WeightFormat = flags.WeightFormat;
                ColorFormat ColorFormat = flags.ColorFormat;
                UInt32 SkinningWeightsCount = flags.SkinningWeightsCount;
                BinaryReader r = new BinaryReader(stream);

                int currentVertexSize = GetVertexSize(TexCoordFormat, VertexPositionFormat, WeightFormat, ColorFormat, (int)SkinningWeightsCount);
                int vertexSizeDifference = meshSec.VertexSize - currentVertexSize;
                long positionAfterHeader = stream.Position;

                if (meshSec.TriangleStripCount > 0 || flags.Primitive == PrimitiveType.PRIMITIVE_TRIANGLE_STRIP)
                {
                    int vertInd = 0;
                    for (int p = 0; p < meshSec.TriangleStripCount; p++)
                    {
                        for (int s = 0; s < (TriangleStripValues[p] - 2); s++)
                        {
                            if (s % 2 == 0)
                            {
                                meshChunk.Indices.Add(vertInd + s);
                                meshChunk.Indices.Add(vertInd + s + 1);
                                meshChunk.Indices.Add(vertInd + s + 2);
                            }
                            else
                            {
                                meshChunk.Indices.Add(vertInd + s + 2);
                                meshChunk.Indices.Add(vertInd + s + 1);
                                meshChunk.Indices.Add(vertInd + s);
                            }
                        }

                        VertNumber += TriangleStripValues[p];
                        vertInd += TriangleStripValues[p];
                    }
                }

                for (int v = 0; v < meshSec.VertexCount; v++)
                {
                    int vertDiff = vertexSizeDifference;
                    long vertexStartPos = stream.Position;
                    int vertexIncreaseAmount = 0;

                    for (int i = 0; i < (SkinningWeightsCount + 1); i++)
                    {
                        switch (WeightFormat)
                        {
                            case CoordinateFormat.NORMALIZED_8_BITS:
                                pmo.jointWeights.Add(stream.ReadByte() / 127.0f);
                                break;
                            case CoordinateFormat.NORMALIZED_16_BITS:
                                pmo.jointWeights.Add(stream.ReadUInt16() / 32767.0f);
                                break;
                            case CoordinateFormat.FLOAT_32_BITS:
                                pmo.jointWeights.Add(stream.ReadFloat());
                                break;
                        }
                    }

                    Vector2 currentTexCoord;

                    switch (TexCoordFormat)
                    {
                        case CoordinateFormat.NORMALIZED_8_BITS:
                            currentTexCoord.X = stream.ReadByte() / 127.0f;
                            currentTexCoord.Y = stream.ReadByte() / 127.0f;
                            meshChunk.textureCoordinates.Add(currentTexCoord);
                            break;
                        case CoordinateFormat.NORMALIZED_16_BITS:
                            vertexIncreaseAmount = ((0x2 - (Convert.ToInt32(stream.Position - vertexStartPos) & 0x1)) & 0x1);
                            stream.Seek(vertexIncreaseAmount, SeekOrigin.Current);
                            vertDiff -= vertexIncreaseAmount;

                            currentTexCoord.X = stream.ReadUInt16() / 32767.0f;
                            currentTexCoord.Y = stream.ReadUInt16() / 32767.0f;
                            meshChunk.textureCoordinates.Add(currentTexCoord);
                            break;
                        case CoordinateFormat.FLOAT_32_BITS:
                            vertexIncreaseAmount = ((0x4 - (Convert.ToInt32(stream.Position - vertexStartPos) & 0x3)) & 0x3);
                            stream.Seek(vertexIncreaseAmount, SeekOrigin.Current);
                            vertDiff -= vertexIncreaseAmount;

                            currentTexCoord.X = stream.ReadFloat();
                            currentTexCoord.Y = stream.ReadFloat();
                            meshChunk.textureCoordinates.Add(currentTexCoord);
                            break;
                    }

                    switch (ColorFormat)
                    {
                        case Pmo.ColorFormat.NO_COLOR:
                            meshChunk.colors.Add(new Vector4(0, 0, 0, 0));
                            break;
                        case Pmo.ColorFormat.BGR_5650_16BITS:
                            stream.ReadUInt16();
                            break;
                        case Pmo.ColorFormat.ABGR_5551_16BITS:
                            stream.ReadUInt16();
                            break;
                        case Pmo.ColorFormat.ABGR_4444_16BITS:
                            stream.ReadUInt16();
                            break;
                        case Pmo.ColorFormat.ABGR_8888_32BITS:
                            vertexIncreaseAmount = ((0x4 - (Convert.ToInt32(stream.Position - vertexStartPos) & 0x3)) & 0x3);
                            stream.Seek(vertexIncreaseAmount, SeekOrigin.Current);
                            vertDiff -= vertexIncreaseAmount;

                            meshChunk.colors.Add(new Vector4(stream.ReadByte(), stream.ReadByte(), stream.ReadByte(), stream.ReadByte()));
                            break;
                    }

                    Vector3 currentVertex;

                    // Handle triangles and triangle strips.
                    switch (VertexPositionFormat)
                    {
                        case CoordinateFormat.NORMALIZED_8_BITS:
                            currentVertex.X = r.ReadSByte() / 127.0f * pmo.header.ModelScale;
                            currentVertex.Y = r.ReadSByte() / 127.0f * pmo.header.ModelScale;
                            currentVertex.Z = r.ReadSByte() / 127.0f * pmo.header.ModelScale;
                            meshChunk.vertices.Add(currentVertex);
                            break;
                        case CoordinateFormat.NORMALIZED_16_BITS:
                            vertexIncreaseAmount = ((0x2 - (Convert.ToInt32(stream.Position - vertexStartPos) & 0x1)) & 0x1);
                            stream.Seek(vertexIncreaseAmount, SeekOrigin.Current);
                            vertDiff -= vertexIncreaseAmount;

                            currentVertex.X = (float)stream.ReadInt16() / 32767.0f * pmo.header.ModelScale;
                            currentVertex.Y = (float)stream.ReadInt16() / 32767.0f * pmo.header.ModelScale;
                            currentVertex.Z = (float)stream.ReadInt16() / 32767.0f * pmo.header.ModelScale;
                            meshChunk.vertices.Add(currentVertex);
                            break;
                        case CoordinateFormat.FLOAT_32_BITS:
                            vertexIncreaseAmount = ((0x4 - (Convert.ToInt32(stream.Position - vertexStartPos) & 0x3)) & 0x3);
                            stream.Seek(vertexIncreaseAmount, SeekOrigin.Current);
                            vertDiff -= vertexIncreaseAmount;

                            currentVertex.X = stream.ReadFloat() * pmo.header.ModelScale;
                            currentVertex.Y = stream.ReadFloat() * pmo.header.ModelScale;
                            currentVertex.Z = stream.ReadFloat() * pmo.header.ModelScale;
                            meshChunk.vertices.Add(currentVertex);
                            break;
                    }

                    stream.Seek(vertDiff, SeekOrigin.Current);

                    pmo.TextureIndices.Add(meshSec.TextureID);

                    if (meshSec.TriangleStripCount <= 0)
                    {
                        meshChunk.Indices.Add(v);
                        VertNumber++;
                    }
                }

                uint remainder = ((uint)stream.Position % 4);

                if (remainder != 0)
                {
                    stream.Seek(remainder, SeekOrigin.Current);
                }

                VertCnt = meshSec.VertexCount;
                pmo.Meshes.Add(meshChunk);
            }

            // Read textures.
            for (int i = 0; i < pmo.textureInfo.Length; i++)
            {
                stream.Seek(pmo.textureInfo[i].TextureOffset + 0x10, SeekOrigin.Begin);
                uint tm2size = stream.ReadUInt32() + 0x10;
                stream.Seek(pmo.textureInfo[i].TextureOffset, SeekOrigin.Begin);

                byte[] TextureBuffer = new byte[tm2size];
                TextureBuffer = stream.ReadBytes((int)tm2size);

                pmo.Textures.Add(TextureBuffer);
            }

            // Read Skeleton.
            stream.Seek(pmo.header.SkeletonOffset, SeekOrigin.Begin);
            pmo.skeletonHeader = BinaryMapping.ReadObject<SkeletonHeader>(stream);
            pmo.jointList = new JointData[pmo.skeletonHeader.JointCount];
            for(int j = 0; j < pmo.skeletonHeader.JointCount; j++)
            {
                pmo.jointList[j] = BinaryMapping.ReadObject<JointData>(stream);
            }

            return pmo;
        }

        public static bool IsValid(Stream stream) =>
            stream.Length >= 4 &&
            new BinaryReader(stream.SetPosition(0)).ReadUInt32() == MagicCode;
    }
}
