using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using OpenKh.Common;
using OpenKh.Game.DataContent;
using OpenKh.Game.Infrastructure;
using OpenKh.Game.States;
using System.IO;

namespace OpenKh.Game
{
    public class OpenKhGame : Microsoft.Xna.Framework.Game
    {
        private GraphicsDeviceManager graphics;
        private SpriteBatch spriteBatch;

        private readonly IDataContent _dataContent;
        private readonly Kernel _kernel;
        private readonly ArchiveManager archiveManager;
        private readonly InputManager inputManager;
        private IState state;

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
        }

        protected override void Initialize()
        {
            state = new MapState();
            state.Initialize(new StateInitDesc
            {
                DataContent = _dataContent,
                ArchiveManager = archiveManager,
                Kernel = _kernel,
                InputManager = inputManager,
                GraphicsDevice = graphics
            });

            base.Initialize();
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

            state?.Update(new DeltaTimes
            {
                DeltaTime = 1.0 / 60.0
            });
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            graphics.GraphicsDevice.Clear(Color.CornflowerBlue);

            // TODO: Add your drawing code here

            state?.Draw(new DeltaTimes
            {

            });
            base.Draw(gameTime);
        }
    }
}
