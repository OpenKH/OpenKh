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

                        if (DoNotExtractAgain && File.Exists(outputFileName))
                            continue;

                        Console.WriteLine(outputFileName);
                        CreateDirectoryForFile(outputFileName);

                        File.Create(outputFileName).Using(stream => stream.Write(img.SetPosition(entry.Offset).ReadBytes(entry.DataLength)));

                        var hdAsset = new EgsHdAsset(img.SetPosition(entry.Offset), entry);
                        File.Create(outputFileName).Using(stream => stream.Write(hdAsset.ReadData()));

                        outputFileName = Path.Combine(outputDir, REMASTERED_FILES_FOLDER_NAME, fileName);

                        foreach (var asset in hdAsset.Assets)
                        {
                            var outputFileNameRemastered = Path.Combine(GetHDAssetFolder(outputFileName), asset);

                            Console.WriteLine(outputFileNameRemastered);
                            CreateDirectoryForFile(outputFileNameRemastered);

                            var assetData = hdAsset.ReadRemasteredAsset(asset);
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

                protected void Patch(string pkgFile, string inputFolder, string outputFolder)
                {
                    var outputDir = outputFolder ?? Path.GetFileNameWithoutExtension(pkgFile);

                    // Get files to inject in the PKG
                    var inputFiles = GetAllFiles(inputFolder).ToList();

                    var hedFile = Path.ChangeExtension(pkgFile, "hed");
                    using var hedStream = File.OpenRead(hedFile);
                    using var pkgStream = File.OpenRead(pkgFile);

                    var hedEntries = Hed.Read(hedStream).ToList();

                    if (!Directory.Exists(outputDir))
                        Directory.CreateDirectory(outputDir);

                    using var patchedHedStream = File.Create(Path.Combine(outputDir, Path.GetFileName(hedFile)));
                    using var patchedPkgStream = File.Create(Path.Combine(outputDir, Path.GetFileName(pkgFile)));

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
                        if (inputFiles.Contains(filename))
                        {
                            inputFiles.Remove(filename);

                            var asset = new EgsHdAsset(pkgStream.SetPosition(entry.Offset), entry);
                            var fileToInject = Path.Combine(inputFolder, filename);
                            var shouldCompressData = asset.OriginalAssetHeader.CompressedLength > 0;
                            var newHedEntry = ReplaceFile(fileToInject, patchedHedStream, patchedPkgStream, asset, shouldCompressData, entry);

                            //Console.WriteLine("HED");
                            //Console.WriteLine($"ActualLength: {entry.ActualLength} | {newHedEntry.ActualLength}");
                            //Console.WriteLine($"DataLength: {entry.DataLength} | {newHedEntry.DataLength}");
                            //Console.WriteLine($"Offset: {entry.Offset} | {newHedEntry.Offset}");
                            //Console.WriteLine($"MD5: {EpicGamesAssets.ToString(entry.MD5)} | {EpicGamesAssets.ToString(newHedEntry.MD5)}");
                        }
                        // Write the original data
                        else
                        {
                            pkgStream.SetPosition(entry.Offset);

                            var data = new byte[entry.DataLength];
                            var dataLenght = pkgStream.Read(data, 0, entry.DataLength);

                            if (dataLenght != entry.DataLength)
                            {
                                throw new Exception($"Error, can't read  {entry.DataLength} bytes for file {filename}. (only read {dataLenght})");
                            }

                            // Write the HED entry with new offset
                            entry.Offset = patchedPkgStream.Position;
                            BinaryMapping.WriteObject<Hed.Entry>(patchedHedStream, entry);

                            // Write the PKG file with the original asset file data
                            patchedPkgStream.Write(data);
                        }
                    }

                    // Add all files that are not in the original HED file and inject them in the PKG stream too
                    foreach (var filename in inputFiles)
                    {
                        var newFilePath = Path.Combine(inputFolder, filename);
                        AddFile(newFilePath, true, patchedHedStream, patchedPkgStream);
                        Console.WriteLine($"Added a new file: {filename}");
                    }
                }

                private Hed.Entry AddFile(string input, bool shouldCompressData, FileStream hedStream, FileStream pkgStream)
                {
                    using var newFileStream = File.OpenRead(input);
                    var filename = GetRelativePath(input, Path.Combine(InputFolder, ORIGINAL_FILES_FOLDER_NAME));
                    var compressedData = shouldCompressData ? CompressData(newFileStream.ReadAllBytes()) : newFileStream.ReadAllBytes();
                    var comrpessedDataLenght = compressedData.Length == newFileStream.Length ? -1 : compressedData.Length;
                    var offset = pkgStream.Position;

                    // Encrypt and write current file data in the PKG stream
                    var header = CreateAssetHeader(newFileStream, comrpessedDataLenght);

                    // The seed used for encryption is the data header
                    var seed = new MemoryStream();
                    BinaryMapping.WriteObject<EgsHdAsset.Header>(seed, header);
                    var encryptionKey = seed.ReadAllBytes();
                    var encryptedFileData = EgsEncryption.Encrypt(compressedData, encryptionKey);

                    BinaryMapping.WriteObject<EgsHdAsset.Header>(pkgStream, header);
                    pkgStream.Write(encryptedFileData);

                    // Write a new entry in the HED stream
                    var hedEntry = CreateHedEntry(filename, newFileStream, compressedData, offset);
                    BinaryMapping.WriteObject<Hed.Entry>(hedStream, hedEntry);

                    return hedEntry;
                }

                private Hed.Entry ReplaceFile(string fileToInject, FileStream hedStream, FileStream pkgStream, EgsHdAsset asset, bool shouldCompressData = true, Hed.Entry originalHedHeader = null)
                {
                    using var newFileStream = File.OpenRead(fileToInject);
                    var filename = GetRelativePath(fileToInject, Path.Combine(InputFolder, ORIGINAL_FILES_FOLDER_NAME));
                    var offset = pkgStream.Position;
                    var compressedData = shouldCompressData ? CompressData(newFileStream.ReadAllBytes()) : newFileStream.ReadAllBytes();
                    var comrpessedDataLenght = compressedData.Length == newFileStream.Length ? asset.OriginalAssetHeader.CompressedLength : compressedData.Length;

                    // Encrypt and write current file data in the PKG stream
                    var header = CreateAssetHeader(
                        newFileStream,
                        comrpessedDataLenght,
                        asset.OriginalAssetHeader.RemasteredAssetCount,
                        asset.OriginalAssetHeader.Unknown0c
                    );

                    // The seed used for encryption is the data header6
                    var seed = new MemoryStream();
                    BinaryMapping.WriteObject<EgsHdAsset.Header>(seed, header);

                    var encryptionKey = seed.ReadAllBytes();
                    var encryptedFileData = header.CompressedLength >= 1 ? EgsEncryption.Encrypt(compressedData, encryptionKey) : compressedData;

                    BinaryMapping.WriteObject<EgsHdAsset.Header>(pkgStream, header);

                    Console.WriteLine($"Replaced original file: {filename}");

                    var remasteredHeaders = new List<EgsHdAsset.RemasteredEntry>();

                    // Is there remastered assets?
                    if (header.RemasteredAssetCount > 0)
                    {
                        remasteredHeaders = ReplaceRemasteredAssets(fileToInject, asset, pkgStream, encryptionKey, encryptedFileData);
                    }
                    else
                    {
                        // Make sure to write the original file after remastered assets headers
                        pkgStream.Write(encryptedFileData);
                    }

                    // Write a new entry in the HED stream
                    var hedEntry = CreateHedEntry(filename, newFileStream, compressedData, offset, remasteredHeaders);

                    BinaryMapping.WriteObject<Hed.Entry>(hedStream, hedEntry);

                    //Console.WriteLine("Data header");
                    //Console.WriteLine($"CompressedLength: {asset.OriginalAssetHeader.CompressedLength} | {header.CompressedLength}");
                    //Console.WriteLine($"DecompressedLength: {asset.OriginalAssetHeader.DecompressedLength} | {header.DecompressedLength}");
                    //Console.WriteLine($"RemasteredAssetCount: {asset.OriginalAssetHeader.RemasteredAssetCount} | {header.RemasteredAssetCount}");
                    //Console.WriteLine($"Unknown0c: {asset.OriginalAssetHeader.Unknown0c} | {header.Unknown0c}");

                    return hedEntry;
                }

                private List<EgsHdAsset.RemasteredEntry> ReplaceRemasteredAssets(string originalFile, EgsHdAsset asset, FileStream pkgStream, byte[] seed, byte[] originalAssetData)
                {
                    var newRemasteredHeaders = new List<EgsHdAsset.RemasteredEntry>();
                    var relativePath = GetRelativePath(originalFile, Path.Combine(InputFolder, ORIGINAL_FILES_FOLDER_NAME));
                    var remasteredAssetsFolder = Path.Combine(InputFolder, REMASTERED_FILES_FOLDER_NAME, relativePath);

                    var allRemasteredAssetsData = new MemoryStream();
                    // 0x30 is the size of this header
                    var totalRemasteredAssetHeadersSize = asset.RemasteredAssetHeaders.Count() * 0x30;

                    foreach (var remasteredAssetHeader in asset.RemasteredAssetHeaders.Values)
                    {
                        var remasteredAssetFile = remasteredAssetHeader.Name;
                        var assetFilePath = Path.Combine(remasteredAssetsFolder, remasteredAssetFile);
                        byte[] assetData;

                        if (!File.Exists(assetFilePath))
                        {
                            assetData = asset.ReadRemasteredAsset(remasteredAssetFile);
                        }
                        else
                        {
                            using var assetFileStream = File.OpenRead(assetFilePath);
                            assetData = assetFileStream.ReadAllBytes();
                        }

                        var compressedData = asset.RemasteredAssetHeaders[remasteredAssetFile].CompressedLength < 0 ? assetData : CompressData(assetData);
                        var currentOffset = (int)(pkgStream.Position + totalRemasteredAssetHeadersSize + allRemasteredAssetsData.Position + compressedData.Length);

                        var remasteredEntry = new EgsHdAsset.RemasteredEntry()
                        {
                            CompressedLength = compressedData.Length == assetData.Length ? -1 : compressedData.Length,
                            DecompressedLength = (int)assetData.Length,
                            Name = remasteredAssetFile,
                            Offset = currentOffset,
                            Unknown24 = asset.RemasteredAssetHeaders[remasteredAssetFile].Unknown24
                        };

                        newRemasteredHeaders.Add(remasteredEntry);

                        // Write asset header in the PKG stream
                        BinaryMapping.WriteObject<EgsHdAsset.RemasteredEntry>(pkgStream, remasteredEntry);

                        Console.WriteLine($"Replaced remastered file: {relativePath}/{remasteredAssetFile}");

                        var encryptedData = EgsEncryption.Encrypt(compressedData, seed);

                        // Don't write into the PKG stream yet as we need to write
                        // all HD assets header juste after original file's data
                        allRemasteredAssetsData.Write(encryptedData);
                    }

                    pkgStream.Write(originalAssetData);
                    pkgStream.Write(allRemasteredAssetsData.ReadAllBytes());

                    return newRemasteredHeaders;
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
                    var deflateStream = new ZlibStream(compressedStream, Ionic.Zlib.CompressionMode.Compress, Ionic.Zlib.CompressionLevel.Default, true);

                    deflateStream.Write(data, 0, data.Length);
                    deflateStream.Close();

                    var compressedData = compressedStream.ReadAllBytes();

                    // Make sure compressed data is aligned with 0x10
                    int padding = compressedData.Length % 0x10 == 0 ? 0 : (0x10 - compressedData.Length % 0x10);
                    Array.Resize(ref compressedData, compressedData.Length + padding);

                    return compressedData;
                }
            }

            public static Hed.Entry CreateHedEntry(string filename, FileStream fileStream, byte[] data, long offset, List<EgsHdAsset.RemasteredEntry> remasteredHeaders = null)
            {
                var fileHash = CreateMD5(filename);
                // 0x10 => size of the original asset header
                // 0x30 => size of the remastered asset header
                var dataLength = data.Length + 0x10;

                if (remasteredHeaders != null)
                {
                    foreach (var header in remasteredHeaders)
                    {
                        dataLength += header.CompressedLength + 0x30;
                    }
                }

                return new Hed.Entry()
                {
                    MD5 = ToBytes(fileHash),
                    ActualLength = (int)fileStream.Length,
                    DataLength = dataLength,
                    Offset = offset
                };
            }

            public static EgsHdAsset.Header CreateAssetHeader(FileStream fileStream, int compressedDataLenght, int remasteredAssetCount = 0, int unknown0c = 0x0)
            {
                return new EgsHdAsset.Header()
                {
                    CompressedLength = compressedDataLenght,
                    DecompressedLength = (int)fileStream.Length,
                    RemasteredAssetCount = remasteredAssetCount,
                    Unknown0c = unknown0c
                };
            }

            private static string GetHDAssetFolder(string assetFile)
            {
                var parentFolder = Directory.GetParent(assetFile).FullName;
                var assetFolderName = Path.Combine(parentFolder, $"{Path.GetFileName(assetFile)}");

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
