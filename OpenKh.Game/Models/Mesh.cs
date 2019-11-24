using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace OpenKh.Game.Models
{
    public class Mesh
    {
        public PrimitiveType PrimitiveType { get; set; }
        public VertexPositionTexture[] Vertices { get; set; }
        public int Start { get; set; }
        public int Count { get; set; }

        public static Mesh FromSample()
        {
            var vertices = new VertexPositionTexture[6];
            vertices[0].Position = new Vector3(-20, -20, 0);
            vertices[1].Position = new Vector3(-20, 20, 0);
            vertices[2].Position = new Vector3(20, -20, 0);
            vertices[3].Position = vertices[1].Position;
            vertices[4].Position = new Vector3(20, 20, 0);
            vertices[5].Position = vertices[2].Position;

            return new Mesh
            {
                PrimitiveType = PrimitiveType.TriangleList,
                Vertices = vertices,
                Start = 0,
                Count = 2
            };
        }
    }
}
