using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using OpenKh.Common;
using OpenKh.Engine.Parsers;
using OpenKh.Game.Infrastructure;
using OpenKh.Game.Models;
using OpenKh.Kh2;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace OpenKh.Game.States
{
    public class MapState : IState
    {
        private GraphicsDeviceManager _graphics;
        private InputManager _input;
        private List<Mesh> _models;
        private BasicEffect _effect;
        private double fieldOfView;
        private Vector3 _cameraYpr;

        public double FieldOfView { get => fieldOfView; set => fieldOfView = Math.Max(0, Math.Min(Math.PI, value)); }
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

        public void Initialize(StateInitDesc initDesc)
        {
            _graphics = initDesc.GraphicsDevice;
            _input = initDesc.InputManager;
            _effect = new BasicEffect(_graphics.GraphicsDevice);

            FieldOfView = MathHelper.PiOver4 * 2;
            CameraPosition = new Vector3(0, 500, 0);
            CameraLookAt = new Vector3(0, 0, 0);
            CameraUp = Vector3.UnitY;
            CameraRotationYawPitchRoll = new Vector3(-180, 0, 10);

            _models = new List<Mesh>
            {
                FromMdlx(_graphics.GraphicsDevice, "obj/P_EX100.mdlx"),
            };
        }

        public void Destroy()
        {
        }

        public void Update(DeltaTimes deltaTimes)
        {
            const double Speed = 100.0;
            var speed = (float)(deltaTimes.DeltaTime * Speed);

            if (_input.W) CameraPosition += Vector3.Multiply(CameraLookAt, speed * 5);
            if (_input.S) CameraPosition -= Vector3.Multiply(CameraLookAt, speed * 100);

            if (_input.Up) CameraRotationYawPitchRoll += new Vector3(0, 0, 1 * speed);
            if (_input.Down) CameraRotationYawPitchRoll -= new Vector3(0, 0, 1 * speed);
            if (_input.Left) CameraRotationYawPitchRoll += new Vector3(1 * speed, 0, 0);
            if (_input.Right) CameraRotationYawPitchRoll -= new Vector3(1 * speed, 0, 0);
        }

        public void Draw(DeltaTimes deltaTimes)
        {
            var aspectRatio = _graphics.PreferredBackBufferWidth / (float)_graphics.PreferredBackBufferHeight;
            var nearClipPlane = 1;
            var farClipPlane = 100000;

            _effect.Projection = Matrix.CreatePerspectiveFieldOfView(
                (float)fieldOfView, aspectRatio, nearClipPlane, farClipPlane);
            _effect.World = Matrix.CreateLookAt(CameraPosition, CameraPosition + CameraLookAt, CameraUp);

            _effect.GraphicsDevice.RasterizerState = new RasterizerState()
            {
                CullMode = CullMode.CullClockwiseFace
            };

            foreach (var pass in _effect.CurrentTechnique.Passes)
            {
                pass.Apply();

                foreach (var mesh in _models)
                {
                    //if (_effect.Texture != mesh.Texture)
                    //{
                    //    _effect.Texture = mesh.Texture;
                    //    _effect.TextureEnabled = _effect.Texture != null;
                    //    pass.Apply();
                    //}

                    //_graphics.GraphicsDevice.DrawUserPrimitives(
                    //    mesh.PrimitiveType, mesh.Vertices, mesh.Start, mesh.Count);

                    var index = 0;
                    foreach (var part in mesh.Parts)
                    {
                        //using (var vb = CreateVertexBuffer(mesh.Segments[part.SegmentId]))
                        //{
                        //    using (var ib = CreateIndexBuffer(part))
                        //    {
                        //        _graphics.GraphicsDevice.SetVertexBuffer(vb);
                        //        _graphics.GraphicsDevice.Indices = ib;
                        //        _graphics.GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, part.Indices.Length / 3);
                        //    }
                        //}

                        var texture = mesh.Textures[part.TextureId & 0xffff];
                        if (_effect.Texture != texture)
                        {
                            _effect.Texture = texture;
                            _effect.TextureEnabled = _effect.Texture != null;
                            pass.Apply();
                        }

                        _graphics.GraphicsDevice.DrawUserIndexedPrimitives(
                            PrimitiveType.TriangleList,
                            mesh.Segments[index].Vertices,
                            0,
                            mesh.Segments[index].Vertices.Length,
                            part.Indices,
                            0,
                            part.Indices.Length / 3);

                        index = (index + 1) % mesh.Segments.Length;
                    }
                }
            }
        }

        private static Mesh FromMdlx(GraphicsDevice graphics, string fileName)
        {
            var barEntries = File.OpenRead(fileName).Using(stream => Bar.Read(stream));
            var entry = barEntries.FirstOrDefault(x => x.Type == Bar.EntryType.Vif);
            if (entry != null)
            {
                var mdlx = Mdlx.Read(entry.Stream);
                var model = new MdlxParser(mdlx).Model;

                return new Mesh
                {
                    Segments = model.Segments.Select(segment => new Mesh.Segment
                    {
                        Vertices = segment.Vertices.Select(vertex => new VertexPositionColorTexture
                        {
                            Position = new Vector3(vertex.X, vertex.Y, vertex.Z),
                            TextureCoordinate = new Vector2(vertex.U, vertex.V),
                            Color = new Color((uint)vertex.Color)
                        }).ToArray()
                    }).ToArray(),
                    Parts = model.Parts.Select(part => new Mesh.Part
                    {
                        Indices = part.Indices,
                        SegmentId = part.SegmentId,
                        TextureId = part.TextureId
                    }).ToArray(),
                    Textures = Enumerable.Range(0, model.Parts.Max(part => part.TextureId & 15) + 1)
                        .Select(i => $@"..\reseach\mdlx\P_EX100 export\tim_ (07)\tex{i} (19)_1.png")
                        .Select(texFileName => File.OpenRead(texFileName).Using(stream => Texture2D.FromStream(graphics, stream)))
                        .ToArray()
                };
            }

            return null;
        }
    }
}
