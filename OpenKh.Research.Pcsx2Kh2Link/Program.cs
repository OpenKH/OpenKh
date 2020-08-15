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
                var processes = Process.GetProcessesByName("pcsx2");
                if (processes.Length == 0)
                {
                    return 1;
                }

                foreach (var process in processes)
                {
                    Console.WriteLine(process);

                    var processStream = new ProcessStream(process, ToolConstants.Pcsx2BaseAddress, ToolConstants.Ps2MemoryLength);
                    var bufferedStream = new BufferedStream(processStream, 0x10000);

                    bufferedStream.Position = 0x100130;
                    var part = Encoding.GetEncoding("latin1").GetString(bufferedStream.ReadBytes(16));
                    if (part != "\xFE\x01\x02\x3C\x00\x02\x03\x3C\x00\x00\x42\x24\x00\x00\x63\x24")
                    {
                        Console.WriteLine("This is not pcsx2 we are looking for.");
                        continue;
                    }

                    bufferedStream.Position = 0x4F6480;
                    var list = Enumerable.Range(0, 200)
                        .Select(_ => BinaryMapping.ReadObject<LoadedEntry>(bufferedStream))
                        .Where(it => it.Addr1 != 0)
                        .ToArray();

                    foreach (var entry in list)
                    {
                        Console.WriteLine(entry);
                    }
                }

                return 0;
            }
        }
    }
}
