using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using OpenKh.Common;
using OpenKh.Engine.Parsers;
using OpenKh.Game.Extensions;
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
        private List<Mesh> _models = new List<Mesh>();
        private BasicEffect _effect;
        private double fieldOfView;
        private Vector3 _cameraYpr;
        private const int _languageId = 0;

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

            _graphics.GraphicsDevice.BlendState = BlendState.NonPremultiplied;

            LoadMap(2, 4);
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
            var farClipPlane = int.MaxValue;

            _effect.VertexColorEnabled = true;
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
                    var index = 0;
                    foreach (var part in mesh.Parts)
                    {
                        if (part.Indices.Length == 0)
                            continue;

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

        private void LoadMap(int worldIndex, int mapIndex)
        {
            var fileName = $"map/{Constants.Languages[_languageId]}/{Constants.WorldIds[worldIndex]}{mapIndex:D02}.map";

            _models.Clear();

            var entries = File.OpenRead(fileName).Using(stream => Bar.Read(stream));
            AddMesh(FromMdlx(_graphics.GraphicsDevice, entries, "MAP"));
            AddMesh(FromMdlx(_graphics.GraphicsDevice, entries, "SK0"));
        }

        private static Mesh FromMdlx(GraphicsDevice graphics, string fileName)
        {
            var barEntries = File.OpenRead(fileName).Using(stream => Bar.Read(stream));
            var modelEntry = barEntries.FirstOrDefault(x => x.Type == Bar.EntryType.Vif);
            var textureEntry = barEntries.FirstOrDefault(x => x.Type == Bar.EntryType.Tim2);
            if (modelEntry != null && textureEntry != null)
            {
                FromMdlx(graphics, modelEntry.Stream, textureEntry.Stream);
            }

            return null;
        }

        private static Mesh FromMdlx(GraphicsDevice graphics, IEnumerable<Bar.Entry> entries, string name)
        {
            var modelEntry = entries.FirstOrDefault(x => x.Type == Bar.EntryType.Vif && x.Name == name);
            var textureEntry = entries.FirstOrDefault(x => x.Type == Bar.EntryType.Tim2 && x.Name == name);
            if (modelEntry != null && textureEntry != null)
                return FromMdlx(graphics, modelEntry.Stream, textureEntry.Stream);

            return null;
        }

        private static Mesh FromMdlx(GraphicsDevice graphics, Stream mdlxStream, Stream texturesStream)
        {
            var mdlx = Mdlx.Read(mdlxStream);
            var model = new MdlxParser(mdlx).Model;
            var textures = ModelTexture.Read(texturesStream);

            return new Mesh
            {
                Segments = model.Segments.Select(segment => new Mesh.Segment
                {
                    Vertices = segment.Vertices.Select(vertex => new VertexPositionColorTexture
                    {
                        Position = new Vector3(vertex.X, vertex.Y, vertex.Z),
                        TextureCoordinate = new Vector2(vertex.U, vertex.V),
                        Color = new Color((vertex.Color >> 16) & 0xff, (vertex.Color >> 8) & 0xff, vertex.Color & 0xff, (vertex.Color >> 24) & 0xff)
                    }).ToArray()
                }).ToArray(),
                Parts = model.Parts.Select(part => new Mesh.Part
                {
                    Indices = part.Indices,
                    SegmentId = part.SegmentId,
                    TextureId = part.TextureId
                }).ToArray(),
                Textures = textures.Images.Select(texture => texture.CreateTexture(graphics)).ToArray()
            };
        }

        private void AddMesh(Mesh mesh)
        {
            if (mesh == null)
                return;

            _models.Add(mesh);
        }
    }
}
