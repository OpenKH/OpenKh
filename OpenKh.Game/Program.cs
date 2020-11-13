using McMaster.Extensions.CommandLineUtils;
using OpenKh.Game.Debugging;
using System;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace OpenKh.Game
{
    [Command("OpenKh.Game")]
    [VersionOptionFromMember("--version", MemberName = nameof(GetVersion))]
    public class Program
    {
        public static readonly string ProductVersion = typeof(Program).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion;

        [STAThread]
        static void Main(string[] args)
        {
            Log.Info("Boot");
            Log.Info($"Version {ProductVersion}");
            void run()
            {
                Config.Open();
                Config.Listen();

                CommandLineApplication.Execute<Program>(args);

                Config.Close();
                Log.Info("End");
            }

#if DEBUG
            run();
#else
            try
            {
                run();
            }
            catch (Exception ex)
            {
                Log.Err("A fatal error has occurred. Please attach this log to https://github.com/xeeynamo/openkh/issues");
                Catch(ex);
                Log.Close();

                throw ex;
            }
#endif
            Log.Close();
        }

        private static string GetVersion() => ProductVersion;


        [Required]
        [Argument(0, "Content path", "Location of game's data")]
        public string ContentPath { get; }

        private void OnExecute()
        {
            using var game = new OpenKhGame(new OpenKhGameStartup
            {
                ContentPath = ContentPath
            });

            game.Run();
        }

        private static void Catch(Exception ex)
        {
            Log.Err($"{ex.GetType().Name}: {ex.Message}:\n{ex.StackTrace}");
            if (ex.InnerException != null)
                Catch(ex.InnerException);
        }
    }
}
