using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using OpenKh.Common;
using OpenKh.Engine;
using OpenKh.Engine.MonoGame;
using OpenKh.Engine.Parsers;
using OpenKh.Bbs;
using OpenKh.Kh2;
using OpenKh.Kh2.Ard;
using OpenKh.Kh2.Extensions;
using OpenKh.Kh2.Models;
using OpenKh.Tools.BbsMapStudio.Interfaces;
using OpenKh.Tools.BbsMapStudio.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using xna = Microsoft.Xna.Framework;

namespace OpenKh.Tools.BbsMapStudio
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
            BlendFactor = xna.Color.White,
            MultiSampleMask = int.MaxValue,
            IndependentBlendEnable = false
        };

        private readonly xna.GraphicsDeviceManager _graphicsManager;
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

        public MapRenderer(ContentManager content, xna.GraphicsDeviceManager graphics)
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
            var MapArc = File.OpenRead(fileName).Using(Arc.Read);
            var MapEnt = MapArc.FirstOrDefault(x => x.Name.Contains(".pmp"));
            
            if(MapEnt != null)
            {
                var BbsMap = Pmp.Read(new MemoryStream(MapEnt.Data));
                int pmoIndex = 0;

                for (int i = 0; i < BbsMap.objectInfo.Count; i++)
                {
                    Pmp.ObjectInfo currentInfo = BbsMap.objectInfo[i];
                    if (currentInfo.PMO_Offset != 0)
                    {
                        Vector3 Loc = new Vector3(currentInfo.PositionX, currentInfo.PositionY, currentInfo.PositionZ);
                        Vector3 Rot = new Vector3(currentInfo.RotationX, currentInfo.RotationY, currentInfo.RotationZ);
                        Vector3 Scl = new Vector3(currentInfo.ScaleX, currentInfo.ScaleY, currentInfo.ScaleZ);
                        MapMeshGroups.Add(new MeshGroupModel(_graphics, "", BbsMap.PmoList[pmoIndex], BbsMap.PmoList[pmoIndex].texturesData, i, Loc * 100, Rot, Scl, BbsMap.hasDifferentMatrix[pmoIndex]));
                        pmoIndex++;
                    }
                }
                
            }
            else
            {
                //
            }
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

            var worldView = Camera.World;
            var specialWorldView = worldView;
            specialWorldView.M14 = 0;
            specialWorldView.M24 = 0;
            specialWorldView.M34 = 0;
            specialWorldView.M41 = 0;
            specialWorldView.M42 = 0;
            specialWorldView.M43 = 0;
            specialWorldView.M44 = 1;

            _graphics.RasterizerState = new RasterizerState()
            {
                CullMode = CullMode.CullClockwiseFace
            };
            _graphics.DepthStencilState = new DepthStencilState();
            _graphics.BlendState = DefaultBlendState;

            _shader.Pass(pass =>
            {
                _shader.SetProjectionView(Camera.Projection);
                _shader.SetWorldView(Camera.World);
                _shader.SetModelViewIdentity();
                pass.Apply();

                foreach (var mesh in MapMeshGroups.Where(x => x.IsVisible))
                {
                    _shader.SetModelView(Matrix4x4.CreateRotationX(mesh.Rotation.X) *
                                Matrix4x4.CreateRotationY(mesh.Rotation.Y) *
                                Matrix4x4.CreateRotationZ(mesh.Rotation.Z) *
                                Matrix4x4.CreateScale(mesh.Scale.X, mesh.Scale.Y, mesh.Scale.Z) *
                                Matrix4x4.CreateTranslation(mesh.Location.X, mesh.Location.Y, mesh.Location.Z));

                    int AxisNumberChanged = 0;
                    AxisNumberChanged += Convert.ToInt32(mesh.Scale.X < 0);
                    AxisNumberChanged += Convert.ToInt32(mesh.Scale.Y < 0);
                    AxisNumberChanged += Convert.ToInt32(mesh.Scale.Z < 0);

                    if (AxisNumberChanged == 1 || AxisNumberChanged == 3)
                        _graphics.RasterizerState = RasterizerState.CullCounterClockwise;
                    else
                        _graphics.RasterizerState = RasterizerState.CullClockwise;

                    RenderMeshNew(pass, mesh.MeshGroup, true);
                }
                    
                foreach (var mesh in MapMeshGroups.Where(x => x.IsVisible))
                {
                    if (mesh.hasDifferentMatrix)
                    {
                        _shader.SetWorldView(ref specialWorldView);
                        _graphics.DepthStencilState = DepthStencilState.DepthRead;
                    }
                    else
                    {
                        _shader.SetWorldView(ref worldView);
                        _graphics.DepthStencilState = DepthStencilState.Default;
                    }

                    _shader.SetModelView(Matrix4x4.CreateRotationX(mesh.Rotation.X) *
                                Matrix4x4.CreateRotationY(mesh.Rotation.Y) *
                                Matrix4x4.CreateRotationZ(mesh.Rotation.Z) *
                                Matrix4x4.CreateScale(mesh.Scale.X, mesh.Scale.Y, mesh.Scale.Z) *
                                Matrix4x4.CreateTranslation(mesh.Location.X, mesh.Location.Y, mesh.Location.Z));

                    int AxisNumberChanged = 0;
                    AxisNumberChanged += Convert.ToInt32(mesh.Scale.X < 0);
                    AxisNumberChanged += Convert.ToInt32(mesh.Scale.Y < 0);
                    AxisNumberChanged += Convert.ToInt32(mesh.Scale.Z < 0);

                    if (AxisNumberChanged == 1 || AxisNumberChanged == 3)
                        _graphics.RasterizerState = RasterizerState.CullCounterClockwise;
                    else
                        _graphics.RasterizerState = RasterizerState.CullClockwise;

                    RenderMeshNew(pass, mesh.MeshGroup, false);
                }
                    

                _shader.SetRenderTexture(pass, _whiteTexture);

                if (CurrentSpawnPoint != null)
                {
                    foreach (var spawnPoint in CurrentSpawnPoint.SpawnPoints)
                    {
                        foreach (var entity in spawnPoint.Entities)
                        {
                            _shader.SetModelView(Matrix4x4.CreateRotationX(entity.RotationX) *
                                Matrix4x4.CreateRotationY(entity.RotationY) *
                                Matrix4x4.CreateRotationZ(entity.RotationZ) *
                                Matrix4x4.CreateTranslation(entity.PositionX, -entity.PositionY, -entity.PositionZ));
                            RenderMeshNew(pass, CurrentSpawnPoint.ObjEntryCtrl[entity.ObjectId], true);
                        }

                        _graphics.RasterizerState = new RasterizerState()
                        {
                            CullMode = CullMode.None
                        };
                        _shader.SetRenderTexture(pass, _whiteTexture);
                        foreach (var item in spawnPoint.EventActivators)
                        {
                            _shader.SetModelView(Matrix4x4.CreateRotationX(item.RotationX) *
                                Matrix4x4.CreateRotationY(item.RotationY) *
                                Matrix4x4.CreateRotationZ(item.RotationZ) *
                                Matrix4x4.CreateScale(item.ScaleX, item.ScaleY, item.ScaleZ) *
                                Matrix4x4.CreateTranslation(item.PositionX, -item.PositionY, -item.PositionZ));
                            pass.Apply();

                            var color = new xna.Color(1f, 0f, 0f, .5f);
                            var vertices = new PositionColoredTextured[]
                            {
                                new PositionColoredTextured(-1, -1, -1, 0, 0, 1f, 0f, 0f, 1f),
                                new PositionColoredTextured(+1, -1, -1, 0, 0, 1f, 0f, 0f, 1f),
                                new PositionColoredTextured(+1, +1, -1, 0, 0, 1f, 0f, 0f, 1f),
                                new PositionColoredTextured(-1, +1, -1, 0, 0, 1f, 0f, 0f, 1f),
                                new PositionColoredTextured(-1, -1, +1, 0, 0, 1f, 0f, 0f, 1f),
                                new PositionColoredTextured(+1, -1, +1, 0, 0, 1f, 0f, 0f, 1f),
                                new PositionColoredTextured(+1, +1, +1, 0, 0, 1f, 0f, 0f, 1f),
                                new PositionColoredTextured(-1, +1, +1, 0, 0, 1f, 0f, 0f, 1f),
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
                            _graphics.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, vertices, 0, 8, indices, 0, 12, MeshLoader.PositionColoredTexturedVertexDeclaration);
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

                _shader.UseAlphaMask = true;

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
    }
}
