using Microsoft.Xna.Framework;
using OpenKh.Common;
using OpenKh.Game.DataContent;
using OpenKh.Game.Debugging;
using OpenKh.Game.Infrastructure;
using OpenKh.Game.States;
using OpenKh.Game.States.Title;
using System;
using System.Collections.Generic;
using System.IO;

namespace OpenKh.Game
{
    public class OpenKhGameStartup
    {
        public string ContentPath { get; internal set; }
        public int InitialState { get; internal set; }
        public int InitialMap { get; internal set; }
        public int InitialPlace { get; internal set; }
        public int InitialSpawnScriptMap { get; internal set; }
        public int InitialSpawnScriptBtl { get; internal set; }
        public int InitialSpawnScriptEvt { get; internal set; }
    }

    public class OpenKhGame : Microsoft.Xna.Framework.Game, IStateChange
    {
        private GraphicsDeviceManager graphics;

        private readonly Dictionary<string, string> GlobalSettings;
        private readonly IDataContent _dataContent;
        private readonly Kernel _kernel;
        private readonly ArchiveManager archiveManager;
        private readonly InputManager inputManager;
        private readonly DebugOverlay _debugOverlay;
        private readonly OpenKhGameStartup _startup;
        private IState state;
        private bool _isResolutionChanged;

        public int State
        {
            set
            {
                Log.Info($"State={value}");
                switch (value)
                {
                    case 0:
                        state = new TitleState();
                        state.Initialize(GetStateInitDesc());
                        break;
                    case 1:
                        state = new MapState();
                        state.Initialize(GetStateInitDesc());
                        break;
                    case 2:
                        // WARNING: this is quite buggy since no game context will be passed to
                        // the menu, but it useful for quick menu testing.
                        var myState = new MenuState(null);
                        myState.Initialize(GetStateInitDesc());
                        myState.OpenMenu();
                        state = myState;
                        break;
                    default:
                        Log.Err($"Invalid state {value}");
                        return;
                }

                _debugOverlay.OnUpdate = state.DebugUpdate;
                _debugOverlay.OnDraw = state.DebugDraw;
            }
        }

        public OpenKhGame(OpenKhGameStartup startup)
        {
            _startup = startup;
            GlobalSettings = new Dictionary<string, string>();
            TryAddSetting("WorldId", startup.InitialMap);
            TryAddSetting("PlaceId", startup.InitialPlace);
            //TryAddSetting("SpawnId", );
            TryAddSetting("SpawnScriptMap", startup.InitialSpawnScriptMap);
            TryAddSetting("SpawnScriptBtl", startup.InitialSpawnScriptBtl);
            TryAddSetting("SpawnScriptEvt", startup.InitialSpawnScriptEvt);

            var contentPath = startup.ContentPath ?? Config.DataPath;

            _dataContent = CreateDataContent(contentPath, Config.IdxFilePath, Config.ImgFilePath);
            _dataContent = new MultipleDataContent(new ModDataContent(), _dataContent);
            if (Kernel.IsReMixFileHasHdAssetHeader(_dataContent, "fm"))
            {
                Log.Info("ReMIX files with HD asset header detected");
                _dataContent = new HdAssetContent(_dataContent);
            }

            _dataContent = new SafeDataContent(_dataContent);

            _kernel = new Kernel(_dataContent);
            var resolutionWidth = GetResolutionWidth();
            var resolutionHeight = GetResolutionHeight();

            Log.Info($"Internal game resolution set to {resolutionWidth}x{resolutionHeight}");

            graphics = new GraphicsDeviceManager(this)
            {
                PreferredBackBufferWidth = (int)Math.Round(resolutionWidth * Config.ResolutionBoost),
                PreferredBackBufferHeight = (int)Math.Round(resolutionHeight * Config.ResolutionBoost),
                IsFullScreen = Config.IsFullScreen,
            };

            Content.RootDirectory = "Content";
            IsMouseVisible = true;

            archiveManager = new ArchiveManager(_dataContent);
            inputManager = new InputManager();
            _debugOverlay = new DebugOverlay(this);

            Config.OnConfigurationChange += () =>
            {
                var resolutionWidth = GetResolutionWidth();
                var resolutionHeight = GetResolutionHeight();

                var backBufferWidth = (int)Math.Round(resolutionWidth * Config.ResolutionBoost);
                var backBufferHeight = (int)Math.Round(resolutionHeight * Config.ResolutionBoost);

                if (graphics.PreferredBackBufferWidth != backBufferWidth ||
                    graphics.PreferredBackBufferHeight != backBufferHeight ||
                    graphics.IsFullScreen != Config.IsFullScreen)
                {
                    graphics.PreferredBackBufferWidth = backBufferWidth;
                    graphics.PreferredBackBufferHeight = backBufferHeight;
                    graphics.IsFullScreen = Config.IsFullScreen;
                    _isResolutionChanged = true;
                    Log.Info($"Internal game resolution set to {resolutionWidth}x{resolutionHeight}");
                }
            };
        }

        protected override void Initialize()
        {
            _debugOverlay.Initialize(GetStateInitDesc());
            State = _startup.InitialState;

            base.Initialize();
        }

        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.

            // TODO: use this.Content to load your game content here
        }

        protected override void Update(GameTime gameTime)
        {
            inputManager.Update(gameTime);
            if (inputManager.IsExit)
                Exit();

            var deltaTimes = GetDeltaTimes(gameTime);

            if (Config.DebugMode)
                _debugOverlay.Update(deltaTimes);
            state?.Update(deltaTimes);
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            if (_isResolutionChanged)
            {
                graphics.ApplyChanges();
                _isResolutionChanged = false;
            }

            graphics.GraphicsDevice.Clear(Color.CornflowerBlue);

            var deltaTimes = GetDeltaTimes(gameTime);

            state?.Draw(deltaTimes);
            if (Config.DebugMode)
                _debugOverlay.Draw(deltaTimes);
            base.Draw(gameTime);
        }

        private void TryAddSetting(string key, string value)
        {
            if (!string.IsNullOrEmpty(value))
                GlobalSettings.Add(key, value);
        }

        private void TryAddSetting(string key, int value)
        {
            if (value >= 0)
                GlobalSettings.Add(key, value.ToString());
        }

        private StateInitDesc GetStateInitDesc()
        {
            return new StateInitDesc
            {
                DataContent = _dataContent,
                ArchiveManager = archiveManager,
                Kernel = _kernel,
                InputManager = inputManager,
                ContentManager = Content,
                GraphicsDevice = graphics,
                StateChange = this,
                StateSettings = GlobalSettings,
            };
        }

        private DeltaTimes GetDeltaTimes(GameTime gameTime)
        {
            return new DeltaTimes
            {
                DeltaTime = 1.0 / 60.0 * Config.GameSpeed
            };
        }

        private static IDataContent CreateDataContent(string basePath, string idxFileName, string imgFileName)
        {
            Log.Info($"Base directory is {basePath}");

            var idxFullPath = Path.Combine(basePath, idxFileName);
            var imgFullPath = Path.Combine(basePath, imgFileName);
            if (File.Exists(idxFullPath) && File.Exists(imgFullPath))
            {
                Log.Info($"{idxFullPath} and {imgFullPath} has been found");

                var imgStream = File.OpenRead(imgFullPath);
                var idxDataContent = File.OpenRead(idxFullPath)
                    .Using(stream => new IdxDataContent(stream, imgStream));
                return new MultipleDataContent(
                    new StandardDataContent(basePath),
                    idxDataContent,
                    new IdxMultipleDataContent(idxDataContent, imgStream)
                );
            }
            else
            {
                Log.Info($"No {idxFullPath} or {imgFullPath}, loading extracted files");
                return new StandardDataContent(basePath);
            }
        }

        private static int GetResolutionHeight()
        {
            var resolutionHeight = Config.ResolutionHeight;
            if (resolutionHeight == 0)
                resolutionHeight = Global.ResolutionHeight;
            return resolutionHeight;
        }

        private int GetResolutionWidth()
        {
            var resolutionWidth = Config.ResolutionWidth;
            if (resolutionWidth == 0)
                resolutionWidth = _kernel.IsReMix ? Global.ResolutionRemixWidth : Global.ResolutionWidth;
            return resolutionWidth;
        }
    }
}
