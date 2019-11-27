using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using OpenKh.Game.Infrastructure;
using OpenKh.Game.Models;
using OpenKh.Common;
using OpenKh.Engine.Parsers;
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
        private List<Mesh> _mesh;
        private BasicEffect _effect;
        private double fieldOfView;

        public double FieldOfView { get => fieldOfView; set => fieldOfView = Math.Max(0, Math.Min(Math.PI, value)); }
        public Vector3 CameraPosition { get; set; }
        public Vector3 CameraLookAt { get; set; }
        public Vector3 CameraUp { get; set; }

        public void Initialize(StateInitDesc initDesc)
        {
            _graphics = initDesc.GraphicsDevice;
            _input = initDesc.InputManager;
            _effect = new BasicEffect(_graphics.GraphicsDevice);

            FieldOfView = MathHelper.PiOver4;
            CameraPosition = new Vector3(0, 40, 30);
            CameraLookAt = Vector3.Zero;
            CameraUp = Vector3.UnitZ;

            _mesh = new List<Mesh>
            {
                Mesh.FromSample()
            }
            .Concat(FromMdlx("obj/P_EX100.mdlx"))
            .ToList();
        }

        public void Destroy()
        {
        }

        public void Update(DeltaTimes deltaTimes)
        {
            FieldOfView += (float)(deltaTimes.DeltaTime * 0.5);
        }

        public void Draw(DeltaTimes deltaTimes)
        {
            var aspectRatio = _graphics.PreferredBackBufferWidth / (float)_graphics.PreferredBackBufferHeight;
            var nearClipPlane = 1;
            var farClipPlane = 200;

            _effect.Projection = Matrix.CreatePerspectiveFieldOfView(
                (float)fieldOfView, aspectRatio, nearClipPlane, farClipPlane);
            _effect.View = Matrix.CreateLookAt(CameraPosition, CameraLookAt, CameraUp);

            foreach (var pass in _effect.CurrentTechnique.Passes)
            {
                pass.Apply();

                foreach (var mesh in _mesh)
                {
                    _graphics.GraphicsDevice.DrawUserPrimitives(
                        mesh.PrimitiveType, mesh.Vertices, mesh.Start, mesh.Count);
                }
            }
        }

        private static Mesh[] FromMdlx(string fileName)
        {
            var barEntries = File.OpenRead(fileName).Using(stream => Bar.Read(stream));
            var entry = barEntries.FirstOrDefault(x => x.Type == Bar.EntryType.Vif);
            if (entry != null)
            {
                var mdlx = new Mdlx(entry.Stream);
                var models = new MdlxParser(mdlx).Models;

                return models.Select(model =>
                {
                    var vertices = model.Vertices.Select(vertex => new VertexPositionTexture
                    {
                        Position = new Vector3(vertex.X, vertex.Y, vertex.Z),
                        TextureCoordinate = new Vector2(vertex.Tu, vertex.Tv)
                    }).ToArray();

                    return new Mesh(vertices, PrimitiveType.TriangleList);
                }).ToArray();

            }

            return Array.Empty<Mesh>();
        }
    }
}
