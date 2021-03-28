using McMaster.Extensions.CommandLineUtils;
using System;
using System.IO;
using System.Reflection;
using OpenKh.Command.TexFooter.Subcommands;

namespace OpenKh.Command.TexFooter
{
    [Command("OpenKh.Command.TexFooter")]
    [VersionOptionFromMember("--version", MemberName = nameof(GetVersion))]
    [Subcommand(typeof(ExportCommand), typeof(ImportCommand), typeof(YmlToBinCommand), typeof(BinToYmlCommand))]
    class Program
    {
        static int Main(string[] args)
        {
            try
            {
                return CommandLineApplication.Execute<Program>(args);
            }
            catch (FileNotFoundException e)
            {
                Console.WriteLine($"The file {e.FileName} cannot be found. The program will now exit.");
                return 1;
            }
            catch (Exception e)
            {
                Console.WriteLine($"FATAL ERROR: {e.Message}\n{e.StackTrace}");
                return 1;
            }
        }

        protected int OnExecute(CommandLineApplication app)
        {
            app.ShowHelp();
            return 1;
        }

        private static string GetVersion()
            => typeof(Program).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion;
    }
}
