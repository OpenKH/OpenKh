using System;
using System.Collections.Generic;
using System.Text;
using OpenKh.Engine.Motion;
using OpenKh.Kh2;
using OpenKh.Bbs;
using System.Numerics;

namespace OpenKh.Engine.Parsers
{
    public class PmoParser : IModelMotion
    {
        public PmoParser(Pmo pmo, float Scale)
        {
            MeshDescriptors = new List<MeshDescriptor>();
            MeshDescriptor currentMesh = new MeshDescriptor();

            for (int x = 0; x < pmo.Meshes.Count; x++)
            {
                var vertices = new PositionColoredTextured[pmo.Meshes[x].vertices.Count];
                for (var i = 0; i < vertices.Length; i++)
                {
                    Vector4 color;

                    if(Pmo.GetFlags(pmo.meshSection[x]).UniformDiffuseFlag)
                    {
                        byte[] byt = BitConverter.GetBytes(pmo.meshSectionOpt2[x].DiffuseColor);
                        if (BitConverter.IsLittleEndian)
                            Array.Reverse(byt);

                        color.X = 0xFF;
                        color.Y = 0xFF;
                        color.Z = 0xFF;
                        color.W = byt[0];
                    }
                    else
                    {
                        color = pmo.Meshes[x].colors[i];
                        color.Y = 0xFF;
                        color.Z = 0xFF;
                        color.W = 0xFF;
                    }

                    vertices[i].X = pmo.Meshes[x].vertices[i].X * Scale;
                    vertices[i].Y = pmo.Meshes[x].vertices[i].Y * Scale;
                    vertices[i].Z = pmo.Meshes[x].vertices[i].Z * Scale;
                    vertices[i].Tu = pmo.Meshes[x].textureCoordinates[i].X;
                    vertices[i].Tv = pmo.Meshes[x].textureCoordinates[i].Y;
                    vertices[i].R = (byte)color.W;
                    vertices[i].G = (byte)color.Z;
                    vertices[i].B = (byte)color.Y;
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
        }

        public List<MeshDescriptor> MeshDescriptors { get; private set; }

        public List<Mdlx.Bone> Bones => new List<Mdlx.Bone>();

        Matrix4x4[] IModelMotion.InitialPose => new System.Numerics.Matrix4x4[0];

        public void ApplyMotion(System.Numerics.Matrix4x4[] matrices)
        {
            
        }
    }
}
