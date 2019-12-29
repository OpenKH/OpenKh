using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using OpenKh.Game.DataContent;
using OpenKh.Game.Infrastructure;
using OpenKh.Game.States;

namespace OpenKh.Game
{
    public class OpenKhGame : Microsoft.Xna.Framework.Game
    {
        private GraphicsDeviceManager graphics;
        private SpriteBatch spriteBatch;

        private readonly IDataContent dataContent;
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

            dataContent = new StandardDataContent();
            archiveManager = new ArchiveManager(dataContent);
            inputManager = new InputManager();
        }

        protected override void Initialize()
        {
            // TODO: Add your initialization logic here

            state = new MapState();
            state.Initialize(new StateInitDesc
            {
                DataContent = dataContent,
                ArchiveManager = archiveManager,
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
