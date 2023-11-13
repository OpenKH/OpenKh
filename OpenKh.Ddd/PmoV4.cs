using OpenKh.Common;
using OpenKh.Common.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using Xe.BinaryMapper;

namespace OpenKh.Ddd
{
    public class PmoV4
    {
        private static readonly IBinaryMapping Mapping =
           MappingConfiguration.DefaultConfiguration()
               .ForTypeMatrix4x4()
               .Build();

        private const UInt32 MagicCode = 0x4F4D50;

        // Same as BBS (V3)
        public class Header
        {
            [Data] public UInt32 MagicCode { get; set; }
            [Data] public byte Number { get; set; }
            [Data] public byte Group { get; set; }
            [Data] public byte Version { get; set; }
            [Data] public byte Padding1 { get; set; }
            [Data] public byte TextureCount { get; set; }
            [Data] public byte Padding2 { get; set; }
            [Data] public ushort Flag { get; set; }
            [Data] public UInt32 SkeletonOffset { get; set; }
            [Data] public UInt32 ModelOffset0 { get; set; }
            [Data] public ushort TriangleCount { get; set; }
            [Data] public ushort VertexCount { get; set; }
            [Data] public float ModelScale { get; set; }
            [Data] public UInt32 ModelOffset1 { get; set; }
            [Data(Count = 32)] public float[] BoundingBox { get; set; }
        }

        // Same as BBS (V3)
        public class TextureInfo
        {
            [Data] public uint TextureOffset { get; set; }
            [Data(Count = 12)] public string TextureName { get; set; }
            [Data] public float ScrollX { get; set; }
            [Data] public float ScrollY { get; set; }
            [Data(Count=8)] public byte[] Padding { get; set; }
        }

        // New
        public class ModelHeader
        {
            [Data] public uint MeshList0HeaderOffset { get; set; }
            [Data] public uint MeshList1HeaderOffset { get; set; }
            [Data] public uint VertexCount { get; set; }
            [Data] public uint Unknown { get; set; }
            [Data] public uint MeshList0Count { get; set; }
            [Data] public uint MeshList1Count { get; set; }
            [Data] public uint VertexDataOffset { get; set; }
        }

        // Same as BBS (V3)
        public class MeshSection
        {
            [Data] public ushort VertexCount { get; set; }
            [Data] public byte TextureID { get; set; }
            [Data] public byte VertexSize { get; set; } // In bytes.
            [Data] public int VertexFlags { get; set; }
            [Data] public byte Group { get; set; }
            [Data] public byte TriangleStripCount { get; set; }
            [Data] public ushort Attribute { get; set; }

            public bool isTriangleStrip
            {
                get {
                    return BitsUtil.Int.GetBit(VertexFlags, 30);
                }
                set {
                    BitsUtil.Int.SetBit(VertexFlags, 30, value);
                }
            }
        }

        public enum VertexAttribute
        {
            ATTRIBUTE_BLEND_NONE = 0,
            ATTRIBUTE_NOMATERIAL = 1,
            ATTRIBUTE_GLARE = 2,
            ATTRIBUTE_BACK = 4,
            ATTRIBUTE_DIVIDE = 8,
            ATTRIBUTE_TEXALPHA = 16,
            ATTRIBUTE_FLAG_SHIFT = 24,
            ATTRIBUTE_PRIM_SHIFT = 28,
            ATTRIBUTE_BLEND_SEMITRANS = 32,
            ATTRIBUTE_BLEND_ADD = 64,
            ATTRIBUTE_BLEND_SUB = 96,
            ATTRIBUTE_BLEND_MASK = 224,
            ATTRIBUTE_8 = 256,
            ATTRIBUTE_9 = 512,
            ATTRIBUTE_DROPSHADOW = 1024,
            ATTRIBUTE_ENVMAP = 2048,
            ATTRIBUTE_12 = 4096,
            ATTRIBUTE_13 = 8192,
            ATTRIBUTE_14 = 16384,
            ATTRIBUTE_15 = 32768,
            ATTRIBUTE_COLOR = 16777216,
            ATTRIBUTE_NOWEIGHT = 33554432,
        }

        // Same as BBS (V3)
        public class SkeletonHeader
        {
            [Data] public uint MagicValue { get; set; }
            [Data] public uint Padding1 { get; set; }
            [Data] public ushort BoneCount { get; set; }
            [Data] public ushort Padding2 { get; set; }
            [Data] public ushort SkinnedBoneCount { get; set; }
            [Data] public ushort nStdBone { get; set; }
        }

        // Same as BBS (V3)
        public class BoneData
        {
            [Data] public ushort BoneIndex { get; set; }
            [Data] public ushort Padding1 { get; set; }
            [Data] public ushort ParentBoneIndex { get; set; }
            [Data] public ushort Padding2 { get; set; }
            [Data] public ushort SkinnedBoneIndex { get; set; }
            [Data] public ushort Padding3 { get; set; }
            [Data] public uint Padding4 { get; set; }
            [Data(Count = 16)] public string JointName { get; set; }
            [Data] public Matrix4x4 Transform { get; set; }
            [Data] public Matrix4x4 InverseTransform { get; set; }
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

        public static UInt32 GetBitFieldRange(int value, int start = 0, int length = 1)
        {
            UInt32 bit = (uint)value << 32 - (start + length);
            bit >>= 32 - length;
            return bit;
        }

        public class WeightData
        {
            [Data] public CoordinateFormat coordFormat { get; set; }
            [Data] public List<float> weights { get; set; }
        }

        public class Model
        {
            public ModelHeader Header { get; set; }
            public List<MeshChunks> Meshes { get; set; }
        }

        public class MeshChunks
        {
            [Data] public bool IsTextureOpaque { get; set; }
            [Data] public int MeshNumber { get; set; }
            [Data] public MeshSection SectionInfo { get; set; }
            [Data] public MeshSectionOptional1 SectionInfo_opt1 { get; set; }
            [Data] public MeshSectionOptional2 SectionInfo_opt2 { get; set; }
            [Data] public UInt16[] TriangleStripValues { get; set; }
            [Data] public int TextureID { get; set; }
            [Data] public List<WeightData> jointWeights { get; set; }
            [Data] public List<Vector2> textureCoordinates { get; set; }
            [Data] public List<Vector4> colors { get; set; }
            [Data] public List<Vector3> vertices { get; set; }
            [Data] public List<int> Indices { get; set; }

            public MeshChunks()
            {
                IsTextureOpaque = true;
                MeshNumber = 0;
                TriangleStripValues = new UInt16[0];
                TextureID = 0;
                jointWeights = new List<WeightData>();
                textureCoordinates = new List<Vector2>();
                colors = new List<Vector4>();
                vertices = new List<Vector3>();
                Indices = new List<int>();
            }
        }

        // Textures are based on DDS files. They have a 0x70 header and then the texture data uncompressed using 2 bytes
        public class PmoV4Texture
        {
            public PmoV4TextureHeader Header { get; set; }
            public byte[] Data { get; set; }
            public byte[] PngFile { get; set; } // Decode the data to turn it into a png file

            public class PmoV4TextureHeader
            {
                [Data] public uint Magic { get; set; }
                [Data] public uint unk_04 { get; set; }
                [Data] public uint unk_08 { get; set; }
                [Data] public uint PData { get; set; }
                [Data] public uint unk_10 { get; set; }
                [Data] public uint DataSize { get; set; }
                [Data] public uint unk_18 { get; set; }
                [Data] public uint PixelFormat { get; set; }
                [Data] public ushort Width { get; set; }
                [Data] public ushort Height { get; set; }
                [Data] public uint unk_24 { get; set; }
            }
        }
        enum PmoV4_TEX_FORMAT
        {
            RGBA_8888 = 0,
            RGB_888 = 1,
            RGBA_5551 = 2,
            RGB_565 = 3,
            RGBA_4444 = 4,
            LA8 = 5,
            HILO8 = 6,
            L8 = 7,
            A8 = 8,
            LA4 = 9,
            L4 = 10,
            A4 = 11,
            ETC1 = 12,
            ETC1A4 = 13
        };

        // PMO Header.
        public Header header { get; set; }
        // Data block for textures. The order will reflect their texture index.
        public TextureInfo[] textureInfo { get; set; }
        public List<PmoV4Texture> Textures { get; set; }
        // Texture data blobs.
        //public List<Tm2> texturesData = new List<Tm2>();
        // Header of the skeleton.
        public SkeletonHeader skeletonHeader { get; set; }
        // Joints present in the skeleton.
        public BoneData[] boneList;

        public Model Model0 = new Model();
        public Model Model1 = new Model();
        //public List<MeshChunks> Meshes = new List<MeshChunks>();

        public uint PMO_StartPosition = 0;

        public static void ReadHeader(Stream stream, PmoV4 pmo)
        {
            pmo.header = Mapping.ReadObject<Header>(stream);
        }

        public static void ReadTextureSection(Stream stream, PmoV4 pmo)
        {
            pmo.textureInfo = new TextureInfo[pmo.header.TextureCount];
            for (ushort i = 0; i < pmo.header.TextureCount; i++)
            {
                pmo.textureInfo[i] = Mapping.ReadObject<TextureInfo>(stream);
            }
        }

        public static void ReadModelData(Stream stream, PmoV4 pmo, int ModelNumber = 0)
        {
            // Go to model position.
            if (ModelNumber == 0)
            {
                stream.Seek(pmo.PMO_StartPosition + pmo.header.ModelOffset0, SeekOrigin.Begin);
                pmo.Model0.Header = Mapping.ReadObject<ModelHeader>(stream);
            }
            else
            {
                stream.Seek(pmo.PMO_StartPosition + pmo.header.ModelOffset1, SeekOrigin.Begin);
                pmo.Model1.Header = Mapping.ReadObject<ModelHeader>(stream);
            }
        }

        public static void ReadMeshData(Stream stream, PmoV4 pmo, int ModelNumber = 0, int MeshNumber = 0)
        {
            Model model = ModelNumber == 0 ? pmo.Model0 : pmo.Model1;
            if(model.Meshes == null)
                model.Meshes = new List<MeshChunks>();

            uint meshChunkCount = 0;
            if (MeshNumber == 0)
            {
                stream.Seek(pmo.PMO_StartPosition + model.Header.MeshList0HeaderOffset, SeekOrigin.Begin);
                meshChunkCount = model.Header.MeshList0Count;
            }
            else
            {
                stream.Seek(pmo.PMO_StartPosition + model.Header.MeshList1HeaderOffset, SeekOrigin.Begin);
                meshChunkCount = model.Header.MeshList1Count;
            }

            int DEBUG_totalVertices = 0;
            int DEBUG_totalVertexSize = 0;
            for (int i = 0; i < meshChunkCount; i++)
            {
                MeshChunks meshChunk = new MeshChunks();
                meshChunk.SectionInfo = Mapping.ReadObject<MeshSection>(stream);
                DEBUG_totalVertices += meshChunk.SectionInfo.VertexCount;
                DEBUG_totalVertexSize += meshChunk.SectionInfo.VertexCount * meshChunk.SectionInfo.VertexSize;

                if(meshChunk.SectionInfo.VertexSize != 0x16)
                {
                    throw new Exception("Oh wow. There's a model that actually uses a different vertex format. This should be checked");
                }

                // Exit if Vertex Count is zero.
                if (meshChunk.SectionInfo.VertexCount <= 0)
                    break;

                meshChunk.TextureID = meshChunk.SectionInfo.TextureID;
                VertexFlags flags = GetFlags(meshChunk.SectionInfo);

                bool isColorFlagRisen = flags.UniformDiffuseFlag;

                if (pmo.header.SkeletonOffset != 0)
                    meshChunk.SectionInfo_opt1 = Mapping.ReadObject<MeshSectionOptional1>(stream);
                //if (isColorFlagRisen)
                meshChunk.SectionInfo_opt2 = Mapping.ReadObject<MeshSectionOptional2>(stream);

                // Triangles
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
                else if (flags.Primitive == PrimitiveType.PRIMITIVE_TRIANGLE_STRIP)
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
                else if (flags.Primitive == PrimitiveType.PRIMITIVE_TRIANGLE)
                {
                    for (int s = 0; s < (meshChunk.SectionInfo.VertexCount - 2); s += 3)
                    {
                        meshChunk.Indices.Add(s + 0);
                        meshChunk.Indices.Add(s + 1);
                        meshChunk.Indices.Add(s + 2);
                    }
                }

                model.Meshes.Add(meshChunk);
            }

            // Vertices
            stream.Seek(pmo.PMO_StartPosition + model.Header.VertexDataOffset, SeekOrigin.Begin);
            for (int i = 0; i < model.Meshes.Count; i++)
            {
                MeshChunks meshChunk = model.Meshes[i];

                // Data from section header
                VertexFlags flags = GetFlags(meshChunk.SectionInfo);
                CoordinateFormat TexCoordFormat = flags.TextureCoordinateFormat;
                CoordinateFormat VertexPositionFormat = flags.PositionFormat;
                CoordinateFormat WeightFormat = flags.WeightFormat;
                ColorFormat ColorFormat = flags.ColorFormat;
                UInt32 SkinningWeightsCount = flags.SkinningWeightsCount;
                bool isColorFlagRisen = flags.UniformDiffuseFlag;
                BinaryReader r = new BinaryReader(stream);
                //long positionAfterHeader = stream.Position;

                for (int v = 0; v < meshChunk.SectionInfo.VertexCount; v++)
                {
                    long vertexStartPos = stream.Position;
                    int vertexIncreaseAmount = 0;

                    // Vertex Weights.
                    /*if (pmo.header.SkeletonOffset != 0 && WeightFormat != CoordinateFormat.NO_VERTEX)
                    {
                        WeightData WeightList = new WeightData();
                        WeightList.weights = new List<float>();
                        WeightList.coordFormat = WeightFormat;

                        for (int j = 0; j < (SkinningWeightsCount + 1); j++)
                        {
                            switch (WeightFormat)
                            {
                                case CoordinateFormat.NORMALIZED_8_BITS:
                                    WeightList.weights.Add(stream.ReadByte() / 128.0f);
                                    break;
                                case CoordinateFormat.NORMALIZED_16_BITS:
                                    WeightList.weights.Add(stream.ReadUInt16() / 32768.0f);
                                    break;
                                case CoordinateFormat.FLOAT_32_BITS:
                                    WeightList.weights.Add(stream.ReadFloat());
                                    break;
                                case CoordinateFormat.NO_VERTEX:
                                    break;
                            }
                        }

                        meshChunk.jointWeights.Add(WeightList);
                    }*/

                    Vector2 currentTexCoord = new Vector2(0, 0);

                    TexCoordFormat = PmoV4.CoordinateFormat.NORMALIZED_16_BITS; // Flag is different than BBS and DDD seems to always use this
                    switch (TexCoordFormat)
                    {
                        case CoordinateFormat.NORMALIZED_8_BITS:
                            currentTexCoord.X = stream.ReadByte() / 128.0f;
                            currentTexCoord.Y = stream.ReadByte() / 128.0f;
                            meshChunk.textureCoordinates.Add(currentTexCoord);
                            break;
                        case CoordinateFormat.NORMALIZED_16_BITS:
                            vertexIncreaseAmount = ((0x2 - (Convert.ToInt32(stream.Position - vertexStartPos) & 0x1)) & 0x1);
                            stream.Seek(vertexIncreaseAmount, SeekOrigin.Current);

                            currentTexCoord.X = stream.ReadUInt16() / 32768.0f;
                            currentTexCoord.Y = stream.ReadUInt16() / 32768.0f;
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

                    Vector4 col;

                    if (isColorFlagRisen)
                    {
                        uint c = meshChunk.SectionInfo_opt2.DiffuseColor;
                        col.X = c % 0x100;
                        col.Y = (c >> 8) % 0x100;
                        col.Z = (c >> 16) % 0x100;
                        col.W = (c >> 24) % 0x100;

                        meshChunk.colors.Add(col);
                    }
                    else
                    {
                        ColorFormat = PmoV4.ColorFormat.ABGR_8888_32BITS; // Flag is different than BBS and DDD seems to always use this
                        switch (ColorFormat)
                        {
                            case PmoV4.ColorFormat.NO_COLOR:
                                meshChunk.colors.Add(new Vector4(0xFF, 0xFF, 0xFF, 0xFF));
                                break;
                            case PmoV4.ColorFormat.BGR_5650_16BITS:
                                stream.ReadUInt16();
                                break;
                            case PmoV4.ColorFormat.ABGR_5551_16BITS:
                                stream.ReadUInt16();
                                break;
                            case PmoV4.ColorFormat.ABGR_4444_16BITS:
                                stream.ReadUInt16();
                                break;
                            case PmoV4.ColorFormat.ABGR_8888_32BITS:
                                vertexIncreaseAmount = ((0x4 - (Convert.ToInt32(stream.Position - vertexStartPos) & 0x3)) & 0x3);
                                stream.Seek(vertexIncreaseAmount, SeekOrigin.Current);

                                col.X = stream.ReadByte();
                                col.Y = stream.ReadByte();
                                col.Z = stream.ReadByte();
                                col.W = stream.ReadByte();
                                meshChunk.colors.Add(col);
                                break;
                        }
                    }

                    Vector3 currentVertex;

                    // Handle triangles and triangle strips.
                    switch (VertexPositionFormat)
                    {
                        case CoordinateFormat.NORMALIZED_8_BITS:
                            currentVertex.X = r.ReadSByte() / 128.0f;
                            currentVertex.Y = r.ReadSByte() / 128.0f;
                            currentVertex.Z = r.ReadSByte() / 128.0f;
                            meshChunk.vertices.Add(currentVertex);
                            break;
                        case CoordinateFormat.NORMALIZED_16_BITS:
                            vertexIncreaseAmount = ((0x2 - (Convert.ToInt32(stream.Position - vertexStartPos) & 0x1)) & 0x1);
                            stream.Seek(vertexIncreaseAmount, SeekOrigin.Current);

                            currentVertex.X = (float)stream.ReadInt16() / 32768.0f;
                            currentVertex.Y = (float)stream.ReadInt16() / 32768.0f;
                            currentVertex.Z = (float)stream.ReadInt16() / 32768.0f;
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
                        default:
                            throw new Exception("Invalid coordinate format");
                    }

                    // Weights
                    stream.Position += 8;

                    //stream.Seek(vertexStartPos + meshChunk.SectionInfo.VertexSize, SeekOrigin.Begin);

                    //if (flags.Primitive == PrimitiveType.PRIMITIVE_TRIANGLE)
                    //{
                    //    meshChunk.Indices.Add(v);
                    //}
                }
            }
        }

        public static PmoV4 Read(Stream stream)
        {
            PmoV4 pmo = new PmoV4();
            pmo.PMO_StartPosition = (uint)stream.Position;

            ReadHeader(stream, pmo);
            ReadTextureSection(stream, pmo);

            // Read all data
            if (pmo.header.ModelOffset0 != 0)
            {
                ReadModelData(stream, pmo, 0);
                if (pmo.Model0.Header.MeshList0Count > 0)
                    ReadMeshData(stream, pmo, 0, 0);
                if (pmo.Model0.Header.MeshList1Count > 0)
                    ReadMeshData(stream, pmo, 0, 1);
            }
            /*if (pmo.header.ModelOffset1 != 0)
            {
                ReadModelData(stream, pmo, 1);
                if (pmo.Model1.Header.MeshList0Count > 0)
                    ReadMeshData(stream, pmo, 1, 0);
                if(pmo.Model1.Header.MeshList1Count > 0)
                    ReadMeshData(stream, pmo, 1, 1);
            }*/

            // Read textures.
            pmo.Textures = new List<PmoV4Texture>();
            for (int i = 0; i < pmo.textureInfo.Length; i++)
            {
                if (pmo.textureInfo[i].TextureOffset != 0)
                {
                    PmoV4Texture texture = new PmoV4Texture();
                    stream.Seek(pmo.textureInfo[i].TextureOffset, SeekOrigin.Begin);
                    texture.Header = Mapping.ReadObject<PmoV4Texture.PmoV4TextureHeader>(stream);
                    texture.Data = stream.ReadBytes((int)texture.Header.DataSize);
                    pmo.Textures.Add(texture);
                }
            }

            // Read Skeleton.
            if (pmo.header.SkeletonOffset != 0)
            {
                stream.Seek(pmo.PMO_StartPosition + pmo.header.SkeletonOffset, SeekOrigin.Begin);
                pmo.skeletonHeader = Mapping.ReadObject<SkeletonHeader>(stream);
                pmo.boneList = new BoneData[pmo.skeletonHeader.BoneCount];
                for (int j = 0; j < pmo.skeletonHeader.BoneCount; j++)
                {
                    pmo.boneList[j] = Mapping.ReadObject<BoneData>(stream);
                }
            }

            return pmo;
        }

        private List<uint> TextureOffsets = new List<uint>();

        // Can't be shorter than the header size.
        public static bool IsValid(Stream stream) =>
            stream.Length >= 0xA0 &&
            stream.SetPosition(0).ReadUInt32() == MagicCode;
    }
}
