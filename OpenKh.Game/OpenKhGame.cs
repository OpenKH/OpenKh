using Microsoft.Xna.Framework;
using OpenKh.Common;
using OpenKh.Game.DataContent;
using OpenKh.Game.Debugging;
using OpenKh.Game.Infrastructure;
using OpenKh.Game.States;
using OpenKh.Kh2;
using System;
using System.IO;

namespace OpenKh.Game
{
    public class OpenKhGame : Microsoft.Xna.Framework.Game, IStateChange
    {
        private GraphicsDeviceManager graphics;

        private readonly IDataContent _dataContent;
        private readonly Kernel _kernel;
        private readonly ArchiveManager archiveManager;
        private readonly InputManager inputManager;
        private readonly DebugOverlay _debugOverlay;
        private IState state;

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

                        _debugOverlay.OnUpdate = state.DebugUpdate;
                        _debugOverlay.OnDraw = state.DebugDraw;
                        break;
                    case 1:
                        state = new MapState();
                        state.Initialize(GetStateInitDesc());

                        _debugOverlay.OnUpdate = state.DebugUpdate;
                        _debugOverlay.OnDraw = state.DebugDraw;
                        break;
                    default:
                        Log.Err($"Invalid state {value}");
                        break;
                }    
            }
        }

        public OpenKhGame()
        {
            _dataContent = CreateDataContent(".", "KH2.IDX", "KH2.IMG");
            if (Kernel.IsReMixFileHasHdAssetHeader(_dataContent, "fm"))
            {
                Log.Info("ReMIX files with HD asset header detected");
                _dataContent = new HdAssetContent(_dataContent);
            }

            _dataContent = new SafeDataContent(_dataContent);

            _kernel = new Kernel(_dataContent);

            var resolutionWidth = _kernel.IsReMix ?
                Global.ResolutionRemixWidth :
                Global.ResolutionWidth;
            Log.Info($"Internal game resolution set to {resolutionWidth}x{Global.ResolutionHeight}");

            graphics = new GraphicsDeviceManager(this)
            {
                PreferredBackBufferWidth = (int)Math.Round(resolutionWidth * Global.ResolutionBoostRatio),
                PreferredBackBufferHeight = (int)Math.Round(Global.ResolutionHeight * Global.ResolutionBoostRatio)
            };

            Content.RootDirectory = "Content";
            IsMouseVisible = true;

            archiveManager = new ArchiveManager(_dataContent);
            inputManager = new InputManager();
            _debugOverlay = new DebugOverlay(this);
        }

        protected override void Initialize()
        {
            _debugOverlay.Initialize(GetStateInitDesc());
            State = 0;

            base.Initialize();
        }

        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.

            // TODO: use this.Content to load your game content here
        }

        protected override void Update(GameTime gameTime)
        {
            inputManager.Update();
            if (inputManager.IsExit)
                Exit();

            var deltaTimes = GetDeltaTimes(gameTime);

            _debugOverlay.Update(deltaTimes);
            state?.Update(deltaTimes);
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            graphics.GraphicsDevice.Clear(Color.CornflowerBlue);

            var deltaTimes = GetDeltaTimes(gameTime);

            state?.Draw(deltaTimes);
            _debugOverlay.Draw(deltaTimes);
            base.Draw(gameTime);
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
            };
        }

        private DeltaTimes GetDeltaTimes(GameTime gameTime)
        {
            return new DeltaTimes
            {
                DeltaTime = 1.0 / 60.0
            };
        }

        private static IDataContent CreateDataContent(string basePath, string idxFileName, string imgFileName)
        {
            Log.Info($"Base directory is {basePath}");
            if (File.Exists(idxFileName) && File.Exists(imgFileName))
            {
                Log.Info($"{idxFileName} and {imgFileName} has been found");

                var imgStream = File.OpenRead(imgFileName);
                var idxDataContent = File.OpenRead(idxFileName)
                    .Using(stream => new IdxDataContent(stream, imgStream));
                return new MultipleDataContent(
                    new StandardDataContent(basePath),
                    idxDataContent,
                    new IdxMultipleDataContent(idxDataContent, imgStream)
                );
            }
            else
            {
                Log.Info($"No {idxFileName} or {imgFileName}, loading extracted files");
                return new StandardDataContent(basePath);
            }
        }
    }
}
