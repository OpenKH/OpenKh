using OpenKh.Tools.Common.CustomImGui;
using System;

namespace OpenKh.Tools.LayoutEditor
{
    class Program : IDisposable
    {
        [STAThread]
        static void Main(string[] args)
        {
            using var program = new Program(args);
            program.Run();
        }

        const int InitialWindowWidth = 1000;
        const int InitialWindowHeight = 800;
        private readonly MonoGameImGuiBootstrap _bootstrap;
        private App _app;

        public Program(string[] args)
        {
            _bootstrap = new MonoGameImGuiBootstrap(
                InitialWindowWidth,
                InitialWindowHeight,
                Initialize);
            _bootstrap.MainLoop = MainLoop;
        }

        private void Initialize(MonoGameImGuiBootstrap bootstrap)
        {
            _app = new App(bootstrap);
        }

        public void Run()
        {
            _bootstrap.Run();
        }

        private void MainLoop(MonoGameImGuiBootstrap obj)
        {
            if (_app.MainLoop())
                _bootstrap.Exit();
        }

        public void Dispose()
        {
            _bootstrap?.Dispose();
        }
    }
}
