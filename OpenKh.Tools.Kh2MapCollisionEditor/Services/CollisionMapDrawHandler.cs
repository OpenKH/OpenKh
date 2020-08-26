using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using OpenKh.Kh2;
using System.Collections.Generic;
using System.Linq;

namespace OpenKh.Tools.Kh2MapCollisionEditor.Services
{
    public class CollisionMapDrawHandler
    {
        private static readonly Color[] ColorPalette = new Color[]
        {
                Color.Red,
                Color.Green,
                Color.Blue,
                Color.Green,
                Color.Blue,
                Color.Yellow,
                Color.Cyan,
                Color.Fuchsia,
        };

        private GraphicsDevice _graphicsDevice;
        private BasicEffect _effect;
        private int _width;
        private int _height;

        public CoctLogical Coct { get; set; }
        public Camera Camera { get; private set; }

        public void Initialize(GraphicsDevice graphicsDevice)
        {
            _graphicsDevice = graphicsDevice;
            _graphicsDevice.RasterizerState = new RasterizerState()
            {
                CullMode = CullMode.CullClockwiseFace
            };

            _effect = new BasicEffect(_graphicsDevice)
            {
                VertexColorEnabled = true,
                TextureEnabled = false,
                FogEnabled = true,
                FogEnd = 1400.0f,
                FogStart = 0,
            };

            Camera = new Camera()
            {
                CameraPosition = new Vector3(0, -500, 0)
            };
        }

        public void Destroy()
        {

        }

        public void DrawBegin()
        {
            _graphicsDevice.Clear(Color.Black);
            var renderTarget = _graphicsDevice.GetRenderTargets().First().RenderTarget as RenderTarget2D;
            _width = renderTarget.Width;
            _height = renderTarget.Height;

            const float speed = 1.0f;
            var aspectRatio = _width / (float)_height;
            Camera.AspectRatio = aspectRatio;

            Camera.CameraRotationYawPitchRoll += new Vector3(1 * speed, 0, 0);

            _effect.Projection = Camera.Projection;
            _effect.World = Camera.World;

            foreach (var pass in _effect.CurrentTechnique.Passes)
            {
                pass.Apply();

                if (Coct == null)
                    DrawDummy();
                else
                    DrawCoct(Coct);

            }

        }

        public void DrawEnd()
        {

        }

        private void DrawDummy()
        {
            var v = GenerateVertex(
                0.0f, 0.0f, 0.0f,
                1.0f, 0.0f, 0.0f,
                1.0f, 1.0f, 0.0f
                ).ToArray();

            _graphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, v, 0, v.Length / 3);
        }

        private void DrawCoct(CoctLogical coct)
        {
            var paletteIndex = 0;

            for (int i1 = 0; i1 < coct.CollisionMeshGroupList.Count; i1++)
            {
                var color = ColorPalette[paletteIndex++ % ColorPalette.Length];
                var c1 = coct.CollisionMeshGroupList[i1];
                foreach (var c2 in c1.Meshes)
                {
                    foreach (var c3 in c2.Items)
                    {
                        VertexPositionColorTexture[] vertices;
                        var v1 = coct.VertexList[c3.Vertex1];
                        var v2 = coct.VertexList[c3.Vertex2];
                        var v3 = coct.VertexList[c3.Vertex3];

                        if (c3.Vertex4 >= 0)
                        {
                            var v4 = coct.VertexList[c3.Vertex4];
                            vertices = GenerateVertex(
                                color,
                                v1.X, v1.Y, v1.Z,
                                v2.X, v2.Y, v2.Z,
                                v3.X, v3.Y, v3.Z,
                                v1.X, v1.Y, v1.Z,
                                v3.X, v3.Y, v3.Z,
                                v4.X, v4.Y, v4.Z)
                                .ToArray();
                        }
                        else
                        {
                            vertices = GenerateVertex(
                                color,
                                v1.X, v1.Y, v1.Z,
                                v2.X, v2.Y, v2.Z,
                                v3.X, v3.Y, v3.Z)
                                .ToArray();
                        }

                        _graphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, vertices, 0, vertices.Length / 3);
                    }
                }
            }
        }

        private IEnumerable<VertexPositionColorTexture> GenerateVertex(params float[] n)
        {
            var colorPattern = new Color[]
            {
                Color.Red,
                Color.Green,
                Color.Blue,
                Color.Green,
                Color.Blue,
                Color.Yellow,
            };


            for (var i = 0; i < n.Length - 2; i += 3)
            {
                yield return new VertexPositionColorTexture
                {
                    Position = new Vector3 { X = n[i], Y = n[i + 1], Z = n[i + 2] },
                    Color = colorPattern[i / 3]
                };
            }
        }

        private IEnumerable<VertexPositionColorTexture> GenerateVertex(Color color, params float[] n)
        {
            for (var i = 0; i < n.Length - 2; i += 3)
            {
                yield return new VertexPositionColorTexture
                {
                    Position = new Vector3 { X = n[i], Y = n[i + 1], Z = n[i + 2] },
                    Color = color
                };
            }
        }
    }
}
