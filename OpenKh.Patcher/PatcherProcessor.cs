using OpenKh.Common;
using OpenKh.Imaging;
using OpenKh.Kh2;
using OpenKh.Kh2.Messages;
using OpenKh.Command.Bdxio.Models;
using OpenKh.Command.Bdxio.Utils;
using System;
using System.Collections.Concurrent;
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

            public static List<string> Regions = new List<string>()
            {
                "us",
                "jp",
                "us",
                "uk",
                "it",
                "sp",
                "gr",
                "fr",
                "fm",
            };

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
            public void CopyOriginalFile(string fileName, string dstFile)
            {
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

        List<string> GamesList = new List<string>()
        {
            "kh2",
            "kh1",
            "bbs",
            "Recom",
        };

        public void Patch(string originalAssets, string outputDir, Metadata metadata, string modBasePath, int platform = 1, bool fastMode = false, IDictionary<string, string> packageMap = null, string LaunchGame = null)
        {
            
            var context = new Context(metadata, originalAssets, modBasePath, outputDir);
            try
            {
                
                if (metadata.Assets == null)
                    throw new Exception("No assets found.");
                if (metadata.Game != null && GamesList.Contains(metadata.Game.ToLower()) && metadata.Game.ToLower() != LaunchGame.ToLower())
                    return;

                metadata.Assets.AsParallel().ForAll(assetFile =>
                {
                    var names = new List<string>();
                    names.Add(assetFile.Name);
                    if (assetFile.Multi != null)
                        names.AddRange(assetFile.Multi.Select(x => x.Name).Where(x => !string.IsNullOrEmpty(x)));

                    foreach (var name in names)
                    {
                        if (assetFile.Platform == null)
                            assetFile.Platform = "both";

                        if (assetFile.Required && !File.Exists(context.GetOriginalAssetPath(name)))
                            continue;

                        string _packageFile = null;
                        switch (LaunchGame)
                        {
                            case "kh1":
                                _packageFile = assetFile.Package != null && !fastMode ? assetFile.Package : "kh1_first";
                                break;
                            case "bbs":
                                _packageFile = assetFile.Package != null && !fastMode ? assetFile.Package : "bbs_first";
                                break;
                            case "Recom":
                                if (assetFile!= null)
                                _packageFile = "Recom";
                                break;
                            default:
                                _packageFile = assetFile.Package != null && !fastMode ? assetFile.Package : "kh2_first";
                                break;
                        }

                        var dstFile = context.GetDestinationPath(name);
                        var packageMapLocation = "";
                        var _pcFile = name.Contains("remastered") || name.Contains("raw");

                        // Special case for mods that want to bundle scripts or DLL files
                        var nonGameFile = name.StartsWith("scripts/") || name.StartsWith("scripts\\") || name.StartsWith("dlls/") || name.StartsWith("dlls\\");

                        var _extraPath = _pcFile ? "" : "original/";

                        if (nonGameFile)
                        {
                            packageMapLocation = "special/" + name;
                        }
                        else
                        {
                            switch (platform)
                            {
                                default:
                                {
                                    if (assetFile.Platform.ToLower() == "pc")
                                        continue;

                                    else if (_pcFile)
                                        continue;
                                }
                                break;

                                case 2:
                                {
                                    if (assetFile.Platform.ToLower() == "ps2")
                                        continue;

                                    if (assetFile.Platform.ToLower() != "ps2")
                                        packageMapLocation = _packageFile + "/" + _extraPath + name;

                                    else if (_pcFile)
                                        packageMapLocation = _packageFile + "/" + name;
                                }
                                break;
                            }
                        }

                        if (packageMap != null && packageMapLocation.Length > 0)
                        {
                            // Protect against multiple mods having the same file where one uses forward slash and one uses backslash
                            packageMap[name.Replace("\\", "/")] = packageMapLocation;
                        }

                        context.EnsureDirectoryExists(dstFile);

                        try
                        {
                            context.CopyOriginalFile(name, dstFile);

                            using var _stream = File.Open(dstFile, FileMode.OpenOrCreate, FileAccess.ReadWrite);
                            PatchFile(context, assetFile, _stream);

                            _stream.Close();
                            _stream.Dispose();
                        }

                        catch (IOException) { }
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
                case "bdscript":
                    PatchBdscript(context, assetFile, stream);
                    break;
                case "spawnpoint":
                    PatchSpawnPoint(context, assetFile, stream);
                    break;
                case "listpatch":
                    PatchList(context, assetFile.Source, stream);
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

            string srcFile; 

            if (assetFile.Source[0].Type == "internal")
            {
                srcFile = context.GetOriginalAssetPath(assetFile.Source[0].Name);
            }
            else
            {
                srcFile = context.GetSourceModAssetPath(assetFile.Source[0].Name);
                if (!File.Exists(srcFile))
                    throw new FileNotFoundException($"The mod does not contain the file {assetFile.Source[0].Name}", srcFile);
            }
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
                try
                {
                    var png = PngImage.Read(srcStream);
                    return Imgd.Create(png.Size, png.PixelFormat, png.GetData(), png.GetClut(), source.IsSwizzled);
                }
                catch (Exception ex)
                {
                    throw new Exception($"Unable to decode the PNG '{source.Name}': {ex.Message}");
                }
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
                        "je" => Encoders.JapaneseEvent,
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

        private static void PatchBdscript(Context context, AssetFile assetFile, Stream stream)
        {

            if (assetFile.Source == null || assetFile.Source.Count == 0)
                throw new Exception($"File '{assetFile.Name}' does not contain any source");

            var scriptName = assetFile.Source[0].Name;
            var srcFile = context.GetSourceModAssetPath(scriptName);
            if (!File.Exists(srcFile))
                throw new FileNotFoundException($"The mod does not contain the file {scriptName}", srcFile);

 
            var programsInput = File.ReadAllText(context.GetSourceModAssetPath(scriptName));
            var ascii = BdxAsciiModel.ParseText(programsInput);
            var decoder = new BdxEncoder(
                header: new YamlDotNet.Serialization.DeserializerBuilder()
                    .Build()
                    .Deserialize<BdxHeader>(
                        ascii.Header ?? ""
                    ),
                script: ascii.GetLineNumberRetainedScriptBody(),
                scriptName: scriptName,
                loadScript: programsInput => programsInput
                );

            stream.SetPosition(0);
            stream.Write(decoder.Content);

        }

        private static void PatchSpawnPoint(Context context, AssetFile assetFile, Stream stream)
        {
            if (assetFile.Source == null || assetFile.Source.Count == 0)
                throw new Exception($"File '{assetFile.Name}' does not contain any source");

            var srcFile = context.GetSourceModAssetPath(assetFile.Source[0].Name);
            if (!File.Exists(srcFile))
                throw new FileNotFoundException($"The mod does not contain the file {assetFile.Source[0].Name}", srcFile);

            var spawnPoint = Helpers.YamlDeserialize<List<Kh2.Ard.SpawnPoint>>(File.ReadAllText(srcFile));

            Kh2.Ard.SpawnPoint.Write(stream.SetPosition(0), spawnPoint);
        }

        private static readonly Dictionary<string, byte> characterMap = new Dictionary<string, byte>(){
            { "Sora", 1 }, { "Donald", 2 }, { "Goofy", 3 },  { "Mickey", 4 },  { "Auron", 5 }, { "PingMulan",6 }, { "Aladdin", 7 },  { "Sparrow", 8 }, { "Beast", 9 },  { "Jack", 10 },  { "Simba", 11 }, { "Tron", 12 }, { "Riku", 13 }, { "Roxas", 14}, {"Ping", 15} 
        };

        private static readonly IDeserializer deserializer = new DeserializerBuilder().IgnoreUnmatchedProperties().Build();

        private static void PatchList(Context context, List<AssetFile> sources, Stream stream)
        {
            foreach (var source in sources)
            {
                string sourceText = File.ReadAllText(context.GetSourceModAssetPath(source.Name));
                switch (source.Type)
                {
                    case "trsr":
                        var trsrList = Kh2.SystemData.Trsr.Read(stream).ToDictionary(x => x.Id, x => x);
                        var moddedTrsr = deserializer.Deserialize<Dictionary<ushort, Kh2.SystemData.Trsr>>(sourceText);
                        foreach (var treasure in moddedTrsr)
                        {
                            if (trsrList.ContainsKey(treasure.Key))
                            {
                                if (treasure.Value.World == 0)
                                {
                                    trsrList[treasure.Key].ItemId = treasure.Value.ItemId;
                                }
                                else
                                {
                                    trsrList[treasure.Key] = treasure.Value;
                                }
                            }
                            else
                            {
                                trsrList.Add(treasure.Key, treasure.Value);
                            }

                        }
                        Kh2.SystemData.Trsr.Write(stream.SetPosition(0), trsrList.Values);
                        break;

                    case "item":
                        var itemList = Kh2.SystemData.Item.Read(stream);
                        var moddedItem = deserializer.Deserialize<Kh2.SystemData.Item>(sourceText);
                        if (moddedItem.Items != null)
                        {
                            foreach (var item in moddedItem.Items)
                            {
                                var itemToUpdate = itemList.Items.FirstOrDefault(x => x.Id == item.Id);
                                if (itemToUpdate != null)
                                {
                                    itemList.Items[itemList.Items.IndexOf(itemToUpdate)] = item;
                                }
                                else
                                {
                                    itemList.Items.Add(item);
                                }
                            }
                        }
                        if (moddedItem.Stats != null)
                        {
                            foreach (var item in moddedItem.Stats)
                            {
                                var itemToUpdate = itemList.Stats.FirstOrDefault(x => x.Id == item.Id);
                                if (itemToUpdate != null)
                                {
                                    itemList.Stats[itemList.Stats.IndexOf(itemToUpdate)] = item;
                                }
                                else
                                {
                                    itemList.Stats.Add(item);
                                }
                            }
                        }
                        itemList.Write(stream.SetPosition(0));
                        break;

                    case "fmlv":
                        var formRaw = Kh2.Battle.Fmlv.Read(stream).ToList();
                        var formList = new Dictionary<Kh2.Battle.Fmlv.FormFm, List<Kh2.Battle.Fmlv>>();
                        foreach (var form in formRaw)
                        {
                            if (!formList.ContainsKey(form.FinalMixForm))
                            {
                                formList.Add(form.FinalMixForm, new List<Kh2.Battle.Fmlv>());
                            }
                            formList[form.FinalMixForm].Add(form);
                        }
                        var moddedForms = deserializer.Deserialize<Dictionary<Kh2.Battle.Fmlv.FormFm, List<FmlvDTO>>>(sourceText);
                        foreach (var form in moddedForms)
                        {
                            foreach (var level in form.Value)
                            {
                                formList[form.Key][level.FormLevel - 1].Ability = (ushort)level.Ability;
                                formList[form.Key][level.FormLevel - 1].Exp = level.Experience;
                                formList[form.Key][level.FormLevel - 1].Unk1 = level.GrowthAbilityLevel;

                            }

                        }
                        Kh2.Battle.Fmlv.Write(stream.SetPosition(0), formList.Values.SelectMany(x => x).ToList());
                        break;

                    case "lvup":
                        var levelList = Kh2.Battle.Lvup.Read(stream);
                        var moddedLevels = deserializer.Deserialize<Dictionary<string, Dictionary<int, Kh2.Battle.Lvup.PlayableCharacter.Level>>>(sourceText);

                        foreach (var character in moddedLevels)
                        {
                            foreach (var level in moddedLevels[character.Key])
                            {
                                levelList.Characters[characterMap[character.Key] - 1].Levels[level.Key - 1] = moddedLevels[character.Key][level.Key];
                            }
                        }

                        levelList.Write(stream.SetPosition(0));
                        break;

                    case "bons":
                        var bonusRaw = Kh2.Battle.Bons.Read(stream);
                        var bonusList = new Dictionary<byte, Dictionary<string, Kh2.Battle.Bons>>();
                        foreach (var bonus in bonusRaw)
                        {
                            if (!bonusList.ContainsKey(bonus.RewardId))
                            {
                                bonusList.Add(bonus.RewardId, new Dictionary<string, Kh2.Battle.Bons>());
                            }
                            var character = characterMap.First(x => x.Value == bonus.CharacterId).Key;
                            if (!bonusList[bonus.RewardId].ContainsKey(character))
                            {
                                bonusList[bonus.RewardId].Add(character, bonus);
                            }
                        }
                        var moddedBonus = deserializer.Deserialize<Dictionary<byte, Dictionary<string, Kh2.Battle.Bons>>>(sourceText);
                        foreach (var bonus in moddedBonus)
                        {
                            if (!bonusList.ContainsKey(bonus.Key))
                            {
                                bonusList.Add(bonus.Key, new Dictionary<string, Kh2.Battle.Bons>());
                            }
                            foreach (var character in moddedBonus[bonus.Key])
                            {
                                if (!bonusList[bonus.Key].ContainsKey(character.Key))
                                {
                                    bonusList[bonus.Key].Add(character.Key, new Kh2.Battle.Bons());
                                }
                                bonusList[bonus.Key][character.Key] = character.Value;
                            }
                        }

                        Kh2.Battle.Bons.Write(stream.SetPosition(0), bonusList.Values.SelectMany(x => x.Values));
                        break;

                    case "atkp":
                        var atkpList = Kh2.Battle.Atkp.Read(stream);
                        var moddedAtkp = deserializer.Deserialize<List<Kh2.Battle.Atkp>>(sourceText);
                        foreach (var attack in moddedAtkp)
                        {
                            var oldAtkp = atkpList.First(x => x.Id == attack.Id && x.SubId == attack.SubId);
                            atkpList[atkpList.IndexOf(oldAtkp)] = attack;
                        }
                        Kh2.Battle.Atkp.Write(stream.SetPosition(0), atkpList);
                        break;

                    case "objentry":
                        var objEntryList = Kh2.Objentry.Read(stream).ToDictionary(x => x.ObjectId, x => x);
                        var moddedObjEntry = deserializer.Deserialize<Dictionary<uint, Kh2.Objentry>>(sourceText);
                        foreach (var objEntry in moddedObjEntry)
                        {
                            if (objEntryList.ContainsKey(objEntry.Key))
                            {
                                objEntryList[objEntry.Key] = objEntry.Value;
                            }
                            else
                            {
                                objEntryList.Add(objEntry.Key, objEntry.Value);
                            }
                        }
                        Kh2.Objentry.Write(stream.SetPosition(0), objEntryList.Values);
                        break;

                    case "plrp":
                        var plrpList = Kh2.Battle.Plrp.Read(stream);
                        var moddedPlrp = deserializer.Deserialize<List<Kh2.Battle.Plrp>>(sourceText);
                        foreach (var plrp in moddedPlrp)
                        {
                            var oldPlrp = plrpList.First(x => x.Character == plrp.Character && x.Id == plrp.Id);
                            plrpList[plrpList.IndexOf(oldPlrp)] = plrp;
                        }
                        Kh2.Battle.Plrp.Write(stream.SetPosition(0), plrpList);
                        break;

                    case "cmd":
                        var cmdList = Kh2.SystemData.Cmd.Read(stream); 
                        var moddedCmd = deserializer.Deserialize<List<Kh2.SystemData.Cmd>>(sourceText); 
                        foreach (var commands in moddedCmd) 
                        {
                            var oldCommands = cmdList.First(x => x.Id == commands.Id && x.Id == commands.Id);
                            cmdList[cmdList.IndexOf(oldCommands)] = commands;
                        }
                        Kh2.SystemData.Cmd.Write(stream.SetPosition(0), cmdList);
                        break;

                    case "enmp":
                        var enmpList = Kh2.Battle.Enmp.Read(stream);
                        var moddedEnmp = deserializer.Deserialize<List<Kh2.Battle.Enmp>>(sourceText);
                        foreach (var enmp in moddedEnmp)
                        {
                            var oldEnmp = enmpList.First(x => x.Id == enmp.Id);
                            enmpList[enmpList.IndexOf(oldEnmp)] = enmp;
                        }
                        Kh2.Battle.Enmp.Write(stream.SetPosition(0), enmpList);
                        break;
                    case "sklt":
                        var skltList = Kh2.SystemData.Sklt.Read(stream);
                        var moddedSklt = deserializer.Deserialize<List<Kh2.SystemData.Sklt>>(sourceText);
                        foreach (var sklt in moddedSklt)
                        {
                            var oldSklt = skltList.First(x => x.CharacterId == sklt.CharacterId);
                            skltList[skltList.IndexOf(oldSklt)] = sklt;
                        }
                        Kh2.SystemData.Sklt.Write(stream.SetPosition(0), skltList);
                        break;
                       
                    case "przt":
                        var prztList = Kh2.Battle.Przt.Read(stream);
                        var moddedPrzt = deserializer.Deserialize<List<Kh2.Battle.Przt>>(sourceText);
                        foreach (var przt in moddedPrzt)
                        {
                            var oldPrzt = prztList.First(x => x.Id == przt.Id);
                            prztList[prztList.IndexOf(oldPrzt)] = przt;
                        }
                        Kh2.Battle.Przt.Write(stream.SetPosition(0), prztList);
                        break;

                    case "magc":
                        var magcList = Kh2.Battle.Magc.Read(stream); 
                        var moddedMagc = deserializer.Deserialize<List<Kh2.Battle.Magc>>(sourceText); 
                        foreach (var magc in moddedMagc)
                        {
                            var oldMagc = magcList.First(x => x.Id == magc.Id && x.Level == magc.Level);
                            magcList[magcList.IndexOf(oldMagc)] = magc;
                        }
                        Kh2.Battle.Magc.Write(stream.SetPosition(0), magcList);
                        break;

                    default:
                        break;
                }
            }

        }
    }
}
