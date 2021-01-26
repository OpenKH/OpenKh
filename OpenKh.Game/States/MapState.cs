using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using OpenKh.Bbs;
using OpenKh.Common;
using OpenKh.Engine;
using OpenKh.Engine.MonoGame;
using OpenKh.Engine.Parsers;
using OpenKh.Game.Debugging;
using OpenKh.Game.Entities;
using OpenKh.Game.Infrastructure;
using OpenKh.Kh2;
using OpenKh.Kh2.Extensions;
using OpenKh.Kh2.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace OpenKh.Game.States
{
    public class MapState : IState, IGameContext, IDebugConsumer
    {
        private readonly static BlendState AlphaBlendState = new BlendState()
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

        public Kernel Kernel { get; private set; }
        private IDataContent _dataContent;
        private ArchiveManager _archiveManager;
        private GraphicsDeviceManager _graphics;
        private InputManager _input;
        private IStateChange _stateChange;
        private List<MeshGroup> _models = new List<MeshGroup>();
        private List<MeshGroup> _bobModels = new List<MeshGroup>();
        private KingdomShader _shader;
        private Camera _camera;

        public IField Field { get; private set; }

        private int _objEntryId = 0x236; // PLAYER
        private bool isDebugMode = true;
        private List<BobEntity> _bobEntities = new List<BobEntity>();
        private List<PmpEntity> _pmpEntities = new List<PmpEntity>();
        private List<MeshGroup> _pmpModels = new List<MeshGroup>();

        private MenuState _menuState;

        public void Initialize(StateInitDesc initDesc)
        {
            Kernel = initDesc.Kernel;
            _dataContent = initDesc.DataContent;
            _archiveManager = initDesc.ArchiveManager;
            _graphics = initDesc.GraphicsDevice;
            _input = initDesc.InputManager;
            _stateChange = initDesc.StateChange;
            _shader = new KingdomShader(initDesc.ContentManager);
            _camera = new Camera()
            {
                CameraPosition = new Vector3(0, 100, 200),
                CameraRotationYawPitchRoll = new Vector3(90, 0, 10),
            };
            _menuState = new MenuState(this);

            Kernel.World = initDesc.StateSettings.GetInt("WorldId", Kernel.World);
            Kernel.Area = initDesc.StateSettings.GetInt("PlaceId", Kernel.Area);
            Kernel.Entrance = initDesc.StateSettings.GetInt("SpawnId", Kernel.Entrance);
            Field = new Kh2Field(
                Kernel,
                _camera,
                initDesc.StateSettings,
                _graphics.GraphicsDevice,
                _shader,
                _input);

            BasicallyForceToReloadEverything();
            _menuState.Initialize(initDesc);
        }

        public void Destroy()
        {
            _menuState.Destroy();
        }

        public void Update(DeltaTimes deltaTimes)
        {
            if (_menuState.IsMenuOpen)
            {
                _menuState.Update(deltaTimes);
                return;
            }

            if (_input.IsStart)
            {
                _menuState.OpenMenu();
            }
            else if (isDebugMode)
            {
                const double Speed = 100.0;
                var speed = (float)(deltaTimes.DeltaTime * Speed);

                if (_input.W)
                    _camera.CameraPosition += Vector3.Multiply(_camera.CameraLookAtX, speed * 5);
                if (_input.S)
                    _camera.CameraPosition -= Vector3.Multiply(_camera.CameraLookAtX, speed * 5);
                if (_input.A)
                    _camera.CameraPosition -= Vector3.Multiply(_camera.CameraLookAtY, speed * 5);
                if (_input.D)
                    _camera.CameraPosition += Vector3.Multiply(_camera.CameraLookAtY, speed * 5);

                if (_input.Up)
                    _camera.CameraRotationYawPitchRoll += new Vector3(0, 0, 1 * speed);
                if (_input.Down)
                    _camera.CameraRotationYawPitchRoll -= new Vector3(0, 0, 1 * speed);
                if (_input.Left)
                    _camera.CameraRotationYawPitchRoll += new Vector3(1 * speed, 0, 0);
                if (_input.Right)
                    _camera.CameraRotationYawPitchRoll -= new Vector3(1 * speed, 0, 0);

                Field.Update(deltaTimes.DeltaTime);
            }
        }

        public void Draw(DeltaTimes deltaTimes)
        {
            _camera.AspectRatio = _graphics.PreferredBackBufferWidth / (float)_graphics.PreferredBackBufferHeight;

            _graphics.GraphicsDevice.RasterizerState = RasterizerState.CullClockwise;

            _shader.Pass(pass =>
            {
                _graphics.GraphicsDevice.BlendState = BlendState.Opaque;
                _shader.UseAlphaMask = true;

                DrawAllMeshes(pass, /*passRenderOpaque=*/true);

                _graphics.GraphicsDevice.BlendState = AlphaBlendState;
                _shader.UseAlphaMask = false;

                DrawAllMeshes(pass, /*passRenderOpaque=*/false);
            });
            Field.Draw();

            if (_menuState.IsMenuOpen)
            {
                _menuState.Draw(deltaTimes);
            }
        }

        private void DrawAllMeshes(EffectPass pass, bool passRenderOpaque)
        {
            _graphics.GraphicsDevice.DepthStencilState = passRenderOpaque ? DepthStencilState.Default : DepthStencilState.DepthRead;

            _shader.ProjectionView = _camera.Projection;
            _shader.WorldView = _camera.World;
            _shader.ModelView = Matrix.Identity;
            pass.Apply();

            foreach (var mesh in _models)
            {
                RenderMeshNew(pass, mesh, passRenderOpaque);
            }

            _graphics.GraphicsDevice.DepthStencilState = DepthStencilState.Default;

            Field.ForEveryModel((entity, model) =>
            {
                _shader.ProjectionView = _camera.Projection;
                _shader.WorldView = _camera.World;
                _shader.ModelView = entity.GetMatrix().ToXna();
                pass.Apply();

                RenderMeshNew(pass, model, passRenderOpaque);
            });

            foreach (var entity in _bobEntities)
            {
                _shader.ProjectionView = _camera.Projection;
                _shader.WorldView = _camera.World;
                _shader.ModelView = entity.GetMatrix().ToXna();
                pass.Apply();

                RenderMeshNew(pass, _bobModels[entity.BobIndex], passRenderOpaque);
            }

            foreach (var ent in _pmpEntities)
            {
                if (ent.DifferentMatrix)
                {
                    Matrix world = _camera.World;
                    world.M14 = 0;
                    world.M24 = 0;
                    world.M34 = 0;
                    world.M41 = 0;
                    world.M42 = 0;
                    world.M43 = 0;
                    world.M44 = 1;
                    _shader.WorldView = world;
                    _graphics.GraphicsDevice.DepthStencilState = DepthStencilState.DepthRead;
                }
                else
                {
                    _shader.WorldView = _camera.World;
                    _graphics.GraphicsDevice.DepthStencilState = DepthStencilState.Default;
                }

                int AxisNumberChanged = 0;
                AxisNumberChanged += Convert.ToInt32(ent.Scaling.X < 0);
                AxisNumberChanged += Convert.ToInt32(ent.Scaling.Y < 0);
                AxisNumberChanged += Convert.ToInt32(ent.Scaling.Z < 0);

                if (AxisNumberChanged == 1 || AxisNumberChanged == 3)
                {
                    _graphics.GraphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;
                }
                else
                {
                    _graphics.GraphicsDevice.RasterizerState = RasterizerState.CullClockwise;
                }


                _shader.ProjectionView = _camera.Projection;
                _shader.ModelView = ent.GetMatrix().ToXna();
                _shader.UseAlphaMask = true;
                pass.Apply();

                RenderMeshNew(pass, _pmpModels[ent.Index], passRenderOpaque);

            }
        }

        private void RenderMeshNew(EffectPass pass, IMonoGameModel model, bool passRenderOpaque)
        {
            if (model?.MeshDescriptors == null)
                return;

            foreach (var meshDescriptor in model.MeshDescriptors)
            {
                if (meshDescriptor.Indices.Length == 0 || meshDescriptor.IsOpaque != passRenderOpaque)
                    continue;

                var textureIndex = meshDescriptor.TextureIndex & 0xffff;
                if (textureIndex < model.Textures.Length)
                    _shader.SetRenderTexture(pass, model.Textures[textureIndex]);

                _graphics.GraphicsDevice.DrawUserIndexedPrimitives(
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

        private void BasicallyForceToReloadEverything()
        {
            _models.Clear();
            _bobModels.Clear();

            switch (Field)
            {
                case Kh2Field kh2Field:
                    kh2Field.LoadMapArd(Kernel.World, Kernel.Area);
                    LoadMap(Kernel.World, Kernel.Area);
                    break;
            }
        }

        private void LoadBBSMap(string MapPath)
        {
            Pmp pmp = Pmp.Read(File.OpenRead(MapPath));
            List<MeshGroup> group = new List<MeshGroup>();

            int PmoIndex = 0;
            for (int i = 0; i < pmp.objectInfo.Count; i++)
            {
                Pmp.ObjectInfo currentInfo = pmp.objectInfo[i];

                if (currentInfo.PMO_Offset != 0)
                {
                    PmpEntity pmpEnt = new PmpEntity(PmoIndex,
                        new System.Numerics.Vector3(currentInfo.PositionX, currentInfo.PositionY, currentInfo.PositionZ),
                        new System.Numerics.Vector3(currentInfo.RotationX, currentInfo.RotationY, currentInfo.RotationZ),
                        new System.Numerics.Vector3(currentInfo.ScaleX, currentInfo.ScaleY, currentInfo.ScaleZ));

                    pmpEnt.DifferentMatrix = pmp.hasDifferentMatrix[PmoIndex];
                    PmoParser pParser = new PmoParser(pmp.PmoList[PmoIndex], 100.0f);
                    List<Tim2KingdomTexture> BbsTextures = new List<Tim2KingdomTexture>();

                    MeshGroup g = new MeshGroup();
                    g.MeshDescriptors = pParser.MeshDescriptors;
                    g.Textures = new IKingdomTexture[pmp.PmoList[PmoIndex].header.TextureCount];

                    for (int j = 0; j < pmp.PmoList[PmoIndex].header.TextureCount; j++)
                    {
                        BbsTextures.Add(new Tim2KingdomTexture(pmp.PmoList[PmoIndex].texturesData[j], _graphics.GraphicsDevice));
                        g.Textures[j] = BbsTextures[j];
                    }

                    _pmpEntities.Add(pmpEnt);
                    _pmpModels.Add(g);
                    PmoIndex++;
                }
            }
        }

        private void LoadMap(int worldIndex, int placeIndex)
        {
            Log.Info($"Map={worldIndex},{placeIndex}");

            var fileName = Kernel.GetMapFileName(worldIndex, placeIndex);
            var entries = _dataContent.FileOpen(fileName).Using(Bar.Read);
            AddMesh(FromMdlx(_graphics.GraphicsDevice, entries, "SK0"));
            AddMesh(FromMdlx(_graphics.GraphicsDevice, entries, "SK1"));
            AddMesh(FromMdlx(_graphics.GraphicsDevice, entries, "MAP"));

            _bobEntities = entries.ForEntry("out", Bar.EntryType.BgObjPlacement, BobDescriptor.Read)?
                .Select(x => new BobEntity(x))?.ToList() ?? new List<BobEntity>();

            var bobModels = entries.ForEntries("BOB", Bar.EntryType.Model, Mdlx.Read).ToList();
            var bobTextures = entries.ForEntries("BOB", Bar.EntryType.ModelTexture, ModelTexture.Read).ToList();

            for (var i = 0; i < bobModels.Count; i++)
            {
                _bobModels.Add(new MeshGroup
                {
                    MeshDescriptors = MeshLoader.FromKH2(bobModels[i]).MeshDescriptors,
                    Textures = bobTextures[i].LoadTextures(_graphics.GraphicsDevice).ToArray()
                });
            }
        }

        private static MeshGroup FromMdlx(GraphicsDevice graphics, IEnumerable<Bar.Entry> entries, string name)
        {
            var model = entries.ForEntry(name, Bar.EntryType.Model, Mdlx.Read);
            if (model == null)
                return null;

            var textures = entries.ForEntry(name, Bar.EntryType.ModelTexture, ModelTexture.Read);
            if (model == null)
                return null;

            return new MeshGroup
            {
                MeshDescriptors = MeshLoader.FromKH2(model).MeshDescriptors,
                Textures = textures.LoadTextures(graphics).ToArray()
            };
        }

        private void AddMesh(MeshGroup mesh)
        {
            if (mesh == null)
            {
                Log.Warn("AddMesh received null");
                return;
            }

            _models.Add(mesh);
        }

        public void LoadTitleScreen() => _stateChange.State = 0;

        public void LoadPlace(int world, int area, int entrance)
        {
            Kernel.World = world;
            Kernel.Area = area;
            Kernel.Entrance = entrance;

            BasicallyForceToReloadEverything();
        }

        private bool DebugMode { get; set; } = true;
        public void DebugUpdate(IDebug debug)
        {
            if (_input.IsDebug)
                DebugMode = !DebugMode;
        }

        public void DebugDraw(IDebug debug)
        {
            if (_menuState.IsMenuOpen)
                return;

            if (DebugMode)
            {
                debug.Println($"MAP: {Kh2.Constants.WorldIds[Kernel.World]}{Kernel.Area:D02}");
                debug.Println($"POS ({_camera.CameraPosition.X:F0}, {_camera.CameraPosition.Y:F0}, {_camera.CameraPosition.Z:F0})");
                debug.Println($"LKT ({_camera.CameraLookAt.X:F0}, {_camera.CameraLookAt.Y:F0}, {_camera.CameraLookAt.Z:F0})");
            }
        }
    }
}
