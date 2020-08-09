using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using OpenKh.Kh2;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OpenKh.Tools.Kh2MapStudio.Models
{
    class CollisionModel : IDisposable
    {
        private static readonly Color[] ColorPalette = Enumerable
            .Range(0, 40)
            .Select(x => FromHue(x * 78.75f))
            .ToArray();

        private static Color FromHue(float src_h)
        {
            float h = src_h * 2;

            int hi = (int)(h / 60.0f) % 6;
            float f = (h / 60.0f) - hi;
            float q = 1.0f - f;

            return hi switch
            {
                1 => new Color(q, 1f, 0f),
                2 => new Color(0f, 1f, f),
                3 => new Color(0f, q, 1f),
                4 => new Color(f, 0f, 1f),
                5 => new Color(1f, 0f, q),
                _ => new Color(1f, f, 0f),
            };
        }

        private VertexBuffer _vertexBuffer;

        public CollisionModel(Coct coct)
        {
            Coct = coct;
        }


        public Coct Coct { get; }

        public Assimp.Scene Scene => GetScene(Coct);

        public bool IsVisible { get; set; }

        public void Dispose()
        {
            _vertexBuffer?.Dispose();
        }

        public void Draw(GraphicsDevice graphics)
        {
            if (!IsVisible) return;

            _vertexBuffer?.Dispose();
            _vertexBuffer = null;
            if (_vertexBuffer == null)
                _vertexBuffer = CreateVertexBufferForCollision(graphics, Coct);

            graphics.SetVertexBuffer(_vertexBuffer);
            graphics.DrawPrimitives(PrimitiveType.TriangleList, 0, _vertexBuffer.VertexCount);
            graphics.SetVertexBuffer(null);
        }

        private static VertexBuffer CreateVertexBufferForCollision(GraphicsDevice graphics, Coct rawCoct)
        {
            var vb = new VertexBuffer(
                graphics,
                VertexPositionColorTexture.VertexDeclaration,
                GetVertices(rawCoct).Count,
                BufferUsage.WriteOnly);
            vb.SetData(GetVertices(rawCoct).ToArray());

            return vb;
        }

        private static Assimp.Scene GetScene(Coct rawCoct)
        {
            var color = new Color(1f, 1f, 1f, 1f);
            var scene = new Assimp.Scene();
            scene.RootNode = new Assimp.Node("root");
            var coct = new CoctLogical(rawCoct);

            for (int i = 0; i < coct.CollisionMeshGroupList.Count; i++)
            {
                var j = 0;
                var meshGroup = coct.CollisionMeshGroupList[i];
                foreach (var mesh in meshGroup.Meshes)
                {
                    var faceIndex = 0;
                    var sceneMesh = new Assimp.Mesh($"Mesh{i}_{j}", Assimp.PrimitiveType.Triangle);

                    foreach (var item in mesh.Items)
                    {
                        var v1 = coct.VertexList[item.Vertex1];
                        var v2 = coct.VertexList[item.Vertex2];
                        var v3 = coct.VertexList[item.Vertex3];

                        List<VertexPositionColorTexture> vertices;
                        if (item.Vertex4 >= 0)
                        {
                            var v4 = coct.VertexList[item.Vertex4];
                            vertices = GenerateVertex(
                                color,
                                v1.X, v1.Y, v1.Z,
                                v2.X, v2.Y, v2.Z,
                                v3.X, v3.Y, v3.Z,
                                v1.X, v1.Y, v1.Z,
                                v3.X, v3.Y, v3.Z,
                                v4.X, v4.Y, v4.Z).ToList();
                            sceneMesh.Faces.Add(new Assimp.Face(new int[]
                            {
                                faceIndex++, faceIndex++, faceIndex++,
                                faceIndex++, faceIndex++, faceIndex++
                            }));
                        }
                        else
                        {
                            vertices = GenerateVertex(
                                color,
                                v1.X, v1.Y, v1.Z,
                                v2.X, v2.Y, v2.Z,
                                v3.X, v3.Y, v3.Z).ToList();
                            sceneMesh.Faces.Add(new Assimp.Face(new int[]
                            {
                                faceIndex++, faceIndex++, faceIndex++
                            }));
                        }

                        sceneMesh.Vertices.AddRange(vertices
                            .Select(x => new Assimp.Vector3D
                            {
                                X = x.Position.X,
                                Y = x.Position.Y,
                                Z = x.Position.Z,
                            }));
                    }

                    j++;
                    scene.Meshes.Add(sceneMesh);
                }
            }

            scene.RootNode.MeshIndices.AddRange(Enumerable.Range(0, scene.MeshCount));
            return scene;
        }

        private static List<VertexPositionColorTexture> GetVertices(Coct rawCoct)
        {
            var coct = new CoctLogical(rawCoct);
            var vertices = new List<VertexPositionColorTexture>();
            for (int i1 = 0; i1 < coct.CollisionMeshGroupList.Count; i1++)
            {
                var meshGroup = coct.CollisionMeshGroupList[i1];
                foreach (var mesh in meshGroup.Meshes)
                {
                    foreach (var item in mesh.Items)
                    {
                        var color = ColorPalette[(int)Math.Abs((item.PlaneD * ColorPalette.Length)) % ColorPalette.Length];
                        var v1 = coct.VertexList[item.Vertex1];
                        var v2 = coct.VertexList[item.Vertex2];
                        var v3 = coct.VertexList[item.Vertex3];

                        if (item.Vertex4 >= 0)
                        {
                            var v4 = coct.VertexList[item.Vertex4];
                            vertices.AddRange(GenerateVertex(
                                color,
                                v1.X, v1.Y, v1.Z,
                                v2.X, v2.Y, v2.Z,
                                v3.X, v3.Y, v3.Z,
                                v1.X, v1.Y, v1.Z,
                                v3.X, v3.Y, v3.Z,
                                v4.X, v4.Y, v4.Z));
                        }
                        else
                        {
                            vertices.AddRange(GenerateVertex(
                                color,
                                v1.X, v1.Y, v1.Z,
                                v2.X, v2.Y, v2.Z,
                                v3.X, v3.Y, v3.Z));
                        }
                    }
                }
            }

            return vertices;
        }

        private static IEnumerable<VertexPositionColorTexture> GenerateVertex(Color color, params float[] n)
        {
            for (var i = 0; i < n.Length - 2; i += 3)
            {
                yield return new VertexPositionColorTexture
                {
                    Position = new Vector3 { X = n[i], Y = -n[i + 1], Z = -n[i + 2] },
                    Color = color
                };
            }
        }
    }
}
