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
            var vertices = new PositionColoredTextured[pmo.vertices.Count];
            for (var i = 0; i < vertices.Length; i++)
            {
                byte maxX = 0xff;
                byte maxY = 0xff;
                byte maxZ = 0xff;
                byte maxW = 0xff;
                maxX -= (byte)pmo.colors[i].X;
                maxY -= (byte)pmo.colors[i].Y;
                maxZ -= (byte)pmo.colors[i].Z;
                maxW -= (byte)pmo.colors[i].W;
                vertices[i].X = pmo.vertices[i].X * Scale;
                vertices[i].Y = pmo.vertices[i].Y * Scale;
                vertices[i].Z = pmo.vertices[i].Z * Scale;
                vertices[i].Tu = pmo.textureCoordinates[i].X;
                vertices[i].Tv = pmo.textureCoordinates[i].Y;
                vertices[i].R = maxX;
                vertices[i].G = maxY;
                vertices[i].B = maxZ;
                vertices[i].A = maxW;
            }

            MeshDescriptor meshDescriptor;

            meshDescriptor = new MeshDescriptor()
            {
                Vertices = vertices,
                Indices = pmo.Indices.ToArray(),
                TextureIndex = 0,
                IsOpaque = true
            };

            MeshDescriptors = new List<MeshDescriptor>();
            MeshDescriptors.Add(meshDescriptor);
        }

        public List<MeshDescriptor> MeshDescriptors { get; private set; }

        public List<Mdlx.Bone> Bones => new List<Mdlx.Bone>();

        Matrix4x4[] IModelMotion.InitialPose => new System.Numerics.Matrix4x4[0];

        public void ApplyMotion(System.Numerics.Matrix4x4[] matrices)
        {
            
        }
    }
}
