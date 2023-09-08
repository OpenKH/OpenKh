using McMaster.Extensions.CommandLineUtils;
using OpenKh.Common;
using OpenKh.Patcher;
using System.Collections.Concurrent;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace OpenKh.Command.Patcher
{
    [Command("OpenKh.Command.Patcher")]
    [VersionOptionFromMember("--version", MemberName = nameof(GetVersion))]
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                CommandLineApplication.Execute<Program>(args);
            }
            catch (FileNotFoundException e)
            {
                Console.WriteLine($"The file {e.FileName} cannot be found. The program will now exit.");
            }
            catch (Exception e)
            {
                Console.WriteLine($"FATAL ERROR: {e.Message}\n{e.StackTrace}");
            }
        }
        private static string GetVersion()
            => typeof(Program).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion;

        [Required]
        [Option(ShortName = "o", LongName = "output_folder", Description = "Output folder where built mod files should go")]
        public string OutputFolder { get; }

        [Required]
        [Option(ShortName = "m", LongName = "mods_file", Description = "File listing which mods are enabled in order")]
        public string ModsFile { get; }

        [Required]
        [Option(ShortName = "f", LongName = "mods_folder", Description = "Folder containing installed mods")]
        public string ModsFolder { get; }

        [Required]
        [Option(ShortName = "d", LongName = "game_data", Description = "Folder containing game's extracted data")]
        public string DataFolder { get; }

        [Required]
        [Option(ShortName = "g", LongName = "game_id", Description = "Which game to patch for")]
        [AllowedValues("kh1", "kh2", "bbs", "Recom")]
        public string GameId { get; }

        private void OnExecute()
        {
            var enabled = File.ReadAllLines(ModsFile);

            Directory.Delete(OutputFolder, true);
            Directory.CreateDirectory(OutputFolder);

            var patcher = new PatcherProcessor();
            foreach (var mod_name in enabled)
            {
                var mod_folder = Path.Combine(ModsFolder, mod_name);
                var metadata = File.OpenRead(Path.Combine(mod_folder, "mod.yml")).Using(Metadata.Read);
                Console.WriteLine($"Patching {mod_name}");
                patcher.Patch(DataFolder, OutputFolder, metadata, mod_folder, LaunchGame: GameId);
            }
        }
    }
}
