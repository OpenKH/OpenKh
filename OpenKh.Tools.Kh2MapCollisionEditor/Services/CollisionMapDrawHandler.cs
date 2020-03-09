using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using OpenKh.Kh2;
using System;
using System.Collections.Generic;
using System.Linq;
using Xe.Drawing;

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
        private double _fieldOfView;
        private Vector3 _cameraYpr;

        public Coct Coct { get; set; }
        public double FieldOfView { get => _fieldOfView; set => _fieldOfView = Math.Max(0, Math.Min(Math.PI, value)); }
        public Vector3 CameraPosition { get; set; }
        public Vector3 CameraLookAt { get; set; }
        public Vector3 CameraRotationYawPitchRoll
        {
            get => _cameraYpr;
            set
            {
                _cameraYpr = value;
                var matrix = Matrix.CreateFromYawPitchRoll(value.X / 180.0f * 3.14159f, value.Y / 180.0f * 3.14159f, value.Z / 180.0f * 3.14159f);
                CameraLookAt = Vector3.Transform(new Vector3(1, 0, 0), matrix);
            }
        }
        public Vector3 CameraUp { get; set; }

        public void Initialize(GraphicsDevice graphicsDevice)
        {
            _graphicsDevice = graphicsDevice;
            _graphicsDevice.RasterizerState = new RasterizerState()
            {
                CullMode = CullMode.CullClockwiseFace
            };

            FieldOfView = MathHelper.PiOver4 * 2;
            CameraPosition = new Vector3(0, -500, 0);
            CameraLookAt = new Vector3(0, 0, 0);
            CameraUp = Vector3.UnitY;
            CameraRotationYawPitchRoll = new Vector3(-180, 0, 10);

            _effect = new BasicEffect(_graphicsDevice)
            {
                VertexColorEnabled = true,
                TextureEnabled = false,
                FogEnabled = true,
                FogEnd = 1400.0f,
                FogStart = 0,
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
            var nearClipPlane = 1;
            var farClipPlane = int.MaxValue;

            CameraRotationYawPitchRoll += new Vector3(1 * speed, 0, 0);
            _effect.Projection = Matrix.CreatePerspectiveFieldOfView(
                (float)FieldOfView, aspectRatio, nearClipPlane, farClipPlane);
            _effect.World = Matrix.CreateLookAt(CameraPosition, CameraPosition + CameraLookAt, CameraUp);

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

        private void DrawCoct(Coct coct)
        {
            var paletteIndex = 0;

            for (int i1 = 0; i1 < coct.Collision1.Count; i1++)
            {
                var c1 = coct.Collision1[i1];
                foreach (var c2 in c1.Meshes)
                {
                    var color = ColorPalette[paletteIndex++ % ColorPalette.Length];
                    foreach (var c3 in c2.Items)
                    {
                        VertexPositionColorTexture[] vertices;
                        var v1 = coct.CollisionVertices[c3.Vertex1];
                        var v2 = coct.CollisionVertices[c3.Vertex2];
                        var v3 = coct.CollisionVertices[c3.Vertex3];

                        if (c3.Vertex4 >= 0)
                        {
                            var v4 = coct.CollisionVertices[c3.Vertex4];
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
