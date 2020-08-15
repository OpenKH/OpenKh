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

        class LoadedEntry
        {
            [Data] public uint Unk0 { get; set; }
            [Data(Count = 24)] public string FileName { get; set; }
            [Data(Count = 16)] public byte[] Unk1 { get; set; }
            [Data] public int Len { get; set; }
            [Data] public int Addr1 { get; set; }
            [Data] public int Addr2 { get; set; }
            [Data(Count = 24)] public byte[] Unk2 { get; set; }

            public override string ToString() => $"{Addr1:X8}-{Addr1 + Len - 1:X8} {FileName}";
        }

        [Command("list", Description = "list KH2 loaded items")]
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
