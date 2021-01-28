using System;
using System.Collections.Generic;
using OpenKh.Engine.Motion;
using OpenKh.Kh2;
using OpenKh.Bbs;
using System.Numerics;
using Assimp;

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
                    vertices[i].R = 0xFF;
                    vertices[i].G = 0xFF;
                    vertices[i].B = 0xFF;
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

            foreach (Pmo.BoneData boneData in pmo.boneList)
            {
                Mdlx.Bone otherBone = new Mdlx.Bone();
                otherBone.Index = boneData.BoneIndex;
                otherBone.Parent = boneData.ParentBoneIndex;
                otherBone.TranslationX = boneData.Transform[0];
                otherBone.TranslationY = boneData.Transform[1];
                otherBone.TranslationZ = boneData.Transform[2];
                otherBone.TranslationW = boneData.Transform[3];
                otherBone.RotationX = boneData.Transform[4];
                otherBone.RotationY = boneData.Transform[5];
                otherBone.RotationZ = boneData.Transform[6];
                otherBone.RotationW = boneData.Transform[7];
                otherBone.ScaleX = boneData.Transform[8];
                otherBone.ScaleY = boneData.Transform[9];
                otherBone.ScaleZ = boneData.Transform[10];
                otherBone.ScaleW = boneData.Transform[11];

                Bones.Add(otherBone);
            }
        }

        public List<MeshDescriptor> MeshDescriptors { get; private set; }

        public List<Mdlx.Bone> Bones => new List<Mdlx.Bone>();

        System.Numerics.Matrix4x4[] IModelMotion.InitialPose => new System.Numerics.Matrix4x4[0];

        System.Numerics.Matrix4x4[] IModelMotion.CurrentPose => new System.Numerics.Matrix4x4[0];

        public void ApplyMotion(System.Numerics.Matrix4x4[] matrices)
        {
            
        }
    }
}
