using OpenKh.Common;
using OpenKh.Imaging;
using OpenKh.Kh2;
using OpenKh.Kh2.Messages;
using System;
using System.Collections.Generic;
using System.Globalization;
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
                        File.Copy(originalFile, dstFile,true);
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
                    var names = new List<string>();
                    names.Add(assetFile.Name);
                    if (assetFile.Multi != null)
                        names.AddRange(assetFile.Multi.Select(x => x.Name).Where(x => !string.IsNullOrEmpty(x)));

                    foreach (var name in names)
                    {
                        if (assetFile.Required && !File.Exists(context.GetOriginalAssetPath(name)))
                            continue;

                        context.CopyOriginalFile(name);
                        var dstFile = context.GetDestinationPath(name);

                        using var stream = File.Open(dstFile, FileMode.OpenOrCreate, FileAccess.ReadWrite);
                        PatchFile(context, assetFile, stream);
                    }
                });
            }
            catch (Exception ex)
            {
                Log.Err($"Patcher failed: {ex.Message}");
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
                    PatchImageImz(context, assetFile, stream);
                    break;
                case "fac":
                    Imgd.WriteAsFac(stream, assetFile.Source.Select(x => CreateImageImd(context, x)));
                    break;
                case "kh2msg":
                    PatchKh2Msg(context, assetFile.Source, stream);
                    break;
                case "areadatascript":
                    PatchAreaDataScript(context, assetFile.Source, stream);
                    break;
                case "listreplace":
                    PatchListReplace(context, assetFile.Source, stream);
                    break;
                default:
                    Log.Warn($"Method '{assetFile.Method}' not recognized for '{assetFile.Name}'. Falling back to 'copy'");
                    CopyFile(context, assetFile, stream);
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
            else if (Imgd.IsValid(srcStream))
                return Imgd.Read(srcStream);

            throw new Exception($"Image source '{source.Name}' not recognized");
        }

        private static void PatchImageImz(Context context, AssetFile assetFile, Stream stream)
        {
            var index = 0;
            var images = Imgz.IsValid(stream) ? Imgz.Read(stream).ToList() : new List<Imgd>();
            foreach (var source in assetFile.Source)
            {
                if (source.Index > 0)
                    index = source.Index;

                var imd = CreateImageImd(context, source);
                if (images.Count <= index)
                    images.Add(imd);
                else
                    images[index] = imd;

                index++;
            }

            Imgz.Write(stream.SetPosition(0), images);
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
                    {
                        if (strId.Length > 2 && strId[1] == 'x')
                        {
                            if (!ushort.TryParse(strId.Substring(2), NumberStyles.HexNumber, null, out id))
                                throw new Exception($"Message ID '{strId} in '{source.Name}' must be between 0 and 65535");
                        }
                        else
                            throw new Exception($"Message ID '{strId} in '{source.Name}' must be between 0 and 65535");
                    }
                    if (!msg.TryGetValue(source.Language, out var text))
                        continue;

                    var encoder = source.Language switch
                    {
                        "jp" => Encoders.JapaneseSystem,
                        "tr" => Encoders.TurkishSystem,
                        _ => Encoders.InternationalSystem,
                    };

                    var data = encoder.Encode(MsgSerializer.DeserializeText(text ?? string.Empty).ToList());
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

        private static void PatchAreaDataScript(Context context, List<AssetFile> sources, Stream stream)
        {
            var scripts = Kh2.Ard.AreaDataScript.IsValid(stream) ?
                Kh2.Ard.AreaDataScript.Read(stream).ToDictionary(x => x.ProgramId, x => x) :
                new Dictionary<short, Kh2.Ard.AreaDataScript>();
            foreach (var source in sources)
            {
                var programsInput = File.ReadAllText(context.GetSourceModAssetPath(source.Name));
                foreach (var newScript in Kh2.Ard.AreaDataScript.Compile(programsInput))
                    scripts[newScript.ProgramId] = newScript;
            }

            Kh2.Ard.AreaDataScript.Write(stream.SetPosition(0), scripts.Values);
        }

        private static readonly Dictionary<string, byte> characterMap = new Dictionary<string, byte>(){
            { "Sora", 1 }, { "Donald", 2 }, { "Goofy", 3 },  { "Mickey", 4 },  { "Auron", 5 }, { "PingMulan",6 }, { "Aladdin", 7 },  { "Sparrow", 8 }, { "Beast", 9 },  { "Jack", 10 },  { "Simba", 11 }, { "Tron", 12 }, { "Riku", 13 }, { "Roxas", 14} 
        };

        private static readonly IDeserializer deserializer = new DeserializerBuilder().IgnoreUnmatchedProperties().Build();
            
        private static void PatchListReplace(Context context, List<AssetFile> sources, Stream stream)
        {
            foreach (var source in sources)
            {
                    string sourceText = File.ReadAllText(context.GetSourceModAssetPath(source.Name));
                    if (source.Name.Contains("enc"))
                    {
                        byte[] data = System.Convert.FromBase64String(sourceText);
                        sourceText = System.Text.ASCIIEncoding.ASCII.GetString(data);
                    }
                    switch (source.Type)
                    {
                        case "trsr":
                            var trsrList = Kh2.SystemData.Trsr.Read(stream).ToDictionary(x => x.Id, x => x);
                            var moddedTrsr = deserializer.Deserialize<Dictionary<int, Kh2.SystemData.Trsr>>(sourceText);
                            foreach (KeyValuePair<int, Kh2.SystemData.Trsr> treasure in moddedTrsr)
                            {
                                trsrList[treasure.Value.Id].ItemId = treasure.Value.ItemId;
                            }
                                Kh2.SystemData.Trsr.Write(stream.SetPosition(0), trsrList.Values);
                            break;

                        case "item":
                            var itemList = Kh2.SystemData.Item.Read(stream);
                            var moddedItem = deserializer.Deserialize<Kh2.SystemData.Item>(sourceText);
                            itemList = moddedItem;
                            itemList.Write(stream.SetPosition(0));
                            break;

                        case "fmlv":
                            var formList = Kh2.Battle.Fmlv.Read(stream).ToDictionary(x => String.Concat(x.FormFm, x.FormLevel), x => x);
                            var moddedForms = deserializer.Deserialize<Dictionary<string, Kh2.Battle.Fmlv.Level>>(sourceText);
                            foreach (KeyValuePair<string, Kh2.Battle.Fmlv.Level> form in moddedForms)
                            {
                                formList[form.Key].Ability = form.Value.Ability;
                                formList[form.Key].LevelGrowthAbility = form.Value.LevelGrowthAbility;
                                formList[form.Key].Exp = form.Value.Exp;
                            }
                            Kh2.Battle.Fmlv.Write(stream.SetPosition(0), formList.Values);
                            break;

                        case "lvup":
                            var levelList = Kh2.Battle.Lvup.Read(stream);
                            var moddedLevels = deserializer.Deserialize<Kh2.Battle.Lvup>(sourceText);
                            int i = 0;
                            foreach (Kh2.Battle.Lvup.PlayableCharacter character in moddedLevels.Characters)
                            {
                                levelList.Characters[i].Levels = character.Levels;
                                i++;
                            }
                            levelList.Write(stream.SetPosition(0));
                            break;

                        case "bons":
                            var bonusList = Kh2.Battle.Bons.Read(stream).ToDictionary(x => String.Concat(x.RewardId, characterMap.FirstOrDefault(y => y.Value == x.CharacterId).Key), x => x);
                            var moddedBonus = deserializer.Deserialize<Dictionary<string, Kh2.Battle.Bons>>(sourceText);
                            foreach (KeyValuePair<string, Kh2.Battle.Bons> bonus in moddedBonus)
                            {
                                bonusList[bonus.Key] = bonus.Value;
                            }
                            Kh2.Battle.Bons.Write(stream.SetPosition(0), bonusList.Values);
                            break;

                        default:
                            break;
                    }
             }
            
        }
    }
}
