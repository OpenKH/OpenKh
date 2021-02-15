using OpenKh.Common;
using OpenKh.Imaging;
using OpenKh.Kh2;
using OpenKh.Kh2.Messages;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using YamlDotNet.Serialization;

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
            public void CopyOriginalFile(string fileName)
            {
                var dstFile = GetDestinationPath(fileName);
                EnsureDirectoryExists(dstFile);

                if (!File.Exists(dstFile))
                {
                    var originalFile = GetOriginalAssetPath(fileName);
                    if (File.Exists(originalFile))
                        File.Copy(originalFile, dstFile);
                }
            }
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

                metadata.Assets.AsParallel().ForAll(assetFile =>
                {
                    context.CopyOriginalFile(assetFile.Name);
                    var dstFile = context.GetDestinationPath(assetFile.Name);

                    using var stream = File.Open(dstFile, FileMode.OpenOrCreate, FileAccess.ReadWrite);
                    PatchFile(context, assetFile, stream);
                });
            }
            catch (Exception ex)
            {
                throw new PatcherException(metadata, ex);
            }
        }

        private static void PatchFile(Context context, AssetFile assetFile, Stream stream)
        {
            if (assetFile == null)
                throw new Exception("Asset file is null.");

            switch (assetFile.Method)
            {
                case "copy":
                    CopyFile(context, assetFile, stream);
                    break;
                case "bar":
                case "binarc":
                    PatchBinarc(context, assetFile, stream);
                    break;
                case "imd":
                case "imgd":
                    CreateImageImd(context, assetFile.Source[0]).Write(stream);
                    break;
                case "imz":
                case "imgz":
                    Imgz.Write(stream, assetFile.Source.Select(x => CreateImageImd(context, x)));
                    break;
                case "fac":
                    Imgd.WriteAsFac(stream, assetFile.Source.Select(x => CreateImageImd(context, x)));
                    break;
                case "kh2msg":
                    PatchKh2Msg(context, assetFile.Source, stream);
                    break;
            }

            stream.SetLength(stream.Position);
        }

        private static void CopyFile(Context context, AssetFile assetFile, Stream stream)
        {
            if (assetFile.Source == null || assetFile.Source.Count == 0)
                throw new Exception($"File '{assetFile.Name}' does not contain any source");

            var srcFile = context.GetSourceModAssetPath(assetFile.Source[0].Name);
            if (!File.Exists(srcFile))
                throw new FileNotFoundException($"The mod does not contain the file {assetFile.Source[0].Name}", srcFile);

            using var srcStream = File.OpenRead(srcFile);
            srcStream.CopyTo(stream);
        }

        private static void PatchBinarc(Context context, AssetFile assetFile, Stream stream)
        {
            var binarc = Bar.IsValid(stream) ? Bar.Read(stream) :
                new Bar()
                {
                    Motionset = assetFile.MotionsetType
                };

            foreach (var file in assetFile.Source)
            {
                if (!Enum.TryParse<Bar.EntryType>(file.Type, true, out var barEntryType))
                    throw new Exception($"BinArc type {file.Type} not recognized");

                var entry = binarc.FirstOrDefault(x => x.Name == file.Name && x.Type == barEntryType);
                if (entry == null)
                {
                    entry = new Bar.Entry
                    {
                        Name = file.Name,
                        Type = barEntryType,
                        Stream = new MemoryStream()
                    };
                    binarc.Add(entry);
                }

                PatchFile(context, file, entry.Stream);
            }

            Bar.Write(stream.SetPosition(0), binarc);
            foreach (var entry in binarc)
                entry.Stream?.Dispose();
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

        private static void PatchKh2Msg(Context context, List<AssetFile> sources, Stream stream)
        {
            var msgs = Msg.IsValid(stream) ? Msg.Read(stream) : new List<Msg.Entry>();

            foreach (var source in sources)
            {
                if (string.IsNullOrEmpty(source.Language))
                    throw new Exception($"No language specified in '{source.Name}'");

                var content = File.ReadAllText(context.GetSourceModAssetPath(source.Name));
                var patchedMsgs = new Deserializer().Deserialize<List<Dictionary<string, string>>>(content);
                foreach (var msg in patchedMsgs)
                {
                    if (!msg.TryGetValue("id", out var strId))
                        throw new Exception($"Source '{source.Name}' contains a message without an ID");
                    if (!ushort.TryParse(strId, out var id))
                        throw new Exception($"Message ID '{strId} in '{source.Name}' must be between 0 and 65535");
                    if (!msg.TryGetValue(source.Language, out var text))
                        continue;

                    var encoder = source.Language switch
                    {
                        "jp" => Encoders.JapaneseSystem,
                        "tr" => Encoders.TurkishSystem,
                        _ => Encoders.InternationalSystem,
                    };

                    var data = encoder.Encode(MsgSerializer.DeserializeText(text).ToList());
                    var originalMsg = msgs.FirstOrDefault(x => x.Id == id);
                    if (originalMsg == null)
                        msgs.Add(new Msg.Entry
                        {
                            Id = id,
                            Data = data
                        });
                    else
                        originalMsg.Data = data;
                }
            }

            Msg.WriteOptimized(stream.SetPosition(0), msgs);
        }
    }
}
