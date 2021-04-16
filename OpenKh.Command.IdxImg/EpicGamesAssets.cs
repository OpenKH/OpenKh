using Ionic.Zlib;
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
         Subcommand(typeof(PackCommand)),
         Subcommand(typeof(PatchCommand)),
         Subcommand(typeof(ListCommand))]
        private class EpicGamesAssets
        {
            private const string ORIGINAL_FILES_FOLDER_NAME = "original";
            private const string REMASTERED_FILES_FOLDER_NAME = "remastered";

            #region MD5 names

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
                .Concat(IdxName.Names.Where(x => x.StartsWith("gumibattle/se/")).Select(x => x.Replace(".seb", ".win32.scd")))
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
                .Concat(EgsHdAsset.DddNames)
                .Concat(EgsHdAsset.BbsNames)
                .Concat(EgsHdAsset.RecomNames)
                .Concat(EgsHdAsset.MareNames)
                .Concat(EgsHdAsset.SettingMenuNames)
                .Concat(EgsHdAsset.TheaterNames)
                .Concat(EgsHdAsset.Kh1AdditionalNames)
                .Concat(EgsHdAsset.Launcher28Names)
                .Distinct()
                .ToDictionary(x => ToString(MD5.HashData(Encoding.UTF8.GetBytes(x))), x => x);

            #endregion

            protected int OnExecute(CommandLineApplication app)
            {
                app.ShowHelp();
                return 1;
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
                            fileName = $"{hash}.dat";

                        var outputFileName = Path.Combine(outputDir, ORIGINAL_FILES_FOLDER_NAME, fileName); 
                        ;
                        if (DoNotExtractAgain && File.Exists(outputFileName))
                            continue;

                        Console.WriteLine(outputFileName);
                        CreateDirectoryForFile(outputFileName);

                        File.Create(outputFileName).Using(stream => stream.Write(img.SetPosition(entry.Offset).ReadBytes(entry.DataLength)));

                        var hdAsset = new EgsHdAsset(img.SetPosition(entry.Offset));
                        File.Create(outputFileName).Using(stream => stream.Write(hdAsset.ReadData()));

                        outputFileName = Path.Combine(outputDir, REMASTERED_FILES_FOLDER_NAME, fileName);
                        foreach (var asset in hdAsset.Assets)
                        {
                            var outputFileNameRemastered = Path.Combine(GetHDAssetFolder(outputFileName), asset);
                            
                            Console.WriteLine(outputFileNameRemastered);
                            CreateDirectoryForFile(outputFileNameRemastered);

                            var assetData = hdAsset.ReadAsset(asset);
                            File.Create(outputFileNameRemastered).Using(stream => stream.Write(assetData));
                        }
                    }
                }

                private static void CreateDirectoryForFile(string fileName)
                {
                    var directoryName = Path.GetDirectoryName(fileName);
                    if (!Directory.Exists(directoryName))
                        Directory.CreateDirectory(directoryName);
                }
            }

            [Command("pack", Description = "Pack a folder in a PKG file (with its HED companion).")]
            private class PackCommand
            {
                [Required]
                [Argument(0, Description = "Folder to pack using EGS format.")]
                public string InputFolder { get; set; }

                [Option(CommandOptionType.SingleValue, Description = "Folder inside where the packed content will be dropped.", ShortName = "o", LongName = "output")]
                public string OutputDir { get; set; }

                [Option(CommandOptionType.SingleValue, Description = "File path of the original hed we want to repack.", ShortName = "hed", LongName = "originalHedPath")]
                public string OriginalHed { get; set; }

                private string _originalFilesFolder;

                protected int OnExecute(CommandLineApplication app)
                {
                    _originalFilesFolder = Path.Combine(InputFolder, ORIGINAL_FILES_FOLDER_NAME);

                    if (!Directory.Exists(_originalFilesFolder))
                    {
                        Console.WriteLine($"Unable to find folder {_originalFilesFolder}, please make sure files to packs are there.");
                        return -1;
                    }

                    Pack(_originalFilesFolder, OutputDir, OriginalHed);

                    return 0;
                }

                protected void Pack(string inputFolder, string output, string originalHed = null)
                {
                    var outputDir = output ?? Directory.GetParent(inputFolder).FullName;
                    var files = Directory.EnumerateFiles(inputFolder, "*.*", SearchOption.AllDirectories).OrderBy(x => x, StringComparer.Ordinal);

                    var outputFilename = Path.GetFileName(InputFolder);
                    var outputHedFile = Path.Join(outputDir, $"{outputFilename}.hed");
                    var outputPkgFile = Path.Join(outputDir, $"{outputFilename}.pkg");

                    var hedStream = File.Create(outputHedFile);
                    var pkgStream = File.Create(outputPkgFile);

                    // TODO: Remove this when we will find out what are the last unknown fields in Hed.Entry, Header and RemasteredEntry structures
                    if (!string.IsNullOrEmpty(originalHed))
                    {
                        using var originalHedStream = File.OpenRead(originalHed);
                        using var originalPkgStream = File.OpenRead(Path.ChangeExtension(originalHed, "pkg"));

                        var hedEntries = Hed.Read(originalHedStream).ToList();

                        foreach (var file in files)
                        {
                            var relativeFilePath = GetRelativePath(file, _originalFilesFolder);
                            var md5Hash = ToBytes(CreateMD5(relativeFilePath));
                            var originalHedEntry = hedEntries.FirstOrDefault(entry => entry.MD5.SequenceEqual(md5Hash));
                            EgsHdAsset originalAsset = null;

                            if (originalHedEntry != null)
                            {
                                // Get original PKG entry
                                originalAsset = new EgsHdAsset(originalPkgStream.SetPosition(originalHedEntry.Offset));
                            }

                            AddFile(file, hedStream, pkgStream);
                            Console.WriteLine($"Packed: {file}");
                        }
                    }
                    else
                    {
                        foreach (var file in files)
                        {
                            AddFile(file, hedStream, pkgStream);
                            Console.WriteLine($"Packed: {file}");
                        }
                    }

                    Console.WriteLine($"Output HED file location: {outputHedFile}");
                    Console.WriteLine($"Output PKG file location: {outputPkgFile}");
                }

                private Hed.Entry AddFile(string input, FileStream hedStream, FileStream pkgStream)
                {
                    var newFileStream = File.OpenRead(input);
                    var filename = GetRelativePath(input, _originalFilesFolder);
                    var compressedData = CompressData(newFileStream.ReadAllBytes());

                    // Write a new entry in the HED stream
                    var hedEntry = CreateHedEntry(filename, newFileStream, compressedData, pkgStream.Length);

                    BinaryMapping.WriteObject<Hed.Entry>(hedStream, hedEntry);

                    // Encrypt and write current file data in the PKG stream
                    var header = CreateAssetHeader(newFileStream, compressedData, 0);

                    // The seed used for encryption is the data header
                    var seed = new MemoryStream();
                    BinaryMapping.WriteObject<EgsHdAsset.Header>(seed, header);

                    var encryptedFileData = EgsEncryption.Encrypt(
                        compressedData,
                        seed.ReadAllBytes()
                    );

                    BinaryMapping.WriteObject<EgsHdAsset.Header>(pkgStream, header);
                    pkgStream.Write(encryptedFileData);

                    newFileStream.Close();

                    return hedEntry;
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
                    var originalFilesFolder = Path.Combine(InputFolder, ORIGINAL_FILES_FOLDER_NAME);

                    if (!Directory.Exists(originalFilesFolder))
                    {
                        Console.WriteLine($"Unable to find folder {originalFilesFolder}, please make sure files to packs are there.");
                        return -1;
                    }

                    Patch(PkgFile, originalFilesFolder, OutputDir);

                    return 0;
                }

                private Dictionary<long, int> FindRemasteredAssetsLocation(IEnumerable<Hed.Entry> hedEntries, FileStream pkgStream, IEnumerable<string> filesToCheck)
                {
                    var remasteredAssetsLocation = new Dictionary<long, int>(); // data offset => data length

                    foreach (var entry in hedEntries)
                    {
                        var hash = EpicGamesAssets.ToString(entry.MD5);

                        // We don't know this filename, we ignore it
                        if (!Names.TryGetValue(hash, out var filename))
                        {
                            continue;
                        }

                        if (filesToCheck.Contains(filename))
                        {
                            var seed = pkgStream.ReadBytes(0x10);
                            var originalHeader = BinaryMapping.ReadObject<EgsHdAsset.Header>(new MemoryStream(seed));

                            var remasteredAssets = Enumerable.Range(0, originalHeader.RemasteredAssetCount)
                                                             .Select(_ => BinaryMapping.ReadObject<Egs.EgsHdAsset.RemasteredEntry>(pkgStream))
                                                             .ToList();
                        }
                    }

                    return remasteredAssetsLocation;
                }

                protected void Patch(string pkgFile, string inputFolder, string outputFolder)
                {
                    var outputDir = outputFolder ?? Path.GetFileNameWithoutExtension(pkgFile);

                    // Get files to inject in the PKG
                    var filesToReplace = GetAllFiles(inputFolder);

                    var hedFile = Path.ChangeExtension(pkgFile, "hed");
                    var hedStream = File.OpenRead(hedFile);
                    var pkgStream = File.OpenRead(pkgFile);

                    var hedEntries = Hed.Read(hedStream).ToList();

                    if (!Directory.Exists(outputDir))
                        Directory.CreateDirectory(outputDir);

                    using var patchedHedStream = File.Create(Path.Combine(outputDir, Path.GetFileName(hedFile)));
                    using var patchedPkgStream = File.Create(Path.Combine(outputDir, Path.GetFileName(pkgFile)));

                    var pkgOffset = 0L;

                    // Remastered assets is not contiguous with the original asset in memory
                    // so we need to browse the PKG one time before to find their location
                    var remasteredAssetsLocation = FindRemasteredAssetsLocation(hedEntries, pkgStream, filesToReplace);

                    pkgStream.SetPosition(0);

                    foreach (var entry in hedEntries)
                    {
                        var hash = EpicGamesAssets.ToString(entry.MD5);

                        // We don't know this filename, we ignore it
                        if (!Names.TryGetValue(hash, out var filename))
                        {
                            continue;
                        }

                        // Replace the found files
                        if (filesToReplace.Contains(filename))
                        {
                            var seed = pkgStream.ReadBytes(0x10);
                            var originalHeader = BinaryMapping.ReadObject<EgsHdAsset.Header>(new MemoryStream(seed));

                            //var remasteredAssets = Enumerable.Range(0, originalHeader.RemasteredAssetCount)
                            //                        .Select(_ => BinaryMapping.ReadObject<Egs.EgsHdAsset.RemasteredEntry>(pkgStream))
                            //                        .ToList();

                            // Skipped the older asset data as it will be replaced
                            var skippedData = new byte[entry.DataLength - 0x10];
                            pkgStream.Read(skippedData, 0, entry.DataLength - 0x10);

                            var fileToInject = Path.Combine(inputFolder, filename);
                            var newHedEntry = ReplaceFile(fileToInject, patchedHedStream, patchedPkgStream, originalHeader);

                            pkgOffset += newHedEntry.DataLength;

                            Console.WriteLine($"Replaced file: {filename}");
                        }
                        // Write the original data
                        else
                        {
                            entry.Offset = pkgOffset;

                            BinaryMapping.WriteObject<Hed.Entry>(patchedHedStream, entry);

                            var data = new byte[entry.DataLength];
                            var dataLenght = pkgStream.Read(data, 0, entry.DataLength);

                            patchedPkgStream.Write(data);

                            if (dataLenght != entry.DataLength)
                            {
                                throw new Exception($"Error, can't read  {entry.DataLength} bytes for file {filename}. (only read {dataLenght})");
                            }

                            pkgOffset += dataLenght;
                        }
                    }
                }

                private Hed.Entry ReplaceFile(string fileToInject, FileStream hedStream, FileStream pkgStream, EgsHdAsset.Header originalHeader)
                {
                    var newFileStream = File.OpenRead(fileToInject);
                    var filename = GetRelativePath(fileToInject, InputFolder);

                    var compressedData = CompressData(newFileStream.ReadAllBytes());

                    // Write a new entry in the HED stream
                    var hedEntry = CreateHedEntry(filename, newFileStream, compressedData, pkgStream.Length);

                    BinaryMapping.WriteObject<Hed.Entry>(hedStream, hedEntry);

                    // Encrypt and write current file data in the PKG stream
                    var header = CreateAssetHeader(
                        newFileStream, 
                        compressedData, 
                        originalHeader.RemasteredAssetCount,
                        originalHeader.Unknown0c
                    );

                    // The seed used for encryption is the data header
                    var seed = new MemoryStream();
                    BinaryMapping.WriteObject<EgsHdAsset.Header>(seed, header);

                    var encryptedFileData = EgsEncryption.Encrypt(compressedData, seed.ReadAllBytes());

                    BinaryMapping.WriteObject<EgsHdAsset.Header>(pkgStream, header);

                    // Is there remastered assets?
                    if (header.RemasteredAssetCount > 0)
                    {
                        ReplaceRemasteredAssets(fileToInject, compressedData.Length, pkgStream);
                    }

                    // Make sure to write the original file after remastered assets headers
                    pkgStream.Write(encryptedFileData);

                    newFileStream.Close();

                    return hedEntry;
                }

                private void ReplaceRemasteredAssets(string originalFile, int originalFileSize, FileStream pkgStream)
                {
                    var remasteredAssetsFolder = GetHDAssetFolder(originalFile);

                    if (Directory.Exists(remasteredAssetsFolder))
                    {
                        var remasteredAssetFiles = GetAllFiles(remasteredAssetsFolder);
                        var allRemasteredAssetsData = new MemoryStream();
                        var totalRemasteredAssetHeadersSize = (remasteredAssetFiles.Count() * 0x30);

                        foreach (var remasteredAssetFile in remasteredAssetFiles)
                        {
                            var assetFileStream = File.OpenRead(remasteredAssetFile);
                            var compressedData = CompressData(assetFileStream.ReadAllBytes());
                            var offsetPosition = 0;

                            var remasteredEntry = new EgsHdAsset.RemasteredEntry()
                            {
                                CompressedLength = compressedData.Length,
                                DecompressedLength = (int)assetFileStream.Length,
                                Name = remasteredAssetFile,
                                Offset = (int)pkgStream.Position + totalRemasteredAssetHeadersSize + offsetPosition + originalFileSize, // 0x30 is the size of this header
                                Unknown24 = 0 // TODO: Use the original data
                            };

                            // Write asset header in the PKG stream
                            BinaryMapping.WriteObject<EgsHdAsset.RemasteredEntry>(pkgStream, remasteredEntry);

                            var assetSeed = new MemoryStream();
                            BinaryMapping.WriteObject<EgsHdAsset.RemasteredEntry>(assetSeed, remasteredEntry);

                            var encryptedData = EgsEncryption.Encrypt(assetFileStream.ReadAllBytes(), assetSeed.ReadAllBytes());

                            // Don't write into the PKG stream yet as we need to write
                            // all HD assets header juste after original file's data
                            allRemasteredAssetsData.Write(encryptedData);

                            assetFileStream.Close();

                            offsetPosition += encryptedData.Length;
                        }

                        pkgStream.Write(allRemasteredAssetsData.ReadAllBytes());
                    }
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
                    using var hedStream = File.OpenRead(InputHed);
                    var entries = Hed.Read(hedStream);

                    foreach (var entry in entries)
                    {
                        var hash = EpicGamesAssets.ToString(entry.MD5);
                        if (!Names.TryGetValue(hash, out var fileName))
                            fileName = $"{hash}.dat";

                        Console.WriteLine(fileName);
                    }

                    return 0;
                }
            }

            #region Utils

            private static IEnumerable<string> GetAllFiles(string folder)
            {
                return Directory.EnumerateFiles(folder, "*.*", SearchOption.AllDirectories)
                                .Select(x => x.Replace($"{folder}\\", "")
                                .Replace(@"\", "/"));
            }

            private static string ToString(byte[] data)
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

            public static byte[] CompressData(byte[] data)
            {
                using (MemoryStream compressedStream = new MemoryStream())
                {
                    var deflateStream = new ZlibStream(compressedStream, Ionic.Zlib.CompressionMode.Compress, true);

                    // Make sure compressed data is aligned with 0x10
                    int padding = data.Length % 0x10 == 0 ? 0 : (0x10 - data.Length % 0x10);
                    Array.Resize(ref data, data.Length + padding);

                    deflateStream.Write(data, 0, data.Length);
                    deflateStream.Close();

                    return compressedStream.ReadAllBytes();
                }
            }

            public static Hed.Entry CreateHedEntry(string filename, FileStream fileStream, byte[] compressedData, long offset)
            {
                var fileHash = CreateMD5(filename);

                return new Hed.Entry()
                {
                    MD5 = ToBytes(fileHash),
                    ActualLength = (int)fileStream.Length,
                    DataLength = compressedData.Length + 0x10, // Size of the file header
                    Offset = offset
                };
            }

            public static  EgsHdAsset.Header CreateAssetHeader(FileStream fileStream, byte[] compressedData, int remasteredAssetCount = 0, int unknown0c = 0x0)
            {
                return new EgsHdAsset.Header()
                {
                    CompressedLength = compressedData.Length,
                    DecompressedLength = (int)fileStream.Length,
                    RemasteredAssetCount = remasteredAssetCount,
                    Unknown0c = unknown0c
                };
            }

            private static string GetHDAssetFolder(string assetFile)
            {
                var parentFolder = Directory.GetParent(assetFile).FullName;
                var assetFolderName = Path.Combine(parentFolder, $"HD-{Path.GetFileName(assetFile)}");

                return assetFolderName;
            }

            private static string GetRelativePath(string filePath, string origin)
            {
                return filePath.Replace($"{origin}\\", "").Replace(@"\", "/");
            }

            #endregion
        }
    }
}
