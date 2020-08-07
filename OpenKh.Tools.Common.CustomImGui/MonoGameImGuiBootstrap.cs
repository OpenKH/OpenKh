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
        private MonoGameImGui _imGuiRenderer;
        private ImFontPtr _imSegoeUiFont;

        public string Title
        {
            get => Window.Title;
            set => Window.Title = value;
        }

        public bool ImGuiWantCaptureKeyboard => _imGuiRenderer.ImGuiWantCaptureKeyboard;
        public bool ImGuiWantCaptureMouse => _imGuiRenderer.ImGuiWantCaptureMouse;
        public bool ImGuiWantTextInput => _imGuiRenderer.ImGuiWantTextInput;

        public Action<MonoGameImGuiBootstrap> MainLoop { get; set; }

        public GraphicsDeviceManager GraphicsDeviceManager { get; }

        public MonoGameImGuiBootstrap(
            int initialWindowWidth, int initialWindowHeight,
            Action<MonoGameImGuiBootstrap> initFunc)
        {
            GraphicsDeviceManager = new GraphicsDeviceManager(this)
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

        public void RebindTexture(IntPtr textureId, Texture2D texture) =>
            _imGuiRenderer.RebindTexture(textureId, texture);

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
            GraphicsDeviceManager.GraphicsDevice.Clear(Color.White);

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
