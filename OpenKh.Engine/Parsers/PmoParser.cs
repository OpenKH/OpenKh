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

                    if(Pmo.GetFlags(pmo.Meshes[x].SectionInfo).UniformDiffuseFlag)
                    {
                        var colorData = BitConverter.GetBytes(pmo.Meshes[x].SectionInfo_opt2.DiffuseColor);

                        color.X = colorData[0] / 255f;
                        color.Y = colorData[1] / 255f;
                        color.Z = colorData[2] / 255f;
                        color.W = colorData[3] / 255f;
                    }
                    else
                    {
                        color.X = pmo.Meshes[x].colors[i].X / 255f;
                        color.Y = pmo.Meshes[x].colors[i].Y / 255f;
                        color.Z = pmo.Meshes[x].colors[i].Z / 255f;
                        color.W = pmo.Meshes[x].colors[i].W / 255f;
                    }

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
        }

        public List<MeshDescriptor> MeshDescriptors { get; private set; }

        public List<Mdlx.Bone> Bones => new List<Mdlx.Bone>();

        Matrix4x4[] IModelMotion.InitialPose => new System.Numerics.Matrix4x4[0];
        Matrix4x4[] IModelMotion.CurrentPose => new System.Numerics.Matrix4x4[0];

        public void ApplyMotion(System.Numerics.Matrix4x4[] matrices)
        {
            
        }
    }
}
