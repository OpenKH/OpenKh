using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace OpenKh.Game.Models
{
    public class Mesh
    {
        public PrimitiveType PrimitiveType { get; set; }
        public VertexPositionColorTexture[] Vertices { get; set; }
        public int Start { get; set; }
        public int Count { get; set; }
        public Texture2D Texture { get; set; }

        public Mesh(VertexPositionColorTexture[] vertices, PrimitiveType primitiveType)
        {
            PrimitiveType = primitiveType;
            Vertices = vertices;
            Start = 0;
            Count = GetPrimitiveCount(vertices.Length, PrimitiveType.TriangleList);
        }

        public static Mesh FromSample(Texture2D texture = null)
        {
            var vertices = new VertexPositionColorTexture[6];
            vertices[0].Position = new Vector3(-20, -20, 0);
            vertices[1].Position = new Vector3(-20, 20, 0);
            vertices[2].Position = new Vector3(20, -20, 0);
            vertices[3].Position = vertices[1].Position;
            vertices[4].Position = new Vector3(20, 20, 0);
            vertices[5].Position = vertices[2].Position;

            vertices[0].TextureCoordinate = new Vector2(0, 0);
            vertices[1].TextureCoordinate = new Vector2(0, 1);
            vertices[2].TextureCoordinate = new Vector2(1, 0);

            vertices[3].TextureCoordinate = vertices[1].TextureCoordinate;
            vertices[4].TextureCoordinate = new Vector2(1, 1);
            vertices[5].TextureCoordinate = vertices[2].TextureCoordinate;

            return new Mesh(vertices, PrimitiveType.TriangleList)
            {
                Texture = texture
            };
        }

        private static int GetPrimitiveCount(int verticesCount, PrimitiveType primitiveType)
        {
            switch (primitiveType)
            {
                case PrimitiveType.TriangleList:
                    return verticesCount / 3;
                default:
                    throw new NotImplementedException();
            }
        }
    }
}
