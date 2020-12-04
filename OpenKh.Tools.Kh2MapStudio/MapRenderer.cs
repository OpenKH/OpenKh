using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using OpenKh.Common;
using OpenKh.Engine.MonoGame;
using OpenKh.Kh2;
using OpenKh.Kh2.Ard;
using OpenKh.Kh2.Extensions;
using OpenKh.Kh2.Models;
using OpenKh.Tools.Kh2MapStudio.Interfaces;
using OpenKh.Tools.Kh2MapStudio.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace OpenKh.Tools.Kh2MapStudio
{
    class MapRenderer : ILayerController, ISpawnPointController
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
        private readonly Texture2D _whiteTexture;
        private bool _showBobs = true;

        public Camera Camera { get; }

        public IObjEntryController ObjEntryController { get; set; }

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

        public bool? ShowMapCollision
        {
            get => MapCollision != null ? (bool?)MapCollision.IsVisible : null;
            set => MapCollision.IsVisible = value ?? false;
        }

        public bool? ShowCameraCollision
        {
            get => CameraCollision != null ? (bool?)CameraCollision.IsVisible : null;
            set => CameraCollision.IsVisible = value ?? false;
        }

        public bool? ShowLightCollision
        {
            get => LightCollision != null ? (bool?)LightCollision.IsVisible : null;
            set => LightCollision.IsVisible = value ?? false;
        }


        internal List<Bar.Entry> MapBarEntries { get; private set; }
        internal List<Bar.Entry> ArdBarEntries { get; private set; }
        internal List<MeshGroupModel> MapMeshGroups { get; }
        internal List<MeshGroupModel> BobMeshGroups { get; }
        internal List<BobDescriptor> BobDescriptors { get; }
        internal CollisionModel MapCollision { get; set; }
        internal CollisionModel CameraCollision { get; set; }
        internal CollisionModel LightCollision { get; set; }

        public List<SpawnPointModel> SpawnPoints { get; private set; }
        public SpawnPointModel CurrentSpawnPoint { get; private set; }
        public string SelectSpawnPoint
        {
            get => CurrentSpawnPoint?.Name ?? string.Empty;
            set => CurrentSpawnPoint = SpawnPoints.FirstOrDefault(x => x.Name == value);
        }

        public SpawnScriptModel SpawnScriptMap { get; private set; }
        public SpawnScriptModel SpawnScriptBattle { get; private set; }
        public SpawnScriptModel SpawnScriptEvent { get; private set; }

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

            _whiteTexture = new Texture2D(_graphics, 2, 2);
            _whiteTexture.SetData(Enumerable.Range(0, 2 * 2 * sizeof(int)).Select(_ => (byte)0xff).ToArray());
        }

        public void OpenMap(string fileName)
        {
            Close();
            MapBarEntries = File.OpenRead(fileName).Using(Bar.Read);
            LoadMapComponent(MapBarEntries, "SK0");
            LoadMapComponent(MapBarEntries, "SK1");
            LoadMapComponent(MapBarEntries, "MAP");

            var bobDescEntry = MapBarEntries
                .Where(x => x.Name == "out" && x.Type == Bar.EntryType.BgObjPlacement)
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

            var mapCollisionEntry = MapBarEntries
                .Where(x => x.Name.StartsWith("ID_") && x.Type == Bar.EntryType.MapCollision)
                .FirstOrDefault();
            if (mapCollisionEntry != null)
                MapCollision = new CollisionModel(Coct.Read(mapCollisionEntry.Stream));

            var cameraCollisionEntry = MapBarEntries
                .Where(x => x.Name.StartsWith("CH_") && x.Type == Bar.EntryType.CameraCollision)
                .FirstOrDefault();
            if (cameraCollisionEntry != null)
                CameraCollision = new CollisionModel(Coct.Read(cameraCollisionEntry.Stream));

            var lightCollisionEntry = MapBarEntries
                .Where(x => x.Name == "COL_" && x.Type == Bar.EntryType.LightData)
                .FirstOrDefault();
            if (lightCollisionEntry != null)
                LightCollision = new CollisionModel(Coct.Read(lightCollisionEntry.Stream));
        }

        public void SaveMap(string fileName)
        {
            var memStream = new MemoryStream();
            BobDescriptor.Write(memStream, BobDescriptors);

            MapBarEntries.AddOrReplace(new Bar.Entry
            {
                Name = "out",
                Type = Bar.EntryType.BgObjPlacement,
                Stream = memStream
            });

            File.Create(fileName).Using(stream => Bar.Write(stream, MapBarEntries));
        }

        public void OpenArd(string fileName)
        {
            ArdBarEntries = File.OpenRead(fileName).Using(Bar.Read);
            SpawnPoints = ArdBarEntries
                .Where(x => x.Type == Bar.EntryType.SpawnPoint && x.Stream.Length > 0)
                .Select(x =>
                new SpawnPointModel(ObjEntryController, x.Name, SpawnPoint.Read(x.Stream.SetPosition(0))))
                .ToList();
            SelectSpawnPoint = "m_00";

            SpawnScriptMap = SpawnScriptModel.Create(ArdBarEntries, "map");
            SpawnScriptBattle = SpawnScriptModel.Create(ArdBarEntries, "btl");
            SpawnScriptEvent = SpawnScriptModel.Create(ArdBarEntries, "evt");
        }

        public void SaveArd(string fileName)
        {
            foreach (var spawnPointModel in SpawnPoints)
            {
                var memStream = new MemoryStream();
                SpawnPoint.Write(memStream, spawnPointModel.SpawnPoints);

                ArdBarEntries.AddOrReplace(new Bar.Entry
                {
                    Name = spawnPointModel.Name,
                    Type = Bar.EntryType.SpawnPoint,
                    Stream = memStream
                });
            }

            SpawnScriptMap?.SaveToBar(ArdBarEntries);
            SpawnScriptBattle?.SaveToBar(ArdBarEntries);
            SpawnScriptEvent?.SaveToBar(ArdBarEntries);

            File.Create(fileName).Using(stream => Bar.Write(stream, ArdBarEntries));
        }

        public void Close()
        {
            foreach (var meshGroup in MapMeshGroups)
                meshGroup?.Dispose();
            MapMeshGroups.Clear();

            foreach (var meshGroup in BobMeshGroups)
                meshGroup?.Dispose();
            BobMeshGroups.Clear();
            BobDescriptors.Clear();

            MapCollision?.Dispose();
            CameraCollision?.Dispose();
            LightCollision?.Dispose();
        }

        public void Update(float deltaTime)
        {

        }

        public void Draw()
        {
            var viewport = _graphics.Viewport;
            Camera.AspectRatio = viewport.Width / (float)viewport.Height;

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

                _shader.SetRenderTexture(pass, _whiteTexture);
                MapCollision?.Draw(_graphics);
                CameraCollision.Draw(_graphics);
                LightCollision?.Draw(_graphics);

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
                        RenderMeshNew(pass, BobMeshGroups[entity.BobIndex].MeshGroup, true);
                    }
                }

                if (CurrentSpawnPoint != null)
                {
                    foreach (var spawnPoint in CurrentSpawnPoint.SpawnPoints)
                    {
                        foreach (var entity in spawnPoint.Entities)
                        {
                            _shader.ModelView = Matrix.CreateRotationX(entity.RotationX) *
                                Matrix.CreateRotationY(entity.RotationY) *
                                Matrix.CreateRotationZ(entity.RotationZ) *
                                Matrix.CreateTranslation(entity.PositionX, -entity.PositionY, -entity.PositionZ);
                            RenderMeshNew(pass, CurrentSpawnPoint.ObjEntryCtrl[entity.ObjectId], true);
                        }

                        _graphics.RasterizerState = new RasterizerState()
                        {
                            CullMode = CullMode.None
                        };
                        _shader.SetRenderTexture(pass, _whiteTexture);
                        foreach (var item in spawnPoint.EventActivators)
                        {
                            _shader.ModelView = Matrix.CreateRotationX(item.RotationX) *
                                Matrix.CreateRotationY(item.RotationY) *
                                Matrix.CreateRotationZ(item.RotationZ) *
                            Matrix.CreateScale(item.ScaleX, item.ScaleY, item.ScaleZ) *
                                Matrix.CreateTranslation(item.PositionX, -item.PositionY, -item.PositionZ);
                            pass.Apply();

                            var color = new Color(1f, 0f, 0f, .5f);
                            var texCoord = new Vector2();
                            var vertices = new VertexPositionColorTexture[]
                            {
                                new VertexPositionColorTexture(new Vector3(-1, -1, -1), color, texCoord),
                                new VertexPositionColorTexture(new Vector3(+1, -1, -1), color, texCoord),
                                new VertexPositionColorTexture(new Vector3(+1, +1, -1), color, texCoord),
                                new VertexPositionColorTexture(new Vector3(-1, +1, -1), color, texCoord),
                                new VertexPositionColorTexture(new Vector3(-1, -1, +1), color, texCoord),
                                new VertexPositionColorTexture(new Vector3(+1, -1, +1), color, texCoord),
                                new VertexPositionColorTexture(new Vector3(+1, +1, +1), color, texCoord),
                                new VertexPositionColorTexture(new Vector3(-1, +1, +1), color, texCoord),
                            };
                            var indices = new int[]
                            {
                                0, 1, 3, 3, 1, 2,
                                1, 5, 2, 2, 5, 6,
                                5, 4, 6, 6, 4, 7,
                                4, 0, 7, 7, 0, 3,
                                3, 2, 7, 7, 2, 6,
                                4, 5, 0, 0, 5, 1
                            };
                            _graphics.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, vertices, 0, 8, indices, 0, 12);
                        }
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
                    meshDescriptor.Indices.Length / 3,
                    MeshLoader.PositionColoredTexturedVertexDeclaration);
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
