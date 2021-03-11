using McMaster.Extensions.CommandLineUtils;
using System;
using System.IO;
using System.Reflection;

namespace OpenKh.Command.Model
{
    [Command("OpenKh.Command.Model")]
    [VersionOptionFromMember("--version", MemberName = nameof(GetVersion))]
    [Subcommand(typeof(RecomCommand))]
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
    }
}
