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

                    color.X = pmo.Meshes[x].colors[i].X;
                    color.Y = pmo.Meshes[x].colors[i].Y;
                    color.Z = pmo.Meshes[x].colors[i].Z;
                    color.W = pmo.Meshes[x].colors[i].W;

                    vertices[i].X = pmo.Meshes[x].vertices[i].X * pmo.header.ModelScale * Scale;
                    vertices[i].Y = pmo.Meshes[x].vertices[i].Y * pmo.header.ModelScale * Scale;
                    vertices[i].Z = pmo.Meshes[x].vertices[i].Z * pmo.header.ModelScale * Scale;
                    vertices[i].Tu = pmo.Meshes[x].textureCoordinates[i].X;
                    vertices[i].Tv = pmo.Meshes[x].textureCoordinates[i].Y;
                    vertices[i].R = (byte)color.X;
                    vertices[i].G = (byte)color.Y;
                    vertices[i].B = (byte)color.Z;
                    vertices[i].A = 0xFF;
                }

                currentMesh = new MeshDescriptor()
                {
                    Vertices = vertices,
                    Indices = pmo.Meshes[x].Indices.ToArray(),
                    TextureIndex = pmo.Meshes[x].TextureID,
                    IsOpaque = true
                };

                MeshDescriptors.Add(currentMesh);
            }

            if(pmo.header.SkeletonOffset != 0)
            {
                List<Matrix4x4> matrices = new List<Matrix4x4>();
                List<Mdlx.Bone> skeleton = new List<Mdlx.Bone>();

                foreach (Pmo.BoneData boneData in pmo.boneList)
                {
                    Matrix4x4 mtx = new Matrix4x4();
                    mtx.M11 = boneData.Transform[0];
                    mtx.M12 = boneData.Transform[1];
                    mtx.M13 = boneData.Transform[2];
                    mtx.M14 = boneData.Transform[3];
                    mtx.M21 = boneData.Transform[4];
                    mtx.M22 = boneData.Transform[5];
                    mtx.M23 = boneData.Transform[6];
                    mtx.M24 = boneData.Transform[7];
                    mtx.M31 = boneData.Transform[8];
                    mtx.M32 = boneData.Transform[9];
                    mtx.M33 = boneData.Transform[10];
                    mtx.M34 = boneData.Transform[11];
                    mtx.M41 = boneData.Transform[12];
                    mtx.M42 = boneData.Transform[13];
                    mtx.M43 = boneData.Transform[14];
                    mtx.M44 = boneData.Transform[15];
                    mtx = Matrix4x4.Transpose(mtx);
                    matrices.Add(mtx);

                    Vector3 loc;
                    Quaternion quat;
                    Vector3 scl;
                    
                    Matrix4x4.Decompose(mtx, out scl, out quat, out loc);

                    Mdlx.Bone otherBone = new Mdlx.Bone();
                    otherBone.Index = boneData.BoneIndex;
                    otherBone.Parent = (boneData.ParentBoneIndex == 0xFFFF) ? 0 : boneData.ParentBoneIndex;
                    otherBone.TranslationX = loc.X;
                    otherBone.TranslationY = loc.Y;
                    otherBone.TranslationZ = loc.Z;
                    otherBone.TranslationW = 0;
                    otherBone.RotationX = quat.X;
                    otherBone.RotationY = quat.Y;
                    otherBone.RotationZ = quat.Z;
                    otherBone.RotationW = quat.W;
                    otherBone.ScaleX = scl.X;
                    otherBone.ScaleY = scl.Y;
                    otherBone.ScaleZ = scl.Z;
                    otherBone.ScaleW = 1.0f;

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
