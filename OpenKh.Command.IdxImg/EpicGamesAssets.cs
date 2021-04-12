using McMaster.Extensions.CommandLineUtils;
using OpenKh.Bbs;
using OpenKh.Common;
using OpenKh.Egs;
using OpenKh.Kh1;
using OpenKh.Kh2;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Xe.BinaryMapper;

namespace OpenKh.Command.IdxImg
{
    partial class Program
    {
        [Command("hed", Description = "Make operation on the Epic Games Store release of Kingdom Hearts"),
         Subcommand(typeof(ExtractCommand)),
         Subcommand(typeof(PackCommand))]
        private class EpicGamesAssets
        {
            private static readonly IEnumerable<string> KH2Names = IdxName.Names
                .Concat(IdxName.Names.Where(x => x.Contains("anm/")).SelectMany(x => new string[]
                {
                    x.Replace("anm/", "anm/jp/"),
                    x.Replace("anm/", "anm/us/"),
                    x.Replace("anm/", "anm/fm/")
                }))
                .Concat(Kh2.Constants.Languages.SelectMany(lang =>
                    Kh2.Constants.WorldIds.SelectMany(world =>
                        Enumerable.Range(0, 64).Select(index => Path.Combine("ard", lang).Replace('\\', '/') + $"/{world}{index:D02}.ard"))))
                .Concat(Kh2.Constants.Languages.SelectMany(lang =>
                    Kh2.Constants.WorldIds.SelectMany(world =>
                        Enumerable.Range(0, 64).Select(index => Path.Combine("map", lang).Replace('\\', '/') + $"/{world}{index:D02}.map"))))
                .Concat(Kh2.Constants.Languages.SelectMany(lang =>
                    Kh2.Constants.WorldIds.SelectMany(world =>
                        Enumerable.Range(0, 64).Select(index => Path.Combine("map", lang).Replace('\\', '/') + $"/{world}{index:D02}.bar"))))
                .Concat(IdxName.Names.Where(x => x.StartsWith("bgm/")).Select(x => x.Replace(".bgm", ".win32.scd")))
                .Concat(IdxName.Names.Where(x => x.StartsWith("se/")).Select(x => x.Replace(".seb", ".win32.scd")))
                .Concat(IdxName.Names.Where(x => x.StartsWith("vagstream/")).Select(x => x.Replace(".vas", ".win32.scd")))
                .Concat(IdxName.Names.Where(x => x.StartsWith("voice/")).Select(x => x
                    .Replace(".vag", ".win32.scd")
                    .Replace(".vsb", ".win32.scd")))
                .Concat(new string[]
                {
                    "item-011.imd",
                    "KH2.IDX",
                    "ICON/ICON0.PNG",
                    "ICON/ICON0_EN.png",
                });
            private static readonly Dictionary<string, string> Names = KH2Names
                .Concat(Idx1Name.Names)
                .Concat(EgsHdAsset.BbsNames)
                .Concat(EgsHdAsset.RecomNames)
                .Concat(EgsHdAsset.MareNames)
                .Concat(EgsHdAsset.SettingMenuNames)
                .Concat(EgsHdAsset.TheaterNames)
                .Concat(EgsHdAsset.Kh1AdditionalNames)
                .Distinct()
                .ToDictionary(x => ToString(MD5.HashData(Encoding.UTF8.GetBytes(x))), x => x);

            protected int OnExecute(CommandLineApplication app)
            {
                app.ShowHelp();
                return 1;
            }

            static string ToString(byte[] data)
            {
                var sb = new StringBuilder(data.Length * 2);
                for (var i = 0; i < data.Length; i++)
                    sb.Append(data[i].ToString("X02"));

                return sb.ToString();
            }

            public static byte[] ToBytes(string hex)
            {
                return Enumerable.Range(0, hex.Length)
                                 .Where(x => x % 2 == 0)
                                 .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
                                 .ToArray();
            }

            public static string CreateMD5(string input)
            {
                // Use input string to calculate MD5 hash
                using (System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create())
                {
                    byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(input);
                    byte[] hashBytes = md5.ComputeHash(inputBytes);

                    // Convert the byte array to hexadecimal string
                    StringBuilder sb = new StringBuilder();
                    for (int i = 0; i < hashBytes.Length; i++)
                    {
                        sb.Append(hashBytes[i].ToString("X2"));
                    }
                    return sb.ToString();
                }
            }

            private class ExtractCommand
            {
                [Required]
                [Argument(0, Description = "Kingdom Hearts HED input filter; you can specify 'kh2_first.hed', 'kh2_*' or even '*' to extract everything.")]
                public string InputHed { get; set; }

                [Option(CommandOptionType.SingleValue, Description = "Path where the content will be extracted", ShortName = "o", LongName = "output")]
                public string OutputDir { get; set; }

                [Option(CommandOptionType.NoValue, Description = "Do not extract files that are already found in the destination directory", ShortName = "n")]
                public bool DoNotExtractAgain { get; set; }

                protected int OnExecute(CommandLineApplication app)
                {
                    var directory = Path.GetDirectoryName(InputHed);
                    var filter = Path.GetFileName(InputHed);

                    Directory
                        .GetFiles(string.IsNullOrEmpty(directory) ? "." : directory, filter)
                        .Where(x => x.EndsWith(".hed"))
                        .AsParallel()
                        .ForAll(inputHed => Extract(inputHed, OutputDir));

                    return 0;
                }

                protected void Extract(string inputHed, string output)
                {
                    var outputDir = output ?? Path.GetFileNameWithoutExtension(inputHed);
                    using var hedStream = File.OpenRead(inputHed);
                    using var img = File.OpenRead(Path.ChangeExtension(inputHed, "pkg"));

                    foreach (var entry in Hed.Read(hedStream))
                    {
                        var hash = EpicGamesAssets.ToString(entry.MD5);
                        if (!Names.TryGetValue(hash, out var fileName))
                            fileName = hash;

                        var outputFileName = Path.Combine(outputDir, fileName);
                        if (DoNotExtractAgain && File.Exists(outputFileName))
                            continue;

                        Console.WriteLine(fileName);
                        var extractDir = Path.GetDirectoryName(outputFileName);
                        if (!Directory.Exists(extractDir))
                            Directory.CreateDirectory(extractDir);

                        var hdAsset = new EgsHdAsset(img.SetPosition(entry.Offset));
                        File.Create(outputFileName).Using(stream => stream.Write(hdAsset.ReadData()));
                    }
                }
            }

            private class PackCommand
            {
                [Required]
                [Argument(0, Description = "Folder to pack using EGS format.")]
                public string InputFolder { get; set; }

                [Option(CommandOptionType.SingleValue, Description = "Folder inside where the packed content will be dropped", ShortName = "o", LongName = "output")]
                public string OutputDir { get; set; }

                protected int OnExecute(CommandLineApplication app)
                {
                    Pack(InputFolder, OutputDir);

                    return 0;
                }

                protected void Pack(string inputFolder, string output)
                {
                    var outputDir = output ?? Directory.GetParent(inputFolder).FullName;
                    var files = Directory.EnumerateFiles(inputFolder, "*.*", SearchOption.AllDirectories).OrderBy(x => x, StringComparer.Ordinal);

                    var outputFilename = Path.GetFileName(inputFolder);
                    var outputHedFile = Path.Join(outputDir, $"{outputFilename}.hed");
                    var outputPkgFile = Path.Join(outputDir, $"{outputFilename}.pkg");

                    var hedStream = File.Create(outputHedFile);
                    var pkgStream = File.Create(outputPkgFile);

                    var offset = 0L;

                    foreach (var file in files)
                    {
                        var filename = file.Replace($"{inputFolder}\\", "").Replace("\\", "/");
                        var hash = CreateMD5(filename);
                        var fileStream = File.OpenRead(file);
                        var fileSize = (int)fileStream.Length;

                        // Write HED entry for the current file
                        var hedEntry = new Hed.Entry()
                        {
                            MD5 = ToBytes(hash),
                            Offset = offset,
                            DataLength = fileSize + 0x10,
                            ActualLength = fileSize,
                        };

                        BinaryMapping.WriteObject<Hed.Entry>(hedStream, hedEntry);

                        // Encrypt and write current file data in the PKG stream
                        var header = new EgsHdAsset.Header()
                        {
                            CompressedLength = -1,
                            DecompressedLength = fileSize,
                            RemasteredAssetCount = 0,
                            Unknown0c = 0
                        };

                        // Is it an HD assets?
                        if (filename.Contains('-'))
                        {
                            var split = filename.Split('-');

                        }

                        // The seed used for encryption is the data header
                        var seed = new MemoryStream();
                        BinaryMapping.WriteObject<EgsHdAsset.Header>(seed, header);

                        var encryptedFileData = EgsEncryption.Encrypt(
                            fileStream.ReadAllBytes(),
                            seed.SetPosition(0).ReadAllBytes()
                        );

                        BinaryMapping.WriteObject<EgsHdAsset.Header>(pkgStream, header);
                        pkgStream.Write(encryptedFileData);


                        Console.WriteLine($"Packed: {filename}");

                        offset += hedEntry.DataLength;

                        fileStream.Close();
                    }

                    Console.WriteLine($"Output HED file location: {outputHedFile}");
                    Console.WriteLine($"Output PKG file location: {outputPkgFile}");
                }
            }

            //private class ListCommand
            //{
            //    [Required]
            //    [FileExists]
            //    [Argument(0, Description = "Kingdom Hearts II IDX file, paired with a IMG")]
            //    public string InputIdx { get; set; }

            //    [Option(CommandOptionType.NoValue, Description = "Sort file list by their position in the IMG", ShortName = "s", LongName = "sort")]
            //    public bool Sort { get; set; }

            //    protected int OnExecute(CommandLineApplication app)
            //    {
            //        var entries = OpenIdx(InputIdx);
            //        if (Sort)
            //            entries = entries.OrderBy(x => x.Offset);

            //        foreach (var entry in entries)
            //            Console.WriteLine(IdxName.Lookup(entry) ?? $"@{entry.Hash32:X08}-{entry.Hash16:X04}");

            //        return 0;
            //    }
            //}
        }
    }
}
