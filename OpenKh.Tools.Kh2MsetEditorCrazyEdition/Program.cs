using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.DependencyInjection;
using OpenKh.Tools.Common.CustomImGui;
using OpenKh.Tools.Kh2MsetEditorCrazyEdition.DependencyInjection;
using System;
using System.IO;
using System.Reflection;

namespace OpenKh.Tools.Kh2MsetEditorCrazyEdition
{
    [Command("OpenKh.Tools.Kh2MsetEditorCrazyEdition")]
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

        [Argument(0, "KH2 GamePath")]
        public string? GamePath { get; }

        [Option(Description = "Path to Kh2Presets dir", ShortName = "d", LongName = "kh2-presets")]
        public string? Kh2PresetsDir { get; }
        #endregion

        const int InitialWindowWidth = 1000;
        const int InitialWindowHeight = 800;

        public void Run()
        {
            var builder = new ServiceCollection();

            builder.UseKh2MsetEditorCrazyEdition(
                InitialWindowWidth,
                InitialWindowHeight,
                GamePath,
                Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Kh2PresetsDir ?? "Kh2Presets"))
            );

            using var container = builder.BuildServiceProvider();

            var bootstrap = container.GetRequiredService<MonoGameImGuiBootstrap>();

            bootstrap.Run();
        }

        public void Dispose()
        {
            // nop
        }
    }
}
