using OpenKh.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using Xe.BinaryMapper;

namespace OpenKh.Recom
{
    // MDL format researched and reversed by akderebur
    public class Mdl
    {
        private static readonly IBinaryMapping Mapping =
           MappingConfiguration.DefaultConfiguration()
               .ForTypeVector2()
               .ForTypeVector3()
               .ForTypeVector4()
               .ForTypeMatrix4x4()
               .Build();

        public class Header
        {
            [Data] public byte BoneCount { get; set; }
            [Data] public byte Unk01 { get; set; }
            [Data] public byte Unk02 { get; set; }
            [Data] public byte Unk03 { get; set; }
            [Data] public int SkeletonOffset { get; set; }
            [Data] public int MatricesOffset { get; set; }
            [Data] public int MeshCount { get; set; }
            [Data] public int MeshOffset { get; set; }
            [Data] public int VifOffset { get; set; }
            [Data] public int VifOffset2 { get; set; }
            [Data] public int Unk1C { get; set; }
            [Data] public int Unk20 { get; set; }
            [Data] public int Unk24 { get; set; }
            [Data] public int Unk28 { get; set; }
            [Data] public int Unk2C { get; set; }
        }

        public class Bone
        {
            [Data(Count = 16)] public string Name { get; set; }
            [Data] public short ParIds { get; set; }
            [Data] public short Unknown { get; set; }
            public Matrix4x4 Matrix;

            public override string ToString() =>
                $"{Name} {ParIds:X} {Unknown:X}";
        }

        public class Material
        {
            [Data(Count = 32)] public string Name { get; set; }
        }

        public class Mesh
        {
            [Data] public short PrimitiveCount { get; set; }
            [Data] public short Flags { get; set; }
            [Data] public int Unk04 { get; set; }
            [Data] public int Unk08 { get; set; }
            [Data] public int Unk0c { get; set; }
            [Data] public int Unk10 { get; set; }
            [Data] public short VertexCount { get; set; }
            [Data] public short Unk16 { get; set; }
            [Data] public short Unk18 { get; set; }
            [Data] public short Unk1a { get; set; }
            [Data] public byte Unk1c { get; set; }
            [Data] public byte TestStr { get; set; }
            [Data] public byte Unk1e { get; set; }
            [Data] public byte Unk1f { get; set; }
            [Data] public byte Unk20 { get; set; }
            [Data] public byte Unk21 { get; set; }
            [Data] public short Unk22 { get; set; }
            [Data] public short Unk24 { get; set; }
            [Data] public byte Unk26 { get; set; }
            [Data] public byte VertexBufferStride { get; set; }
            [Data] public byte Unk28 { get; set; }
            [Data] public byte Unk29 { get; set; }
            [Data] public short Unk2A { get; set; }
            [Data] public int Unk2C { get; set; }

            public VertexInfo[] Vertices { get; set; }
        }

        public class VertexInfo
        {
            [Data] public short PrimitiveType { get; set; }
            [Data] public short Unk02 { get; set; }
            [Data] public float Unk04 { get; set; }
            [Data] public float Unk08 { get; set; }
            [Data] public short Unk0c { get; set; }
            [Data] public short Unk0e { get; set; }
            public VertexPosition Position { get; set; }
            public VertexTexture Texture { get; set; }

            public override string ToString()
            {
                var sb = new StringBuilder();
                sb.Append($"XYZ({Position}) UV({Texture})");

                return sb.ToString();
            }
        }

        public class VertexPosition
        {
            [Data] public Vector3 Position { get; set; }
            [Data] public short Unk1c { get; set; }
            [Data] public short BoneId { get; set; }

            public override string ToString() =>
                Position.ToString();
        }

        public class VertexTexture
        {
            [Data] public Vector2 TextureUv { get; set; }
            [Data] public float Unk28 { get; set; }
            [Data] public int MeshId { get; set; }

            public override string ToString() =>
                TextureUv.ToString();
        }

        public Header MyHeader { get; set; }
        public List<Bone> Bones { get; set; }
        public List<Material> Materials { get; set; }
        public List<Mesh> Meshes { get; set; }

        private Mdl(Stream stream, int baseOffset)
        {
            MyHeader = Mapping.ReadObject<Header>(stream);

            stream.Position = baseOffset + MyHeader.SkeletonOffset;
            Bones = Enumerable
                .Range(0, MyHeader.BoneCount)
                .Select(x => BinaryMapping.ReadObject<Bone>(stream))
                .ToList();

            stream.Position = baseOffset + MyHeader.MatricesOffset;
            foreach (var bone in Bones)
                bone.Matrix = stream.ReadMatrix4x4();

            stream.Position = baseOffset + MyHeader.MeshOffset;
            Materials = Enumerable
                .Range(0, MyHeader.MeshCount)
                .Select(x => BinaryMapping.ReadObject<Material>(stream))
                .ToList();

            stream.Position = baseOffset + MyHeader.VifOffset;
            Meshes = new List<Mesh>();
            while (true)
            {
                var mesh = Mapping.ReadObject<Mesh>(stream);
                if (mesh.PrimitiveCount == 0)
                    break;
                Meshes.Add(mesh);
                mesh.Vertices = new VertexInfo[mesh.VertexCount];

                //if (mesh.TestStr > 0)
                //    mesh.VertexBufferStride = 64;

                var vertexBufferStart = stream.Position;
                switch (mesh.VertexBufferStride)
                {
                    case 0x20:
                        for (var i = 0; i < mesh.Vertices.Length; i++)
                        {
                            mesh.Vertices[i] = Mapping.ReadObject<VertexInfo>(stream);
                            mesh.Vertices[i].Position = Mapping.ReadObject<VertexPosition>(stream);
                        }
                        break;
                    case 0x30:
                        for (var i = 0; i < mesh.Vertices.Length; i++)
                        {
                            mesh.Vertices[i] = Mapping.ReadObject<VertexInfo>(stream);
                            mesh.Vertices[i].Position = Mapping.ReadObject<VertexPosition>(stream);
                            mesh.Vertices[i].Texture = Mapping.ReadObject<VertexTexture>(stream);
                        }
                        break;
                    default:
                        throw new NotImplementedException(
                            $"VB stride is {mesh.VertexBufferStride:X}");
                }

                stream.SetPosition(vertexBufferStart + mesh.Vertices.Length * mesh.VertexBufferStride + 0x10);
            }
        }

        private void Write(Stream stream)
        {
            Mapping.WriteObject(stream, MyHeader);

            foreach (var bone in Bones)
                Mapping.WriteObject(stream, bone);
            foreach (var bone in Bones)
                stream.Write(bone.Matrix);
        }

        public static List<Mdl> Read(Stream stream)
        {
            var modelOffsets = new List<int>();
            stream.SetPosition(0);
            while (stream.Position < stream.Length)
            {
                var offset = stream.ReadInt32();
                if (offset == 0)
                    break;

                modelOffsets.Add(offset);
            }

            return modelOffsets.Select(offset =>
            {
                stream.SetPosition(offset);
                return new Mdl(stream, offset);
            }).ToList();
        }

        public static void Write(Stream stream, IEnumerable<Mdl> mdls)
        {
            stream.Write(0x10);
            stream.Write(0x34EA0);
            stream.Write(0);
            stream.Write(0);

            foreach (var mdl in mdls)
                mdl.Write(stream);
        }
    }
}
