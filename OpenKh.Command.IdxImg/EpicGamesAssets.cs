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

                        var hdAsset = new EgsHdAsset(img.SetPosition(entry.Offset));
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

                    var hedFile = Path.ChangeExtension(pkgFile, "hed");
                    using var hedStream = File.OpenRead(hedFile);
                    using var pkgStream = File.OpenRead(pkgFile);

                    if (!Directory.Exists(outputDir))
                        Directory.CreateDirectory(outputDir);

                    using var patchedHedStream = File.Create(Path.Combine(outputDir, Path.GetFileName(hedFile)));
                    using var patchedPkgStream = File.Create(Path.Combine(outputDir, Path.GetFileName(pkgFile)));

					var pkgOffset = 0L;
					
					pkgStream.SetPosition(0);
					
                    foreach (var entry in Hed.Read(hedStream))
                    {
                        var hash = EpicGamesAssets.ToString(entry.MD5);

                        // We don't know this filename, we ignore it
                        if (!Names.TryGetValue(hash, out var filename))
                        {
							Console.WriteLine($"No name for hash: {hash}!");
                            continue;
                        }

                        // Replace the found files
                        var asset = new EgsHdAsset(pkgStream.SetPosition(entry.Offset));
						var fileToInject = Path.Combine(inputFolder, filename);
						var newHedEntry = ReplaceFile(fileToInject, inputFolder, patchedHedStream, patchedPkgStream, asset, entry);

                        pkgOffset += newHedEntry.DataLength;
                    }
                }

				private Hed.Entry ReplaceFile(string filename, string inputFolder, FileStream hedStream, FileStream pkgStream, EgsHdAsset asset, Hed.Entry originalHedHeader = null)
				{
                    var filesToReplace = GetAllFiles(inputFolder);
                    var originalfilename = GetRelativePath(filename, Path.Combine(InputFolder, ORIGINAL_FILES_FOLDER_NAME));

					byte[] data = new byte[asset.OriginalAssetHeader.DecompressedLength];
					byte[] compressedData = new byte[]{};
					byte[] encryptedData = new byte[]{};
                    // Encrypt and write current file data in the PKG stream
                    var header = CreateAssetHeader(
                        data,
                        asset.OriginalAssetHeader.CompressedLength,
                        asset.OriginalAssetHeader.RemasteredAssetCount,
                        asset.OriginalAssetHeader.Unknown0c
                    );
					
					var encryptionKey = asset.Key;
					if (filesToReplace.Contains(originalfilename))
					{
						Console.WriteLine($"Replacing original: {originalfilename}!");
						Console.WriteLine(header.CompressedLength);
						// Read Raw data to set the correct offset even if we're reading from an external file
						data = asset.ReadRawData();
						header.DecompressedLength = data.Length;
						data = File.ReadAllBytes(filename);
						header.DecompressedLength = data.Length;
						compressedData = header.CompressedLength > -1 ? CompressData(data) : data;
						if(header.CompressedLength > -1) header.CompressedLength = compressedData.Length;
						Console.WriteLine(header.CompressedLength);
						if(header.CompressedLength > -2){
							var seed = new MemoryStream();

							// The seed used for encryption is the data header6
							BinaryMapping.WriteObject<EgsHdAsset.Header>(seed, header);
							encryptionKey = seed.ReadAllBytes();

							// Encrypt file
							encryptedData = header.CompressedLength > 2 ? EgsEncryption.Encrypt(compressedData, encryptionKey) : compressedData;
						}
					}else{
						Console.WriteLine($"Keeping original: {originalfilename}!");
						//data = asset.ReadRawData();
						compressedData = asset.ReadRawData();
						encryptedData = compressedData;
					}				
					var offset = pkgStream.Position;
                    BinaryMapping.WriteObject<EgsHdAsset.Header>(pkgStream, header);

                    var remasteredHeaders = new List<EgsHdAsset.RemasteredEntry>();

                    // Is there remastered assets?
                    if (header.RemasteredAssetCount > 0)
                    {
						remasteredHeaders = ReplaceRemasteredAssets(originalfilename, asset, pkgStream, encryptionKey, encryptedData, data);

                        // If a remastered asset is not replaced, we still want to count its size for the HED entry

                        foreach (var remasteredAssetName in asset.RemasteredAssetHeaders.Keys)
                        {
                            if (!remasteredHeaders.Exists(header => header.Name == remasteredAssetName))
                            {
                                remasteredHeaders.Add(asset.RemasteredAssetHeaders[remasteredAssetName]);
                            }
                        }
                    }
                    else
                    {
                        // Make sure to write the original file after remastered assets headers
                        pkgStream.Write(encryptedData);
                    }

                    // Write a new entry in the HED stream
                    var hedEntry = CreateHedEntry(originalfilename, data, (int)(pkgStream.Position - offset), offset, remasteredHeaders);

                    BinaryMapping.WriteObject<Hed.Entry>(hedStream, hedEntry);

                    return hedEntry;
                }
				
                private List<EgsHdAsset.RemasteredEntry> ReplaceRemasteredAssets(string originalFile, EgsHdAsset asset, FileStream pkgStream, byte[] seed, byte[] originalAssetData, byte[] originalUncompressedData)
                {
                    var newRemasteredHeaders = new List<EgsHdAsset.RemasteredEntry>();
                    var relativePath = GetRelativePath(originalFile, Path.Combine(InputFolder, ORIGINAL_FILES_FOLDER_NAME));
                    
					var baseRemasteredFolder = Path.Combine(InputFolder, REMASTERED_FILES_FOLDER_NAME, relativePath);
                    var remasteredAssetsFolder = GetHDAssetFolder(baseRemasteredFolder);

					var remasteredAssetFiles = asset.Assets;
					var allRemasteredAssetsData = new MemoryStream();

					// 0x30 is the size of this header
					var totalRemasteredAssetHeadersSize = (remasteredAssetFiles.Count() * 0x30);
					var offsetPosition = originalUncompressedData.Length;

					foreach (var remasteredAssetFile in remasteredAssetFiles)
					{
						var assetFilePath = Path.Combine(remasteredAssetsFolder, remasteredAssetFile);

						var uncompressedData = new byte[]{};
						var compressedData = new byte[]{};
						var encryptedData = new byte[]{};
						if (File.Exists(assetFilePath)){
							Console.WriteLine($"Replacing remastered file: {relativePath}/{remasteredAssetFile}");
							uncompressedData = File.ReadAllBytes(assetFilePath);
							compressedData = asset.RemasteredAssetHeaders[remasteredAssetFile].CompressedLength > -1 ? CompressData(uncompressedData) : uncompressedData;
							Console.WriteLine(asset.RemasteredAssetHeaders[remasteredAssetFile].CompressedLength);

							//Read the remastered file regardless, to set the correct offset
							encryptedData = asset.ReadRawRemasteredAsset(remasteredAssetFile);
							encryptedData = asset.RemasteredAssetHeaders[remasteredAssetFile].CompressedLength > -2 ? EgsEncryption.Encrypt(compressedData, seed) : compressedData;
						}else{
							Console.WriteLine($"Keeping remastered file: {relativePath}/{remasteredAssetFile}");
							encryptedData = asset.ReadRawRemasteredAsset(remasteredAssetFile);
							uncompressedData = new byte[asset.RemasteredAssetHeaders[remasteredAssetFile].DecompressedLength];
						}

						var currentOffset = totalRemasteredAssetHeadersSize + offsetPosition + 0x10;

						var entryCompressedLength = encryptedData.Length;
						if(asset.RemasteredAssetHeaders[remasteredAssetFile].CompressedLength == -1) entryCompressedLength = (int)-1; // -1 is uncompressed
						if(asset.RemasteredAssetHeaders[remasteredAssetFile].CompressedLength == -2) entryCompressedLength = (int)-2; // -2 is uncompressed and unencrypted

						var remasteredEntry = new EgsHdAsset.RemasteredEntry()
						{
							CompressedLength = entryCompressedLength,
							DecompressedLength = (int)uncompressedData.Length,
							Name = remasteredAssetFile,
							Offset = currentOffset,
							Unknown24 = asset.RemasteredAssetHeaders[remasteredAssetFile].Unknown24
						};

						newRemasteredHeaders.Add(remasteredEntry);

						// Write asset header in the PKG stream
						BinaryMapping.WriteObject<EgsHdAsset.RemasteredEntry>(pkgStream, remasteredEntry);

						// Don't write into the PKG stream yet as we need to write
						// all HD assets header juste after original file's data
						allRemasteredAssetsData.Write(encryptedData);
						if(encryptedData.Length % 0x10 != 0){
							allRemasteredAssetsData.Write(Enumerable.Repeat((byte)0xCD, 16 - (encryptedData.Length % 0x10)).ToArray());
						}

						offsetPosition += uncompressedData.Length;
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
			
            public static Hed.Entry CreateHedEntry(string filename, byte[] data, int dataLength, long offset, List<EgsHdAsset.RemasteredEntry> remasteredHeaders)
            {
                var fileHash = CreateMD5(filename);
                // 0x10 => size of the original asset header
                // 0x30 => size of the remastered asset header

                return new Hed.Entry()
                {
                    MD5 = ToBytes(fileHash),
                    Offset = offset,
                    DataLength = dataLength,
                    ActualLength = (int)data.Length
                };
            }

            public static EgsHdAsset.Header CreateAssetHeader(byte[] data, int compressedDataLength, int remasteredAssetCount = 0, int unknown0c = 0x0)
            {
                return new EgsHdAsset.Header()
                {
                    CompressedLength = compressedDataLength,
                    DecompressedLength = (int)data.Length,
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
