using McMaster.Extensions.CommandLineUtils;
using OpenKh.Game.Debugging;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
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

        [Option(ShortName = "state", Description = "Boot the game into a specific state (0 = Title, 1 = Map, 2 = Menu)")]
        public int InitialState { get; set; }

        [Option(ShortName = "world", Description = "Boot the game into a specific world ID (eg. 'dc')")]
        public string InitialWorld { get; set; }

        [Option(ShortName = "place", Description = "Boot the game into a specific place ID (eg. for dc06 specify '6')")]
        public int InitialPlace { get; set; }

        [Option(ShortName = "spawn-map", Description = "Force the boot map to use a specific spawn script program ID for MAP")]
        public int InitialSpawnScriptMap { get; set; }

        [Option(ShortName = "spawn-btl", Description = "Force the boot map to use a specific spawn script program ID for BTL")]
        public int InitialSpawnScriptBtl { get; set; }

        [Option(ShortName = "spawn-evt", Description = "Force the boot map to use a specific spawn script program ID for EVT")]
        public int InitialSpawnScriptEvt { get; set; }

        private void OnExecute()
        {
            using var game = new OpenKhGame(new OpenKhGameStartup
            {
                ContentPath = ContentPath,
                InitialState = InitialState,
                InitialMap = Kh2.Constants.WorldIds
                    .Select((world, index) => (world, index))
                    .Concat(new (string world, int index)[]
                    {
                        (InitialWorld, -1)
                    })
                    .Where(x => x.world == InitialWorld)
                    .Select(x => x.index)
                    .FirstOrDefault(),
                InitialPlace = InitialPlace,
                InitialSpawnScriptMap = InitialSpawnScriptMap,
                InitialSpawnScriptBtl = InitialSpawnScriptBtl,
                InitialSpawnScriptEvt = InitialSpawnScriptEvt,
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
