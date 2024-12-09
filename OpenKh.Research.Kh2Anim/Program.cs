using McMaster.Extensions.CommandLineUtils;
using OpenKh.Common.Exceptions;
using OpenKh.Research.Kh2Anim.Subcommands;
using System;
using System.IO;
using System.Reflection;

namespace OpenKh.Research.Kh2Anim
{
    [Command("OpenKh.Research.Kh2Anim")]
    [VersionOptionFromMember("--version", MemberName = nameof(GetVersion))]
    [Subcommand(
        typeof(BakeCommand),
        typeof(FryCommand),
        typeof(SimpleMdlxCommand)
    )]
    class Program
    {
        static int Main(string[] args)
        {
            try
            {
                return CommandLineApplication.Execute<Program>(args);
            }
            catch (InvalidFileException e)
            {
                Console.WriteLine(e.Message);
                return 3;
            }
            catch (FileNotFoundException e)
            {
                Console.WriteLine($"The file {e.FileName} cannot be found. The program will now exit.");
                return 2;
            }
            catch (Exception e)
            {
                Console.WriteLine($"FATAL ERROR: {e.Message}\n{e.StackTrace}");
                return -1;
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
