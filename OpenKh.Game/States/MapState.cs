using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using OpenKh.Game.Debugging;
using OpenKh.Game.Infrastructure;
using OpenKh.Common;
using OpenKh.Kh2;
using OpenKh.Kh2.Extensions;
using System.Collections.Generic;
using System.Linq;
using OpenKh.Kh2.Models;
using OpenKh.Game.Entities;
using OpenKh.Kh2.Ard;
using OpenKh.Engine.MonoGame;

namespace OpenKh.Game.States
{
    public class MapState : IState, IGameContext
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

        private int _worldId;
        private int _placeId;
        private int _spawnId;
        private int _spawnScriptMap;
        private int _spawnScriptBtl;
        private int _spawnScriptEvt;

        private int _objEntryId = 0x236; // PLAYER
        private bool _enableCameraMovement = true;
        private List<ObjectEntity> _objectEntities = new List<ObjectEntity>();
        private List<BobEntity> _bobEntities = new List<BobEntity>();

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
            _worldId = initDesc.StateSettings.GetInt("WorldId", 2);
            _placeId = initDesc.StateSettings.GetInt("PlaceId", 4);
            _spawnId = initDesc.StateSettings.GetInt("SpawnId", 99);
            _spawnScriptMap = initDesc.StateSettings.GetInt("SpawnScriptMap", 0x06);
            _spawnScriptBtl = initDesc.StateSettings.GetInt("SpawnScriptBtl", 0x01);
            _spawnScriptEvt = initDesc.StateSettings.GetInt("SpawnScriptEvt", 0x16);

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
            else if (_enableCameraMovement)
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

                foreach (var entity in _objectEntities.Where(x => x.IsMeshLoaded))
                {
                    entity.Update((float)deltaTimes.DeltaTime);
                }
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

            foreach (var entity in _objectEntities.Where(x => x.IsMeshLoaded))
            {
                _shader.ProjectionView = _camera.Projection;
                _shader.WorldView = _camera.World;
                _shader.ModelView = entity.GetMatrix();
                pass.Apply();

                RenderMeshNew(pass, entity, passRenderOpaque);
            }

            foreach (var entity in _bobEntities)
            {
                _shader.ProjectionView = _camera.Projection;
                _shader.WorldView = _camera.World;
                _shader.ModelView = entity.GetMatrix();
                pass.Apply();

                RenderMeshNew(pass, _bobModels[entity.BobIndex], passRenderOpaque);
            }
        }

        private void RenderMeshNew(EffectPass pass, IMonoGameModel model, bool passRenderOpaque)
        {
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
            _objectEntities.Clear();

            LoadMapArd(_worldId, _placeId);
            LoadMap(_worldId, _placeId);
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

        private void LoadMapArd(int worldIndex, int placeIndex)
        {
            string fileName;
            if (Kernel.IsReMix)
                fileName = $"ard/{Kernel.Language}/{Constants.WorldIds[worldIndex]}{placeIndex:D02}.ard";
            else
                fileName = $"ard/{Constants.WorldIds[worldIndex]}{placeIndex:D02}.ard";

            var entries = _dataContent.FileOpen(fileName).Using(Bar.Read);
            RunSpawnScript(entries, "map", _spawnScriptMap);
            RunSpawnScript(entries, "btl", _spawnScriptBtl);
            RunSpawnScript(entries, "evt", _spawnScriptEvt);
        }

        private void RunSpawnScript(IEnumerable<Bar.Entry> barEntries, string spawnScriptName, int programId)
        {
            var spawnScript = barEntries.ForEntry(spawnScriptName, Bar.EntryType.SpawnScript, SpawnScript.Read);
            if (spawnScript == null)
                return;

            var program = spawnScript.FirstOrDefault(x => x.ProgramId == programId);
            if (program == null)
                return;

            foreach (var function in program.Functions)
            {
                switch (function.Opcode)
                {
                    case SpawnScript.Operation.Spawn:
                        var spawn = function.AsString(0);
                        Log.Info($"Loading spawn {spawn}");
                        var spawnPoints = barEntries.ForEntry(spawn, Bar.EntryType.SpawnPoint, SpawnPoint.Read);
                        if (spawnPoints != null)
                        {
                            foreach (var spawnPoint in spawnPoints)
                            {
                                foreach (var desc in spawnPoint.Entities)
                                    SpawnEntity(desc);
                            }
                        }
                        else
                            Log.Warn($"Unable to find spawn \"{spawn}\".");
                        break;
                }
            }
        }

        private void SpawnEntity(SpawnPoint.Entity entityDesc)
        {
            var entity = ObjectEntity.FromSpawnPoint(Kernel, entityDesc);
            entity.LoadMesh(_graphics.GraphicsDevice);

            _objectEntities.Add(entity);
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

        public void LoadPlace(int worldId, int placeId, int spawnIndex)
        {
            _worldId = worldId;
            _placeId = placeId;
            _spawnId = spawnIndex;

            BasicallyForceToReloadEverything();
        }

        #region DEBUG

        private class DebugPlace
        {
            public int Index { get; set; }
            public int WorldId { get; set; }
            public int PlaceId { get; set; }
            public ushort MessageId { get; set; }
        }

        private int _debugType = 0;
        private int _debugPlaceCursor = 0;
        private int _debugObjentryCursor = 0;
        private DebugPlace[] _places;

        public void DebugUpdate(IDebug debug)
        {
            if (_input.IsDebug)
            {
                if (!IsDebugMode())
                {
                    EnableDebugMode();

                    if (_places == null)
                        _places = DebugLoadPlaceList();

                    _debugObjentryCursor = Kernel.ObjEntries
                        .Select((entry, i) => new { entry, i })
                        .FirstOrDefault(x => x.entry.ObjectId == _objEntryId)?.i ?? 0;
                }
                else
                    DisableDebugMode();
            }

            if (IsDebugMode())
            {
                if (_input.IsMenuLeft)
                    _debugType--;
                else if (_input.IsMenuRight)
                    _debugType++;
                _debugType %= 3;

                if (_debugType == 0)
                { }
                else if (_debugType == 1)
                    DebugUpdatePlaceList();
                else if (_debugType == 2)
                    if (_input.IsCross)
                        debug.State = 0;
            }
        }

        public void DebugDraw(IDebug debug)
        {
            if (_menuState.IsMenuOpen)
                return;

            if (IsDebugMode())
            {
                if (_debugType == 0)
                    DebugDrawEntities(debug);
                else if (_debugType == 1)
                    DebugDrawPlaceList(debug);
                else if (_debugType == 2)
                    debug.Println("Press X to return to title screen");
            }
            else
            {
                debug.Println($"MAP: {Constants.WorldIds[_worldId]}{_placeId:D02}");
                debug.Println($"POS ({_camera.CameraPosition.X:F0}, {_camera.CameraPosition.Y:F0}, {_camera.CameraPosition.Z:F0})");
                debug.Println($"YPR ({_camera.CameraRotationYawPitchRoll.X:F0}, {_camera.CameraRotationYawPitchRoll.Y:F0}, {_camera.CameraRotationYawPitchRoll.Z:F0})");
            }
        }

        private bool IsDebugMode() => _enableCameraMovement == false;
        private void EnableDebugMode() => _enableCameraMovement = false;
        private void DisableDebugMode() => _enableCameraMovement = true;
        private int Increment(int n) => n + (_input.IsShift ? 10 : 1);
        private int Decrement(int n) => n - (_input.IsShift ? 10 : 1);

        private void DebugUpdatePlaceList()
        {
            if (_input.IsMenuUp)
                _debugPlaceCursor = Decrement(_debugPlaceCursor);
            else if (_input.IsMenuDown)
                _debugPlaceCursor = Increment(_debugPlaceCursor);
            if (_debugPlaceCursor < 0)
                _debugPlaceCursor = _places.Length - 1;
            _debugPlaceCursor %= _places.Length;

            if (_input.IsCross)
            {
                var map = _places[_debugPlaceCursor];
                _worldId = map.WorldId;
                _placeId = map.PlaceId;

                BasicallyForceToReloadEverything();
                DisableDebugMode();
            }
        }

        public void DebugDrawEntities(IDebug debug)
        {
            foreach (var entity in _objectEntities)
            {

            }
        }

        private void DebugDrawPlaceList(IDebug debug)
        {
            debug.Println("MAP SELECTION");
            debug.Println("");

            foreach (var place in _places.Skip(_debugPlaceCursor))
            {
                debug.Print($"{(place.Index == _debugPlaceCursor ? '>' : ' ')} ");
                debug.Print($"{Constants.WorldIds[place.WorldId]}{place.PlaceId:D02} ");
                debug.Println(place.MessageId);
            }
        }

        private DebugPlace[] DebugLoadPlaceList() => Kernel.Places
            .Select(x => new
            {
                World = x.Key,
                Places = x.Value.Select((place, i) => new
                {
                    Index = i,
                    Place = place
                })
            })
            .SelectMany(x => x.Places, (x, place) => new DebugPlace
            {
                WorldId = Constants.WorldIds
                    .Select((World, Index) => new { World, Index })
                    .Where(e => e.World == x.World)
                    .Select(x => x.Index).FirstOrDefault(),
                PlaceId = place.Index,
                MessageId = place.Place.MessageId
            })
            .Select((x, i) =>
            {
                x.Index = i;
                return x;
            })
            .ToArray();
        #endregion
    }
}
