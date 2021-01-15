using OpenKh.Common;
using OpenKh.Common.Utils;
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
            [Data] public int MeshNumber { get; set; }
            [Data] public MeshSection SectionInfo { get; set; }
            [Data] public MeshSectionOptional1 SectionInfo_opt1 { get; set; }
            [Data] public MeshSectionOptional2 SectionInfo_opt2 { get; set; }
            [Data] public UInt16[] TriangleStripValues { get; set; }
            [Data] public int TextureID { get; set; }
            [Data] public List<float> jointWeights { get; set; }
            [Data] public List<Vector2> textureCoordinates { get; set; }
            [Data] public List<Vector4> colors { get; set; }
            [Data] public List<Vector3> vertices { get; set; }
            [Data] public List<int> Indices { get; set; }

            public MeshChunks()
            {
                MeshNumber = 0;
                TriangleStripValues = new UInt16[0];
                TextureID = 0;
                jointWeights = new List<float>();
                textureCoordinates = new List<Vector2>();
                colors = new List<Vector4>();
                vertices = new List<Vector3>();
                Indices = new List<int>();
            }
        }

        // PMO Header.
        public Header header { get; set; }
        // Data block for textures. The order will reflect their texture index.
        public TextureInfo[] textureInfo { get; set; }
        // Texture data blobs.
        public List<byte[]> texturesData = new List<byte[]>();
        // Header of the skeleton.
        public SkeletonHeader skeletonHeader { get; set; }
        // Joints present in the skeleton.
        public JointData[] jointList;

        public List<MeshChunks> Meshes = new List<MeshChunks>();

        public int PMO_StartPosition = 0;

        public static void ReadHeader(Stream stream, Pmo pmo)
        {
            pmo.header = BinaryMapping.ReadObject<Header>(stream);
        }

        public static void ReadTextureSection(Stream stream, Pmo pmo)
        {
            pmo.textureInfo = new TextureInfo[pmo.header.TextureCount];
            for (ushort i = 0; i < pmo.header.TextureCount; i++)
                pmo.textureInfo[i] = BinaryMapping.ReadObject<TextureInfo>(stream);
        }

        public static void ReadMeshData(Stream stream, Pmo pmo, int MeshNumber = 0)
        {
            // Go to mesh position.
            if(MeshNumber == 0) stream.Seek(pmo.PMO_StartPosition + pmo.header.MeshOffset0, SeekOrigin.Begin);
            else                stream.Seek(pmo.PMO_StartPosition + pmo.header.MeshOffset1, SeekOrigin.Begin);

            UInt16 VertCnt = 0xFFFF;

            while (VertCnt > 0)
            {
                MeshChunks meshChunk = new MeshChunks();
                meshChunk.MeshNumber = MeshNumber;

                meshChunk.SectionInfo = BinaryMapping.ReadObject<MeshSection>(stream);

                // Exit if Vertex Count is zero.
                if (meshChunk.SectionInfo.VertexCount <= 0)
                    break;

                meshChunk.TextureID = meshChunk.SectionInfo.TextureID;
                VertexFlags flags = GetFlags(meshChunk.SectionInfo);
                
                bool isColorFlagRisen = flags.UniformDiffuseFlag;

                if (pmo.header.SkeletonOffset != 0)
                    meshChunk.SectionInfo_opt1 = BinaryMapping.ReadObject<MeshSectionOptional1>(stream);
                if (isColorFlagRisen)
                    meshChunk.SectionInfo_opt2 = BinaryMapping.ReadObject<MeshSectionOptional2>(stream);
                if (meshChunk.SectionInfo.TriangleStripCount > 0)
                {
                    meshChunk.TriangleStripValues = new UInt16[meshChunk.SectionInfo.TriangleStripCount];
                    for (int i = 0; i < meshChunk.SectionInfo.TriangleStripCount; i++)
                    {
                        meshChunk.TriangleStripValues[i] = stream.ReadUInt16();
                    }
                }

                // Get Formats.
                CoordinateFormat TexCoordFormat = flags.TextureCoordinateFormat;
                CoordinateFormat VertexPositionFormat = flags.PositionFormat;
                CoordinateFormat WeightFormat = flags.WeightFormat;
                ColorFormat ColorFormat = flags.ColorFormat;
                UInt32 SkinningWeightsCount = flags.SkinningWeightsCount;
                BinaryReader r = new BinaryReader(stream);
                long positionAfterHeader = stream.Position;

                if (meshChunk.SectionInfo.TriangleStripCount > 0)
                {
                    int vertInd = 0;
                    for (int p = 0; p < meshChunk.SectionInfo.TriangleStripCount; p++)
                    {
                        for (int s = 0; s < (meshChunk.TriangleStripValues[p] - 2); s++)
                        {
                            if (s % 2 == 0)
                            {
                                meshChunk.Indices.Add(vertInd + s + 0);
                                meshChunk.Indices.Add(vertInd + s + 1);
                                meshChunk.Indices.Add(vertInd + s + 2);
                            }
                            else
                            {
                                meshChunk.Indices.Add(vertInd + s + 0);
                                meshChunk.Indices.Add(vertInd + s + 2);
                                meshChunk.Indices.Add(vertInd + s + 1);
                            }
                        }

                        vertInd += meshChunk.TriangleStripValues[p];
                    }
                }
                else
                {
                    if (flags.Primitive == PrimitiveType.PRIMITIVE_TRIANGLE_STRIP)
                    {
                        for (int s = 0; s < (meshChunk.SectionInfo.VertexCount - 2); s++)
                        {
                            if (s % 2 == 0)
                            {
                                meshChunk.Indices.Add(s + 0);
                                meshChunk.Indices.Add(s + 1);
                                meshChunk.Indices.Add(s + 2);
                            }
                            else
                            {
                                meshChunk.Indices.Add(s + 1);
                                meshChunk.Indices.Add(s + 0);
                                meshChunk.Indices.Add(s + 2);
                            }
                        }
                    }
                }

                for (int v = 0; v < meshChunk.SectionInfo.VertexCount; v++)
                {
                    long vertexStartPos = stream.Position;
                    int vertexIncreaseAmount = 0;

                    if(pmo.header.SkeletonOffset != 0 && WeightFormat != CoordinateFormat.NO_VERTEX)
                    {
                        for (int i = 0; i < (SkinningWeightsCount + 1); i++)
                        {
                            switch (WeightFormat)
                            {
                                case CoordinateFormat.NORMALIZED_8_BITS:
                                    meshChunk.jointWeights.Add(stream.ReadByte() / 127.0f);
                                    break;
                                case CoordinateFormat.NORMALIZED_16_BITS:
                                    meshChunk.jointWeights.Add(stream.ReadUInt16() / 32767.0f);
                                    break;
                                case CoordinateFormat.FLOAT_32_BITS:
                                    meshChunk.jointWeights.Add(stream.ReadFloat());
                                    break;
                                case CoordinateFormat.NO_VERTEX:
                                    break;
                            }
                        }
                    }

                    Vector2 currentTexCoord = new Vector2(0, 0);

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

                            currentTexCoord.X = stream.ReadUInt16() / 32767.0f;
                            currentTexCoord.Y = stream.ReadUInt16() / 32767.0f;
                            meshChunk.textureCoordinates.Add(currentTexCoord);
                            break;
                        case CoordinateFormat.FLOAT_32_BITS:
                            vertexIncreaseAmount = ((0x4 - (Convert.ToInt32(stream.Position - vertexStartPos) & 0x3)) & 0x3);
                            stream.Seek(vertexIncreaseAmount, SeekOrigin.Current);

                            currentTexCoord.X = stream.ReadFloat();
                            currentTexCoord.Y = stream.ReadFloat();
                            meshChunk.textureCoordinates.Add(currentTexCoord);
                            break;
                        case CoordinateFormat.NO_VERTEX:
                            meshChunk.textureCoordinates.Add(currentTexCoord);
                            break;
                    }

                    switch (ColorFormat)
                    {
                        case Pmo.ColorFormat.NO_COLOR:
                            meshChunk.colors.Add(new Vector4(0xFF, 0xFF, 0xFF, 0xFF));
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

                            Vector4 col;
                            col.X = stream.ReadByte();
                            col.Y = stream.ReadByte();
                            col.Z = stream.ReadByte();
                            col.W = stream.ReadByte();
                            meshChunk.colors.Add(col);
                            break;
                    }

                    Vector3 currentVertex;

                    // Handle triangles and triangle strips.
                    switch (VertexPositionFormat)
                    {
                        case CoordinateFormat.NORMALIZED_8_BITS:
                            currentVertex.X = r.ReadSByte() / 127.0f;
                            currentVertex.Y = r.ReadSByte() / 127.0f;
                            currentVertex.Z = r.ReadSByte() / 127.0f;
                            meshChunk.vertices.Add(currentVertex);
                            break;
                        case CoordinateFormat.NORMALIZED_16_BITS:
                            vertexIncreaseAmount = ((0x2 - (Convert.ToInt32(stream.Position - vertexStartPos) & 0x1)) & 0x1);
                            stream.Seek(vertexIncreaseAmount, SeekOrigin.Current);

                            currentVertex.X = (float)stream.ReadInt16() / 32767.0f;
                            currentVertex.Y = (float)stream.ReadInt16() / 32767.0f;
                            currentVertex.Z = (float)stream.ReadInt16() / 32767.0f;
                            meshChunk.vertices.Add(currentVertex);
                            break;
                        case CoordinateFormat.FLOAT_32_BITS:
                            vertexIncreaseAmount = ((0x4 - (Convert.ToInt32(stream.Position - vertexStartPos) & 0x3)) & 0x3);
                            stream.Seek(vertexIncreaseAmount, SeekOrigin.Current);

                            currentVertex.X = stream.ReadFloat();
                            currentVertex.Y = stream.ReadFloat();
                            currentVertex.Z = stream.ReadFloat();
                            meshChunk.vertices.Add(currentVertex);
                            break;
                    }

                    stream.Seek(vertexStartPos + meshChunk.SectionInfo.VertexSize, SeekOrigin.Begin);

                    if (flags.Primitive == PrimitiveType.PRIMITIVE_TRIANGLE)
                    {
                        meshChunk.Indices.Add(v);
                    }
                }

                VertCnt = meshChunk.SectionInfo.VertexCount;
                pmo.Meshes.Add(meshChunk);

                // Find position of next data chunk.
                stream.Seek(positionAfterHeader + (meshChunk.SectionInfo.VertexCount * meshChunk.SectionInfo.VertexSize), SeekOrigin.Begin);
                stream.Seek(stream.Position % 4, SeekOrigin.Current);
            }
        }

        public static Pmo Read(Stream stream)
        {
            Pmo pmo = new Pmo();
            pmo.PMO_StartPosition = (int)stream.Position;

            ReadHeader(stream, pmo);
            ReadTextureSection(stream, pmo);

            // Read all data
            if (pmo.header.MeshOffset0 != 0) ReadMeshData(stream, pmo, 0);
            if (pmo.header.MeshOffset1 != 0) ReadMeshData(stream, pmo, 1);

            // Read textures.
            for (int i = 0; i < pmo.textureInfo.Length; i++)
            {
                stream.Seek(pmo.textureInfo[i].TextureOffset + 0x10, SeekOrigin.Begin);
                uint tm2size = stream.ReadUInt32() + 0x10;
                stream.Seek(pmo.textureInfo[i].TextureOffset, SeekOrigin.Begin);

                byte[] TextureBuffer = new byte[tm2size];
                TextureBuffer = stream.ReadBytes((int)tm2size);

                pmo.texturesData.Add(TextureBuffer);
            }

            // Read Skeleton.
            if(pmo.header.SkeletonOffset != 0)
            {
                stream.Seek(pmo.PMO_StartPosition + pmo.header.SkeletonOffset, SeekOrigin.Begin);
                pmo.skeletonHeader = BinaryMapping.ReadObject<SkeletonHeader>(stream);
                pmo.jointList = new JointData[pmo.skeletonHeader.JointCount];
                for (int j = 0; j < pmo.skeletonHeader.JointCount; j++)
                {
                    pmo.jointList[j] = BinaryMapping.ReadObject<JointData>(stream);
                }
            }

            return pmo;
        }

        public static void Write(Stream stream, Pmo pmo)
        {
            stream.Position = 0;
            bool hasSwappedToSecondModel = false;
            BinaryMapping.WriteObject<Pmo.Header>(stream, pmo.header);

            for(int i = 0; i < pmo.header.TextureCount; i++) BinaryMapping.WriteObject<Pmo.TextureInfo>(stream, pmo.textureInfo[i]);

            // Write Mesh Data.
            for(int j = 0; j < pmo.Meshes.Count; j++)
            {
                if(!hasSwappedToSecondModel && pmo.Meshes[j].MeshNumber == 1)
                {
                    hasSwappedToSecondModel = true;

                    for (uint b = 0; b < 0xC; b++)
                        stream.Write((byte)0x00);

                    for (uint b = 0; stream.Position % 0x10 != 0; b++)
                        stream.Write((byte)0x00);
                }

                MeshChunks chunk = pmo.Meshes[j];

                BinaryMapping.WriteObject<Pmo.MeshSection>(stream, chunk.SectionInfo);
                if (chunk.SectionInfo_opt1 != null) BinaryMapping.WriteObject<Pmo.MeshSectionOptional1>(stream, chunk.SectionInfo_opt1);
                if (chunk.SectionInfo_opt2 != null) BinaryMapping.WriteObject<Pmo.MeshSectionOptional2>(stream, chunk.SectionInfo_opt2);
                
                if(chunk.TriangleStripValues.Length > 0)
                {
                    for(int z = 0; z < chunk.TriangleStripValues.Length; z++)
                    {
                        stream.Write((ushort)chunk.TriangleStripValues[z]);
                    }
                }

                for(int k = 0; k < pmo.Meshes[j].SectionInfo.VertexCount; k++)
                {
                    long vertexStartPos = stream.Position;
                    int vertexIncreaseAmount = 0;
                    
                    VertexFlags flags = Pmo.GetFlags(chunk.SectionInfo);

                    // Write Joints.
                    if (flags.WeightFormat != CoordinateFormat.NO_VERTEX)
                    {
                        for(int w = 0; w < flags.SkinningWeightsCount + 1; w++)
                        {
                            int currentIndex = w + (k * (flags.SkinningWeightsCount + 1));

                            switch (flags.WeightFormat)
                            {
                                case CoordinateFormat.NORMALIZED_8_BITS:
                                    stream.Write((byte)(chunk.jointWeights[currentIndex] * 127.0f));
                                    break;
                                case CoordinateFormat.NORMALIZED_16_BITS:
                                    stream.Write((byte)(chunk.jointWeights[currentIndex] * 32767.0f));
                                    break;
                                case CoordinateFormat.FLOAT_32_BITS:
                                    StreamExtensions.Write(stream, chunk.jointWeights[currentIndex]);
                                    break;
                            }
                        }
                    }

                    // Write Texture Coords.
                    switch (flags.TextureCoordinateFormat)
                    {
                        case CoordinateFormat.NORMALIZED_8_BITS:
                            stream.Write((byte)(chunk.textureCoordinates[k].X * 127.0f));
                            stream.Write((byte)(chunk.textureCoordinates[k].Y * 127.0f));
                            break;
                        case CoordinateFormat.NORMALIZED_16_BITS:
                            vertexIncreaseAmount = ((0x2 - (Convert.ToInt32(stream.Position - vertexStartPos) & 0x1)) & 0x1);
                            for (int a = 0; a < vertexIncreaseAmount; a++) stream.Write((byte)0xAB);

                            stream.Write((ushort)(chunk.textureCoordinates[k].X * 32767.0f));
                            stream.Write((ushort)(chunk.textureCoordinates[k].Y * 32767.0f));
                            break;
                        case CoordinateFormat.FLOAT_32_BITS:
                            vertexIncreaseAmount = ((0x4 - (Convert.ToInt32(stream.Position - vertexStartPos) & 0x3)) & 0x3);
                            for (int a = 0; a < vertexIncreaseAmount; a++)
                                stream.Write((byte)0xAB);

                            StreamExtensions.Write(stream, chunk.textureCoordinates[k].X);
                            StreamExtensions.Write(stream, chunk.textureCoordinates[k].Y);
                            break;
                    }

                    // Write colors.
                    switch (flags.ColorFormat)
                    {
                        case Pmo.ColorFormat.NO_COLOR:
                            break;
                        case Pmo.ColorFormat.BGR_5650_16BITS:
                            break;
                        case Pmo.ColorFormat.ABGR_5551_16BITS:
                            break;
                        case Pmo.ColorFormat.ABGR_4444_16BITS:
                            break;
                        case Pmo.ColorFormat.ABGR_8888_32BITS:
                            vertexIncreaseAmount = ((0x4 - (Convert.ToInt32(stream.Position - vertexStartPos) & 0x3)) & 0x3);
                            for (int a = 0; a < vertexIncreaseAmount; a++)
                                stream.Write((byte)0xAB);

                            stream.Write((byte)(chunk.colors[k].X));
                            stream.Write((byte)(chunk.colors[k].Y));
                            stream.Write((byte)(chunk.colors[k].Z));
                            stream.Write((byte)(chunk.colors[k].W));
                            break;
                    }

                    // Write vertices.
                    switch (flags.PositionFormat)
                    {
                        case CoordinateFormat.NORMALIZED_8_BITS:
                            StreamExtensions.Write(stream, (sbyte)(chunk.vertices[k].X * 127.0f));
                            StreamExtensions.Write(stream, (sbyte)(chunk.vertices[k].Y * 127.0f));
                            StreamExtensions.Write(stream, (sbyte)(chunk.vertices[k].Z * 127.0f));
                            break;
                        case CoordinateFormat.NORMALIZED_16_BITS:
                            vertexIncreaseAmount = ((0x2 - (Convert.ToInt32(stream.Position - vertexStartPos) & 0x1)) & 0x1);
                            for (int a = 0; a < vertexIncreaseAmount; a++)
                                stream.Write((byte)0xAB);

                            StreamExtensions.Write(stream, (short)(chunk.vertices[k].X * 32767.0f));
                            StreamExtensions.Write(stream, (short)(chunk.vertices[k].Y * 32767.0f));
                            StreamExtensions.Write(stream, (short)(chunk.vertices[k].Z * 32767.0f));
                            break;
                        case CoordinateFormat.FLOAT_32_BITS:
                            vertexIncreaseAmount = ((0x4 - (Convert.ToInt32(stream.Position - vertexStartPos) & 0x3)) & 0x3);
                            for (int a = 0; a < vertexIncreaseAmount; a++)
                                stream.Write((byte)0xAB);

                            StreamExtensions.Write(stream, chunk.vertices[k].X);
                            StreamExtensions.Write(stream, chunk.vertices[k].Y);
                            StreamExtensions.Write(stream, chunk.vertices[k].Z);
                            break;
                    }

                    int padding = ((int)vertexStartPos + chunk.SectionInfo.VertexSize) - (int)stream.Position;
                    for (int p = 0; p < padding; p++)
                        stream.Write((byte)0xAB);
                }

                // Remainder.
                uint remainder = (uint)stream.Position % 4;
                for (int p = 0; p < remainder; p++)
                    stream.Write((byte)0x00);

                if(j == (pmo.Meshes.Count - 1))
                {
                    for (uint b = 0; b < 0xC; b++)
                        stream.Write((byte)0x00);

                    for (uint b = 0; stream.Position % 0x10 != 0; b++)
                        stream.Write((byte)0x00);
                }
            }

            // Write textures.
            for(int t = 0; t < pmo.texturesData.Count; t++)
            {
                stream.Write(pmo.texturesData[t]);
            }

            if(pmo.header.SkeletonOffset != 0)
            {
                BinaryMapping.WriteObject<SkeletonHeader>(stream, pmo.skeletonHeader);

                for (int joint = 0; joint < pmo.jointList.Length; joint++)
                {
                    BinaryMapping.WriteObject<JointData>(stream, pmo.jointList[joint]);
                }
            }
        }

        // Can't be shorter than the header size.
        public static bool IsValid(Stream stream) =>
            stream.Length >= 0xA0 &&
            stream.SetPosition(0).ReadUInt32() == MagicCode;
    }
}
