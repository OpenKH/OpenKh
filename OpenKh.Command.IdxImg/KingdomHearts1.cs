using McMaster.Extensions.CommandLineUtils;
using OpenKh.Common;
using OpenKh.Kh1;
using System;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;

namespace OpenKh.Command.IdxImg
{
    partial class Program
    {
        [Command("kh1", Description = "Make operation on Kingdom Hearts 1 ISO file."),
         Subcommand(typeof(ExtractCommand)),
         Subcommand(typeof(ListCommand))]
        private class KingdomHearts1
        {
            private const int IsoBlockAlign = 0x800;

            protected int OnExecute(CommandLineApplication app)
            {
                app.ShowHelp();
                return 1;
            }

            private class ExtractCommand
            {
                [Required]
                [FileExists]
                [Argument(0, Description = "Path to the Kingdom Hearts ISO file")]
                public string InputIso { get; set; }

                [Option(CommandOptionType.SingleValue, Description = "Path where the content will be extracted", ShortName = "o", LongName = "output")]
                public string OutputDir { get; set; }

                [Option(CommandOptionType.NoValue, Description = "Do not extract files that are already found in the destination directory", ShortName = "n")]
                public bool DoNotExtractAgain { get; set; }

                protected int OnExecute(CommandLineApplication app)
                {
                    var outputDir = OutputDir ?? Path.Combine(Path.GetDirectoryName(InputIso), "extract");
                    using var stream = File.OpenRead(InputIso);

                    var firstBlock = IsoUtility.GetFileOffset(stream, "SYSTEM.CNF;1");
                    if (firstBlock == -1)
                        throw new IOException("The file specified seems to not be a valid PlayStation 2 ISO.");

                    var kingdomIdxBlock = IsoUtility.GetFileOffset(stream, "KINGDOM.IDX;1");
                    if (kingdomIdxBlock == -1)
                        throw new IOException("The file specified seems to not be a Kingdom Hearts 1 ISO");

                    var idx = Idx1.Read(stream.SetPosition(kingdomIdxBlock * IsoBlockAlign));
                    var img = new Img1(stream, idx, firstBlock);

                    ExtractIdx(img, outputDir, DoNotExtractAgain);
                    return 0;
                }

                public static void ExtractIdx(
                    Img1 img,
                    string basePath,
                    bool doNotExtractAgain)
                {
                    foreach (var entry in img.Entries)
                    {
                        var fileName = entry.Key;
                        if (fileName == null)
                            fileName = $"@noname/{entry.Value.Hash:X08}";

                        var outputFile = Path.Combine(basePath, fileName);
                        if (doNotExtractAgain && File.Exists(outputFile))
                            continue;

                        var outputDir = Path.GetDirectoryName(outputFile);
                        if (Directory.Exists(outputDir) == false)
                            Directory.CreateDirectory(outputDir);

                        Console.WriteLine(fileName);
                        using var file = File.Create(outputFile);
                        img.FileOpen(entry.Value).CopyTo(file);
                    }
                }
            }

            private class ListCommand
            {
                private Program Parent { get; set; }

                [Required]
                [FileExists]
                [Argument(0, Description = "Path to the Kingdom Hearts ISO file")]
                public string InputIso { get; set; }

                [Option(CommandOptionType.NoValue, Description = "Sort file list by their position in the ISO", ShortName = "s", LongName = "sort")]
                public bool Sort { get; set; }

                protected int OnExecute(CommandLineApplication app)
                {
                    using var stream = File.OpenRead(InputIso);

                    var firstBlock = IsoUtility.GetFileOffset(stream, "SYSTEM.CNF;1");
                    if (firstBlock == -1)
                        throw new IOException("The file specified seems to not be a valid PlayStation 2 ISO.");

                    var kingdomIdxBlock = IsoUtility.GetFileOffset(stream, "KINGDOM.IDX;1");
                    if (kingdomIdxBlock == -1)
                        throw new IOException("The file specified seems to not be a Kingdom Hearts 1 ISO");

                    var idx = Idx1.Read(stream.SetPosition(kingdomIdxBlock * IsoBlockAlign));
                    foreach (var entry in idx.OrderBy(x => x.IsoBlock))
                        Console.WriteLine(Idx1Name.Lookup(entry) ?? $"@{entry.Hash:X08}");

                    return 0;
                }
            }
        }
    }
}
