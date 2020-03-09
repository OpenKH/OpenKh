using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using OpenKh.Engine.Parsers;
using OpenKh.Game.Extensions;
using OpenKh.Game.Infrastructure;
using OpenKh.Game.Models;
using OpenKh.Kh2;
using System.Collections.Generic;
using System.Linq;

namespace OpenKh.Game.States
{
    public class MapState : IState
    {
        private ArchiveManager _archiveManager;
        private GraphicsDeviceManager _graphics;
        private InputManager _input;
        private List<Mesh> _models = new List<Mesh>();
        private BasicEffect _effect;
        private Camera _camera;
        private const int _languageId = 0;

        public void Initialize(StateInitDesc initDesc)
        {
            _archiveManager = initDesc.ArchiveManager;
            _graphics = initDesc.GraphicsDevice;
            _input = initDesc.InputManager;
            _effect = new BasicEffect(_graphics.GraphicsDevice);
            _camera = new Camera()
            {
                CameraPosition = new Vector3(0, 500, 0),
                CameraRotationYawPitchRoll = new Vector3(90, 0, 10),
            };

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

            if (_input.W) _camera.CameraPosition += Vector3.Multiply(_camera.CameraLookAt, speed * 5);
            if (_input.S) _camera.CameraPosition -= Vector3.Multiply(_camera.CameraLookAt, speed * 5);

            if (_input.Up) _camera.CameraRotationYawPitchRoll += new Vector3(0, 0, 1 * speed);
            if (_input.Down) _camera.CameraRotationYawPitchRoll -= new Vector3(0, 0, 1 * speed);
            if (_input.Left) _camera.CameraRotationYawPitchRoll += new Vector3(1 * speed, 0, 0);
            if (_input.Right) _camera.CameraRotationYawPitchRoll -= new Vector3(1 * speed, 0, 0);
        }

        public void Draw(DeltaTimes deltaTimes)
        {
            _camera.AspectRatio = _graphics.PreferredBackBufferWidth / (float)_graphics.PreferredBackBufferHeight;

            _effect.VertexColorEnabled = true;
            _effect.Projection = _camera.Projection;
            _effect.World = _camera.World;

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

            _archiveManager.LoadArchive(fileName);
            AddMesh(FromMdlx(_graphics.GraphicsDevice, _archiveManager, "MAP"));
            AddMesh(FromMdlx(_graphics.GraphicsDevice, _archiveManager, "SK0"));
        }

        private static Mesh FromMdlx(GraphicsDevice graphics, ArchiveManager archiveManager, string name)
        {
            var mdlx = archiveManager.Get<Mdlx>(name);
            var textures = archiveManager.Get<ModelTexture>(name);
            if (mdlx == null || textures == null)
                return null;

            var model = new MdlxParser(mdlx).Model;

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
