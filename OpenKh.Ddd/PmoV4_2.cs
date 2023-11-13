using OpenKh.Common;
using OpenKh.Common.Utils;
using System;
using System.Buffers.Text;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using Xe.BinaryMapper;

namespace OpenKh.Ddd
{
    public class PmoV4_2
    {
        private static readonly IBinaryMapping Mapping =
           MappingConfiguration.DefaultConfiguration()
               .ForTypeMatrix4x4()
               .Build();

        public PmoV4Header Header;
        public List<Material> Materials = new List<Material>();
        public ModelHeader ModelHead;
        public List<MeshPartition> MeshPartitionList1 = new List<MeshPartition>();
        public List<MeshPartition> MeshPartitionList2 = new List<MeshPartition>();
        public Skeleton Skel;


        public class PmoV4Header
        {
            [Data] public int MagicCode { get; set; }
            [Data] public byte Number { get; set; }
            [Data] public byte MeshCount { get; set; }
            [Data] public byte Version { get; set; }
            [Data] public byte Padding1 { get; set; }
            [Data] public byte TextureCount { get; set; }
            [Data] public byte Padding2 { get; set; }
            [Data] public ushort Flag { get; set; }
            [Data] public int SkeletonOffset { get; set; }
            [Data] public int ModelOffset { get; set; }
            [Data] public ushort TriangleCount { get; set; }
            [Data] public ushort VertexCount { get; set; }
            [Data] public float ModelScale { get; set; }
            [Data] public int UnkOffset { get; set; } // Or size
            [Data(Count = 32)] public float[] BoundingBox { get; set; }
        }

        public class Material
        {
            public TextureHeader TexHeader;
            public Ctrt CtrtImage;
        }

        public class TextureHeader
        {
            [Data] public uint TextureOffset { get; set; }
            [Data(Count = 12)] public string TextureName { get; set; }
            [Data] public float ScrollX { get; set; }
            [Data] public float ScrollY { get; set; }
            [Data(Count = 8)] public byte[] Padding { get; set; }
        }

        public class ModelHeader
        {
            [Data] public uint MainModelOffset { get; set; }
            [Data] public uint SecondaryModelOffset { get; set; }
            [Data] public uint MainVertexCount { get; set; }
            [Data] public uint SecondaryVertexCount { get; set; }
            [Data] public uint MeshList0Count { get; set; }
            [Data] public uint MeshList1Count { get; set; }
            [Data] public uint VertexDataOffset { get; set; }
            [Data] public uint VertexDataSize { get; set; }
        }

        public class MeshPartition
        {
            public MeshPartitionHeader Header;
            public List<Vertex> Vertices = new List<Vertex>();

            public override string ToString()
            {
                return "Texture: "+Header.TextureID+ "; VSize: "+Header.VertexSize+"; VCount: " + Header.VertexCount+"; Strip: "+Header.isTriangleStrip+"; triStrip Count: "+Header.TriangleStripCount+"";
            }
        }

        public class MeshPartitionHeader
        {
            [Data] public ushort VertexCount { get; set; }
            [Data] public byte TextureID { get; set; }
            [Data] public byte VertexSize { get; set; } // In bytes.
            [Data] public int VertexFlags { get; set; }
            [Data] public byte Group { get; set; }
            [Data] public byte TriangleStripCount { get; set; }
            [Data] public ushort Attribute { get; set; }
            [Data(Count = 8)] public byte[] BoneIndices { get; set; }
            [Data] public int DiffuseColor { get; set; } // RGBA

            public bool isTriangleStrip
            {
                get
                {
                    return BitsUtil.Int.GetBit(VertexFlags, 30);
                }
                set
                {
                    BitsUtil.Int.SetBit(VertexFlags, 30, value);
                }
            }

            CoordinateFormat F_TextureCoordinateFormat { get => (CoordinateFormat)GetBitFieldRange(VertexFlags, 0, 2); }
            ColorFormat F_ColorFormat { get => (ColorFormat)GetBitFieldRange(VertexFlags, 2, 3); }
            CoordinateFormat F_NormalFormat { get => (CoordinateFormat)GetBitFieldRange(VertexFlags, 5, 2); }
            CoordinateFormat F_PositionFormat { get => (CoordinateFormat)GetBitFieldRange(VertexFlags, 7, 2); }
            CoordinateFormat F_WeightFormat { get => (CoordinateFormat)GetBitFieldRange(VertexFlags, 9, 2); }
            byte F_IndicesFormat { get => (byte)GetBitFieldRange(VertexFlags, 11, 2); }
            bool F_Unused1 { get => (byte)GetBitFieldRange(VertexFlags, 13, 1) == 1; }
            byte F_SkinningWeightsCount { get => (byte)GetBitFieldRange(VertexFlags, 14, 3); }
            bool F_Unused2 { get => (byte)GetBitFieldRange(VertexFlags, 17, 1) == 1; }
            byte F_MorphWeightsCount { get => (byte)GetBitFieldRange(VertexFlags, 18, 3); }
            byte F_Unused3 { get => (byte)GetBitFieldRange(VertexFlags, 21, 2); }
            bool F_SkipTransformPipeline { get => (byte)GetBitFieldRange(VertexFlags, 23, 1) == 1; }
            bool F_UniformDiffuseFlag { get => (byte)GetBitFieldRange(VertexFlags, 24, 1) == 1; }
            byte F_Unknown1 { get => (byte)GetBitFieldRange(VertexFlags, 25, 3); }
            PrimitiveType F_Primitive { get => (PrimitiveType)GetBitFieldRange(VertexFlags, 28, 4); }
        }

        public class Vertex
        {
            [Data] public short TexCoordU { get; set; }
            [Data] public short TexCoordV { get; set; }
            [Data] public byte ColorR { get; set; }
            [Data] public byte ColorG { get; set; }
            [Data] public byte ColorB { get; set; }
            [Data] public byte ColorA { get; set; }
            public float PositionX { get; set; }
            public float PositionY { get; set; }
            public float PositionZ { get; set; }
            public Weight VWeight { get; set; }
            public int Unk { get; set; }

            public class Weight
            {
                [Data] public byte Weight0 { get; set; }
                [Data] public byte Weight1 { get; set; }
                [Data] public byte Weight2 { get; set; }
                [Data] public byte Weight3 { get; set; }
                [Data] public byte Joint0 { get; set; }
                [Data] public byte Joint1 { get; set; }
                [Data] public byte Joint2 { get; set; }
                [Data] public byte Joint3 { get; set; }
            }
        }

        public class Skeleton
        {
            public SkeletonHeader Header;
            public List<BoneData> Bones = new List<BoneData>();
        }

        public class SkeletonHeader
        {
            [Data] public uint MagicValue { get; set; }
            [Data] public uint Padding1 { get; set; }
            [Data] public ushort BoneCount { get; set; }
            [Data] public ushort Padding2 { get; set; }
            [Data] public ushort SkinnedBoneCount { get; set; }
            [Data] public ushort nStdBone { get; set; }
        }

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

        public static PmoV4_2 Read(Stream stream, bool skipTextures = false)
        {
            long BASE = stream.Position;

            if (stream.Length < BASE + 160)
                throw new Exception("PMO V4: Format check - File is not big enough");

            PmoV4_2 thisPmo = new PmoV4_2();

            // FILE HEADER
            thisPmo.Header = BinaryMapping.ReadObject<PmoV4Header>(stream);

            // MATERIAL HEADERS
            thisPmo.Materials = new List<Material>();
            for (int i = 0; i < thisPmo.Header.TextureCount; i++)
            {
                Material material = new Material();
                material.TexHeader = BinaryMapping.ReadObject<TextureHeader>(stream);
                thisPmo.Materials.Add(material);
            }

            // MODEL HEADER
            if (thisPmo.Header.ModelOffset != 0)
            {
                if (stream.Position != BASE + thisPmo.Header.ModelOffset)
                    throw new Exception("PMO V4: Format check - There's some unknown data between the texture headers and the model header");
            }
            else if (thisPmo.Header.UnkOffset != 0)
            {
                if (thisPmo.Header.UnkOffset != 0 && stream.Position != BASE + thisPmo.Header.UnkOffset)
                    throw new Exception("PMO V4: Format check - There's some unknown data between the texture headers and the model header");
            }
            thisPmo.ModelHead = BinaryMapping.ReadObject<ModelHeader>(stream);

            // RESERVED
            int unknownInt = stream.ReadInt32(); // Most of the time 01. Sometimes it has other values. d_ex210 has 03. d_tw010 has 02
            for (int i = 0; i < 7; i++)
            {
                if (stream.ReadInt32() != 0)
                    throw new Exception("PMO V4: Format check - Reserved data between model header and mesh parts is incorrect");
            }

            // MESH NAMES - Unused for now but it can be read
            int[] meshNamePointers = new int[thisPmo.Header.MeshCount];
            string[] meshNames = new string[thisPmo.Header.MeshCount];
            for (int i = 0; i < thisPmo.Header.MeshCount; i++)
            {
                meshNamePointers[i] = stream.ReadInt32();
            }
            for (int i = 0; i < thisPmo.Header.MeshCount; i++)
            {
                if (stream.Position != BASE + meshNamePointers[i])
                    throw new Exception("PMO V4: Format check - Mesh name pointer doesn't check out");

                int modelOffset = (thisPmo.ModelHead.MainModelOffset != 0) ? (int)(BASE + thisPmo.ModelHead.MainModelOffset) : (int)(BASE + thisPmo.ModelHead.SecondaryModelOffset);
                int nameSize = modelOffset - (int)stream.Position;
                if (i < thisPmo.Header.MeshCount - 1)
                {
                    nameSize = meshNamePointers[i + 1] - meshNamePointers[i];
                }
                nameSize--; // Separator

                meshNames[i] = stream.ReadString(nameSize, System.Text.Encoding.UTF8);
                stream.Position += 1; // Separator
            }

            // MAIN MODEL PARTITIONS
            if (thisPmo.ModelHead.MainModelOffset != 0)
            {
                if (stream.Position != BASE + thisPmo.ModelHead.MainModelOffset)
                    throw new Exception("PMO V4: Format check - Not in main model offset");
                stream.Position = BASE + thisPmo.ModelHead.MainModelOffset;
            }
            else if (thisPmo.ModelHead.SecondaryModelOffset != 0)
            {
                if (stream.Position != BASE + thisPmo.ModelHead.SecondaryModelOffset)
                    throw new Exception("PMO V4: Format check - Not in secondary model offset");
                stream.Position = BASE + thisPmo.ModelHead.SecondaryModelOffset;
            }

            thisPmo.MeshPartitionList1 = new List<MeshPartition>();
            for (int i = 0; i < thisPmo.ModelHead.MeshList0Count; i++)
            {
                MeshPartition partition = new MeshPartition();
                partition.Header = BinaryMapping.ReadObject<MeshPartitionHeader>(stream);
                thisPmo.MeshPartitionList1.Add(partition);
            }
            if (thisPmo.ModelHead.MeshList0Count > 0)
            {
                for (int i = 0; i < 6; i++)
                {
                    if (stream.ReadInt32() != 0)
                        throw new Exception("PMO V4: Format check - End of part headers is wrong");
                }
            }

            // SECONDARY MODEL PARTITIONS
            if (thisPmo.ModelHead.MeshList1Count > 0)
            {
                if (stream.Position != BASE + thisPmo.ModelHead.SecondaryModelOffset)
                    throw new Exception("PMO V4: Format check - Not in secondary model offset");
                thisPmo.MeshPartitionList2 = new List<MeshPartition>();
                for (int i = 0; i < thisPmo.ModelHead.MeshList1Count; i++)
                {
                    MeshPartition partition = new MeshPartition();
                    partition.Header = BinaryMapping.ReadObject<MeshPartitionHeader>(stream);
                    thisPmo.MeshPartitionList2.Add(partition);
                }
                if (thisPmo.ModelHead.MeshList1Count > 0)
                {
                    for (int i = 0; i < 6; i++)
                    {
                        if (stream.ReadInt32() != 0)
                            throw new Exception("PMO V4: Format check - End of part headers is wrong");
                    }
                }
            }

            // VERTEX LIST
            if (stream.Position != BASE + thisPmo.ModelHead.VertexDataOffset)
                throw new Exception("PMO V4: Format check - Vertex data pointer not matching");
            for (int i = 0; i < thisPmo.MeshPartitionList1.Count; i++)
            {
                MeshPartition partition = thisPmo.MeshPartitionList1[i];
                for (int j = 0; j < partition.Header.VertexCount; j++)
                {
                    Vertex vertex = BinaryMapping.ReadObject<Vertex>(stream);
                    if (partition.Header.VertexSize == 28)
                    {
                        vertex.PositionX = stream.ReadSingle();
                        vertex.PositionY = stream.ReadSingle();
                        vertex.PositionZ = stream.ReadSingle();
                        vertex.VWeight = BinaryMapping.ReadObject<Vertex.Weight>(stream);
                    }
                    else if (partition.Header.VertexSize == 26)
                    {
                        vertex.PositionX = stream.ReadInt16() / 32768.0f;
                        vertex.PositionY = stream.ReadInt16() / 32768.0f;
                        vertex.PositionZ = stream.ReadInt16() / 32768.0f;
                        vertex.VWeight = BinaryMapping.ReadObject<Vertex.Weight>(stream);
                        vertex.Unk = stream.ReadInt32();
                    }
                    else if (partition.Header.VertexSize == 22)
                    {
                        vertex.PositionX = stream.ReadInt16() / 32768.0f;
                        vertex.PositionY = stream.ReadInt16() / 32768.0f;
                        vertex.PositionZ = stream.ReadInt16() / 32768.0f;
                        vertex.VWeight = BinaryMapping.ReadObject<Vertex.Weight>(stream);
                    }
                    else if (partition.Header.VertexSize == 20)
                    {
                        vertex.PositionX = stream.ReadSingle();
                        vertex.PositionY = stream.ReadSingle();
                        vertex.PositionZ = stream.ReadSingle();
                    }
                    else if (partition.Header.VertexSize == 14)
                    {
                        vertex.PositionX = stream.ReadInt16() / 32768.0f;
                        vertex.PositionY = stream.ReadInt16() / 32768.0f;
                        vertex.PositionZ = stream.ReadInt16() / 32768.0f;
                    }
                    partition.Vertices.Add(vertex);
                }
            }
            for (int i = 0; i < thisPmo.MeshPartitionList2.Count; i++)
            {
                MeshPartition partition = thisPmo.MeshPartitionList2[i];
                for (int j = 0; j < partition.Header.VertexCount; j++)
                {
                    Vertex vertex = BinaryMapping.ReadObject<Vertex>(stream);
                    if (partition.Header.VertexSize == 28)
                    {
                        vertex.PositionX = stream.ReadSingle();
                        vertex.PositionY = stream.ReadSingle();
                        vertex.PositionZ = stream.ReadSingle();
                        vertex.VWeight = BinaryMapping.ReadObject<Vertex.Weight>(stream);
                    }
                    else if (partition.Header.VertexSize == 26)
                    {
                        vertex.PositionX = stream.ReadInt16() / 32768.0f;
                        vertex.PositionY = stream.ReadInt16() / 32768.0f;
                        vertex.PositionZ = stream.ReadInt16() / 32768.0f;
                        vertex.VWeight = BinaryMapping.ReadObject<Vertex.Weight>(stream);
                        vertex.Unk = stream.ReadInt32();
                    }
                    else if (partition.Header.VertexSize == 22)
                    {
                        vertex.PositionX = stream.ReadInt16() / 32768.0f;
                        vertex.PositionY = stream.ReadInt16() / 32768.0f;
                        vertex.PositionZ = stream.ReadInt16() / 32768.0f;
                        vertex.VWeight = BinaryMapping.ReadObject<Vertex.Weight>(stream);
                    }
                    else if (partition.Header.VertexSize == 20)
                    {
                        vertex.PositionX = stream.ReadSingle();
                        vertex.PositionY = stream.ReadSingle();
                        vertex.PositionZ = stream.ReadSingle();
                    }
                    else if (partition.Header.VertexSize == 14)
                    {
                        vertex.PositionX = stream.ReadInt16() / 32768.0f;
                        vertex.PositionY = stream.ReadInt16() / 32768.0f;
                        vertex.PositionZ = stream.ReadInt16() / 32768.0f;
                    }
                    partition.Vertices.Add(vertex);
                }
            }

            // A bunch of 00s. Use and count unknown.

            // TEXTURES (CTRT)
            if (!skipTextures)
            {
                bool materialRead = false;
                for (int i = 0; i < thisPmo.Materials.Count; i++)
                {
                    Material material = thisPmo.Materials[i];
                    if (materialRead)
                    {
                        if (stream.Position > BASE + material.TexHeader.TextureOffset)
                            throw new Exception("PMO V4: Format check - Previous texture was longer than expected");
                        else if (stream.Position < BASE + material.TexHeader.TextureOffset)
                            throw new Exception("PMO V4: Format check - Previous texture was shorter than expected");
                    }

                    stream.Position = BASE + material.TexHeader.TextureOffset;
                    Ctrt ctrt = new Ctrt();
                    ctrt.Header = BinaryMapping.ReadObject<Ctrt.CtrtHeader>(stream);
                    ctrt.EncryptedData = stream.ReadBytes(ctrt.Header.DataSize);
                    material.CtrtImage = ctrt;
                    materialRead = true;
                }
            }
            
            // SKELETON (BON)
            if (thisPmo.Header.SkeletonOffset > 0)
            {
                if (thisPmo.Materials.Count > 0 && stream.Position != BASE + thisPmo.Header.SkeletonOffset)
                    throw new Exception("PMO V4: Format check - Skeleton offset is not right after the textures");
                stream.Position = BASE + thisPmo.Header.SkeletonOffset;
                thisPmo.Skel = new Skeleton();
                thisPmo.Skel.Header = BinaryMapping.ReadObject<SkeletonHeader>(stream);
                thisPmo.Skel.Bones = new List<BoneData>();
                for (int i = 0; i < thisPmo.Skel.Header.BoneCount; i++)
                {
                    thisPmo.Skel.Bones.Add(Mapping.ReadObject<BoneData>(stream));
                }
            }

            return thisPmo;
        }

        public static PmoV4_2 Read(string filepath)
        {
            PmoV4_2 thisPmo = new PmoV4_2();
            using (FileStream stream = new FileStream(filepath, FileMode.Open))
            {
                thisPmo = Read(stream);
            }
            return thisPmo;
        }

        // From BBS PMO - For testing purpose only. Flags are apparently different
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

        public static int GetBitFieldRange(int value, int start = 0, int length = 1)
        {
            int bit = value << 32 - (start + length);
            bit >>= 32 - length;
            return bit;
        }
    }
}
