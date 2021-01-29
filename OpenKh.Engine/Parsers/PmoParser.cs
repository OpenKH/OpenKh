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
                        byte[] byt = BitConverter.GetBytes(pmo.Meshes[x].SectionInfo_opt2.DiffuseColor);

                        color.X = byt[0];
                        color.Y = byt[1];
                        color.Z = byt[2];
                        color.W = byt[3];
                    }
                    else
                    {
                        color.X = pmo.Meshes[x].colors[i].X;
                        color.Y = pmo.Meshes[x].colors[i].Y;
                        color.Z = pmo.Meshes[x].colors[i].Z;
                        color.W = pmo.Meshes[x].colors[i].W;
                    }

                    vertices[i].X = pmo.Meshes[x].vertices[i].X * pmo.header.ModelScale * Scale;
                    vertices[i].Y = pmo.Meshes[x].vertices[i].Y * pmo.header.ModelScale * Scale;
                    vertices[i].Z = pmo.Meshes[x].vertices[i].Z * pmo.header.ModelScale * Scale;
                    vertices[i].Tu = pmo.Meshes[x].textureCoordinates[i].X;
                    vertices[i].Tv = pmo.Meshes[x].textureCoordinates[i].Y;
                    vertices[i].R = (byte)color.X;
                    vertices[i].G = (byte)color.Y;
                    vertices[i].B = (byte)color.Z;
                    vertices[i].A = (byte)color.W;
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
