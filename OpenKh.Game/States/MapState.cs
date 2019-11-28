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
            CameraPosition = new Vector3(0, 316, 238);
            CameraLookAt = new Vector3(0, -40, -30);
            CameraUp = Vector3.UnitZ;

            _mesh = new List<Mesh>
            {
                Mesh.FromSample()
            }
            .Concat(FromMdlx(_graphics.GraphicsDevice, "obj/P_EX100.mdlx"))
            .ToList();
        }

        public void Destroy()
        {
        }

        public void Update(DeltaTimes deltaTimes)
        {
            const double Speed = 1.0;
            var speed = (float)(deltaTimes.DeltaTime * Speed);

            if (_input.Up) CameraPosition += Vector3.Multiply(CameraLookAt, speed);
            if (_input.Down) CameraPosition -= Vector3.Multiply(CameraLookAt, speed);
        }

        public void Draw(DeltaTimes deltaTimes)
        {
            var aspectRatio = _graphics.PreferredBackBufferWidth / (float)_graphics.PreferredBackBufferHeight;
            var nearClipPlane = 1;
            var farClipPlane = 1000;

            _effect.Projection = Matrix.CreatePerspectiveFieldOfView(
                (float)fieldOfView, aspectRatio, nearClipPlane, farClipPlane);
            _effect.View = Matrix.CreateLookAt(CameraPosition, CameraPosition + CameraLookAt, CameraUp);

            foreach (var pass in _effect.CurrentTechnique.Passes)
            {
                pass.Apply();

                foreach (var mesh in _mesh)
                {
                    if (_effect.Texture != mesh.Texture)
                    {
                        _effect.Texture = mesh.Texture;
                        _effect.TextureEnabled = _effect.Texture != null;
                        pass.Apply();
                    }

                    _graphics.GraphicsDevice.DrawUserPrimitives(
                        mesh.PrimitiveType, mesh.Vertices, mesh.Start, mesh.Count);
                }
            }
        }

        private static Mesh[] FromMdlx(GraphicsDevice graphics, string fileName)
        {
            var barEntries = File.OpenRead(fileName).Using(stream => Bar.Read(stream));
            var entry = barEntries.FirstOrDefault(x => x.Type == Bar.EntryType.Vif);
            if (entry != null)
            {
                var mdlx = new Mdlx(entry.Stream);
                var models = new MdlxParser(mdlx).Models;

                return models.Select(model =>
                {
                    var vertices = model.Vertices.Select(vertex => new VertexPositionColorTexture
                    {
                        Position = new Vector3(vertex.X, vertex.Y, vertex.Z),
                        TextureCoordinate = new Vector2(vertex.U, vertex.V),
                        Color = new Color((uint)vertex.Color)
                    }).ToArray();

                    return new Mesh(vertices, PrimitiveType.TriangleList);
                })
                .Select((model, index) =>
                {
                    var path = $@"D:\Hacking\KH2\reseach\mdlx\P_EX100 export\tim_ (07)\tex{index} (19)_1.png";
                    model.Texture = File.OpenRead(path).Using(stream => Texture2D.FromStream(graphics, stream));
                    
                    return model;
                })
                .ToArray();

            }

            return Array.Empty<Mesh>();
        }
    }
}
