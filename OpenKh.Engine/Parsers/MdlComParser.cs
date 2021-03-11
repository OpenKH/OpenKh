using OpenKh.Engine.Motion;
using OpenKh.Kh2;
using OpenKh.Recom;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace OpenKh.Engine.Parsers
{
    public class MdlComParser : IModelMotion
    {
        private Matrix4x4 GetMatrix(List<Mdl.Bone> bones, int index)
        {
            var bone = bones[index];
            var parent = bone.Parent >= 0 ?
                GetMatrix(bones, bone.Parent) :
                Matrix4x4.Identity;
            return Matrix4x4.Multiply(bone.Matrix, parent);
        }

        private Matrix4x4 GetMatrix(List<Mdl.Bone> bones, Matrix4x4[] matrices, int index)
        {
            var bone = bones[index];
            var parent = bone.Parent >= 0 ?
                GetMatrix(bones, bone.Parent) :
                Matrix4x4.Identity;
            return Matrix4x4.Multiply(matrices[index], parent);
        }

        private readonly Mdl _mdl;
        private readonly int[] _materialIndices;

        public MdlComParser(Mdl mdl, Dictionary<string, int> materials)
        {
            _mdl = mdl;
            _materialIndices = _mdl.Materials
                .Select((x, Index) => materials[x.Name])
                .ToArray();
            MeshDescriptors = mdl.Meshes
                .Select(UnstripTriangles)
                .ToList();
            InitialPose = _mdl.Bones.Select(x => x.Matrix).ToArray();
            CurrentPose = InitialPose;
        }

        public List<MeshDescriptor> MeshDescriptors { get; }

        public List<Mdlx.Bone> Bones => new List<Mdlx.Bone>();

        public Matrix4x4[] InitialPose { get; }
        public Matrix4x4[] CurrentPose { get; private set; }

        public void ApplyMotion(Matrix4x4[] matrices)
        {
            var m = new Matrix4x4[matrices.Length];
            for (var i = 0; i < matrices.Length; i++)
                m[i] = GetMatrix(_mdl.Bones, matrices, i);
            CurrentPose = m;

            for (var i = 0; i < MeshDescriptors.Count; i++)
            {
                var meshDescriptor = MeshDescriptors[i];
                var mesh = _mdl.Meshes[i];
                if (mesh.VertexBufferStride == 0x30)
                {
                    for (var j = 0; j < mesh.Vertices.Length; j++)
                    {
                        var pos = Vector3.Transform(mesh.Vertices[j].Position.Position,
                            m[mesh.Vertices[j].Position.BoneIndex]);
                        meshDescriptor.Vertices[j].X = pos.X * 100f;
                        meshDescriptor.Vertices[j].Y = pos.Y * 100f;
                        meshDescriptor.Vertices[j].Z = pos.Z * 100f;
                    }
                }
            }
        }

        private MeshDescriptor UnstripTriangles(Mdl.Mesh mesh)
        {
            var indices = new List<int>(); // TODO optimize this with an array instead
            var vertices = new PositionColoredTextured[mesh.Vertices.Length];

            for (int v = 0; v < mesh.Vertices.Length - 2; v++)
            {
                switch (mesh.Vertices[v + 2].PrimitiveType)
                {
                    case -1: // Discard triangle
                        break;
                    case 0:
                        indices.Add(v);
                        indices.Add(v + 2);
                        indices.Add(v + 1);
                        break;
                    case 0x20: // flip winding
                        indices.Add(v);
                        indices.Add(v + 1);
                        indices.Add(v + 2);
                        break;
                    default:
                        break;
                }
            }

            var materialIndex = 0;
            if (mesh.VertexBufferStride == 0x30)
            {
                for (var i = 0; i < vertices.Length; i++)
                {
                    vertices[i].X = mesh.Vertices[i].Position.Position.X * 100f;
                    vertices[i].Y = mesh.Vertices[i].Position.Position.Y * 100f;
                    vertices[i].Z = mesh.Vertices[i].Position.Position.Z * 100f;
                    vertices[i].Tu = mesh.Vertices[i].Texture.TextureUv.X;
                    vertices[i].Tv = mesh.Vertices[i].Texture.TextureUv.Y;
                    vertices[i].R = 1.0f;
                    vertices[i].G = 1.0f;
                    vertices[i].B = 1.0f;
                    vertices[i].A = 0.5f;
                    materialIndex = mesh.Vertices[i].Texture.MaterialIndex;
                }
            }

            return new MeshDescriptor
            {
                Indices = indices.ToArray(),
                Vertices = vertices,
                IsOpaque = true,
                TextureIndex = _materialIndices[materialIndex],
            };
        }
    }
}
