using System;
using System.Collections.Generic;
using OpenKh.Engine.Motion;
using OpenKh.Kh2;
using OpenKh.Bbs;
using System.Numerics;

namespace OpenKh.Engine.Parsers
{
    public class PmoParser : IModelMotion
    {
        private readonly Pmo aPmo;

        public PmoParser(Pmo pmo, float Scale)
        {
            aPmo = pmo;
            MeshDescriptors = new List<MeshDescriptor>();
            MeshDescriptor currentMesh = new MeshDescriptor();

            for (int x = 0; x < pmo.Meshes.Count; x++)
            {
                var vertices = new PositionColoredTextured[pmo.Meshes[x].vertices.Count];
                for (var i = 0; i < vertices.Length; i++)
                {
                    Vector4 color;

                    color.X = pmo.Meshes[x].colors[i].X / 128.0f;
                    color.Y = pmo.Meshes[x].colors[i].Y / 128.0f;
                    color.Z = pmo.Meshes[x].colors[i].Z / 128.0f;
                    color.W = pmo.Meshes[x].colors[i].W / 128.0f;

                    vertices[i].X = pmo.Meshes[x].vertices[i].X * pmo.header.ModelScale * Scale;
                    vertices[i].Y = pmo.Meshes[x].vertices[i].Y * pmo.header.ModelScale * Scale;
                    vertices[i].Z = pmo.Meshes[x].vertices[i].Z * pmo.header.ModelScale * Scale;
                    vertices[i].Tu = pmo.Meshes[x].textureCoordinates[i].X;
                    vertices[i].Tv = pmo.Meshes[x].textureCoordinates[i].Y;
                    vertices[i].R = color.X;
                    vertices[i].G = color.Y;
                    vertices[i].B = color.Z;
                    vertices[i].A = color.W;
                }

                currentMesh = new MeshDescriptor()
                {
                    Vertices = vertices,
                    Indices = pmo.Meshes[x].Indices.ToArray(),
                    TextureIndex = pmo.Meshes[x].TextureID,
                    IsOpaque = false
                };

                MeshDescriptors.Add(currentMesh);
            }

            if (pmo.header.SkeletonOffset != 0)
            {
                List<Matrix4x4> matrices = new List<Matrix4x4>();
                List<Mdlx.Bone> skeleton = new List<Mdlx.Bone>();

                foreach (Pmo.BoneData boneData in pmo.boneList)
                {
                    Matrix4x4 mtx = boneData.Transform;
                    Matrix4x4 mtx_nd = Matrix4x4.Transpose(mtx);

                    matrices.Add(mtx_nd);

                    Mdlx.Bone otherBone = new Mdlx.Bone();
                    otherBone.Index = boneData.BoneIndex;
                    otherBone.Parent = (boneData.ParentBoneIndex == 0xFFFF) ? 0 : boneData.ParentBoneIndex;
                    otherBone.TranslationX = mtx_nd.Translation.X;
                    otherBone.TranslationY = mtx_nd.Translation.Y;
                    otherBone.TranslationZ = mtx_nd.Translation.Z;
                    otherBone.TranslationW = mtx_nd.M14;
                    otherBone.RotationX = mtx_nd.M21;
                    otherBone.RotationY = mtx_nd.M22;
                    otherBone.RotationZ = mtx_nd.M23;
                    otherBone.RotationW = mtx_nd.M24;
                    otherBone.ScaleX = mtx_nd.M31;
                    otherBone.ScaleY = mtx_nd.M32;
                    otherBone.ScaleZ = mtx_nd.M33;
                    otherBone.ScaleW = mtx_nd.M34;

                    skeleton.Add(otherBone);
                }

                Bones = skeleton;
                InitialPose = matrices.ToArray();
                CurrentPose = InitialPose;
            }
        }

        public List<MeshDescriptor> MeshDescriptors { get; private set; }

        public List<Mdlx.Bone> Bones { get; private set; }

        public Matrix4x4[] InitialPose { get; set; }
        public Matrix4x4[] CurrentPose { get; private set; }

        public void ApplyMotion(Matrix4x4[] matrices)
        {

        }
    }
}
