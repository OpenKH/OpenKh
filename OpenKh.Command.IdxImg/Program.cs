using McMaster.Extensions.CommandLineUtils;
using System;
using System.IO;
using System.Reflection;

namespace OpenKh.Command.IdxImg
{
    [Command("OpenKh.Command.IdxImg")]
    [VersionOptionFromMember("--version", MemberName = nameof(GetVersion))]
    [Subcommand(typeof(KingdomHearts1), typeof(KingdomHearts2), typeof(EpicGamesAssets))]
    partial class Program
    {
        class CustomException : Exception
        {
            public CustomException(string message) :
                base(message)
            { }
        }

        static int Main(string[] args)
        {
            try
            {
                return CommandLineApplication.Execute<Program>(args);
            }
            catch (FileNotFoundException e)
            {
                Console.WriteLine($"The file {e.FileName} cannot be found. The program will now exit.");
                return 2;
            }
            catch (CustomException e)
            {
                Console.WriteLine(e.Message);
                return 1;
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
