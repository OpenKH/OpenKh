using OpenKh.Kh2;
using OpenKh.Common;
using McMaster.Extensions.CommandLineUtils;
using System;
using System.IO;
using System.Reflection;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using OpenKh.Tools.Common;
using Xe.BinaryMapper;
using System.Linq;
using System.Text;

namespace OpenKh.Research.Pcsx2Kh2Link
{
    [Command("OpenKh.Research.Pcsx2Kh2Link")]
    [VersionOptionFromMember("--version", MemberName = nameof(GetVersion))]
    [Subcommand(
        typeof(ListCommand))]
    class Program
    {
        static int Main(string[] args)
        {
            try
            {
                return CommandLineApplication.Execute<Program>(args);
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

        [Command("list", Description = "list KH2fm loaded files")]
        private class ListCommand
        {
            protected int OnExecute(CommandLineApplication app)
            {
                using var search = new LinkToPcsx2();

                foreach (var pcsx2 in search.Pcsx2Refs)
                {
                    using var stream = pcsx2.OpenStream();

                    try
                    {
                        foreach (var entry in stream.GetKH2FMLoadedEntries())
                        {
                            Console.WriteLine(entry);
                        }
                    }
                    catch (LinkToPcsx2.KH2fmNotFoundException)
                    {
                        // ignore
                    }
                }

                return 0;
            }
        }
    }
}
