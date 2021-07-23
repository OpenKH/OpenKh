using OpenKh.Common;
using OpenKh.Kh1;
using OpenKh.Kh2;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Xe.BinaryMapper;

namespace OpenKh.Egs
{
    public class EgsTools
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
            .Concat(new string[] { "dummy.txt" })
            .Distinct()
            .ToDictionary(x => Helpers.ToString(MD5.HashData(Encoding.UTF8.GetBytes(x))), x => x);

        #endregion

        #region Extract

        public static void Extract(string inputHed, string output, bool doNotExtractAgain = false)
        {
            var outputDir = output ?? Path.GetFileNameWithoutExtension(inputHed);
            using var hedStream = File.OpenRead(inputHed);
            using var img = File.OpenRead(Path.ChangeExtension(inputHed, "pkg"));

            foreach (var entry in Hed.Read(hedStream))
            {
                var hash = Helpers.ToString(entry.MD5);
                if (!Names.TryGetValue(hash, out var fileName))
                    fileName = $"{hash}.dat";

                var outputFileName = Path.Combine(outputDir, ORIGINAL_FILES_FOLDER_NAME, fileName);

                if (doNotExtractAgain && File.Exists(outputFileName))
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

        #endregion

        #region Patch

        public static void Patch(string pkgFile, string inputFolder, string outputFolder)
        {
            // Get files to inject in the PKG to detect if we want to include new files or not
            // We only get the original files as for me it doesn't make sense to include
            // new "remastered" asset since it must be linked to an original one
            var patchFiles = Helpers.GetAllFiles(Path.Combine(inputFolder, ORIGINAL_FILES_FOLDER_NAME)).ToList();

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
                var hash = Helpers.ToString(hedHeader.MD5);

                // We don't know this filename, we ignore it
                if (!Names.TryGetValue(hash, out var filename))
                {
                    throw new Exception("Unknown filename!");
                }

                if (patchFiles.Contains(filename))
                {
                    patchFiles.Remove(filename);
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
            foreach (var filename in patchFiles)
            {
                AddFile(inputFolder, filename, patchedHedStream, patchedPkgStream);
                Console.WriteLine($"Added a new file: {filename}");
            }
        }

        private static Hed.Entry AddFile(string inputFolder, string filename, FileStream hedStream, FileStream pkgStream, bool shouldCompressData = false, bool shouldEncryptData = false)
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
                compressedData = Helpers.CompressData(decompressedData);
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
                MD5 = Helpers.ToBytes(Helpers.CreateMD5(filename)),
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
                    compressedData = Helpers.CompressData(decompressedData);
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
                MD5 = Helpers.ToBytes(Helpers.CreateMD5(filename)),
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
            var relativePath = Helpers.GetRelativePath(originalFile, Path.Combine(inputFolder, ORIGINAL_FILES_FOLDER_NAME));
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
                    assetData = remasteredAssetHeader.CompressedLength > -1 ? Helpers.CompressData(assetData) : assetData;
                    assetData = remasteredAssetHeader.CompressedLength > -2 ? EgsEncryption.Encrypt(assetData, seed) : assetData;
                }
                else
                {
                    Console.WriteLine($"Keeping remastered file: {relativePath}/{filename}");

                    // The original file have been replaced, we need to encrypt all remastered asset with the new key
                    if (!seed.SequenceEqual(asset.Seed))
                    {
                        assetData = asset.RemasteredAssetsDecompressedData[filename];
                        assetData = remasteredAssetHeader.CompressedLength > -1 ? Helpers.CompressData(assetData) : assetData;
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

        #endregion

        #region List

        public static void List(string inputHed)
        {
            using var hedStream = File.OpenRead(inputHed);
            var entries = Hed.Read(hedStream);

            foreach (var entry in entries)
            {
                var hash = Helpers.ToString(entry.MD5);
                if (!Names.TryGetValue(hash, out var fileName))
                    fileName = $"{hash}.dat";

                Console.WriteLine(fileName);
            }
        }

        #endregion
    }
}
