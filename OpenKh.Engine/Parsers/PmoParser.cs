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
                    byte maxX = 0xff;
                    byte maxY = 0xff;
                    byte maxZ = 0xff;
                    byte maxW = 0xff;
                    maxX -= (byte)pmo.Meshes[x].colors[i].X;
                    maxY -= (byte)pmo.Meshes[x].colors[i].Y;
                    maxZ -= (byte)pmo.Meshes[x].colors[i].Z;
                    maxW -= (byte)pmo.Meshes[x].colors[i].W;
                    vertices[i].X = pmo.Meshes[x].vertices[i].X * Scale;
                    vertices[i].Y = pmo.Meshes[x].vertices[i].Y * Scale;
                    vertices[i].Z = pmo.Meshes[x].vertices[i].Z * Scale;
                    vertices[i].Tu = pmo.Meshes[x].textureCoordinates[i].X;
                    vertices[i].Tv = pmo.Meshes[x].textureCoordinates[i].Y;
                    vertices[i].R = maxX;
                    vertices[i].G = maxY;
                    vertices[i].B = maxZ;
                    vertices[i].A = maxW;
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
