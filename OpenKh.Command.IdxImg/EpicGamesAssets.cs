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

                        var hdAsset = new EgsHdAsset(img.SetPosition(entry.Offset));

                        File.Create(outputFileName).Using(stream => stream.Write(hdAsset.OriginalData));

                        outputFileName = Path.Combine(outputDir, REMASTERED_FILES_FOLDER_NAME, fileName);

                        foreach (var asset in hdAsset.Assets)
                        {
                            var outputFileNameRemastered = Path.Combine(GetHDAssetFolder(outputFileName), asset);

                            Console.WriteLine(outputFileNameRemastered);
                            CreateDirectoryForFile(outputFileNameRemastered);

                            var assetData = hdAsset.RemasteredAssetsDecompressedData[asset];
                            File.Create(outputFileNameRemastered).Using(stream => stream.Write(assetData));
                        }
                    }
                }

                private static string GetHDAssetFolder(string assetFile)
                {
                    var parentFolder = Directory.GetParent(assetFile).FullName;
                    var assetFolderName = Path.Combine(parentFolder, $"{Path.GetFileName(assetFile)}");

                    return assetFolderName;
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

                private List<string> _patchFiles = new List<string>();

                protected int OnExecute(CommandLineApplication app)
                {
                    // Get files to inject in the PKG to detect if we want to include new files or not
                    // We only get the original files as for me it doesn't make sense to include
                    // new "remastered" asset since it must be linked to an original one
                    _patchFiles = GetAllFiles(Path.Combine(InputFolder, ORIGINAL_FILES_FOLDER_NAME)).ToList();

                    Patch(PkgFile, InputFolder, OutputDir);

                    return 0;
                }

                protected void Patch(string pkgFile, string inputFolder, string outputFolder)
                {
                    var filenames = new List<string>();

                    var remasteredFilesFolder = Path.Combine(inputFolder, REMASTERED_FILES_FOLDER_NAME);

                    var outputDir = outputFolder ?? Path.GetFileNameWithoutExtension(pkgFile);

                    var hedFile = Path.ChangeExtension(pkgFile, "hed");
                    using var hedStream = File.OpenRead(hedFile);
                    using var pkgStream = File.OpenRead(pkgFile);

                    var hedHeaders = Hed.Read(hedStream).ToList();

                    if (!Directory.Exists(outputDir))
                        Directory.CreateDirectory(outputDir);

                    using var patchedHedStream = File.Create(Path.Combine(outputDir, Path.GetFileName(hedFile)));
                    using var patchedPkgStream = File.Create(Path.Combine(outputDir, Path.GetFileName(pkgFile)));

                    foreach (var hedHeader in hedHeaders)
                    {
                        var hash = EpicGamesAssets.ToString(hedHeader.MD5);

                        // We don't know this filename, we ignore it
                        if (!Names.TryGetValue(hash, out var filename))
                        {
                            throw new Exception("Unknown filename!");
                        }

                        if (_patchFiles.Contains(filename))
                        {
                            _patchFiles.Remove(filename);
                        }

                        filenames.Add(filename);

                        var asset = new EgsHdAsset(pkgStream.SetPosition(hedHeader.Offset));

                        if (hedHeader.DataLength > 0)
                        {
                            ReplaceFile(inputFolder, filename, patchedHedStream, patchedPkgStream, asset, hedHeader);
                        }
                        else
                        {
                            Console.WriteLine($"Skipped: {filename}");
                        }
                    }

                    // Add all files that are not in the original HED file and inject them in the PKG stream too
                    foreach (var filename in _patchFiles)
                    {
                        AddFile(inputFolder, filename, patchedHedStream, patchedPkgStream);
                        Console.WriteLine($"Added a new file: {filename}");
                    }
                }

                private Hed.Entry AddFile(string inputFolder, string filename, FileStream hedStream, FileStream pkgStream, bool shouldCompressData = false, bool shouldEncryptData = false)
                {
                    var completeFilePath = Path.Combine(inputFolder, ORIGINAL_FILES_FOLDER_NAME, filename);
                    var offset = pkgStream.Position;

                    #region Data

                    using var newFileStream = File.OpenRead(completeFilePath);

                    var header = new EgsHdAsset.Header()
                    {
                        // CompressedLenght => -2: no compression and encryption, -1: no compression 
                        CompressedLength = !shouldCompressData ? !shouldEncryptData ? -2 : -1 : 0,
                        DecompressedLength = (int)newFileStream.Length,
                        RemasteredAssetCount = 0,
                        CreationDate = -1
                    };

                    var decompressedData = newFileStream.ReadAllBytes();
                    var compressedData = decompressedData.ToArray();

                    if (shouldCompressData)
                    {
                        compressedData = CompressData(decompressedData);
                        header.CompressedLength = compressedData.Length;
                    }

                    // Encrypt and write current file data in the PKG stream
                    // The seed used for encryption is the original data header
                    var seed = new MemoryStream();
                    BinaryMapping.WriteObject<EgsHdAsset.Header>(seed, header);

                    var encryptionSeed = seed.ReadAllBytes();
                    var encryptedData = header.CompressedLength > -2 ? EgsEncryption.Encrypt(compressedData, encryptionSeed) : compressedData;

                    // Write original file header
                    BinaryMapping.WriteObject<EgsHdAsset.Header>(pkgStream, header);

                    // Make sure to write the original file after remastered assets headers
                    pkgStream.Write(encryptedData);

                    #endregion

                    // Write a new entry in the HED stream
                    var hedHeader = new Hed.Entry()
                    {
                        MD5 = ToBytes(CreateMD5(filename)),
                        ActualLength = (int)newFileStream.Length,
                        DataLength = (int)(pkgStream.Position - offset),
                        Offset = offset
                    };

                    BinaryMapping.WriteObject<Hed.Entry>(hedStream, hedHeader);

                    return hedHeader;
                }

                private static Hed.Entry ReplaceFile(
                    string inputFolder,
                    string filename,
                    FileStream hedStream,
                    FileStream pkgStream,
                    EgsHdAsset asset,
                    Hed.Entry originalHedHeader = null)
                {
                    var completeFilePath = Path.Combine(inputFolder, ORIGINAL_FILES_FOLDER_NAME, filename);

                    var offset = pkgStream.Position;
                    var originalHeader = asset.OriginalAssetHeader;

                    // Clone the original asset header
                    var header = new EgsHdAsset.Header()
                    {
                        CompressedLength = originalHeader.CompressedLength,
                        DecompressedLength = originalHeader.DecompressedLength,
                        RemasteredAssetCount = originalHeader.RemasteredAssetCount,
                        CreationDate = originalHeader.CreationDate
                    };

                    // Use the base original asset data by default
                    var decompressedData = asset.OriginalData;
                    var encryptedData = asset.OriginalRawData;
                    var encryptionSeed = asset.Seed;

                    // We want to replace the original file
                    if (File.Exists(completeFilePath))
                    {
                        Console.WriteLine($"Replacing original: {filename}!");

                        using var newFileStream = File.OpenRead(completeFilePath);
                        decompressedData = newFileStream.ReadAllBytes();

                        var compressedData = decompressedData.ToArray();
                        var compressedDataLenght = originalHeader.CompressedLength;

                        // CompressedLenght => -2: no compression and encryption, -1: no compression 
                        if (originalHeader.CompressedLength > -1)
                        {
                            compressedData = CompressData(decompressedData);
                            compressedDataLenght = compressedData.Length;
                        }

                        header.CompressedLength = compressedDataLenght;
                        header.DecompressedLength = decompressedData.Length;

                        // Encrypt and write current file data in the PKG stream

                        // The seed used for encryption is the original data header
                        var seed = new MemoryStream();
                        BinaryMapping.WriteObject<EgsHdAsset.Header>(seed, header);

                        encryptionSeed = seed.ReadAllBytes();
                        encryptedData = header.CompressedLength > -2 ? EgsEncryption.Encrypt(compressedData, encryptionSeed) : compressedData;
                    }

                    // Write original file header
                    BinaryMapping.WriteObject<EgsHdAsset.Header>(pkgStream, header);

                    var remasteredHeaders = new List<EgsHdAsset.RemasteredEntry>();

                    // Is there remastered assets?
                    if (header.RemasteredAssetCount > 0)
                    {
                        remasteredHeaders = ReplaceRemasteredAssets(inputFolder, filename, asset, pkgStream, encryptionSeed, encryptedData);
                    }
                    else
                    {
                        // Make sure to write the original file after remastered assets headers
                        pkgStream.Write(encryptedData);
                    }

                    // Write a new entry in the HED stream
                    var hedHeader = new Hed.Entry()
                    {
                        MD5 = ToBytes(CreateMD5(filename)),
                        ActualLength = decompressedData.Length,
                        DataLength = (int)(pkgStream.Position - offset),
                        Offset = offset
                    };

                    // For unknown reason, some files have a data length of 0
                    if (originalHedHeader.DataLength == 0)
                    {
                        Console.WriteLine($"{filename} => {originalHedHeader.ActualLength} ({originalHedHeader.DataLength})");

                        hedHeader.ActualLength = originalHedHeader.ActualLength;
                        hedHeader.DataLength = originalHedHeader.DataLength;
                    }

                    BinaryMapping.WriteObject<Hed.Entry>(hedStream, hedHeader);

                    return hedHeader;
                }

                private static List<EgsHdAsset.RemasteredEntry> ReplaceRemasteredAssets(string inputFolder, string originalFile, EgsHdAsset asset, FileStream pkgStream, byte[] seed, byte[] originalAssetData)
                {
                    var newRemasteredHeaders = new List<EgsHdAsset.RemasteredEntry>();
                    var relativePath = GetRelativePath(originalFile, Path.Combine(inputFolder, ORIGINAL_FILES_FOLDER_NAME));
                    var remasteredAssetsFolder = Path.Combine(inputFolder, REMASTERED_FILES_FOLDER_NAME, relativePath);

                    var allRemasteredAssetsData = new MemoryStream();
                    // 0x30 is the size of this header
                    var totalRemasteredAssetHeadersSize = asset.RemasteredAssetHeaders.Count() * 0x30;
                    // This offset is relative to the original asset data
                    var offset = totalRemasteredAssetHeadersSize + 0x10 + asset.OriginalAssetHeader.DecompressedLength;

                    if (offset != asset.RemasteredAssetHeaders.Values.First().Offset)
                    {
                        throw new Exception("Something is wrong here!");
                    }

                    foreach (var remasteredAssetHeader in asset.RemasteredAssetHeaders.Values)
                    {
                        var filename = remasteredAssetHeader.Name;
                        var assetFilePath = Path.Combine(remasteredAssetsFolder, filename);

                        // Use base remastered asset data
                        var assetData = asset.RemasteredAssetsCompressedData[filename];
                        var decompressedLength = remasteredAssetHeader.DecompressedLength;

                        if (File.Exists(assetFilePath))
                        {
                            Console.WriteLine($"Replacing remastered file: {relativePath}/{filename}");

                            assetData = File.ReadAllBytes(assetFilePath);
                            decompressedLength = assetData.Length;
                            assetData = remasteredAssetHeader.CompressedLength > -1 ? CompressData(assetData) : assetData;
                            assetData = remasteredAssetHeader.CompressedLength > -2 ? EgsEncryption.Encrypt(assetData, seed) : assetData;
                        }
                        else
                        {
                            Console.WriteLine($"Keeping remastered file: {relativePath}/{filename}");

                            // The original file have been replaced, we need to encrypt all remastered asset with the new key
                            if (!seed.SequenceEqual(asset.Seed))
                            {
                                assetData = asset.RemasteredAssetsDecompressedData[filename];
                                assetData = remasteredAssetHeader.CompressedLength > -1 ? CompressData(assetData) : assetData;
                                assetData = remasteredAssetHeader.CompressedLength > -2 ? EgsEncryption.Encrypt(assetData, seed) : assetData;
                            }
                        }

                        var compressedLength = remasteredAssetHeader.CompressedLength > -1 ? assetData.Length : remasteredAssetHeader.CompressedLength;

                        var newRemasteredAssetHeader = new EgsHdAsset.RemasteredEntry()
                        {
                            CompressedLength = compressedLength,
                            DecompressedLength = decompressedLength,
                            Name = filename,
                            Offset = offset,
                            Unknown24 = remasteredAssetHeader.Unknown24
                        };

                        newRemasteredHeaders.Add(newRemasteredAssetHeader);

                        // Write asset header in the PKG stream
                        BinaryMapping.WriteObject<EgsHdAsset.RemasteredEntry>(pkgStream, newRemasteredAssetHeader);

                        // Don't write into the PKG stream yet as we need to write
                        // all HD assets header juste after original file's data
                        allRemasteredAssetsData.Write(assetData);

                        // Make sure to align remastered asset data on 16 bytes
                        if (assetData.Length % 0x10 != 0)
                        {
                            allRemasteredAssetsData.Write(Enumerable.Repeat((byte)0xCD, 16 - (assetData.Length % 0x10)).ToArray());
                        }

                        offset += decompressedLength;
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

            public static Hed.Entry CreateHedEntry(string filename, byte[] decompressedData, byte[] compressedData, long offset, List<EgsHdAsset.RemasteredEntry> remasteredHeaders = null)
            {
                var fileHash = CreateMD5(filename);
                // 0x10 => size of the original asset header
                // 0x30 => size of the remastered asset header
                var dataLength = compressedData.Length + 0x10;

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
                    ActualLength = decompressedData.Length,
                    DataLength = dataLength,
                    Offset = offset
                };
            }

            private static string GetRelativePath(string filePath, string origin)
            {
                return filePath.Replace($"{origin}\\", "").Replace(@"\", "/");
            }

            #endregion
        }
    }
}
