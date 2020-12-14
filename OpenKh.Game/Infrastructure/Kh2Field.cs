using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using OpenKh.Common;
using OpenKh.Engine;
using OpenKh.Engine.Extensions;
using OpenKh.Engine.MonoGame;
using OpenKh.Engine.Renders;
using OpenKh.Game.Debugging;
using OpenKh.Game.Entities;
using OpenKh.Game.Events;
using OpenKh.Kh2;
using OpenKh.Kh2.Ard;
using OpenKh.Kh2.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using n = System.Numerics;

namespace OpenKh.Game.Infrastructure
{
    public class Kh2Field : IField
    {
        private readonly Kernel _kernel;
        private readonly Camera _camera;
        private readonly GraphicsDevice _graphicsDevice;
        private readonly KingdomShader _shader;
        private readonly Kh2MessageRenderer _messageRenderer;
        private readonly DrawContext _messageDrawContext;
        private readonly Kh2MessageProvider _eventMessageProvider;
        private readonly List<ObjectEntity> _actors = new List<ObjectEntity>();
        private readonly Dictionary<int, ObjectEntity> _actorIds = new Dictionary<int, ObjectEntity>();
        private readonly Dictionary<int, byte[]> _subtitleData = new Dictionary<int, byte[]>();
        private readonly MonoSpriteDrawing _drawing;
        private Bar _binarcArd;
        private EventPlayer _eventPlayer;
        private int _spawnScriptMap;
        private int _spawnScriptBtl;
        private int _spawnScriptEvt;

        private bool _isFading;
        private float _fadeCurrent;
        private float _fadeGoal;
        private Color _fadeCurrentColor;
        private Color _fadeStartColor;
        private Color _fadeEndColor;

        public Kh2Field(
            Kernel kernel,
            Camera camera,
            Dictionary<string, string> settings,
            GraphicsDevice graphicsDevice,
            KingdomShader shader)
        {
            _kernel = kernel;
            _camera = camera;
            _graphicsDevice = graphicsDevice;
            _shader = shader;

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

            _spawnScriptMap = settings.GetInt("SpawnScriptMap", 0x06);
            _spawnScriptBtl = settings.GetInt("SpawnScriptBtl", 0x01);
            _spawnScriptEvt = settings.GetInt("SpawnScriptEvt", 0x16);
            FadeFromBlack(1.0f);
        }

        public void LoadMapArd(int worldIndex, int placeIndex)
        {
            _kernel.DataContent
                .FileOpen($"msg/{_kernel.Language}/{Constants.WorldIds[worldIndex]}.bar")
                .Using(stream => Bar.Read(stream))
                .ForEntry(x => x.Type == Bar.EntryType.List, stream =>
                {
                    _eventMessageProvider.Load(Msg.Read(stream));
                    return true;
                });

            string fileName;
            if (_kernel.IsReMix)
                fileName = $"ard/{_kernel.Language}/{Constants.WorldIds[worldIndex]}{placeIndex:D02}.ard";
            else
                fileName = $"ard/{Constants.WorldIds[worldIndex]}{placeIndex:D02}.ard";

            RemoveAllActors();

            _binarcArd = _kernel.DataContent.FileOpen(fileName).Using(Bar.Read);
            RunSpawnScript(_binarcArd, "map", _spawnScriptMap);
            RunSpawnScript(_binarcArd, "btl", _spawnScriptBtl);
            RunSpawnScript(_binarcArd, "evt", _spawnScriptEvt);

            RunEvent("203");
        }

        public void RunEvent(string eventName)
        {
            _binarcArd.ForEntry(eventName, Bar.EntryType.AnimationLoader, stream =>
            {
                _eventPlayer = new EventPlayer(this, Event.Read(stream));
                RemoveAllActors();

                _eventPlayer.Initialize();
            });
        }

        public void Update(double deltaTime)
        {
            _eventPlayer?.Update(deltaTime);
            foreach (var entity in _actors.Where(x => x.IsMeshLoaded))
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

        public void ForEveryModel(Action<IEntity, IMonoGameModel> action)
        {
            foreach (var actor in _actors)
                action(actor, actor);
        }

        public void AddActor(int actorId, int objectId)
        {
            var entity = new ObjectEntity(_kernel, objectId);
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
                    _actorIds[actorId].Motion.UseCustomMotion(Motion.Read(stream));
                    return true;
                });
            }
            else
            {
                _actorIds[actorId].Motion.UseCustomMotion(null);
            }
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
            _camera.CameraPosition = new Vector3(position.X, position.Y, position.Z);
            _camera.CameraLookAt = new Vector3(lookAt.X, lookAt.Y, lookAt.Z);
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
                _shader.ProjectionView = Matrix.Identity;
                _shader.WorldView = Matrix.Identity;
                _shader.ModelView = Matrix.Identity;
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
                                    AddActor(desc);
                            }
                        }
                        else
                            Log.Warn($"Unable to find spawn \"{spawn}\".");
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
    }
}
