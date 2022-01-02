using OpenKh.Common;
using McMaster.Extensions.CommandLineUtils;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.ComponentModel.DataAnnotations;
using OpenKh.Common.Exceptions;

namespace OpenKh.Command.Rbin
{
    [Command("OpenKh.Command.Rbin")]
    [VersionOptionFromMember("--version", MemberName = nameof(GetVersion))]
    [Subcommand(typeof(ListCommand))]
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
            catch (InvalidFileException e)
            {
                Console.WriteLine($"Invalid File Exception: {e.Message}");
            }
            catch (Exception e)
            {
                Console.WriteLine($"FATAL ERROR: {e.Message}\n{e.StackTrace}");
            }
        }

        private static string GetVersion()
            => typeof(Program).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion;

        protected int OnExecute(CommandLineApplication app)
        {
            app.ShowHelp();
            return 1;
        }

        private class ListCommand
        {
            [Required]
            [Argument(0, "Rbin File", "The rbin file to pack or unpack")]
            public string FileName { get; }

            protected int OnExecute(CommandLineApplication app)
            {
                var fileStream = File.OpenRead(FileName);

                var rbin = Ddd.Rbin.Read(fileStream);

                Console.WriteLine($"Read version {rbin.Version} rbin containing {rbin.FileEntries.Count} files.");
                Console.WriteLine($"Mount point is {rbin.MountPath}");
                Console.WriteLine("Offset, Size, Compressed, Hash, Name");
                foreach(var entry in rbin.FileEntries)
                {
                    Console.WriteLine($"{entry.Offset:X8}, {entry.Size:D8}, {entry.IsCompressed, -5}, {entry.Hash:X8}, {entry.Name}");
                }

                return 0;
            }
        }
        

    }
}
