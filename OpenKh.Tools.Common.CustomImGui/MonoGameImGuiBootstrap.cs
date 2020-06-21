using ImGuiNET;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Diagnostics;
using System.Reflection;

namespace OpenKh.Tools.Common.CustomImGui
{
    public class MonoGameImGuiBootstrap : Game
    {
        public static string ApplicationName = GetApplicationName();

        private readonly Action<MonoGameImGuiBootstrap> _initFunc;
        private GraphicsDeviceManager _graphics;
        private MonoGameImGui _imGuiRenderer;
        private ImFontPtr _imSegoeUiFont;

        public string Title
        {
            get => Window.Title;
            set => Window.Title = value;
        }

        public Action<MonoGameImGuiBootstrap> MainLoop { get; set; }

        public MonoGameImGuiBootstrap(
            int initialWindowWidth, int initialWindowHeight,
            Action<MonoGameImGuiBootstrap> initFunc)
        {
            _graphics = new GraphicsDeviceManager(this)
            {
                PreferredBackBufferWidth = initialWindowWidth,
                PreferredBackBufferHeight = initialWindowHeight
            };

            Content.RootDirectory = "Content";
            Title = ApplicationName;
            _initFunc = initFunc;
        }

        public IntPtr BindTexture(Texture2D texture) =>
            _imGuiRenderer.BindTexture(texture);

        public void UnbindTexture(IntPtr textureId) =>
            _imGuiRenderer.UnbindTexture(textureId);

        protected unsafe override void Initialize()
        {
            _imGuiRenderer = new MonoGameImGui(this);
            ImGuiEx.SetWpfStyle();
            _imSegoeUiFont = ImGuiEx.OpenFontSegoeUi(_imGuiRenderer);
            _initFunc?.Invoke(this);

            base.Initialize();
        }

        protected override void LoadContent()
        {
            // Use this.Content to load your game content here
        }

        protected override void Draw(GameTime gameTime)
        {
            _graphics.GraphicsDevice.Clear(Color.White);

            _imGuiRenderer.BeforeLayout(gameTime);
            ImGui.PushFont(_imSegoeUiFont);
            MainLoop?.Invoke(this);
            ImGui.PopFont();
            _imGuiRenderer.AfterLayout();

            base.Draw(gameTime);
        }

        private static string GetApplicationName()
        {
            var assembly = Assembly.GetEntryAssembly();
            var fvi = FileVersionInfo.GetVersionInfo(assembly.Location);
            return fvi.ProductName;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _imSegoeUiFont.Destroy();
            }

            base.Dispose(disposing);
        }
    }
}
