using McMaster.Extensions.CommandLineUtils;
using OpenKh.Common;
using OpenKh.Common.Exceptions;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;

namespace OpenKh.Command.Rbin
{
    [Command("OpenKh.Command.Rbin")]
    [VersionOptionFromMember("--version", MemberName = nameof(GetVersion))]
    [Subcommand(typeof(ListCommand), typeof(ExtractCommand), typeof(ExtractAllCommand), typeof(UnpackCommand))]
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
            [Argument(0, "Rbin File", "The rbin file to list the contents of.")]
            public string FileName { get; set; }

            protected int OnExecute(CommandLineApplication app)
            {
                var fileStream = File.OpenRead(FileName);

                var rbin = Ddd.Rbin.Read(fileStream);

                Console.WriteLine($"Read version {rbin.Version} rbin containing {rbin.TOC.Count} files.");
                Console.WriteLine($"Mount point is {rbin.MountPath}");
                Console.WriteLine("Offset, Size, Compressed, Hash, Name");
                foreach(var entry in rbin.TOC)
                {
                    Console.WriteLine($"{entry.Offset:X8}, {entry.Size:D8}, {entry.IsCompressed, -5}, {entry.Hash:X8}, {entry.Name}");
                }

                return 0;
            }
        }

        private class ExtractCommand
        {
            [Required]
            [Argument(0, "Rbin File", "The rbin file to extract from")]
            public string RbinFilePath { get; set; }

            [Required]
            [Argument(1, "Target File", "The file to extract from the rbin")]
            public string Target { get; set; }

            [Argument(2, "Output Folder")]
            public string OutputFolder { get; set; }

            protected int OnExecute(CommandLineApplication app)
            {
                if (string.IsNullOrWhiteSpace(OutputFolder))
                {
                    OutputFolder = Environment.CurrentDirectory;
                }

                var rbinStream = File.OpenRead(RbinFilePath);
                var rbin = Ddd.Rbin.Read(rbinStream);
                // TODO: If we knew the hash algorithm this would be a binary search not a linear
                //      one as the toc entries are sorted by hash.
                var tocEntry = rbin.TOC.Find(f => f.Name == Target);
                if (tocEntry == null)
                {
                    Console.WriteLine("Target not found in rbin");
                    return 1;
                }

                Directory.CreateDirectory(OutputFolder);
                ExtractFile(rbinStream, tocEntry, OutputFolder);

                return 0;
            }
        }

        private class ExtractAllCommand
        {
            [Required]
            [Argument(0, "Rbin File", "The rbin file to extract from")]
            public string RbinFilePath { get; set; }

            [Argument(2, "Output Folder")]
            public string OutputFolder { get; set; }

            protected int OnExecute(CommandLineApplication app)
            {
                if (string.IsNullOrWhiteSpace(OutputFolder))
                {
                    OutputFolder = Environment.CurrentDirectory;
                }

                var rbinStream = File.OpenRead(RbinFilePath);
                var rbin = Ddd.Rbin.Read(rbinStream);
                Directory.CreateDirectory(OutputFolder);
                foreach (var tocEntry in rbin.TOC)
                {
                    ExtractFile(rbinStream, tocEntry, OutputFolder);
                    Console.WriteLine($"Wrote {Path.Combine(OutputFolder, tocEntry.Name)}");
                }

                return 0;
            }
        }

        private class UnpackCommand
        {
            [Required]
            [Argument(0, "Rbin Folder")]
            public string SrcFolder { get; set; }

            [Required]
            [Argument(1, "Output Folder")]
            public string DstFolder { get; set; }

            protected int OnExecute(CommandLineApplication app)
            {
                Directory.CreateDirectory(DstFolder);
                using (var vfsStream = File.CreateText(Path.Combine(DstFolder, "@vfs.txt")))
                {
                    var rbinList = Directory.EnumerateFiles(SrcFolder, "*.rbin", SearchOption.TopDirectoryOnly).ToList();
                    Console.WriteLine($"Found {rbinList.Count} rbins");
                    System.Diagnostics.Stopwatch stopwatch = System.Diagnostics.Stopwatch.StartNew();
                    int filecount = 0;
                    List<string> issueFiles = new List<string>();
                    foreach (var rbinPath in rbinList)
                    {
                        using (var rbinStream = File.OpenRead(rbinPath))
                        {
                            var rbin = Ddd.Rbin.Read(rbinStream);
                            vfsStream.WriteLine($"{Path.GetFileName(rbinPath)} => {rbin.MountPath}");
                            string fullMountPath = Path.Combine(DstFolder, rbin.MountPath);
                            Directory.CreateDirectory(fullMountPath);
                            Console.WriteLine($"{Path.GetFileName(rbinPath)} => {fullMountPath}");
                            foreach (var tocEntry in rbin.TOC)
                            {
                                if (ExtractFile(rbinStream, tocEntry, fullMountPath))
                                {
                                    Console.WriteLine($"\tWrote {Path.Combine(fullMountPath, tocEntry.Name)}");
                                }
                                else
                                {
                                    Console.WriteLine($"\tFailed to write {Path.Combine(fullMountPath, tocEntry.Name)}");
                                    issueFiles.Add(Path.Combine(Path.GetFileName(rbinPath), tocEntry.Name));
                                }
                            }
                            filecount += rbin.TOC.Count;
                        }
                    }
                    stopwatch.Stop();
                    Console.WriteLine($"Extracted {filecount - issueFiles.Count} files from {rbinList.Count} rbins in {stopwatch.ElapsedMilliseconds} milliseconds");
                    if (issueFiles.Count > 0)
                    {
                        Console.WriteLine("The following files had issues extracting:");
                        foreach (string file in issueFiles)
                        {
                            Console.WriteLine($"\t{file}");
                        }
                    }
                    else
                    {
                        Console.WriteLine("Zero errors detected.");
                    }
                }

                return 0;
            }
        }
        
        private static bool ExtractFile(FileStream stream, Ddd.Rbin.TocEntry tocEntry, string outputFolder)
        {
            try
            {
                var outPath = Path.GetFullPath(Path.Combine(outputFolder, tocEntry.Name));
                var comparison = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                    ? StringComparison.OrdinalIgnoreCase
                    : StringComparison.Ordinal;

                if (!outPath.StartsWith(outputFolder + Path.DirectorySeparatorChar, comparison))
                {
                    throw new Exception($"The file {tocEntry.Name} is outside the output directory");
                }

                stream.Seek(tocEntry.Offset, SeekOrigin.Begin);
                if (tocEntry.IsCompressed)
                {
                    File.WriteAllBytes(outPath, Ddd.Utils.BLZ.Uncompress(stream, (int)tocEntry.Size));
                }
                else
                {
                    //var writeStream = File.OpenWrite(outPath);
                    using var writeStream = new FileStream(outPath, FileMode.Create, FileAccess.Write, FileShare.None);
                    writeStream.Write(stream.ReadBytes((int)tocEntry.Size));
                }

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"EXCEPTION: {ex.Message}");
                return false;
            }
        }
    }
}
