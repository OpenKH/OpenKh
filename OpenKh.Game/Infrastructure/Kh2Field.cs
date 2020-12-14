using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using OpenKh.Common;
using OpenKh.Engine;
using OpenKh.Engine.MonoGame;
using OpenKh.Game.Debugging;
using OpenKh.Game.Entities;
using OpenKh.Game.Events;
using OpenKh.Kh2;
using OpenKh.Kh2.Ard;
using OpenKh.Kh2.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OpenKh.Game.Infrastructure
{
    public class Kh2Field : IField
    {
        private readonly Kernel _kernel;
        private readonly GraphicsDevice _graphicsDevice;
        private readonly KingdomShader _shader;
        private readonly List<ObjectEntity> _actors = new List<ObjectEntity>();

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
            Dictionary<string, string> settings,
            GraphicsDevice graphicsDevice,
            KingdomShader shader)
        {
            _kernel = kernel;
            _graphicsDevice = graphicsDevice;
            _shader = shader;

            _spawnScriptMap = settings.GetInt("SpawnScriptMap", 0x06);
            _spawnScriptBtl = settings.GetInt("SpawnScriptBtl", 0x01);
            _spawnScriptEvt = settings.GetInt("SpawnScriptEvt", 0x16);
            FadeFromBlack(1.0f);
        }

        public void LoadMapArd(int worldIndex, int placeIndex)
        {
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
            if (_fadeCurrentColor.A > 0)
                DrawFade();
        }

        public void ForEveryModel(Action<IEntity, IMonoGameModel> action)
        {
            foreach (var actor in _actors)
                action(actor, actor);
        }

        public void AddActor(int objectId)
        {

        }

        public void AddActor(SpawnPoint.Entity entityDesc)
        {
            var entity = ObjectEntity.FromSpawnPoint(_kernel, entityDesc);
            entity.LoadMesh(_graphicsDevice);

            _actors.Add(entity);
        }

        public void RemoveAllActors()
        {
            _actors.Clear();
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

        public void FrameFromWhite(float seconds)
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
    }
}
