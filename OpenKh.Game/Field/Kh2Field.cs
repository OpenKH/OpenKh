using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using OpenKh.Common;
using OpenKh.Engine;
using OpenKh.Engine.Extensions;
using OpenKh.Engine.MonoGame;
using OpenKh.Engine.Renders;
using OpenKh.Common;
using OpenKh.Game.Entities;
using OpenKh.Game.Events;
using OpenKh.Game.Infrastructure;
using OpenKh.Kh2;
using OpenKh.Kh2.Ard;
using OpenKh.Kh2.Extensions;
using OpenKh.Kh2.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using n = System.Numerics;

namespace OpenKh.Game.Field
{
    public class Kh2Field : IField
    {
        private readonly Kernel _kernel;
        private readonly Camera _camera;
        private readonly GraphicsDevice _graphicsDevice;
        private readonly KingdomShader _shader;
        private readonly InputManager _inputManager;
        private readonly Kh2MessageRenderer _messageRenderer;
        private readonly DrawContext _messageDrawContext;
        private readonly Kh2MessageProvider _eventMessageProvider;
        private readonly List<ObjectEntity> _actors = new List<ObjectEntity>();
        private readonly Dictionary<int, ObjectEntity> _actorIds = new Dictionary<int, ObjectEntity>();
        private readonly List<MeshGroup> _mapMeshes = new List<MeshGroup>();
        private readonly List<MeshGroup> _skyboxMeshes = new List<MeshGroup>();
        private readonly List<BobEntity> _bobEntities = new List<BobEntity>();
        private readonly List<MeshGroup> _bobModels = new List<MeshGroup>();
        private readonly Dictionary<int, byte[]> _subtitleData = new Dictionary<int, byte[]>();
        private readonly MonoSpriteDrawing _drawing;
        private Bar _binarcAreaData;
        private EventPlayer _eventPlayer;
        private int _spawnScriptMap;
        private int _spawnScriptBtl;
        private int _spawnScriptEvt;
        private bool _isFreeCam;
        private bool _isEventPause;

        private bool _isFading;
        private float _fadeCurrent;
        private float _fadeGoal;
        private Color _fadeCurrentColor;
        private Color _fadeStartColor;
        private Color _fadeEndColor;

        public List<string> Events { get; private set; }

        public Kh2Field(
            Kernel kernel,
            Camera camera,
            Dictionary<string, string> settings,
            GraphicsDevice graphicsDevice,
            KingdomShader shader,
            InputManager inputManager)
        {
            _kernel = kernel;
            _camera = camera;
            _graphicsDevice = graphicsDevice;
            _shader = shader;
            _inputManager = inputManager;

            var viewport = graphicsDevice.Viewport;
            _drawing = new MonoSpriteDrawing(graphicsDevice, _shader);
            _drawing.SetProjection(
                viewport.Width,
                viewport.Height,
                Global.ResolutionWidth,
                Global.ResolutionHeight,
                1.0f);
            _messageRenderer = new Kh2MessageRenderer(_drawing, _kernel.EventMessageContext);
            _messageDrawContext = new DrawContext
            {
                IgnoreDraw = false,

                x = 0,
                y = 0,
                xStart = 0,
                Width = 0,
                Height = 0,
                WindowWidth = 512,

                Scale = 1,
                WidthMultiplier = 1,
                Color = new ColorF(1.0f, 1.0f, 1.0f, 1.0f)
            };
            _eventMessageProvider = new Kh2MessageProvider();

            _spawnScriptMap = settings.GetInt("SpawnScriptMap", -1);
            _spawnScriptBtl = settings.GetInt("SpawnScriptBtl", -1);
            _spawnScriptEvt = settings.GetInt("SpawnScriptEvt", -1);
            FadeFromBlack(1.0f);
        }

        public void UnloadMap()
        {
            Action<List<MeshGroup>> clearMeshGroups = (meshGroups) =>
            {
                foreach (var meshGroup in meshGroups)
                    foreach (var texture in meshGroup.Textures)
                        texture.Dispose();
                meshGroups.Clear();
            };

            clearMeshGroups(_mapMeshes);
            clearMeshGroups(_skyboxMeshes);
            clearMeshGroups(_bobModels);
            _bobEntities.Clear();
            RemoveAllActors();

            _eventPlayer = null;
        }

        public void LoadArea(int world, int area)
        {
            Log.Info($"Area={world},{area}");

            UnloadMap();
            LoadAreaData(world, area);
            // TODO load voices (eg. voice/us/battle/nm0_jack.vsb)
            // TODO load field2d (eg. field2d/jp/nm0field.2dd)
            // TODO load command (eg. field2d/jp/nm1command.2dd)
            LoadMap(world, area);
            LoadMsg(world);
            // TODO load libretto (eg. libretto-nm.bar)
            // TODO load effect
            // TODO load magics
            // TODO load prize
            // TODO load prizebox
            // TODO load entities
            // TODO load summons
            // TODO load mission
            // TODO dispatch entity loading here, not in LoadAreaData
        }

        private void LoadAreaData(int world, int area)
        {
            string fileName;
            if (_kernel.IsReMix)
                fileName = $"ard/{_kernel.Language}/{Constants.WorldIds[world]}{area:D02}.ard";
            else
                fileName = $"ard/{Constants.WorldIds[world]}{area:D02}.ard";

            _binarcAreaData = _kernel.DataContent.FileOpen(fileName).Using(Bar.Read);
            Events = _binarcAreaData
                .Where(x => x.Type == Bar.EntryType.Event)
                .Select(x => x.Name)
                .ToList();

            Log.Info($"Loading spawn {_kernel.SpawnName}");
            RunSpawnScript(_binarcAreaData, "map", _spawnScriptMap >= 0 ? _spawnScriptMap : _kernel.SpawnMap);
            RunSpawnScript(_binarcAreaData, "btl", _spawnScriptBtl >= 0 ? _spawnScriptBtl : _kernel.SpawnBtl);
            RunSpawnScript(_binarcAreaData, "evt", _spawnScriptEvt >= 0 ? _spawnScriptEvt : _kernel.SpawnEvt);
            // TODO units (or entities) should be spawn later, not in RunSpawnScript. This is because
            // we want to avoid to load entities that can be potentially not used.
        }

        private void LoadMap(int world, int area)
        {
            Action<Bar, string, List<MeshGroup>> addMeshes = (binarc, tag, meshGroupList) =>
            {
                var meshes = FromMdlx(_graphicsDevice, binarc, tag);
                if (meshes != null)
                    meshGroupList.Add(meshes);
            };

            Log.Info("Map={0},{1}", world, area);
            var fileName = _kernel.GetMapFileName(world, area);
            var binarc = _kernel.DataContent.FileOpen(fileName).Using(Bar.Read);

            addMeshes(binarc, "SK0", _skyboxMeshes);
            addMeshes(binarc, "SK1", _skyboxMeshes);
            addMeshes(binarc, "MAP", _mapMeshes);

            _bobEntities.AddRange(binarc.ForEntry("out", Bar.EntryType.BgObjPlacement, BobDescriptor.Read)?
                .Select(x => new BobEntity(x))?.ToList() ?? new List<BobEntity>());

            var bobModels = binarc.ForEntries("BOB", Bar.EntryType.Model, Mdlx.Read).ToList();
            var bobTextures = binarc.ForEntries("BOB", Bar.EntryType.ModelTexture, ModelTexture.Read).ToList();

            for (var i = 0; i < bobModels.Count; i++)
            {
                _bobModels.Add(new MeshGroup
                {
                    MeshDescriptors = MeshLoader.FromKH2(bobModels[i]).MeshDescriptors,
                    Textures = bobTextures[i].LoadTextures(_graphicsDevice).ToArray()
                });
            }
        }
        private void LoadMsg(int world) =>
            _kernel.DataContent
                .FileOpen($"msg/{_kernel.Language}/{Constants.WorldIds[world]}.bar")
                .Using(stream => Bar.Read(stream))
                .ForEntry(x => x.Type == Bar.EntryType.List, stream =>
                {
                    _eventMessageProvider.Load(Msg.Read(stream));
                    return true;
                });

        public void PlayEvent(string eventName)
        {
            _actorIds.Clear();
            _subtitleData.Clear();
            _binarcAreaData.ForEntry(eventName, Bar.EntryType.Event, stream =>
            {
                _eventPlayer = new EventPlayer(this, Event.Read(stream));
                RemoveAllActors();

                _eventPlayer.Initialize();
            });
        }

        public void Update(double deltaTime)
        {
            _isEventPause = _inputManager.RightTrigger;
            _isFreeCam = _inputManager.LeftTrigger;

            if (_eventPlayer != null && _isEventPause == false)
            {
                _eventPlayer.Update(deltaTime);
                if (_eventPlayer.IsEnd)
                {
                    FadeFromBlack(1);
                    _eventPlayer = null;
                }
            }

            foreach (var entity in _actors.Where(x => x.IsMeshLoaded && x.IsVisible))
                entity.Update((float)deltaTime);

            if (_isFading)
            {
                UpdateFade(deltaTime);
            }
        }

        public void Draw()
        {
            foreach (var subtitle in _subtitleData)
                if (subtitle.Value != null)
                    DrawSubtitle(subtitle.Value);

            if (_fadeCurrentColor.A > 0)
                DrawFade();
        }

        public void ForEveryStaticModel(Action<IMonoGameModel> action)
        {
            foreach (var mesh in _skyboxMeshes)
                action(mesh);
            foreach (var mesh in _mapMeshes)
                action(mesh);
        }

        public void ForEveryModel(Action<IEntity, IMonoGameModel> action)
        {
            foreach (var bob in _bobEntities)
                action(bob, _bobModels[bob.BobIndex]);
            foreach (var actor in _actors.Where(x => x.IsVisible))
                action(actor, actor);
        }

        public void AddActor(int actorId, int objectId)
        {
            var entity = new ObjectEntity(_kernel, _kernel.GetRealObjectId(objectId));
            entity.LoadMesh(_graphicsDevice);

            _actors.Add(entity);
            _actorIds[actorId] = entity;
        }

        public void SetActorPosition(int actorId, float x, float y, float z, float rotation)
        {
            var actor = _actorIds[actorId];
            actor.Position = new n.Vector3(x, y, z);
            actor.Rotation = new n.Vector3(0, (float)(rotation * Math.PI / 180) , 0);
        }

        public void SetActorAnimation(int actorId, string path)
        {
            var realPath = GetAnbPath(path);
            if (_kernel.DataContent.FileExists(realPath))
            {
                var binarc = _kernel.DataContent.FileOpen(realPath).Using(Bar.Read);
                binarc.ForEntry(x => x.Type == Bar.EntryType.Motion, stream =>
                {
                    _actorIds[actorId].SetMotion(Motion.Read(stream));
                    return true;
                });
            }
            else
            {
                _actorIds[actorId].SetMotion(null);
            }
        }

        public void SetActorVisibility(int actorId, bool visibility)
        {
            _actorIds[actorId].IsVisible = visibility;
        }

        public void AddActor(SpawnPoint.Entity entityDesc)
        {
            var entity = ObjectEntity.FromSpawnPoint(_kernel, entityDesc);
            entity.LoadMesh(_graphicsDevice);

            _actors.Add(entity);
        }

        public void RemoveAllActors()
        {
            foreach (var actor in _actors)
            {
                foreach (var texture in actor.Textures)
                    texture.Dispose();
            }

            _actorIds.Clear();
            _actors.Clear();
        }

        public void SetCamera(
            n.Vector3 position,
            n.Vector3 lookAt,
            float fieldOfView,
            float roll)
        {
            if (_isFreeCam == true)
                return;

            _camera.CameraPosition = new n.Vector3(position.X, position.Y, position.Z);
            _camera.CameraLookAt = new n.Vector3(lookAt.X, lookAt.Y, lookAt.Z);
            _camera.FieldOfView = (float)(fieldOfView * Math.PI / 180);
        }

        public void FadeToBlack(float seconds)
        {
            _isFading = true;
            _fadeCurrent = 0;
            _fadeGoal = seconds;
            _fadeStartColor = new Color(0, 0, 0, 0.0f);
            _fadeEndColor = new Color(0, 0, 0, 1.0f);
            _fadeCurrentColor = _fadeStartColor;
        }

        public void FadeToWhite(float seconds)
        {
            _isFading = true;
            _fadeCurrent = 0;
            _fadeGoal = seconds;
            _fadeStartColor = new Color(1, 1, 1, 0.0f);
            _fadeEndColor = new Color(1, 1, 1, 1.0f);
        }

        public void FadeFromBlack(float seconds)
        {
            _isFading = true;
            _fadeCurrent = 0;
            _fadeGoal = seconds;
            _fadeStartColor = new Color(0, 0, 0, 1.0f);
            _fadeEndColor = new Color(0, 0, 0, 0.0f);
        }

        public void FadeFromWhite(float seconds)
        {
            _isFading = true;
            _fadeCurrent = 0;
            _fadeGoal = seconds;
            _fadeStartColor = new Color(1, 1, 1, 1.0f);
            _fadeEndColor = new Color(1, 1, 1, 0.0f);
        }

        private void UpdateFade(double deltaTime)
        {
            _fadeCurrent += (float)deltaTime;
            if (_fadeCurrent >= _fadeGoal)
            {
                _fadeCurrent = _fadeGoal;
                _isFading = false;
            }

            _fadeCurrentColor.A = (byte)MathEx.Lerp(
                _fadeStartColor.A,
                _fadeEndColor.A,
                _fadeCurrent / _fadeGoal);
        }

        private void DrawFade()
        {
            const float Size = 5000f;
            _shader.Pass(pass =>
            {
                _graphicsDevice.BlendState = BlendState.AlphaBlend;

                _shader.UseAlphaMask = true;
                _shader.SetProjectionViewIdentity();
                _shader.SetWorldViewIdentity();
                _shader.SetModelViewIdentity();
                pass.Apply();

                _graphicsDevice.DrawUserPrimitives(
                    PrimitiveType.TriangleStrip,
                    new VertexPositionColor[]
                    {
                        new VertexPositionColor
                        {
                            Position = new Vector3(-Size, -Size, 0),
                            Color = _fadeCurrentColor
                        },
                        new VertexPositionColor
                        {
                            Position = new Vector3(Size, -Size, 0),
                            Color = _fadeCurrentColor
                        },
                        new VertexPositionColor
                        {
                            Position = new Vector3(-Size, Size, 0),
                            Color = _fadeCurrentColor
                        },
                        new VertexPositionColor
                        {
                            Position = new Vector3(Size, Size, 0),
                            Color = _fadeCurrentColor
                        },
                    }, 0, 2);
            });
        }

        public void ShowSubtitle(int subtitleId, ushort messageId)
        {
            var msgProvider = messageId < 0x8000 ?
                _eventMessageProvider : _kernel.MessageProvider;
            _subtitleData[subtitleId] = msgProvider.GetMessage(messageId);
        }

        public void HideSubtitle(int subtitleId)
        {
            _subtitleData[subtitleId] = null;
        }

        private void DrawSubtitle(byte[] data)
        {
            _messageDrawContext.GlobalScale = 1.0f;
            _messageDrawContext.WidthMultiplier = 1.2f;
            _messageDrawContext.IgnoreDraw = true;
            _messageDrawContext.x = 0;
            _messageDrawContext.y = 0;
            _messageDrawContext.Width = 0;
            _messageRenderer.Draw(_messageDrawContext, data);

            _messageDrawContext.GlobalScale = 1.0f;
            _messageDrawContext.WidthMultiplier = 1.2f;
            _messageDrawContext.x = (_messageDrawContext.WindowWidth  - _messageDrawContext.Width) / 2f;
            _messageDrawContext.y = 350;
            _messageDrawContext.IgnoreDraw = false;
            _messageRenderer.Draw(_messageDrawContext, data);

            _drawing.Flush();
        }

        private void RunSpawnScript(
            IEnumerable<Bar.Entry> barEntries, string spawnScriptName, int programId)
        {
            var script = barEntries.ForEntry(spawnScriptName, Bar.EntryType.AreaDataScript, stream =>
                AreaDataScript.Read(stream, programId));
            if (script == null)
                return;

            foreach (var function in script)
            {
                switch (function)
                {
                    case AreaDataScript.Spawn spawn:
                        Log.Info("Loading spawn {0}", spawn.SpawnSet);
                        var spawnPoints = barEntries.ForEntry(spawn.SpawnSet, Bar.EntryType.AreaDataSpawn, SpawnPoint.Read);
                        if (spawnPoints != null)
                        {
                            foreach (var spawnPoint in spawnPoints)
                            {
                                foreach (var desc in spawnPoint.Entities)
                                {
                                    if (desc.SpawnType == 0 || desc.SpawnArgument == _kernel.Entrance)
                                        AddActor(desc);
                                }
                            }
                        }
                        else
                            Log.Warn("Unable to find spawn \"{0}\".", spawn);
                        break;
                }
            }
        }

        private string GetAnbPath(string path)
        {
            if (_kernel.IsReMix)
            {
                switch (_kernel.RegionId)
                {
                    case 0: // jp
                    case Constants.RegionFinalMix: // fm
                        return $"anm/fm/{path}.anb";
                    default:
                        return $"anm/us/{path}.anb";
                }
            }
            else
                return $"anm/{path}.anb";
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
    }
}
