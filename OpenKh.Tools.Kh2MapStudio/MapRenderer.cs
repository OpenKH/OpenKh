using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using OpenKh.Common;
using OpenKh.Engine.MonoGame;
using OpenKh.Kh2;
using OpenKh.Kh2.Extensions;
using OpenKh.Kh2.Models;
using OpenKh.Tools.Kh2MapStudio.Interfaces;
using OpenKh.Tools.Kh2MapStudio.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Controls;

namespace OpenKh.Tools.Kh2MapStudio
{
    class MapRenderer : ILayerController
    {
        private readonly static BlendState DefaultBlendState = new BlendState()
        {
            ColorSourceBlend = Blend.SourceAlpha,
            AlphaSourceBlend = Blend.SourceAlpha,
            ColorDestinationBlend = Blend.InverseSourceAlpha,
            AlphaDestinationBlend = Blend.InverseSourceAlpha,
            ColorBlendFunction = BlendFunction.Add,
            AlphaBlendFunction = BlendFunction.Add,
            BlendFactor = Color.White,
            MultiSampleMask = int.MaxValue,
            IndependentBlendEnable = false
        };

        private readonly GraphicsDeviceManager _graphicsManager;
        private readonly GraphicsDevice _graphics;
        private readonly KingdomShader _shader;
        private bool _showBobs;

        public Camera Camera { get; }

        public bool? ShowMap
        {
            get => MapMeshGroups.FirstOrDefault(x => x.Name == "MAP")?.IsVisible;
            set
            {
                var mesh = MapMeshGroups.FirstOrDefault(x => x.Name == "MAP");
                if (mesh != null)
                    mesh.IsVisible = value ?? true;
            }
        }

        public bool? ShowSk0
        {
            get => MapMeshGroups.FirstOrDefault(x => x.Name == "SK0")?.IsVisible;
            set
            {
                var mesh = MapMeshGroups.FirstOrDefault(x => x.Name == "SK0");
                if (mesh != null)
                    mesh.IsVisible = value ?? true;
            }
        }

        public bool? ShowSk1
        {
            get => MapMeshGroups.FirstOrDefault(x => x.Name == "SK1")?.IsVisible;
            set
            {
                var mesh = MapMeshGroups.FirstOrDefault(x => x.Name == "SK1");
                if (mesh != null)
                    mesh.IsVisible = value ?? true;
            }
        }

        public bool? ShowBobs
        {
            get => BobDescriptors.Any() ? (bool?)_showBobs : null;
            set => _showBobs = value ?? true;
        }
        public List<Bar.Entry> MapBarEntries { get; private set; }
        public List<Bar.Entry> ArdBarEntries { get; private set; }
        internal List<MeshGroupModel> MapMeshGroups { get; }
        internal List<MeshGroupModel> BobMeshGroups { get; }
        public List<BobDescriptor> BobDescriptors { get; }

        public MapRenderer(ContentManager content, GraphicsDeviceManager graphics)
        {
            _graphicsManager = graphics;
            _graphics = graphics.GraphicsDevice;
            _shader = new KingdomShader(content);
            MapMeshGroups = new List<MeshGroupModel>();
            BobMeshGroups = new List<MeshGroupModel>();
            BobDescriptors = new List<BobDescriptor>();
            Camera = new Camera()
            {
                CameraPosition = new Vector3(0, 100, 200),
                CameraRotationYawPitchRoll = new Vector3(90, 0, 10),
            };
        }

        public void OpenMap(string fileName)
        {
            Close();
            MapBarEntries = File.OpenRead(fileName).Using(Bar.Read);
            LoadMapComponent(MapBarEntries, "SK0");
            LoadMapComponent(MapBarEntries, "SK1");
            LoadMapComponent(MapBarEntries, "MAP");

            var bobDescEntry = MapBarEntries
                .Where(x => x.Name == "out" && x.Type == Bar.EntryType.BobDescriptor)
                .FirstOrDefault();
            if (bobDescEntry != null)
                BobDescriptors.AddRange(BobDescriptor.Read(bobDescEntry.Stream));

            var bobModel = MapBarEntries.Where(x => x.Name == "BOB" && x.Type == Bar.EntryType.Model).ToArray();
            var bobTexture = MapBarEntries.Where(x => x.Name == "BOB" && x.Type == Bar.EntryType.ModelTexture).ToArray();
            var bobCount = Math.Min(bobModel.Length, bobTexture.Length);
            for (var i = 0; i < bobCount; i++)
            {
                var model = Mdlx.Read(bobModel[i].Stream);
                var textures = ModelTexture.Read(bobTexture[i].Stream).Images;
                BobMeshGroups.Add(new MeshGroupModel(_graphics, "BOB", model, textures, i));
            }
        }

        public void SaveMap(string fileName)
        {
            var memStream = new MemoryStream();
            BobDescriptor.Write(memStream, BobDescriptors);

            MapBarEntries.AddOrReplace(new Bar.Entry
            {
                Name = "out",
                Type = Bar.EntryType.BobDescriptor,
                Stream = memStream
            });

            File.Create(fileName).Using(stream => Bar.Write(stream, MapBarEntries));
        }

        public void OpenArd(string fileName)
        {
            ArdBarEntries = File.OpenRead(fileName).Using(Bar.Read);
        }

        public void SaveArd(string fileName)
        {
            File.Create(fileName).Using(stream => Bar.Write(stream, ArdBarEntries));
        }

        public void Close()
        {
            _showBobs = true;

            foreach (var meshGroup in MapMeshGroups)
                meshGroup?.Dispose();
            MapMeshGroups.Clear();

            foreach (var meshGroup in BobMeshGroups)
                meshGroup?.Dispose();
            BobMeshGroups.Clear();
            BobDescriptors.Clear();
        }

        public void Update(float deltaTime)
        {

        }

        public void Draw()
        {
            Camera.AspectRatio = _graphicsManager.PreferredBackBufferWidth / (float)_graphicsManager.PreferredBackBufferHeight;

            _graphics.RasterizerState = new RasterizerState()
            {
                CullMode = CullMode.CullClockwiseFace
            };
            _graphics.DepthStencilState = new DepthStencilState();
            _graphics.BlendState = DefaultBlendState;

            _shader.Pass(pass =>
            {
                _shader.ProjectionView = Camera.Projection;
                _shader.WorldView = Camera.World;
                _shader.ModelView = Matrix.Identity;
                pass.Apply();

                foreach (var mesh in MapMeshGroups.Where(x => x.IsVisible))
                    RenderMeshNew(pass, mesh.MeshGroup, true);
                foreach (var mesh in MapMeshGroups.Where(x => x.IsVisible))
                    RenderMeshNew(pass, mesh.MeshGroup, false);

                if (_showBobs)
                {
                    foreach (var entity in BobDescriptors ?? new List<BobDescriptor>())
                    {
                        if (entity.BobIndex < 0 || entity.BobIndex >= BobMeshGroups.Count)
                            continue;

                        _shader.ModelView = Matrix.CreateRotationX(entity.RotationX) *
                            Matrix.CreateRotationY(entity.RotationY) *
                            Matrix.CreateRotationZ(entity.RotationZ) *
                            Matrix.CreateScale(entity.ScalingX, entity.ScalingY, entity.ScalingZ) *
                            Matrix.CreateTranslation(entity.PositionX, -entity.PositionY, -entity.PositionZ);
                        RenderMeshLegacy(pass, BobMeshGroups[entity.BobIndex].MeshGroup);
                    }
                }
            });
        }

        private void RenderMeshNew(EffectPass pass, MeshGroup mesh, bool passRenderOpaque)
        {
            foreach (var meshDescriptor in mesh.MeshDescriptors)
            {
                if (meshDescriptor.IsOpaque != passRenderOpaque)
                    continue;
                if (meshDescriptor.Indices.Length == 0)
                    continue;

                var textureIndex = meshDescriptor.TextureIndex & 0xffff;
                if (textureIndex < mesh.Textures.Length)
                    _shader.SetRenderTexture(pass, mesh.Textures[textureIndex]);

                _graphics.DrawUserIndexedPrimitives(
                    PrimitiveType.TriangleList,
                    meshDescriptor.Vertices,
                    0,
                    meshDescriptor.Vertices.Length,
                    meshDescriptor.Indices,
                    0,
                    meshDescriptor.Indices.Length / 3);
            }
        }

        private void RenderMeshLegacy(EffectPass pass, MeshGroup mesh)
        {
            var index = 0;
            foreach (var part in mesh.Parts)
            {
                if (part.Indices.Length == 0)
                    continue;

                if (part.IsOpaque)
                {
                    var textureIndex = part.TextureId & 0xffff;
                    if (textureIndex < mesh.Textures.Length)
                        _shader.SetRenderTexture(pass, mesh.Textures[textureIndex]);

                    _graphics.DrawUserIndexedPrimitives(
                        PrimitiveType.TriangleList,
                        mesh.Segments[index].Vertices,
                        0,
                        mesh.Segments[index].Vertices.Length,
                        part.Indices,
                        0,
                        part.Indices.Length / 3);
                }

                index = (index + 1) % mesh.Segments.Length;
            }

            index = 0;
            foreach (var part in mesh.Parts)
            {
                if (part.Indices.Length == 0)
                    continue;

                if (!part.IsOpaque)
                {
                    var textureIndex = part.TextureId & 0xffff;
                    if (textureIndex < mesh.Textures.Length)
                        _shader.SetRenderTexture(pass, mesh.Textures[textureIndex]);

                    _graphics.DrawUserIndexedPrimitives(
                        PrimitiveType.TriangleList,
                        mesh.Segments[index].Vertices,
                        0,
                        mesh.Segments[index].Vertices.Length,
                        part.Indices,
                        0,
                        part.Indices.Length / 3);
                }

                index = (index + 1) % mesh.Segments.Length;
            }
        }

        private void LoadMapComponent(List<Bar.Entry> entries, string componentName)
        {
            var modelEntry = entries.FirstOrDefault(x => x.Name == componentName && x.Type == Bar.EntryType.Model);
            var textureEntry = entries.FirstOrDefault(x => x.Name == componentName && x.Type == Bar.EntryType.ModelTexture);
            if (modelEntry == null || textureEntry == null)
                return;

            var model = Mdlx.Read(modelEntry.Stream);
            var textures = ModelTexture.Read(textureEntry.Stream).Images;
            MapMeshGroups.Add(new MeshGroupModel(_graphics, componentName, model, textures, 0));
        }
    }
}
