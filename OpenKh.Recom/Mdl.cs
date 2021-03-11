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
            [Data] public int BonesOffset { get; set; }
            [Data] public int MatricesOffset { get; set; }
            [Data] public int MaterialCount { get; set; }
            [Data] public int MaterialsOffset { get; set; }
            [Data] public int VifOffset { get; set; }
            [Data] public int VifOffset2 { get; set; }
            [Data] public int UnkOffset { get; set; }
            [Data] public int Unk20 { get; set; }
            [Data] public int Unk24 { get; set; }
            [Data] public int Unk28 { get; set; }
            [Data] public int Unk2C { get; set; }
        }

        public class Bone
        {
            [Data(Count = 16)] public string Name { get; set; }
            [Data] public short Parent { get; set; }
            [Data] public short Unknown { get; set; }
            public Matrix4x4 Matrix;

            public override string ToString() =>
                $"{Name} {Parent:X} {Unknown:X}";
        }

        public class Material
        {
            [Data(Count = 32)] public string Name { get; set; }
        }

        public class Mesh
        {
            public short PrimitiveCount { get; set; }
            public short Flags { get; set; }
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
            [Data] public short BoneIndex { get; set; }

            public override string ToString() =>
                Position.ToString();
        }

        public class VertexTexture
        {
            [Data] public Vector2 TextureUv { get; set; }
            [Data] public float Unk28 { get; set; }
            [Data] public int MaterialIndex { get; set; }

            public override string ToString() =>
                TextureUv.ToString();
        }

        public Header MyHeader { get; set; }
        public List<Bone> Bones { get; set; }
        public List<Material> Materials { get; set; }
        public List<Mesh> Meshes { get; set; }
        public List<Mesh> Meshes2 { get; set; }
        public List<float> Unk { get; set; }

        private Mdl(Stream stream, int baseOffset)
        {
            MyHeader = Mapping.ReadObject<Header>(stream);

            stream.Position = baseOffset + MyHeader.BonesOffset;
            Bones = Enumerable
                .Range(0, MyHeader.BoneCount)
                .Select(x => BinaryMapping.ReadObject<Bone>(stream))
                .ToList();

            stream.Position = baseOffset + MyHeader.MatricesOffset;
            foreach (var bone in Bones)
                bone.Matrix = stream.ReadMatrix4x4();

            stream.Position = baseOffset + MyHeader.MaterialsOffset;
            Materials = Enumerable
                .Range(0, MyHeader.MaterialCount)
                .Select(x => BinaryMapping.ReadObject<Material>(stream))
                .ToList();

            if (MyHeader.VifOffset > 0)
            {
                stream.Position = baseOffset + MyHeader.VifOffset;
                Meshes = ReadMeshes(stream);
            }

            if (MyHeader.VifOffset2 > 0)
            {
                stream.Position = baseOffset + MyHeader.VifOffset2;
                Meshes2 = ReadMeshes(stream);
            }

            Unk = new List<float>();
            stream.Position = baseOffset + MyHeader.UnkOffset;
            while (true)
            {
                var value = stream.ReadSingle();
                if (value == 0)
                    break;
                Unk.Add(value);
            }
        }

        private List<Mesh> ReadMeshes(Stream stream)
        {
            var meshes = new List<Mesh>();
            while (true)
            {
                var primitiveCount = stream.ReadInt16();
                var flags = stream.ReadInt16();
                if (primitiveCount == 0)
                    break;

                var mesh = Mapping.ReadObject<Mesh>(stream);
                mesh.PrimitiveCount = primitiveCount;
                mesh.Flags = flags;
                mesh.Vertices = new VertexInfo[mesh.VertexCount];
                
                meshes.Add(mesh);

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

                stream.Position += 0x10;
            }

            return meshes;
        }

        private void Write(Stream stream)
        {
            if (Bones.Count > byte.MaxValue)
                throw new ArgumentOutOfRangeException("Bones.Count", Bones.Count, $"Bone count can not exceed {byte.MaxValue}.");

            var startPosition = stream.Position;
            stream.Seek(0x30, SeekOrigin.Current);

            MyHeader.BoneCount = (byte)Bones.Count;
            MyHeader.BonesOffset = (int)(stream.Position - startPosition);
            foreach (var bone in Bones)
                Mapping.WriteObject(stream, bone);

            stream.AlignPosition(0x10);
            MyHeader.MatricesOffset = (int)(stream.Position - startPosition);
            foreach (var bone in Bones)
                stream.Write(bone.Matrix);

            stream.AlignPosition(0x10);
            MyHeader.MaterialsOffset = (int)(stream.Position - startPosition);
            MyHeader.MaterialCount = Materials.Count;
            foreach (var material in Materials)
                Mapping.WriteObject(stream, material);

            if (Meshes != null)
            {
                MyHeader.VifOffset = (int)(stream.Position - startPosition);
                Write(stream, Meshes);
            }
            else
                MyHeader.VifOffset = 0;

            if (Meshes2 != null)
            {
                MyHeader.VifOffset2 = (int)(stream.Position - startPosition);
                Write(stream, Meshes2);
            }
            else
                MyHeader.VifOffset2 = 0;

            MyHeader.UnkOffset = (int)(stream.Position - startPosition);
            foreach (var value in Unk)
                stream.Write(value);
            stream.Write(0);
            
            stream.AlignPosition(0x10);
            var endPosition = stream.Position;
            if (stream.Length < stream.Position)
                stream.SetLength(stream.Position);

            stream.Position = startPosition;
            Mapping.WriteObject(stream, MyHeader);
            stream.Position = endPosition;
        }

        private void Write(Stream stream, List<Mesh> meshes)
        {
            foreach (var mesh in meshes)
            {
                stream.Write(mesh.PrimitiveCount);
                stream.Write(mesh.Flags);
                Mapping.WriteObject(stream, mesh);
                foreach (var vertex in mesh.Vertices)
                {
                    switch (mesh.VertexBufferStride)
                    {
                        case 0x20:
                            Mapping.WriteObject(stream, vertex);
                            Mapping.WriteObject(stream, vertex.Position);
                            break;
                        case 0x30:
                            Mapping.WriteObject(stream, vertex);
                            Mapping.WriteObject(stream, vertex.Position);
                            Mapping.WriteObject(stream, vertex.Texture);
                            break;
                    }
                }
                stream.Write(0x17000000);
                stream.AlignPosition(0x10);
            }

            stream.Write((short)0x0000);
            stream.Write((short)0x6000);
            stream.AlignPosition(0x10);
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

        public static void Write(Stream stream, ICollection<Mdl> mdls)
        {
            var modelCount = mdls.Count;
            var headerSize = Helpers.Align(modelCount * 4, 0x10);
            var offsetList = new List<int>(modelCount);

            stream.Position = headerSize;
            foreach (var mdl in mdls)
            {
                offsetList.Add((int)stream.Position);
                mdl.Write(stream);
            }
            stream.SetLength(stream.Position);

            stream.FromBegin();
            foreach (var offset in offsetList)
                stream.Write(offset);

            stream.ToEnd();
        }
    }
}
