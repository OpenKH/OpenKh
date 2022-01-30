using McMaster.Extensions.CommandLineUtils;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Text;
using System.Linq;
using System.Reflection;

namespace OpenKh.Command.Bbsa
{
    [Command("OpenKh.Command.Bbsa")]
    [VersionOptionFromMember("--version", MemberName = nameof(GetVersion))]
    [Subcommand(typeof(RepackCommand), typeof(ExtractCommand), typeof(ListCommand))]
    public class Program
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
            catch (ArchiveNotFoundException e)
            {
                Console.WriteLine(e.Message);
                return 3;
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

        private class RepackCommand
        {
            [Required]
            [DirectoryExists]
            [Argument(0, Description = "Required. Path where the various BBSAx.DAT files are located")]
            public string InputPath { get; set; }

            [Required]
            [DirectoryExists]
            [Argument(1, Description = "Required. Path where the various extracted files are located")]
            public string InputFiles { get; set; }

            [Required]
            [DirectoryExists]
            [Argument(2, Description = "Required. Path where the output will be")]
            public string OutputDir { get; set; }

            [Option(CommandOptionType.SingleValue, Description = "Archive file name prefix. By default it is 'BBS'.", ShortName = "p", LongName = "prefix")]
            public string ArchivePrefix { get; set; }

            protected int OnExecute(CommandLineApplication app)
            {
                var prefix = ArchivePrefix ?? "BBS";

                if (!DoesContainBbsa(InputPath, prefix))
                    throw new ArchiveNotFoundException(InputPath, 0);

                Bbs.Bbsa.RepackfromFolder(InputPath, InputFiles, OutputDir, ArchivePrefix);

                return 0;
            }

        }
        private class ExtractCommand
        {
            [Required]
            [DirectoryExists]
            [Argument(0, Description = "Required. Path where the various BBSAx.DAT files are located")]
            public string InputPath { get; set; }

            [Option(CommandOptionType.SingleValue, Description = "Path where the content will be extracted", ShortName = "o", LongName = "output")]
            public string OutputDir { get; set; }

            [Option(CommandOptionType.SingleValue, Description = "Archive file name prefix. By default it is 'BBS'.", ShortName = "p", LongName = "prefix")]
            public string ArchivePrefix { get; set; }

            [Option(CommandOptionType.NoValue, Description = "Show more file information.", ShortName = "d", LongName = "detailed")]
            public bool DetailedLog { get; set; }

            [Option(CommandOptionType.NoValue, Description = "Export all files information to LOG.txt on Output.", ShortName = "l", LongName = "log")]
            public bool Log { get; set; }

            [Option(CommandOptionType.SingleValue, Description = "Export all files from desired BBS{x}.DAT index archive.", ShortName = "i", LongName = "index")]
            public int Index { get; set; } = -1;

            protected int OnExecute(CommandLineApplication app)
            {
                var prefix = ArchivePrefix ?? "BBS";

                if (!DoesContainBbsa(InputPath, prefix))
                    throw new ArchiveNotFoundException(InputPath, 0);

                var bbsaFileNames = Enumerable.Range(0, 5)
                    .Select(x => Path.Combine(InputPath, $"{prefix}{x}.DAT"));

                var outputDir = OutputDir ?? Path.Combine(Path.GetDirectoryName(InputPath), prefix);

                ExtractArchives(bbsaFileNames, outputDir, prefix, Index, DetailedLog, Log);
                return 0;
            }

            private static void ExtractArchives(IEnumerable<string> bbsaFileNames, string outputDir, string prefix, int index = -1, bool detailed = false, bool log = false)
            {
                var streams = bbsaFileNames
                    .Select(x => File.OpenRead(x))
                    .ToArray();

                var bbsa = Bbs.Bbsa.Read(streams[0]);

                StringBuilder Sb = null;
                if (log)
                {
                    Sb = new StringBuilder();
                    Sb.Append($"OpenKingdomHearts BirthBySleep Archive Command Ansem Report\r\n{DateTime.Now.ToString("F")}\r\n\r\n");
                }

                foreach (var file in index >= 0 ? bbsa.Files.Where(x => x.ArchiveIndex == index) : bbsa.Files)
                {
                    var name = file.CalculateNameWithExtension(i => streams[i]);
                    var bbsaFileStream = file.OpenStream(i => streams[i]);
                    if (bbsaFileStream == null)
                        continue;

                    var destinationFileName = Path.Combine(Path.Combine(outputDir, $"{prefix}{file.ArchiveIndex}"), name);
                    var destinationFolder = Path.GetDirectoryName(destinationFileName);
                    if (!Directory.Exists(destinationFolder))
                        Directory.CreateDirectory(destinationFolder);
                    streams[0].Position = file.Location + 4;
                    var kk = new BinaryReader(streams[0]).ReadUInt32();
                    var offs = kk >> 12;
                    var siz = kk & 0xFFF;
                    var info = (offs << 12) + siz;
                    Bbs.Bbsa.CalculateArchiveOffset(bbsa.GetHeader(), file.offset, out var nuind, out var coffs);

                    Console.WriteLine(name + $" -- {prefix}{file.ArchiveIndex}.DAT");

                    if (detailed)
                    {
                        Console.WriteLine($"OffsetStr: 0x{file.Location.ToString("X2")} on BBS0\n" +
                                $"PointerValue: 0x{kk.ToString("X2")}\n" +
                                $"CalculatedOffset: {coffs}\n" +
                                $"Offset: {offs}\n" +
                                $"Size: 0x{siz.ToString("X2")} Sectors\n" +
                                $"\nSupraInfo: 0x{info.ToString("X2")}\n");
                    }

                    if (log)
                    {
                        Sb.AppendLine(file.Name + $" -- {prefix}{file.ArchiveIndex}.DAT" +
                                    $"\r\nOffsetStr: 0x{file.Location.ToString("X2")} on BBS0\r\n" +
                                        $"PointerValue: 0x{kk.ToString("X2")}\r\n" +
                                        $"CalculatedOffset: {coffs}\r\n" +
                                        $"Offset: {offs}\r\n" +
                                        $"Size: 0x{siz.ToString("X2")} Sectors\r\n" +
                                        $"\nSupraInfo: 0x{info.ToString("X2")}\r\n-------------//--------------\r\n");
                    }

                    using (var outStream = File.Create(destinationFileName))
                        bbsaFileStream.CopyTo(outStream);
                }

                if (log)
                {
                    Sb.AppendLine("---------END--------");
                    File.WriteAllText(Path.Combine(outputDir, "ExtractReportLOG.txt"), Sb.ToString());
                }
            }
        }
        private class ListCommand
        {
            [Required]
            [DirectoryExists]
            [Argument(0, Description = "Required. Path where the various BBSAx.DAT files are located")]
            public string InputPath { get; set; }

            [Option(CommandOptionType.SingleValue, Description = "Archive file name prefix. By default it is 'BBS'.", ShortName = "p", LongName = "prefix")]
            public string ArchivePrefix { get; set; }

            [Option(CommandOptionType.NoValue, Description = "Show more file information.", ShortName = "d", LongName = "detailed")]
            public bool DetailedLog { get; set; }

            [Option(CommandOptionType.NoValue, Description = "Wait for user input to seek next file information.", ShortName = "w", LongName = "wait")]
            public bool WaitEachFile { get; set; }

            [Option(CommandOptionType.NoValue, Description = "List all files information to LOG.txt on Input.", ShortName = "l", LongName = "log")]
            public bool Log { get; set; }

            [Option(CommandOptionType.SingleValue, Description = "List all files from desired BBS{x}.DAT index archive.", ShortName = "i", LongName = "index")]
            public int Index { get; set; }

            protected int OnExecute(CommandLineApplication app)
            {
                var prefix = ArchivePrefix ?? "BBS";

                if (!DoesContainBbsa(InputPath, prefix))
                    throw new ArchiveNotFoundException(InputPath, 0);

                var bbsaFileName = Path.Combine(InputPath, $"{prefix}{0}.DAT");
                using var stream = File.OpenRead(bbsaFileName);
                var bbsa = Bbs.Bbsa.Read(stream);

                StringBuilder Sb = null;
                if (Log)
                {
                    Sb = new StringBuilder();
                    Sb.Append($"OpenKingdomHearts BirthBySleep Archive Command Ansem Report\r\n{DateTime.Now.ToString("F")}\r\n\r\n");
                }
                int i = Index >= 0 ? Index: 0;

                for (; i < 5; i++)
                {
                    foreach (var file in bbsa.Files.Where(x => x.ArchiveIndex==i))
                    {
                            Console.WriteLine(file.Name + $" -- {prefix}{file.ArchiveIndex}.DAT");
                            stream.Position = file.Location + 4;
                            var kk = new BinaryReader(stream).ReadUInt32();
                            var offs = kk >> 12;
                            var siz = kk & 0xFFF;
                            var info = (offs << 12) + siz;
                            Bbs.Bbsa.CalculateArchiveOffset(bbsa.GetHeader(), file.offset, out var nuind, out var coffs);

                            if (DetailedLog)
                            {
                                Console.WriteLine($"\nOffsetStr: 0x{file.Location.ToString("X2")} on BBS0\n" +
                                    $"PointerValue: 0x{kk.ToString("X2")}\n" +
                                    $"CalculatedOffset: {coffs}\n" +
                                    $"Offset: {offs}\n" +
                                    $"Size: 0x{siz.ToString("X2")} Sectors\n" +
                                    $"\nSupraInfo: 0x{info.ToString("X2")}");
                                if (WaitEachFile)
                                    Console.ReadLine();
                            }

                            if (Log)
                            {
                                Sb.AppendLine(file.Name + $" -- {prefix}{file.ArchiveIndex}.DAT" +
                                    $"\r\nOffsetStr: 0x{file.Location.ToString("X2")} on BBS0\r\n" +
                                        $"PointerValue: 0x{kk.ToString("X2")}\r\n" +
                                        $"CalculatedOffset: {coffs}\r\n" +
                                        $"Offset: {offs}\r\n" +
                                        $"Size: 0x{siz.ToString("X2")} Sectors\r\n" +
                                        $"\nSupraInfo: 0x{info.ToString("X2")}\r\n-------------//--------------\r\n");
                            }
                    }
                }


                if (Log)
                {
                    Sb.AppendLine("---------END--------");
                    File.WriteAllText(Path.Combine(InputPath, "ReportLOG.txt"), Sb.ToString());
                }

                return 0;
            }
        }

        private static bool DoesContainBbsa(string path, string prefix) =>
            File.Exists(Path.Combine(path, $"{prefix}{0}.DAT"));
    }
}
