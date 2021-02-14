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
        public class Context
        {
            public Metadata Metadata { get; set; }
            public string OriginalAssetPath { get; }
            public string SourceModAssetPath { get; set; }
            public string DestinationPath { get; set; }

            public Context(
                Metadata metadata,
                string originalAssetPath,
                string sourceModAssetPath,
                string destinationPath)
            {
                Metadata = metadata;
                OriginalAssetPath = originalAssetPath;
                SourceModAssetPath = sourceModAssetPath;
                DestinationPath = destinationPath;
            }

            public string GetOriginalAssetPath(string path) => Path.Combine(OriginalAssetPath, path);
            public string GetSourceModAssetPath(string path) => Path.Combine(SourceModAssetPath, path);
            public string GetDestinationPath(string path) => Path.Combine(DestinationPath, path);
            public void EnsureDirectoryExists(string fileName) => Directory.CreateDirectory(Path.GetDirectoryName(fileName));
        }

        public void Patch(string originalAssets, string outputDir, string modFilePath)
        {
            var metadata = File.OpenRead(modFilePath).Using(Metadata.Read);
            var modBasePath = Path.GetDirectoryName(modFilePath);
            Patch(originalAssets, outputDir, metadata, modBasePath);
        }

        public void Patch(string originalAssets, string outputDir, Metadata metadata, string modBasePath)
        {
            var context = new Context(metadata, originalAssets, modBasePath, outputDir);
            try
            {
                if (metadata.Assets == null)
                    throw new Exception("No assets found.");

                metadata.Assets.AsParallel().ForAll(x => PatchFile(context, x));
            }
            catch (Exception ex)
            {
                throw new PatcherException(metadata, ex);
            }
        }

        private static void PatchFile(Context context, AssetFile assetFile)
        {
            if (assetFile == null)
                throw new Exception("Asset file is null.");

            switch (assetFile.Method)
            {
                case "copy":
                    CopyFile(context, assetFile);
                    break;
                case "binarc":
                    ArchiveBar(context, assetFile);
                    break;
            }

            //if (assets.Binaries != null)
            //    foreach (var binary in assets.Binaries)
            //        PatchBinary(outputDir, modPath, binary);

            //if (assets.BinaryArchives != null)
            //{
            //    foreach (var binarc in assets.BinaryArchives)
            //    {
            //        // Copy original asset to patch if not present to the destination directory
            //        var dstFilePath = Path.Combine(outputDir, binarc.Name);
            //        if (!File.Exists(dstFilePath))
            //        {
            //            var srcDir = Path.Combine(originalAssets, binarc.Name);
            //            if (File.Exists(srcDir))
            //            {
            //                var dstDir = Path.GetDirectoryName(dstFilePath);
            //                Directory.CreateDirectory(dstDir);
            //                File.Copy(srcDir, dstFilePath);
            //            }
            //        }

            //        PatchBinArc(outputDir, modPath, binarc);
            //    }
            //}
        }

        private static void CopyFile(Context context, AssetFile assetFile)
        {
            if (assetFile.Source == null || assetFile.Source.Count == 0)
                throw new Exception($"File '{assetFile.Name}' does not contain any source");

            var srcFile = context.GetSourceModAssetPath(assetFile.Source[0].Name);
            if (!File.Exists(srcFile))
                throw new FileNotFoundException($"The mod does not contain the file {assetFile.Source[0].Name}", srcFile);

            var dstFile = context.GetDestinationPath(assetFile.Name);
            context.EnsureDirectoryExists(dstFile);
            File.Copy(srcFile, dstFile, true);
        }

        private static void ArchiveBar(Context context, AssetFile assetFile)
        {
            var dstFile = context.GetDestinationPath(assetFile.Name);
            if (!File.Exists(dstFile))
            {
                var originalFile = context.GetOriginalAssetPath(assetFile.Name);
                if (File.Exists(originalFile))
                {
                    context.EnsureDirectoryExists(dstFile);
                    File.Copy(originalFile, dstFile);
                }
            }

            var binarc = File.Exists(dstFile) ?
                File.OpenRead(dstFile).Using(Bar.Read) :
                new Bar()
                {
                    Motionset = assetFile.MotionsetType
                };

            foreach (var file in assetFile.Source)
            {
                if (!Enum.TryParse<Bar.EntryType>(file.Type, true, out var barEntryType))
                    throw new Exception($"BinArc type {file.Type} not recognized");

                string srcFile;
                Stream srcStream;
                switch (file.Method)
                {
                    case "copy":
                        srcFile = context.GetSourceModAssetPath(file.Source[0].Name);
                        srcStream = File.OpenRead(srcFile);
                        break;
                    case "image":
                        srcStream = CreateImage(context, file.Source, file.Type);
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

        private static Stream CreateImage(Context context, List<AssetFile> source, string format)
        {
            var stream = new MemoryStream();
            switch (format)
            {
                case "imd":
                case "imgd":
                    CreateImageImd(context, source[0]).Write(stream);
                    break;
                case "imz":
                case "imgz":
                    Imgz.Write(stream, source.Select(x => CreateImageImd(context, x)));
                    break;
                case "fac":
                    Imgd.WriteAsFac(stream, source.Select(x => CreateImageImd(context, x)));
                    break;
                default:
                    stream.Dispose();
                    throw new Exception($"Image format exportation '{format}' not recognized");
            }

            return stream;
        }

        private static Imgd CreateImageImd(Context context, AssetFile source)
        {
            var srcFile = context.GetSourceModAssetPath(source.Name);
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
