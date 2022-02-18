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
                    Console.WriteLine($"Unknown filename (hash: {hash})");
                    continue;
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
                //Console.WriteLine($"Added a new file: {filename}");
            }
        }

        private static Hed.Entry AddFile(string inputFolder, string filename, FileStream hedStream, FileStream pkgStream, bool shouldCompressData = false, bool shouldEncryptData = false)
        {
            var completeFilePath = Path.Combine(inputFolder, ORIGINAL_FILES_FOLDER_NAME, filename);
            var offset = pkgStream.Position;

            #region Data

            using var newFileStream = File.OpenRead(completeFilePath);
            bool RemasterExist = false;
            string RemasteredPath = completeFilePath.Replace("\\original\\", "\\remastered\\");
            if (Directory.Exists(RemasteredPath))
                RemasterExist = true;

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

            SDasset sdasset = new SDasset(filename, decompressedData, RemasterExist);
            RemasterExist = false;

            if (sdasset != null && !sdasset.Invalid)
                header.RemasteredAssetCount = sdasset.TextureCount;

            // Encrypt and write current file data in the PKG stream
            // The seed used for encryption is the original data header
            var seed = new MemoryStream();
            BinaryMapping.WriteObject<EgsHdAsset.Header>(seed, header);

            var encryptionSeed = seed.ReadAllBytes();
            var encryptedData = header.CompressedLength > -2 ? EgsEncryption.Encrypt(compressedData, encryptionSeed) : compressedData;

            // Write original file header
            BinaryMapping.WriteObject<EgsHdAsset.Header>(pkgStream, header);

            if (header.RemasteredAssetCount > 0)
            {
                // Create an "Asset" to pass to ReplaceRemasteredAssets
                EgsHdAsset asset = new EgsHdAsset(header, decompressedData, encryptedData, encryptionSeed);
                ReplaceRemasteredAssets(inputFolder, filename, asset, pkgStream, encryptionSeed, encryptedData, sdasset);
            }
            else
            {
                // Make sure to write the original file after remastered assets headers
                pkgStream.Write(encryptedData);
            }

            #endregion

            // Write a new entry in the HED stream
            var hedHeader = new Hed.Entry()
            {
                MD5 = Helpers.ToBytes(Helpers.CreateMD5(filename)),
                ActualLength = (int)newFileStream.Length,
                DataLength = (int)(pkgStream.Position - offset),
                Offset = offset
            };

            //if (!Names.TryGetValue(Helpers.ToString(hedHeader.MD5), out var existingfilename))
            //{
            //    File.AppendAllText("resources/custom_filenames.txt", filename + "\n");
            //}

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

            SDasset sdasset = null;
            // We want to replace the original file
            if (File.Exists(completeFilePath))
            {
                bool RemasterExist = false;

                Console.WriteLine($"Replacing original: {filename}!");
                string RemasteredPath = completeFilePath.Replace("\\original\\","\\remastered\\");
                if (Directory.Exists(RemasteredPath))
                {
                    Console.WriteLine($"Remastered Folder Exists! Path: {RemasteredPath}");
                    RemasterExist = true;
                }

                using var newFileStream = File.OpenRead(completeFilePath);
                decompressedData = newFileStream.ReadAllBytes();

                sdasset = new SDasset(filename, decompressedData, RemasterExist);

                if (sdasset != null && !sdasset.Invalid)
                    header.RemasteredAssetCount = sdasset.TextureCount;

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
                remasteredHeaders = ReplaceRemasteredAssets(inputFolder, filename, asset, pkgStream, encryptionSeed, encryptedData, sdasset);
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

        private static List<EgsHdAsset.RemasteredEntry> ReplaceRemasteredAssets(string inputFolder, string originalFile, EgsHdAsset asset, FileStream pkgStream, byte[] seed, byte[] originalAssetData, SDasset sdasset)
        {
            var newRemasteredHeaders = new List<EgsHdAsset.RemasteredEntry>();
            var oldRemasteredHeaders = new List<EgsHdAsset.RemasteredEntry>();
            var relativePath = Helpers.GetRelativePath(originalFile, Path.Combine(inputFolder, ORIGINAL_FILES_FOLDER_NAME));
            var remasteredAssetsFolder = Path.Combine(inputFolder, REMASTERED_FILES_FOLDER_NAME, relativePath);

            var allRemasteredAssetsData = new MemoryStream();

            foreach (var remasteredAssetHeader in asset.RemasteredAssetHeaders.Values)
            {
                oldRemasteredHeaders.Add(remasteredAssetHeader);
            }

            //At the moment this only applies on fresh PKGs (or ones that haven't been patched with this modded MDLX before, otherwise we'd neet to analyse ALL MDLX files)
            if (sdasset != null && !sdasset.Invalid)
            {
                //File.AppendAllText("custom_hd_assets.txt", "HD assets for: " + originalFile + "\n");
                while (oldRemasteredHeaders.Count > sdasset.TextureCount)
                {
                    //File.AppendAllText("custom_hd_assets.txt", "Removing: -" + (oldRemasteredHeaders.Count - 1) + ".dds\n");
                    oldRemasteredHeaders.RemoveAt(oldRemasteredHeaders.Count - 1);
                }
                while (oldRemasteredHeaders.Count < sdasset.TextureCount)
                {
                    var newRemasteredAssetHeader = new EgsHdAsset.RemasteredEntry()
                    {
                        CompressedLength = 0,
                        DecompressedLength = 0,
                        Name = "-" + oldRemasteredHeaders.Count + ".dds",
                        Offset = 0,
                        OriginalAssetOffset = 0
                    };
                    //File.AppendAllText("custom_hd_assets.txt", "Adding: -" + oldRemasteredHeaders.Count + ".dds\n");
                    oldRemasteredHeaders.Add(newRemasteredAssetHeader);
                }
                //File.AppendAllText("custom_hd_assets.txt", "\n");
            }

            // 0x30 is the size of this header
            var totalRemasteredAssetHeadersSize = oldRemasteredHeaders.Count() * 0x30;
            // This offset is relative to the original asset data
            var offset = totalRemasteredAssetHeadersSize + 0x10 + asset.OriginalAssetHeader.DecompressedLength;
            List<string> remasteredNames = new List<string>();


            remasteredNames.Clear();
            //grab list of full file paths from current remasteredAssetsFolder path and add them to a list.
            //we use this list later to correctly add the file names to the PKG.
            if (Directory.Exists(remasteredAssetsFolder) && Directory.GetFiles(remasteredAssetsFolder, "*", SearchOption.AllDirectories).Length > 0) //only do this if there are actually file in it.
            {
                remasteredNames.AddRange(Directory.GetFiles(remasteredAssetsFolder, "*", SearchOption.AllDirectories).ToList());
                for (int l = 0; l < remasteredNames.Count; l++) //fix names
                {
                    remasteredNames[l] = remasteredNames[l].Replace(remasteredAssetsFolder, "").Replace(@"\", "/");
                }

                if (remasteredNames.Contains("/-10.dds") || remasteredNames.Contains("/-10.png"))
                {
                    //Make a sorted list tempremasteredNames
                    List<string> tempremasteredNamesD = new List<string>();
                    List<string> tempremasteredNamesP = new List<string>();
                    for (int i = 0; i < remasteredNames.Count; i++)
                    {
                        var filename = "/-"  + i.ToString();
                        Console.WriteLine("TEST for " + filename + ".dds/.png");
                        if (remasteredNames.Contains(filename + ".dds"))
                        {
                            Console.WriteLine(filename + ".dds" + "FOUND!");
                            tempremasteredNamesD.Add(filename + ".dds");
                            remasteredNames.Remove(filename + ".dds");
                        }
                        else if (remasteredNames.Contains(filename + ".png"))
                        {
                            Console.WriteLine(filename + ".png" + "FOUND!");
                            tempremasteredNamesP.Add(filename + ".png");
                            remasteredNames.Remove(filename + ".png");
                        }
                    }
                    //Add the image files at the end
                    //DDS list first, PNG list 2nd, everything else after
                    tempremasteredNamesD.AddRange(tempremasteredNamesP);
                    tempremasteredNamesD.AddRange(remasteredNames);
                    //Add the sorted list back to remasteredNames
                    remasteredNames = tempremasteredNamesD;
                }
            }

            for (int i = 0; i < oldRemasteredHeaders.Count; i++)
            {
                var remasteredAssetHeader = oldRemasteredHeaders[i];
                var filename = remasteredAssetHeader.Name;
                var assetFilePath = Path.Combine(remasteredAssetsFolder, filename);

                //get actual file names ONLY if the remastered asset count is greater than 0 and ONLY if the number of files in the 
                //remastered folder for the SD asset is equal to or greater than what the total count is from what was gotten in SDasset.
                //if those criteria aren't met then do the old method.
                if (remasteredNames.Count >= oldRemasteredHeaders.Count && remasteredNames.Count > 0)
                {
                    //filename = remasteredNames[i].Replace((remasteredAssetsFolder), "").Remove(0, 1);
                    filename = remasteredNames[i].Remove(0, 1);
                    assetFilePath = Path.Combine(remasteredAssetsFolder, filename);
                }

                // Use base remastered asset data
                var assetData = asset.RemasteredAssetsDecompressedData.ContainsKey(filename) ? asset.RemasteredAssetsDecompressedData[filename] : new byte[] { };
                var decompressedLength = remasteredAssetHeader.DecompressedLength;
                var originalAssetOffset = remasteredAssetHeader.OriginalAssetOffset;
                if (File.Exists(assetFilePath))
                {
                    Console.WriteLine($"Replacing remastered file: {relativePath}/{filename}");

                    assetData = File.ReadAllBytes(assetFilePath);
                    decompressedLength = assetData.Length;
                    assetData = remasteredAssetHeader.CompressedLength > -1 ? Helpers.CompressData(assetData) : assetData;
                    assetData = remasteredAssetHeader.CompressedLength > -2 ? EgsEncryption.Encrypt(assetData, seed) : assetData;
                    if (sdasset != null && !sdasset.Invalid)
                        originalAssetOffset = sdasset.Offsets[i];
                }
                else
                {
                    //Console.WriteLine($"Keeping remastered file: {relativePath}/{filename}");
                    // The original file have been replaced, we need to encrypt all remastered asset with the new key
                    if (!seed.SequenceEqual(asset.Seed))
                    {
                        assetData = remasteredAssetHeader.CompressedLength > -1 ? Helpers.CompressData(assetData) : assetData;
                        assetData = remasteredAssetHeader.CompressedLength > -2 ? EgsEncryption.Encrypt(assetData, seed) : assetData;
                        if (sdasset != null && !sdasset.Invalid && sdasset.TextureCount >= i)
                            originalAssetOffset = sdasset.Offsets[i];
                    }
                    else
                    {
                        assetData = asset.RemasteredAssetsCompressedData.ContainsKey(filename) ? asset.RemasteredAssetsCompressedData[filename] : new byte[] { };
                    }
                }
                var compressedLength = remasteredAssetHeader.CompressedLength > -1 ? assetData.Length : remasteredAssetHeader.CompressedLength;

                var newRemasteredAssetHeader = new EgsHdAsset.RemasteredEntry()
                {
                    CompressedLength = compressedLength,
                    DecompressedLength = decompressedLength,
                    Name = filename,
                    Offset = offset,
                    OriginalAssetOffset = originalAssetOffset
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

                //Console.WriteLine(fileName);
            }
        }

        #endregion
    }

    class SDasset
    {
        public List<int> Offsets = new List<int>();
        public int TextureCount = 0;
        public bool Invalid = true;

        public SDasset(string name, byte[] originalAssetData, bool remasterpathtrue)
        {
            dynamic asset = null;
            switch (Path.GetExtension(name), remasterpathtrue)
            {
                case (".2dd", true):
                case (".2ld", true):
                case (".bar", true):
                    asset = new BAR(originalAssetData);
                    break;
                case (".imd", true):
                    asset = new IMD(originalAssetData);
                    break;
                case (".imz", true):
                    asset = new IMZ(originalAssetData);
                    break;
                case (".mdlx", true):
                    asset = new MDLX(originalAssetData);
                    break;
            }
            switch (".a" + (Path.GetExtension(name)), remasterpathtrue)
            {
                case (".a.fm", true):
                case (".a.fr", true):
                case (".a.gr", true):
                case (".a.it", true):
                case (".a.sp", true):
                case (".a.us", true):
                case (".a.uk", true):
                case (".a.jp", true):
                    asset = new BAR(originalAssetData);
                    break;
            }

            if (asset != null && !asset.Invalid)
            {
                Offsets = asset.Offsets;
                TextureCount = asset.TextureCount;
                Invalid = false;
            }
        }
    }

    class IMD
    {
        public List<int> Offsets = new List<int>();
        public int TextureCount = 0;
        public bool Invalid = false;

        public IMD(byte[] originalAssetData)
        {
            using (MemoryStream ms = new MemoryStream(originalAssetData))
            {
                int magic = ms.ReadInt32();
                if (magic != 1145523529)
                { //IMGD
                    Invalid = true;
                    return;
                }

                TextureCount = 1;
                ms.ReadInt32(); //always 256
                int IMDoffset = ms.ReadInt32(); //offset for image data
                Offsets.Add(IMDoffset + 0x20000000);
            }
        }
    }

    class IMZ
    {
        public List<int> Offsets = new List<int>();
        public int TextureCount = 0;
        public bool Invalid = false;

        public IMZ(byte[] originalAssetData)
        {
            using (MemoryStream ms = new MemoryStream(originalAssetData))
            {
                int magic = ms.ReadInt32();
                if (magic != 1514622281)
                { //IMGZ
                    Invalid = true;
                    return;
                }
                ms.ReadInt32();
                ms.ReadInt32();
                TextureCount = ms.ReadInt32();

                for (int i = 0; i < TextureCount; i++)
                {
                    ms.Seek(0x10 + (i * 0x8), SeekOrigin.Begin);
                    int offset = ms.ReadInt32();
                    ms.Seek(offset, SeekOrigin.Begin);
                    int magic2 = ms.ReadInt32();
                    if (magic2 != 1145523529)
                    { //IMGD
                        Invalid = true;
                        return;
                    }

                    ms.ReadInt32(); //always 256
                    int IMDoffset = ms.ReadInt32(); //offset for image data
                    Offsets.Add(offset + IMDoffset + 0x20000000);
                }
            }
        }
    }

    class BAR
    {
        public List<int> Offsets = new List<int>();
        public List<int> OffsetsAudio = new List<int>();
        public int TextureCount = 0;
        public bool Invalid = false;

        public BAR(byte[] originalAssetData)
        {
            using (MemoryStream ms = new MemoryStream(originalAssetData))
            {
                int subcount = 0;
                int offset = 0;
                int suboffset = 0;
                int subtype = 0;
                int Dpxoffset = 0;
                int DpdOffset = 0;
                int DpdOffsets = 0;
                int DpdCount = 0;
                int DpdTexCount = 0;
                int DpdTexOffset = 0;
                int DpdTexOffsets = 0;
                int Unk1Count = 0;
                int Unk2Count = 0;
                int magic2 = 0;
                string magicsound;

                int magic = ms.ReadInt32();
                if (magic != 22167874)
                { //BAR
                    Invalid = true;
                    return;
                }

                int count = ms.ReadInt32();
                for (int i = 0; i < count; i++)
                {
                    ms.Seek(0x10 + (i * 0x10), SeekOrigin.Begin);
                    int type = ms.ReadInt32();
                    //Console.WriteLine("type is - " + type);
                    switch (type)
                    {
                        case (18): //PAX
                            //Console.WriteLine("type is an PAX archive! - " + type);
                            ms.ReadInt32();
                            offset = ms.ReadInt32();
                            ms.Seek(offset, SeekOrigin.Begin);

                            magic2 = ms.ReadInt32();
                            //Console.WriteLine("pax magic is - " + magic2);
                            if (magic2 == 1599619408)
                            { //PAX_

                                ms.ReadInt64();
                                Dpxoffset = ms.ReadInt32();
                                //Console.WriteLine("Dpxoffset is - " + Dpxoffset);
                                ms.Seek(offset + Dpxoffset + 0xC, SeekOrigin.Begin);
                                Unk1Count = ms.ReadInt32();
                                ms.Seek(Unk1Count * 0x20, SeekOrigin.Current);
                                DpdCount = ms.ReadInt32();
                                DpdOffsets = ((int)ms.Position);

                                for (int d = 0; d < DpdCount; d++)
                                {
                                    ms.Seek(DpdOffsets + (d * 0x4), SeekOrigin.Begin);
                                    DpdOffset = ms.ReadInt32();
                                    ms.Seek(offset + Dpxoffset + DpdOffset, SeekOrigin.Begin);

                                    ms.ReadInt32(); // unknown
                                    Unk2Count = ms.ReadInt32();
                                    ms.Seek(Unk2Count * 0x4, SeekOrigin.Current);
                                    DpdTexCount = ms.ReadInt32();
                                    DpdTexOffsets = ((int)ms.Position);

                                    for (int t = 0; t < DpdTexCount; t++)
                                    {
                                        TextureCount += 1;
                                        ms.Seek(DpdTexOffsets + (t * 0x4), SeekOrigin.Begin);
                                        DpdTexOffset = ms.ReadInt32();
                                        Offsets.Add(offset + Dpxoffset + DpdOffset + (DpdTexOffset + 0x20) + 0x20000000);
                                    }
                                }
                            }
                            break;
                        case (24): //IMD
                            //Console.WriteLine("is an image! - " + type);
                            ms.ReadInt32();
                            offset = ms.ReadInt32();
                            ms.Seek(offset, SeekOrigin.Begin);

                            magic2 = ms.ReadInt32();
                            if (magic2 == 1145523529)
                            { //IMGD
                                TextureCount += 1;
                                ms.ReadInt32(); //always 256
                                int IMDoffset = ms.ReadInt32(); //offset for image data
                                Offsets.Add(offset + IMDoffset + 0x20000000);
                            }
                            break;
                        case (29): //IMZ
                            //Console.WriteLine("is an image collection! - " + type);
                            ms.ReadInt32();
                            offset = ms.ReadInt32();
                            ms.Seek(offset, SeekOrigin.Begin);

                            magic2 = ms.ReadInt32();
                            if (magic2 == 1514622281)
                            { //IMGZ
                                ms.ReadInt32();
                                ms.ReadInt32();
                                int ImageCount = ms.ReadInt32();
                                TextureCount += ImageCount;
                                for (int j = 0; j < ImageCount; j++)
                                {
                                    ms.Seek(offset + 0x10 + (j * 0x8), SeekOrigin.Begin);
                                    int IMZoffset = ms.ReadInt32();
                                    ms.Seek(offset + IMZoffset, SeekOrigin.Begin);

                                    int magic3 = ms.ReadInt32();
                                    if (magic3 == 1145523529)
                                    {
                                        ms.ReadInt32(); //always 256
                                        int IMDoffset = ms.ReadInt32(); //offset for image data
                                        Offsets.Add(offset + IMZoffset + IMDoffset + 0x20000000);
                                    }
                                }
                            }
                            break;
                        case (31): //Sound Effects
                            //Console.WriteLine("is audio! - " + type);
                            ms.ReadInt32();
                            offset = ms.ReadInt32();
                            ms.Seek(offset, SeekOrigin.Begin);

                            magicsound = System.Text.Encoding.ASCII.GetString(ms.ReadBytes(6));
                            //Console.WriteLine("magic is - " + magicsound);
                            if (magicsound == "ORIGIN")
                            {
                                TextureCount += 1;
                                OffsetsAudio.Add(-1);
                            }
                            break;
                        case (34): //Voice Audio
                            //Console.WriteLine("is audio! - " + type);
                            ms.ReadInt32();
                            offset = ms.ReadInt32();
                            ms.Seek(offset, SeekOrigin.Begin);

                            magicsound = System.Text.Encoding.ASCII.GetString(ms.ReadBytes(6));
                            //Console.WriteLine("magic is - " + magicsound);
                            if (magicsound == "ORIGIN")
                            {
                                TextureCount += 1;
                                OffsetsAudio.Add(-1);
                            }
                            break;
                        case (46): //sub-BAR
                            //Console.WriteLine("is BAR-ception! - " + type);
                            ms.ReadInt32();
                            offset = ms.ReadInt32();
                            ms.Seek(offset, SeekOrigin.Begin);
                            suboffset = ((int)ms.Position);

                            magic2 = ms.ReadInt32();
                            if (magic2 == 22167874)
                            {
                                subcount = ms.ReadInt32();

                                for (int s = 0; s < subcount; s++)
                                {

                                    ms.Seek(0x10 + suboffset + (s * 0x10), SeekOrigin.Begin);
                                    subtype = ms.ReadInt32();

                                    switch (subtype)
                                    {
                                        case (24): //IMD
                                            //Console.WriteLine("is a BAR-ception image! - " + type);
                                            ms.ReadInt32();
                                            offset = ms.ReadInt32();
                                            ms.Seek(offset + suboffset, SeekOrigin.Begin);

                                            magic2 = ms.ReadInt32();
                                            if (magic2 == 1145523529)
                                            { //IMGD
                                                TextureCount += 1;
                                                ms.ReadInt32(); //always 256
                                                int IMDoffset = ms.ReadInt32(); //offset for image data
                                                Offsets.Add(suboffset + offset + IMDoffset + 0x20000000);
                                            }
                                            break;
                                        case (29): //IMZ
                                            //Console.WriteLine("is a BAR-ception image collection! - " + subtype);
                                            ms.ReadInt32();
                                            offset = ms.ReadInt32();
                                            ms.Seek(offset + suboffset, SeekOrigin.Begin);

                                            magic2 = ms.ReadInt32();
                                            //Console.WriteLine("BAR-ception image collection magic2: - " + magic2);
                                            if (magic2 == 1514622281)
                                            { //IMGZ
                                                ms.ReadInt64();
                                                int ImageCount = ms.ReadInt32();
                                                //Console.WriteLine("BAR-ception imz count: - " + ImageCount);
                                                for (int sj = 0; sj < ImageCount; sj++)
                                                {
                                                    ms.Seek(suboffset + offset +  + 0x10 + (sj * 0x8), SeekOrigin.Begin);
                                                    int IMZoffset = ms.ReadInt32();
                                                    ms.Seek(suboffset + offset + IMZoffset, SeekOrigin.Begin);

                                                    int magic3 = ms.ReadInt32();
                                                    //Console.WriteLine("BAR-ception magic3: - " + magic3);
                                                    if (magic3 == 1145523529)
                                                    {
                                                        TextureCount += 1;
                                                        ms.ReadInt32(); //always 256
                                                        int IMDoffset = ms.ReadInt32(); //offset for image data
                                                        Offsets.Add(suboffset + offset + IMZoffset + IMDoffset + 0x20000000);
                                                    }
                                                }
                                            }
                                            break;
                                    }
                                }
                            }
                            break;
                    }
                }

                //add all audio offsets to the end. they need to always be last
                Offsets.AddRange(OffsetsAudio);
                
                if (TextureCount == 0)
                {
                    Console.WriteLine("BAR doesn't contain hd assets.");
                    Invalid = true;
                    return;
                }
            }
        }
    }

    class MDLX
    {
        public List<int> Offsets = new List<int>();
        public int TextureCount = 0;
        public bool Invalid = false;

        public MDLX(byte[] originalAssetData)
        {
            using (MemoryStream ms = new MemoryStream(originalAssetData))
            {
                ms.ReadInt32();
                int version = ms.ReadInt32();
                if (version != 2 && version != 3 && version != 4)
                { //original: version != 3
                    Invalid = true;
                    return;
                }
                ms.Seek(0x24, SeekOrigin.Begin);
                string tim_ = System.Text.Encoding.ASCII.GetString(ms.ReadBytes(4));
                if (tim_ != "tim_")
                {
                    Invalid = true;
                    return;
                }
                int TIMoffset = ms.ReadInt32();

                ms.Seek(TIMoffset, SeekOrigin.Begin);
                int pointer = ms.ReadInt32();
                if (pointer == -1)
                {
                    Invalid = true;
                    return;
                }

                ms.Seek(TIMoffset + 0x0c, SeekOrigin.Begin);
                TextureCount = ms.ReadInt32();

                ms.ReadInt32();
                ms.ReadInt32();
                int infoOffset = ms.ReadInt32();
                int dataOffset = ms.ReadInt32();

                int diff = 0;
                for (int i = 0; i < TextureCount; i++)
                {
                    int offset = TIMoffset + dataOffset + diff + (i * 0x10) + 0x20000000;
                    Offsets.Add(offset);

                    int textInfoOffset = TIMoffset + infoOffset + 0x20 + 0x40 + 0x10 + (0xA0 * i);
                    ms.Seek(textInfoOffset, SeekOrigin.Begin);
                    ulong num = ms.ReadUInt64();
                    int width = (ushort)(1u << ((int)(num >> 0x1A) & 0x0F));
                    int height = (ushort)(1u << ((int)(num >> 0x1E) & 0x0F));
                    diff += width * height;
                }

                int index = Helpers.IndexOfByteArray(originalAssetData, System.Text.Encoding.UTF8.GetBytes("TEXA"), TIMoffset);

                while (index > -1)
                {
                    ms.Seek(index + 0x0a, SeekOrigin.Begin);
                    int imageToApplyTo = (int)ms.ReadInt16();

                    ms.Seek(0x1c, SeekOrigin.Current);
                    int texaOffset = ms.ReadInt32();
                    int offset = index + texaOffset + 0x08 + (imageToApplyTo * 0x10) + 0x20000000;
                    Offsets.Add(offset);

                    TextureCount++;
                    index = Helpers.IndexOfByteArray(originalAssetData, System.Text.Encoding.UTF8.GetBytes("TEXA"), index + 1);
                }
            }
        }

    }
}
