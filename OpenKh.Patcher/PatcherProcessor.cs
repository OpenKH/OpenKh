using OpenKh.Common;
using OpenKh.Kh2;
using System;
using System.IO;
using System.Linq;

namespace OpenKh.Patcher
{
    public class PatcherProcessor
    {
        public void Patch(string originalAssets, string outputDir, string modFilePath)
        {
            var metadata = File.OpenRead(modFilePath).Using(Metadata.Read);
            var modBasePath = Path.GetDirectoryName(modFilePath);
            Patch(originalAssets, outputDir, metadata, modBasePath);
        }

        public void Patch(string originalAssets, string outputDir, Metadata metadata, string modBasePath)
        {
            try
            {
                if (metadata.Assets == null)
                    throw new Exception("No assets found.");

                PatchKh2(originalAssets, outputDir, modBasePath, metadata.Assets.Kh2);
            }
            catch (Exception ex)
            {
                throw new PatcherException(metadata, ex);
            }
        }

        private void PatchKh2(string originalAssets, string outputDir, string modPath, AssetKh2 assets)
        {
            if (assets == null)
                throw new Exception("No Kingdom Hearts II assets found.");

            if (assets.Binaries != null)
                foreach (var binary in assets.Binaries)
                    PatchBinary(outputDir, modPath, binary);

            if (assets.BinaryArchives != null)
            {
                foreach (var binarc in assets.BinaryArchives)
                {
                    // Copy original asset to patch if not present to the destination directory
                    var dstFilePath = Path.Combine(outputDir, binarc.Name);
                    if (!File.Exists(dstFilePath))
                    {
                        var srcDir = Path.Combine(originalAssets, binarc.Name);
                        if (File.Exists(srcDir))
                        {
                            var dstDir = Path.GetDirectoryName(dstFilePath);
                            Directory.CreateDirectory(dstDir);
                            File.Copy(srcDir, dstFilePath);
                        }
                    }

                    PatchBinArc(outputDir, modPath, binarc);
                }
            }
        }

        private void PatchBinary(string outputDir, string modPath, AssetBinary binaryEntry)
        {
            var srcFile = Path.Combine(modPath, binaryEntry.Name);
            var dstFile = Path.Combine(outputDir, binaryEntry.Name);
            var dstDir = Path.GetDirectoryName(dstFile);

            if (!File.Exists(srcFile))
                throw new FileNotFoundException($"The mod does not contain the file {binaryEntry.Name}", srcFile);
            Directory.CreateDirectory(dstDir);
            File.Copy(srcFile, dstFile, true);
        }

        private void PatchBinArc(string outputDir, string modPath, AssetBinArc assetBinarc)
        {
            var dstFile = Path.Combine(outputDir, assetBinarc.Name);
            var binarc = File.Exists(dstFile) ?
                File.OpenRead(dstFile).Using(Bar.Read) :
                new Bar()
                {
                    Motionset = assetBinarc.MotionsetType
                };

            foreach (var file in assetBinarc.Entries)
            {
                if (!Enum.TryParse<Bar.EntryType>(file.Format, true, out var barEntryType))
                    throw new Exception($"BinArc type {file.Format} not recognized");

                string srcFile;
                Stream srcStream;
                switch (file.Method)
                {
                    case "copy":
                        srcFile = Path.Combine(modPath, file.Source.Name);
                        srcStream = File.OpenRead(srcFile);
                        break;
                    default:
                        throw new Exception($"Patching method {file.Method} not recognized");
                }

                var existingEntry = binarc.FirstOrDefault(x => x.Name == file.Name && x.Type == barEntryType);
                if (existingEntry == null)
                {
                    binarc.Add(new Bar.Entry
                    {
                        Name = file.Name,
                        Type = barEntryType,
                        Stream = srcStream
                    });
                }
                else
                {
                    existingEntry.Stream = srcStream;
                }
            }

            var dstDir = Path.GetDirectoryName(dstFile);
            Directory.CreateDirectory(dstDir);

            File.Create(dstFile).Using(stream => Bar.Write(stream, binarc));

            foreach (var entry in binarc)
                entry.Stream?.Dispose();
        }
    }
}
