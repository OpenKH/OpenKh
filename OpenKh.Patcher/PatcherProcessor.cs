using OpenKh.Common;
using OpenKh.Imaging;
using OpenKh.Kh2;
using System;
using System.Collections.Generic;
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

        private static void PatchKh2(string originalAssets, string outputDir, string modPath, AssetKh2 assets)
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

        private static void PatchBinary(string outputDir, string modPath, AssetBinary binaryEntry)
        {
            var srcFile = Path.Combine(modPath, binaryEntry.Name);
            var dstFile = Path.Combine(outputDir, binaryEntry.Name);
            var dstDir = Path.GetDirectoryName(dstFile);

            if (!File.Exists(srcFile))
                throw new FileNotFoundException($"The mod does not contain the file {binaryEntry.Name}", srcFile);
            Directory.CreateDirectory(dstDir);
            File.Copy(srcFile, dstFile, true);
        }

        private static void PatchBinArc(string outputDir, string modPath, AssetBinArc assetBinarc)
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
                        srcFile = Path.Combine(modPath, file.Source[0].Name);
                        srcStream = File.OpenRead(srcFile);
                        break;
                    case "image":
                        srcStream = CreateImage(modPath, file.Source, file.Format);
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

        private static Stream CreateImage(string modPath, List<AssetSource> source, string format)
        {
            var stream = new MemoryStream();
            switch (format)
            {
                case "imd":
                case "imgd":
                    CreateImageImd(modPath, source[0]).Write(stream);
                    break;
                case "imz":
                case "imgz":
                    Imgz.Write(stream, source.Select(x => CreateImageImd(modPath, x)));
                    break;
                case "fac":
                    Imgd.WriteAsFac(stream, source.Select(x => CreateImageImd(modPath, x)));
                    break;
                default:
                    stream.Dispose();
                    throw new Exception($"Image format exportation '{format}' not recognized");
            }

            return stream;
        }

        private static Imgd CreateImageImd(string modPath, AssetSource source)
        {
            var srcFile = Path.Combine(modPath, source.Name);
            using var srcStream = File.OpenRead(srcFile);
            if (PngImage.IsValid(srcStream))
            {
                var png = PngImage.Read(srcStream);
                return Imgd.Create(png.Size, png.PixelFormat, png.GetData(), png.GetClut(), source.IsSwizzled);
            }

            throw new Exception($"Image source '{source.Name}' not recognized");
        }
    }
}
