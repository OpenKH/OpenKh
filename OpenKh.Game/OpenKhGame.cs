using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using OpenKh.Common;
using OpenKh.Game.DataContent;
using OpenKh.Game.Debugging;
using OpenKh.Game.Infrastructure;
using OpenKh.Game.States;
using System.IO;

namespace OpenKh.Game
{
    public class OpenKhGame : Microsoft.Xna.Framework.Game, IStateChange
    {
        private GraphicsDeviceManager graphics;
        private SpriteBatch spriteBatch;

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
                }    
            }
        }

        public OpenKhGame()
        {
            graphics = new GraphicsDeviceManager(this)
            {
                PreferredBackBufferWidth = 512,
                PreferredBackBufferHeight = 448
            };

            Content.RootDirectory = "Content";
            IsMouseVisible = true;

            if (File.Exists("KH2.IDX") && File.Exists("KH2.IMG"))
            {
                var imgStream = File.OpenRead("KH2.IMG");
                var idxDataContent = File.OpenRead("KH2.IDX")
                    .Using(stream => new IdxDataContent(stream, imgStream));
                _dataContent = new SafeDataContent(new MultipleDataContent(
                    new StandardDataContent(),
                    idxDataContent,
                    new IdxMultipleDataContent(idxDataContent, imgStream)
                ));
            }
            else
                _dataContent = new SafeDataContent(new StandardDataContent());

            archiveManager = new ArchiveManager(_dataContent);
            _kernel = new Kernel(_dataContent);
            inputManager = new InputManager();
            _debugOverlay = new DebugOverlay(this);
        }

        protected override void Initialize()
        {
            _debugOverlay.Initialize(GetStateInitDesc());
            State = 0;

            base.Initialize();
        }

        private StateInitDesc GetStateInitDesc()
        {
            return new StateInitDesc
            {
                DataContent = _dataContent,
                ArchiveManager = archiveManager,
                Kernel = _kernel,
                InputManager = inputManager,
                GraphicsDevice = graphics
            };
        }

        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

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

        private DeltaTimes GetDeltaTimes(GameTime gameTime)
        {
            return new DeltaTimes
            {
                DeltaTime = 1.0 / 60.0
            };
        }
    }
}
