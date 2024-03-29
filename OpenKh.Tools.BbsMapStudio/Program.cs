using McMaster.Extensions.CommandLineUtils;
using OpenKh.Tools.Common.CustomImGui;
using System;
using System.Reflection;

namespace OpenKh.Tools.BbsMapStudio
{
    [Command("OpenKh.Tools.BbsMapStudio")]
    [VersionOptionFromMember("--version", MemberName = nameof(GetVersion))]
    class Program : IDisposable
    {
        [STAThread]
        static int Main(string[] args)
        {
            return CommandLineApplication.Execute<Program>(args);
        }

        protected int OnExecute(CommandLineApplication app)
        {
            Run();
            return 0;
        }

        #region CommandLine
        private static string GetVersion()
            => typeof(Program).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion;

        [Argument(0, "BBS GamePath")]
        public string GamePath { get; }
        #endregion

        const int InitialWindowWidth = 1280;
        const int InitialWindowHeight = 720;
        private readonly MonoGameImGuiBootstrap _bootstrap;
        private App _app;

        public Program()
        {
            _bootstrap = new MonoGameImGuiBootstrap(
                InitialWindowWidth,
                InitialWindowHeight,
                Initialize);
            _bootstrap.MainLoop = MainLoop;


        }

        private void Initialize(MonoGameImGuiBootstrap bootstrap)
        {
            _app = new App(bootstrap, GamePath);
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
