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
        public PmoParser(Pmo pmo)
        {
            var vertices = new PositionColoredTextured[pmo.vertices.Count];
            for (var i = 0; i < vertices.Length; i++)
            {
                vertices[i].X = pmo.vertices[i].X;
                vertices[i].Y = pmo.vertices[i].Y;
                vertices[i].Z = pmo.vertices[i].Z;
                vertices[i].Tu = pmo.textureCoordinates[i].X;
                vertices[i].Tv = pmo.textureCoordinates[i].Y;
                vertices[i].R = 0xFF;
                vertices[i].G = 0xFF;
                vertices[i].B = 0xFF;
                vertices[i].A = 0xFF;
            }

            int[] indices = new int[pmo.vertices.Count];
            for (int i = 0; i < indices.Length; i++)
                indices[i] = i;

            MeshDescriptor meshDescriptor;

            meshDescriptor = new MeshDescriptor()
            {
                Vertices = vertices,
                Indices = indices,
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
