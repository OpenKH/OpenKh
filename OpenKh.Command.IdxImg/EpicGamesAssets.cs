using McMaster.Extensions.CommandLineUtils;
using OpenKh.Common;
using OpenKh.Egs;
using OpenKh.Patcher;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;

namespace OpenKh.Command.IdxImg
{
    partial class Program
    {
        [Command("hed", Description = "Make operation on the Epic Games Store release of Kingdom Hearts"),
         Subcommand(typeof(ExtractCommand)),
         Subcommand(typeof(PatchCommand)),
         Subcommand(typeof(ListCommand)),
         Subcommand(typeof(FullPatchCommand)),
         Subcommand(typeof(BuildCommand))]
        public class EpicGamesAssets
        {
            protected int OnExecute(CommandLineApplication app)
            {
                app.ShowHelp();
                return 1;
            }

            public class ExtractCommand
            {
                [Required]
                [Argument(0, Description = "Kingdom Hearts HED input filter; you can specify 'kh2_first.hed', 'kh2_*' or even '*' to extract everything.")]
                public string InputHed { get; set; }

                [Option(CommandOptionType.SingleValue, Description = "Path where the content will be extracted", ShortName = "o", LongName = "output")]
                public string OutputDir { get; set; }

                [Option(CommandOptionType.NoValue, Description = "Do not extract files that are already found in the destination directory", ShortName = "n")]
                public bool DoNotExtractAgain { get; set; }

                [Option(CommandOptionType.NoValue, Description = "Extract files in their raw bundled format", ShortName = "r", LongName = "raw")]
                public bool ExtractRaw { get; set; }

                protected int OnExecute(CommandLineApplication app)
                {
                    var directory = Path.GetDirectoryName(InputHed);
                    var filter = Path.GetFileName(InputHed);

                    if (!ExtractRaw)
                    {
                        Directory
                            .GetFiles(string.IsNullOrEmpty(directory) ? "." : directory, filter)
                            .Where(x => x.EndsWith(".hed"))
                            .AsParallel()
                            .ForAll(inputHed => Egs.EgsTools.Extract(inputHed, OutputDir, DoNotExtractAgain));
                    }
                    else
                    {
                        Directory
                            .GetFiles(string.IsNullOrEmpty(directory) ? "." : directory, filter)
                            .Where(x => x.EndsWith(".hed"))
                            .AsParallel()
                            .ForAll(inputHed => Egs.EgsTools.ExtractRaw(inputHed, OutputDir, DoNotExtractAgain));
                    }

                    return 0;
                }
            }

            [Command("patch", Description = "Replace one or multiple files in a PKG file.")]
            private class PatchCommand
            {
                [Required]
                [Argument(0, Description = "The PKG file that will be patched.")]
                public string PkgFile { get; set; }

                [Required]
                [Argument(1, Description = "Folder that contains the files to replace.")]
                public string InputFolder { get; set; }

                [Option(CommandOptionType.SingleValue, Description = "Path where the patched PKG will be dropped.", ShortName = "o", LongName = "output")]
                public string OutputDir { get; set; }

                protected int OnExecute(CommandLineApplication app)
                {
                    EgsTools.Patch(PkgFile, InputFolder, OutputDir);

                    return 0;
                }
            }

            [Command("list", Description = "List the content of a HED file ")]
            private class ListCommand
            {
                [Required]
                [Argument(0, Description = "Kingdom Hearts HED input file")]
                public string InputHed { get; set; }

                protected int OnExecute(CommandLineApplication app)
                {
                    EgsTools.List(InputHed);

                    return 0;
                }
            }

            private class BuildCommand
            {
                [Required]
                [Option(ShortName = "o", LongName = "output_folder", Description = "Output folder where built mod files should go")]
                public string OutputFolder { get; }

                [Required]
                [Option(ShortName = "e", LongName = "enabled_mods", Description = "File listing which mods are enabled in order")]
                public string ModsFile { get; }

                [Required]
                [Option(ShortName = "f", LongName = "mods_folder", Description = "Folder containing installed mods")]
                public string ModsFolder { get; }

                [Required]
                [Option(ShortName = "d", LongName = "game_data", Description = "Folder containing game's extracted data")]
                public string DataFolder { get; }

                [Required]
                [Option(ShortName = "g", LongName = "game_id", Description = "Which game to patch for")]
                [McMaster.Extensions.CommandLineUtils.AllowedValues("kh1", "kh2", "bbs", "Recom", "kh3d")]
                public string GameId { get; }

                protected int OnExecute(CommandLineApplication app)
                {
                    var enabled = File.ReadAllLines(ModsFile);

                    if (Directory.Exists(OutputFolder))
                        Directory.Delete(OutputFolder, true);
                    Directory.CreateDirectory(OutputFolder);

                    var map = new ConcurrentDictionary<string, string>();
                    var patcher = new PatcherProcessor();
                    foreach (var mod_name in enabled.Reverse())
                    {
                        var mod_folder = Path.Combine(ModsFolder, mod_name);
                        var metadata = File.OpenRead(Path.Combine(mod_folder, "mod.yml")).Using(Metadata.Read);
                        Console.WriteLine($"Patching {mod_name}");
                        patcher.Patch(DataFolder, OutputFolder, metadata, mod_folder, platform: 2, packageMap: map, LaunchGame: GameId);
                    }

                    using (var packageMapWriter = new StreamWriter(Path.Combine(OutputFolder, "patch-package-map.txt")))
                    {
                        foreach (var entry in map)
                            packageMapWriter.WriteLine(entry.Key + " $$$$ " + entry.Value);
                    }

                    return 0;
                }
            }

            private class FullPatchCommand
            {
                [Required]
                [Option(ShortName = "b", LongName = "build_folder", Description = "Folder containing built mods")]
                public string BuildFolder { get; }
                [Required]
                [Option(ShortName = "o", LongName = "output_folder", Description = "Output folder where patched files should go")]
                public string OutputFolder { get; }
                [Required]
                [Option(ShortName = "f", LongName = "source_folder", Description = "Folder containing game's unpatched files")]
                public string SourceFolder { get; }
                protected int OnExecute(CommandLineApplication app)
                {
                    var packageMapLocation = Path.Combine(BuildFolder, "patch-package-map.txt");
                    var packageMap = File
                        .ReadLines(packageMapLocation)
                        .Select(line => line.Split(" $$$$ "))
                        .ToDictionary(array => array[0], array => array[1]);

                    var patchStagingDir = Path.Combine(BuildFolder, "patch-staging");
                    if (Directory.Exists(patchStagingDir))
                        Directory.Delete(patchStagingDir, true);
                    Directory.CreateDirectory(patchStagingDir);
                    foreach (var entry in packageMap)
                    {
                        var sourceFile = Path.Combine(BuildFolder, entry.Key);
                        var destFile = Path.Combine(patchStagingDir, entry.Value);
                        Directory.CreateDirectory(Path.GetDirectoryName(destFile));
                        File.Move(sourceFile, destFile);
                    }

                    foreach (var directory in Directory.GetDirectories(BuildFolder))
                    {
                        if (!"patch-staging".Equals(Path.GetFileName(directory)))
                            Directory.Delete(directory, true);
                    }

                    var stagingDirs = Directory.GetDirectories(patchStagingDir).Select(directory => Path.GetFileName(directory)).ToHashSet();

                    string[] specialDirs = Array.Empty<string>();
                    var specialStagingDir = Path.Combine(patchStagingDir, "special");
                    if (Directory.Exists(specialStagingDir))
                        specialDirs = Directory.GetDirectories(specialStagingDir).Select(directory => Path.GetFileName(directory)).ToArray();

                    foreach (var packageName in stagingDirs)
                        Directory.Move(Path.Combine(patchStagingDir, packageName), Path.Combine(BuildFolder, packageName));
                    foreach (var specialDir in specialDirs)
                        Directory.Move(Path.Combine(BuildFolder, "special", specialDir), Path.Combine(BuildFolder, specialDir));

                    stagingDirs.Remove("special");
                    Directory.Delete(patchStagingDir, true);

                    var specialModDir = Path.Combine(BuildFolder, "special");
                    if (Directory.Exists(specialModDir))
                        Directory.Delete(specialModDir, true);

                    foreach (var directory in stagingDirs.Select(packageDir => Path.Combine(BuildFolder, packageDir)))
                    {
                        if (specialDirs.Contains(Path.GetDirectoryName(directory)))
                            continue;

                        var patchFiles = new List<string>();
                        var _dirPart = new DirectoryInfo(directory).Name;

                        var _orgPath = Path.Combine(directory, "original");
                        var _rawPath = Path.Combine(directory, "remastered");

                        if (Directory.Exists(_orgPath))
                            patchFiles = OpenKh.Egs.Helpers.GetAllFiles(_orgPath).ToList();

                        if (Directory.Exists(_rawPath))
                            patchFiles.AddRange(OpenKh.Egs.Helpers.GetAllFiles(_rawPath).ToList());

                        string _pkgSoft = _dirPart;
                        var _pkgSource = Path.Combine(SourceFolder, _pkgSoft + ".pkg");

                        var hedFile = Path.ChangeExtension(_pkgSource, "hed");

                        using var hedStream = File.OpenRead(hedFile);
                        using var pkgStream = File.OpenRead(_pkgSource);
                        var hedHeaders = OpenKh.Egs.Hed.Read(hedStream).ToList();

                        if (!Directory.Exists(OutputFolder))
                            Directory.CreateDirectory(OutputFolder);

                        using var patchedHedStream = File.Create(Path.Combine(OutputFolder, Path.GetFileName(hedFile)));
                        using var patchedPkgStream = File.Create(Path.Combine(OutputFolder, Path.GetFileName(_pkgSource)));

                        foreach (var hedHeader in hedHeaders)
                        {
                            var hash = OpenKh.Egs.Helpers.ToString(hedHeader.MD5);

                            // We don't know this filename, we ignore it 
                            if (!OpenKh.Egs.EgsTools.Names.TryGetValue(hash, out var filename))
                                continue;

                            var asset = new OpenKh.Egs.EgsHdAsset(pkgStream.SetPosition(hedHeader.Offset));

                            if (patchFiles.Contains(filename))
                            {
                                patchFiles.Remove(filename);

                                if (hedHeader.DataLength > 0)
                                {
                                    OpenKh.Egs.EgsTools.ReplaceFile(directory, filename, patchedHedStream, patchedPkgStream, asset, hedHeader);
                                    Console.WriteLine($"Replacing File {filename} in {_pkgSoft}");
                                }
                            }

                            else
                            {
                                OpenKh.Egs.EgsTools.ReplaceFile(directory, filename, patchedHedStream, patchedPkgStream, asset, hedHeader);
                                Console.WriteLine($"Skipped File {filename} in {_pkgSoft}");
                            }
                        }

                        // Add all files that are not in the original HED file and inject them in the PKG stream too 
                        foreach (var filename in patchFiles)
                        {
                            OpenKh.Egs.EgsTools.AddFile(directory, filename, patchedHedStream, patchedPkgStream);
                            Console.WriteLine($"Adding File {filename} to {_pkgSoft}");
                        }

                        hedStream.Close();
                        pkgStream.Close();

                        patchedHedStream.Close();
                        patchedPkgStream.Close();
                    }
                    return 0;
                }
            }
        }
    }
}
