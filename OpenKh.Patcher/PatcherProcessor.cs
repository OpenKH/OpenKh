using OpenKh.Bbs;
using OpenKh.Command.Bdxio.Models;
using OpenKh.Command.Bdxio.Utils;
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
using static OpenKh.Kh2.SystemData.Shop;

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

        /// <param name="platform">(GameEdition) 0=OpenKH, 1=PCSX2-EX, 2=PC</param>
        /// <param name="fastMode">If true, always the first package file (kh1_first, bbs_first, kh2_first or such) is selected.</param>
        /// <param name="Tests">If true, always invoke context.CopyOriginalFile</param>
        /// <param name="LaunchGame">(GameId) "kh1", "kh2", "bbs", "Recom", "kh3d"</param>
        /// <param name="Language">en, jp</param>
        public void Patch(
            string originalAssets,
            string outputDir,
            Metadata metadata,
            string modBasePath,
            int platform = 1,
            bool fastMode = false,
            IDictionary<string, string> packageMap = null,
            string LaunchGame = null,
            string Language = "en",
            bool Tests = false,
            Dictionary<string, bool> collectionOptionalEnabledMods = null
        )
        {
            if (collectionOptionalEnabledMods == null)
                collectionOptionalEnabledMods = new Dictionary<string, bool> { };
            var context = new Context(metadata, originalAssets, modBasePath, outputDir);
            try
            {

                if (metadata.Assets == null)
                    throw new Exception("No assets found.");
                if (metadata.Game != null && GamesList.Contains(metadata.Game.ToLower()) && metadata.Game.ToLower() != LaunchGame.ToLower())
                    return;
                if (metadata.IsCollection && !metadata.CollectionGames.Contains(LaunchGame))
                    return;

                var exclusiveLock = new object();
                metadata.Assets.AsParallel().ForAll(assetFile =>
                {
                    if (assetFile.Game != null && assetFile.Game != LaunchGame)
                        return;
                    if (assetFile.CollectionOptional == true)
                        if (!collectionOptionalEnabledMods.ContainsKey(assetFile.Name))
                            return;
                        else if (!collectionOptionalEnabledMods[assetFile.Name])
                            return;
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
                                if (assetFile != null)
                                    _packageFile = "Recom";
                                break;
                            default:
                                _packageFile = assetFile.Package != null && !fastMode ? assetFile.Package : "kh2_first";
                                break;
                        }

                        if (Path.IsPathRooted(name) && !Path.GetPathRoot(name).Equals(Path.DirectorySeparatorChar.ToString(), StringComparison.Ordinal))
                        {
                            Log.Err($"File Path \"" + name + "\" cannot be copied as it is rooted and can cause instability! Aborting...");
                            throw new PatcherException(metadata, new InvalidOperationException("Root Copy Detected!"));
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
                            lock (exclusiveLock)
                            {
                                packageMap[name.Replace("\\", "/")] = packageMapLocation;
                            }
                        }

                        context.EnsureDirectoryExists(dstFile);

                        //Update: Prevent from copying a blank file should it not exist.
                        try
                        {
                            bool multi = false;
                            if (assetFile.Multi != null)
                            {
                                foreach (var entry in assetFile.Multi)
                                {
                                    if (File.Exists(context.GetOriginalAssetPath(entry.Name)))
                                    {
                                        multi = true;
                                    }
                                }
                            }
                            //If editing subfiles (not Method: copy and not Method: imd)  make sure the original file exists OR
                            //If copying a file from the mod (NOT Type: internal) make sure it exists (doesnt check if the location its going normally exists) OR
                            //If copying a file from the users extraction (Type: internal) make sure it exists (doesnt check if the location its going normally exists) OR
                            //Ignore if its from a test

                            var needToCopyOriginalFile =
                                (false
                                || (true
                                    && assetFile.Method != "copy"
                                    && assetFile.Method != "imd"
                                    && (false
                                        || File.Exists(context.GetOriginalAssetPath(assetFile.Name))
                                        || multi
                                    )
                                )
                                || (true
                                    && (assetFile.Method == "copy" || assetFile.Method == "imd")
                                    && assetFile.Source[0].Type != "internal"
                                    && File.Exists(context.GetSourceModAssetPath(assetFile.Source[0].Name))
                                )
                                || (true
                                    && (assetFile.Method == "copy" || assetFile.Method == "imd")
                                    && assetFile.Source[0].Type == "internal"
                                    && (false
                                        || File.Exists(context.GetOriginalAssetPath(assetFile.Source[0].Name))
                                        || multi
                                    )
                                )
                                || Tests
                                );

                            if (needToCopyOriginalFile)
                            {
                                context.CopyOriginalFile(name, dstFile);

                                using var _stream = File.Open(dstFile, FileMode.OpenOrCreate, FileAccess.ReadWrite);
                                PatchFile(context, assetFile, _stream);

                                _stream.Close();
                                _stream.Dispose();
                            }
                            else
                            {
                                // The following codes are for validation purposes only.

                                List<string> globalFilePaths = new List<string> { ".a.fr", ".a.gr", ".a.it", ".a.sp", ".a.us", "/fr/", "/gr/", "/it/", "/sp/", "/us/" };
                                if (assetFile.Method != "copy" && assetFile.Method != "imd")
                                {
                                    if (platform == 2)
                                    {
                                        if (Language != "jp")
                                        {
                                            if (!context.GetOriginalAssetPath(name).Contains(".a.fm") && !context.GetOriginalAssetPath(name).Contains("/jp/"))
                                            {
                                                Log.Warn("File not found: " + context.GetOriginalAssetPath(name) + " Skipping. \nPlease check your game extraction.");
                                            }
                                        }
                                        else
                                        {
                                            if (!globalFilePaths.Any(x => context.GetOriginalAssetPath(name).Contains(x)))
                                            {
                                                Log.Warn("File not found: " + context.GetOriginalAssetPath(name) + " Skipping. \nPlease check your game extraction.");
                                            }
                                        }
                                    }
                                    else
                                    {
                                        Log.Warn("File not found: " + context.GetOriginalAssetPath(name) + " Skipping. \nPlease check your game extraction.");
                                    }
                                }
                                else if (assetFile.Source[0].Type == "internal")
                                {
                                    if (platform == 2)
                                    {
                                        if (Language != "jp")
                                        {
                                            if (!context.GetOriginalAssetPath(assetFile.Source[0].Name).Contains(".a.fm") && !context.GetOriginalAssetPath(assetFile.Source[0].Name).Contains("/jp/") && (assetFile.Multi == null || assetFile.Name == name))
                                            {
                                                Log.Warn("File not found: " + context.GetOriginalAssetPath(assetFile.Source[0].Name) + " Skipping. \nPlease check your game extraction.");
                                            }
                                        }
                                        else
                                        {
                                            if (!globalFilePaths.Any(x => context.GetOriginalAssetPath(assetFile.Source[0].Name).Contains(x)) && (assetFile.Multi == null || assetFile.Name == name))
                                            {
                                                Log.Warn("File not found: " + context.GetOriginalAssetPath(assetFile.Source[0].Name) + " Skipping. \nPlease check your game extraction.");
                                            }
                                        }
                                    }
                                    else
                                    {
                                        Log.Warn("File not found: " + context.GetOriginalAssetPath(assetFile.Source[0].Name) + " Skipping. \nPlease check your game extraction.");
                                    }
                                }
                                else
                                {
                                    Log.Warn("File not found: " + context.GetSourceModAssetPath(assetFile.Source[0].Name) + " Skipping. \nPlease check your mod install of: " + metadata.Title);
                                }
                            }
                        }
                        catch (IOException) { }
                        //This is here so the user does not have to close Mod Manager to see what the warnings were if any. Helpful especially on PC since the build window closes after build unlike emulator where it stays open during mod injection.
                        Log.Flush();
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
                case "bbsarc":
                    PatchBBSArc(context, assetFile, stream);
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
                case "synthpatch":
                    PatchSynth(context, assetFile.Source, stream);
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




        //Binarc Update: Specify by Name OR Index. Some files in BARS may have the same name but different indexes, and you want to patch a later index only.
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

                Bar.Entry entry = null;

                // Check if the name is specified
                if (!string.IsNullOrEmpty(file.Name))
                {
                    entry = binarc.FirstOrDefault(x => x.Name == file.Name && x.Type == barEntryType);
                }
                // If name is not specified but index is
                else if (file.Index >= 0 && file.Index < binarc.Count)
                {
                    entry = binarc[file.Index];
                }

                //If entry is not found by name or index, create a new one
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

        private static void PatchBBSArc(Context context, AssetFile assetFile, Stream stream)
        {
            var entryList = Arc.IsValid(stream) ? Arc.Read(stream).ToList() : new List<Arc.Entry>();
            foreach (var file in assetFile.Source)
            {
                var entry = entryList.FirstOrDefault(e => e.Name == file.Name);

                if (entry == null)
                {
                    entry = new Arc.Entry()
                    {
                        Name = file.Name
                    };
                    entryList.Add(entry);
                }
                else if (entry.IsLink)
                {
                    throw new Exception("Cannot patch an arc link!");
                }

                MemoryStream data = new MemoryStream();
                if (entry.Data != null)
                {
                    data.Write(entry.Data);
                    data.SetPosition(0);
                }
                PatchFile(context, file, data);
                entry.Data = data.ToArray();
            }

            OpenKh.Bbs.Arc.Write(entryList, stream.SetPosition(0));
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

            var spawnPoint = OpenKh.Common.Helpers.YamlDeserialize<List<Kh2.Ard.SpawnPoint>>(File.ReadAllText(srcFile));

            Kh2.Ard.SpawnPoint.Write(stream.SetPosition(0), spawnPoint);
        }

        private static readonly Dictionary<string, byte> characterMap = new Dictionary<string, byte>(){
            { "Sora", 1 }, { "Donald", 2 }, { "Goofy", 3 },  { "Mickey", 4 },  { "Auron", 5 }, { "PingMulan",6 }, { "Aladdin", 7 },  { "Sparrow", 8 }, { "Beast", 9 },  { "Jack", 10 },  { "Simba", 11 }, { "Tron", 12 }, { "Riku", 13 }, { "Roxas", 14}, {"Ping", 15}
        };

        private static readonly Dictionary<string, byte> worldIndexMap = new Dictionary<string, byte>(StringComparer.OrdinalIgnoreCase){
    { "worldzz", 0 }, { "endofsea", 1 }, { "twilighttown", 2 },  { "destinyisland", 3 },  { "hollowbastion", 4 }, { "beastscastle", 5 }, { "olympuscoliseum", 6 },  { "agrabah", 7 }, { "thelandofdragons", 8 },  { "100acrewood", 9 },  { "prideland", 10 }, { "atlantica", 11 }, { "disneycastle", 12 }, { "timelessriver", 13}, {"halloweentown", 14}, { "worldmap", 15 }, { "portroyal", 16 }, { "spaceparanoids", 17 }, { "theworldthatneverwas", 18 }
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
                                    if (item.InsertBefore != 0) // Prioritize InsertBefore
                                    {
                                        var index = itemList.Items.FindIndex(x => x.Id == item.InsertBefore);
                                        if (index >= 0)
                                        {
                                            itemList.Items.Insert(index, item);
                                            continue;
                                        }
                                    }
                                    else if (item.InsertAfter != 0) // If InsertBefore not set, check InsertAfter
                                    {
                                        var index = itemList.Items.FindIndex(x => x.Id == item.InsertAfter);
                                        if (index >= 0)
                                        {
                                            itemList.Items.Insert(index + 1, item);
                                            continue;
                                        }
                                    }

                                    // Default case: append
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
                            //Same general template used for cmd, enmp, and przt.
                            // Check if the attack exists in atkpList based on Id, SubId, and Switch
                            var existingAttack = atkpList.FirstOrDefault(x => x.Id == attack.Id && x.SubId == attack.SubId && x.Switch == attack.Switch);

                            if (existingAttack != null)
                            {
                                // Update existing attack in atkpList
                                atkpList[atkpList.IndexOf(existingAttack)] = attack;
                            }
                            else
                            {
                                // Add the attack to atkpList if it doesn't exist
                                atkpList.Add(attack);
                            }
                        }

                        Kh2.Battle.Atkp.Write(stream.SetPosition(0), atkpList);
                        break;

                    case "lvpm":
                        var lvpmList = Kh2.Battle.Lvpm.Read(stream);
                        var moddedLvpm = deserializer.Deserialize<List<Kh2.Battle.LvpmHelper>>(sourceText);
                        var helperList = Kh2.Battle.LvpmHelper.ConvertLvpmListToHelper(lvpmList);

                        foreach (var level in moddedLvpm)
                        {
                            var oldLvpm = helperList.First(x => x.Level == level.Level);
                            lvpmList[helperList.IndexOf(oldLvpm)] = Kh2.Battle.LvpmHelper.ConvertLvpmHelperToLvpm(level);
                        }
                        Kh2.Battle.Lvpm.Write(stream.SetPosition(0), lvpmList);
                        break;

                    case "objentry":
                        var objEntryList = Kh2.Objentry.Read(stream).ToDictionary(x => x.ObjectId, x => x);
                        var moddedObjEntry = deserializer.Deserialize<Dictionary<uint, Kh2.Objentry>>(sourceText);

                        foreach (var objEntry in moddedObjEntry)
                        {
                            objEntryList[objEntry.Key] = objEntry.Value; // Add or overwrite
                        }

                        // Sort final list by ObjId before writing
                        var sortedEntries = objEntryList.Values.OrderBy(x => x.ObjectId).ToList();

                        Kh2.Objentry.Write(stream.SetPosition(0), sortedEntries);
                        break;

                    case "plrp":
                        var plrpList = Kh2.Battle.Plrp.Read(stream);
                        var moddedPlrp = deserializer.Deserialize<List<Kh2.Battle.Plrp>>(sourceText);
                        foreach (var plrp in moddedPlrp)
                        {
                            var oldPlrp = plrpList.First(x => x.Character == plrp.Character && x.Id == plrp.Id);
                            if (oldPlrp != null)
                            {
                                plrpList[plrpList.IndexOf(oldPlrp)] = plrp;
                            }
                            else
                            {
                                plrpList.Add(plrp);
                            }
                        }
                        Kh2.Battle.Plrp.Write(stream.SetPosition(0), plrpList);
                        break;

                    case "cmd":
                        var cmdList = Kh2.SystemData.Cmd.Read(stream);
                        var moddedCmd = deserializer.Deserialize<List<Kh2.SystemData.Cmd>>(sourceText);

                        foreach (var commands in moddedCmd)
                        {
                            var existingCommand = cmdList.FirstOrDefault(x => x.Id == commands.Id);

                            if (existingCommand != null)
                            {
                                cmdList[cmdList.IndexOf(existingCommand)] = commands;
                            }
                            else
                            {
                                cmdList.Add(commands);
                            }
                        }

                        Kh2.SystemData.Cmd.Write(stream.SetPosition(0), cmdList);
                        break;

                    case "localset":
                        var localList = Kh2.Localset.Read(stream);
                        var moddedLocal = deserializer.Deserialize<List<Kh2.Localset>>(sourceText);

                        foreach (var set in moddedLocal)
                        {
                            var existingSet = localList.FirstOrDefault(x => x.ProgramId == set.ProgramId);

                            if (existingSet != null)
                            {
                                localList[localList.IndexOf(existingSet)] = set;
                            }
                            else
                            {
                                localList.Add(set);
                            }
                        }

                        Kh2.Localset.Write(stream.SetPosition(0), localList);
                        break;

                    case "jigsaw":
                        var jigsawList = Kh2.Jigsaw.Read(stream);
                        var moddedJigsaw = deserializer.Deserialize<List<Kh2.Jigsaw>>(sourceText);

                        foreach (var piece in moddedJigsaw)
                        {
                            //Allow variations of capitalizations with World spellings; can't handle an extra space unfortunately, making it inconsistent with Arif.
                            //Can just use ID's for worlds, though.
                            if (worldIndexMap.TryGetValue(piece.World.ToString().Replace(" ", "").ToLower(), out var worldValue))
                            {
                                piece.World = (Kh2.Jigsaw.WorldList)worldValue;
                            }

                            var existingPiece = jigsawList.FirstOrDefault(x => x.Picture == piece.Picture && x.Part == piece.Part); //Identify a puzzle by its Picture+Part.

                            if (existingPiece != null)
                            {
                                jigsawList[jigsawList.IndexOf(existingPiece)] = piece;
                            }
                            else
                            {
                                jigsawList.Add(piece);
                            }
                        }

                        Kh2.Jigsaw.Write(stream.SetPosition(0), jigsawList);
                        break;



                    case "enmp":
                        var enmpList = Kh2.Battle.Enmp.Read(stream);
                        var moddedEnmp = deserializer.Deserialize<List<Kh2.Battle.Enmp>>(sourceText);

                        foreach (var enmp in moddedEnmp)
                        {
                            var existingEnmp = enmpList.FirstOrDefault(x => x.Id == enmp.Id);

                            if (existingEnmp != null)
                            {
                                enmpList[enmpList.IndexOf(existingEnmp)] = enmp;
                            }
                            else
                            {
                                enmpList.Add(enmp);
                            }
                        }

                        Kh2.Battle.Enmp.Write(stream.SetPosition(0), enmpList);
                        break;

                    case "shop":
                        var shop = Kh2.SystemData.Shop.Read(stream);
                        var moddedShop = deserializer.Deserialize<Kh2.SystemData.Shop.ShopHelper>(sourceText);
                        ushort inventoriesBaseOffset = (ushort)(Kh2.SystemData.Shop.HeaderSize + shop.ShopEntries.Count * Kh2.SystemData.Shop.ShopEntrySize);
                        ushort productsBaseOffset = (ushort)(inventoriesBaseOffset + shop.InventoryEntries.Count * Kh2.SystemData.Shop.InventoryEntrySize);
                        List<Kh2.SystemData.Shop.ShopEntryHelper> moddedShopEntryHelpers = shop.ShopEntries.Select(x => x.ToShopEntryHelper(inventoriesBaseOffset)).ToList();
                        if (moddedShop?.ShopEntryHelpers != null)
                        {
                            foreach (var shopEntryHelper in moddedShop.ShopEntryHelpers)
                            {
                                int entryIndex = moddedShopEntryHelpers.FindIndex(x => x.ShopID == shopEntryHelper.ShopID);
                                if (entryIndex < 0)
                                {
                                    moddedShopEntryHelpers.Add(shopEntryHelper);
                                }
                                else
                                {
                                    moddedShopEntryHelpers[entryIndex] = shopEntryHelper;
                                }
                            }
                        }
                        inventoriesBaseOffset = (ushort)(Kh2.SystemData.Shop.HeaderSize + moddedShopEntryHelpers.Count * Kh2.SystemData.Shop.ShopEntrySize);
                        shop.ShopEntries = moddedShopEntryHelpers.Select(x => x.ToShopEntry(inventoriesBaseOffset)).ToList();
                        List<Kh2.SystemData.Shop.InventoryEntryHelper> moddedInventoryEntryHelpers = shop.InventoryEntries.Select((x, index) => 
                            x.ToInventoryEntryHelper(index, productsBaseOffset)
                        ).ToList();
                        if (moddedShop?.InventoryEntryHelpers != null)
                        {
                            foreach (var inventoryEntryHelper in moddedShop.InventoryEntryHelpers)
                            {
                                int invIndex = inventoryEntryHelper.InventoryIndex;
                                if (invIndex < 0)
                                    throw new InvalidDataException($"Shop listpatch: InventoryIndex {invIndex} is an invalid index.");
                                if (invIndex >= moddedInventoryEntryHelpers.Count)
                                {
                                    int dummiesToAdd = invIndex - moddedInventoryEntryHelpers.Count;
                                    for (int i = 0; i < dummiesToAdd; i++)
                                    {
                                        moddedInventoryEntryHelpers.Add(new Kh2.SystemData.Shop.InventoryEntryHelper {
                                            InventoryIndex = moddedInventoryEntryHelpers.Count + i,
                                            UnlockEventID = 0,
                                            ProductCount = 0,
                                            ProductStartIndex = 0
                                        });
                                    }
                                    moddedInventoryEntryHelpers.Add(inventoryEntryHelper);
                                }
                                else
                                {
                                    moddedInventoryEntryHelpers[invIndex] = inventoryEntryHelper;
                                }
                            }
                        }
                        productsBaseOffset = (ushort)(inventoriesBaseOffset + moddedInventoryEntryHelpers.Count * Kh2.SystemData.Shop.InventoryEntrySize);
                        shop.InventoryEntries = moddedInventoryEntryHelpers.Select(x => x.ToInventoryEntry(productsBaseOffset)).ToList();
                        List<Kh2.SystemData.Shop.ProductEntryHelper> moddedProductEntryHelpers = shop.ProductEntries.Select((x, index) => x.ToProductEntryHelper(index)).ToList();
                        if (moddedShop?.ProductEntryHelpers != null)
                        {
                            foreach (var productEntryHelper in moddedShop.ProductEntryHelpers)
                            {
                                int prodIndex = productEntryHelper.ProductIndex;
                                if (prodIndex < 0)
                                    throw new InvalidDataException($"Shop listpatch: ProductIndex {prodIndex} is an invalid index.");
                                if (prodIndex >= moddedProductEntryHelpers.Count)
                                {
                                    int dummiesToAdd = prodIndex - moddedProductEntryHelpers.Count;
                                    for (int i = 0; i < dummiesToAdd; i++)
                                    {
                                        moddedProductEntryHelpers.Add(new Kh2.SystemData.Shop.ProductEntryHelper
                                        {
                                            ProductIndex = moddedProductEntryHelpers.Count + i,
                                            ItemID = 0
                                        });
                                    }
                                    moddedProductEntryHelpers.Add(productEntryHelper);
                                }
                                else
                                {
                                    moddedProductEntryHelpers[prodIndex] = productEntryHelper;
                                }
                            }
                        }
                        shop.ProductEntries = moddedProductEntryHelpers.Select(x => x.ToProductEntry()).ToList();
                        List<Kh2.SystemData.Shop.ProductEntryHelper> moddedValidProductEntryHelpers = shop.ValidProductEntries.Select((x, index) => x.ToProductEntryHelper(index)).ToList();
                        if (moddedShop?.ValidProductEntryHelpers != null)
                        {
                            foreach (var productEntryHelper in moddedShop.ValidProductEntryHelpers)
                            {
                                int prodIndex = productEntryHelper.ProductIndex;
                                if (prodIndex < 0)
                                    throw new InvalidDataException($"Shop listpatch: ValidProductIndex {prodIndex} is an invalid index.");
                                if (prodIndex >= moddedValidProductEntryHelpers.Count)
                                {
                                    int dummiesToAdd = prodIndex - moddedValidProductEntryHelpers.Count;
                                    for (int i = 0; i < dummiesToAdd; i++)
                                    {
                                        moddedValidProductEntryHelpers.Add(new Kh2.SystemData.Shop.ProductEntryHelper
                                        {
                                            ProductIndex = moddedValidProductEntryHelpers.Count + i,
                                            ItemID = 0
                                        });
                                    }
                                    moddedValidProductEntryHelpers.Add(productEntryHelper);
                                }
                                else
                                {
                                    moddedValidProductEntryHelpers[prodIndex] = productEntryHelper;
                                }
                            }
                        }
                        shop.ValidProductEntries = moddedValidProductEntryHelpers.Select(x => x.ToProductEntry()).ToList();
                        Kh2.SystemData.Shop.Write(stream.SetPosition(0), shop);
                        break;

                    case "sklt":
                        var skltList = Kh2.SystemData.Sklt.Read(stream);
                        var moddedSklt = deserializer.Deserialize<List<Kh2.SystemData.Sklt>>(sourceText);

                        foreach (var sklt in moddedSklt)
                        {
                            var existingSklt = skltList.FirstOrDefault(x => x.CharacterId == sklt.CharacterId);

                            if (existingSklt != null)
                            {
                                skltList[skltList.IndexOf(existingSklt)] = sklt;
                            }
                            else
                            {
                                skltList.Add(sklt);
                            }
                        }

                        Kh2.SystemData.Sklt.Write(stream.SetPosition(0), skltList);
                        break;

                    case "przt":
                        var prztList = Kh2.Battle.Przt.Read(stream);
                        var moddedPrzt = deserializer.Deserialize<List<Kh2.Battle.Przt>>(sourceText);

                        foreach (var przt in moddedPrzt)
                        {
                            var existingPrzt = prztList.FirstOrDefault(x => x.Id == przt.Id);

                            if (existingPrzt != null)
                            {
                                prztList[prztList.IndexOf(existingPrzt)] = przt;
                            }
                            else
                            {
                                prztList.Add(przt);
                            }
                        }
                        Kh2.Battle.Przt.Write(stream.SetPosition(0), prztList);
                        break;

                    case "magc":
                        var magcList = Kh2.Battle.Magc.Read(stream);
                        var moddedMagc = deserializer.Deserialize<List<Kh2.Battle.Magc>>(sourceText);
                        foreach (var magc in moddedMagc)
                        {
                            var existingMagc = magcList.First(x => x.Id == magc.Id && x.Level == magc.Level);
                            if (existingMagc != null)
                            {
                                //existingMagc = MergeHelper.Merge(existingMagc, magc);
                                magcList[magcList.IndexOf(existingMagc)] = magc;
                            }
                            else
                            {
                                magcList.Add(magc);
                            }
                        }
                        Kh2.Battle.Magc.Write(stream.SetPosition(0), magcList);
                        break;

                    case "btlv":
                        var btlvList = Kh2.Battle.Btlv.Read(stream);
                        var moddedBtlv = deserializer.Deserialize<List<Kh2.Battle.Btlv>>(sourceText);

                        foreach (var btlv in moddedBtlv)
                        {
                            var existingBtlv = btlvList.FirstOrDefault(x => x.Id == btlv.Id);

                            if (existingBtlv != null)
                            {
                                btlvList[btlvList.IndexOf(existingBtlv)] = btlv;
                            }
                            else
                            {
                                btlvList.Add(btlv);
                            }
                        }

                        Kh2.Battle.Btlv.Write(stream.SetPosition(0), btlvList);
                        break;

                    case "vtbl":
                        var vtblList = Kh2.Battle.Vtbl.Read(stream);
                        var moddedVtbl = deserializer.Deserialize<List<Kh2.Battle.Vtbl>>(sourceText);

                        foreach (var vtbl in moddedVtbl)
                        {
                            var existingVtbl = vtblList.FirstOrDefault(x => x.Id == vtbl.Id && x.CharacterId == vtbl.CharacterId);
                            //Search for CharacterID & "Action" ID.
                            if (existingVtbl != null)
                            {
                                vtblList[vtblList.IndexOf(existingVtbl)] = vtbl;

                            }
                            else
                            {
                                vtblList.Add(vtbl);
                            }
                        }

                        Kh2.Battle.Vtbl.Write(stream.SetPosition(0), vtblList);
                        break;

                    //Add new limits. ID found -> Continue.
                    case "limt":
                        var limtList = Kh2.Battle.Limt.Read(stream);
                        var moddedLimt = deserializer.Deserialize<List<Kh2.Battle.Limt>>(sourceText);

                        foreach (var limt in moddedLimt)
                        {
                            var existingLimt = limtList.FirstOrDefault(x => x.Id == limt.Id);

                            if (existingLimt != null)
                            {
                                limtList[limtList.IndexOf(existingLimt)] = limt;
                            }
                            else
                            {
                                limtList.Add(limt);
                            }
                        }
                        Kh2.Battle.Limt.Write(stream.SetPosition(0), limtList);
                        break;

                    //Listpatch for Arif. Takes a string as an input for the WorldIndex to use, and ints for the roomIndex to edit.
                    //Strings do not need to worry about capitalization, etc. so long as they have the same characters.
                    case "arif":
                        var originalData = Kh2.SystemData.Arif.Read(stream);
                        var patches = deserializer.Deserialize<Dictionary<string, Dictionary<int, Kh2.SystemData.Arif>>>(sourceText);

                        foreach (var worldPatch in patches)
                        {
                            if (!worldIndexMap.TryGetValue(worldPatch.Key.ToLower().Replace(" ", ""), out var worldIndex))
                            {
                                Log.Warn($"Invalid world index: {worldPatch.Key}");
                            }

                            if (worldIndex >= 0 && worldIndex < originalData.Count)
                            {
                                var worldData = originalData[worldIndex];

                                foreach (var areaPatch in worldPatch.Value)
                                {
                                    int areaIndex = areaPatch.Key;
                                    var patch = areaPatch.Value;

                                    // Add new areas.
                                    while (areaIndex >= worldData.Count)
                                    {
                                        worldData.Add(new Kh2.SystemData.Arif
                                        {
                                            Bgms = new Kh2.SystemData.BgmSet[8],
                                            Reserved = new byte[11]
                                        });

                                        // Initialize the BgmSet elements within the Bgms array
                                        for (int i = 0; i < 8; i++)
                                        {
                                            worldData[worldData.Count - 1].Bgms[i] = new Kh2.SystemData.BgmSet();
                                        }
                                    }
                                    // End of adding new areas.

                                    if (areaIndex >= 0 && areaIndex < worldData.Count)
                                    {
                                        var areaData = worldData[areaIndex];
                                        //Below: Compares each field to see if it's specified in the YML.
                                        //If yes, update w/ YML value.
                                        //If no, retain original value.
                                        areaData.Flags = patch.Flags != 0 ? patch.Flags : areaData.Flags;
                                        areaData.Reverb = patch.Reverb != 0 ? patch.Reverb : areaData.Reverb;
                                        areaData.SoundEffectBank1 = patch.SoundEffectBank1 != 0 ? patch.SoundEffectBank1 : areaData.SoundEffectBank1;
                                        areaData.SoundEffectBank2 = patch.SoundEffectBank2 != 0 ? patch.SoundEffectBank2 : areaData.SoundEffectBank2;
                                        for (int i = 0; i < patch.Bgms.Length && i < areaData.Bgms.Length; i++)
                                        {
                                            areaData.Bgms[i].BgmField = patch.Bgms[i].BgmField != 0 ? (ushort)patch.Bgms[i].BgmField : areaData.Bgms[i].BgmField;
                                            areaData.Bgms[i].BgmBattle = patch.Bgms[i].BgmBattle != 0 ? (ushort)patch.Bgms[i].BgmBattle : areaData.Bgms[i].BgmBattle;
                                        }
                                        areaData.Voice = patch.Voice != 0 ? patch.Voice : areaData.Voice;
                                        areaData.NavigationMapItem = patch.NavigationMapItem != 0 ? patch.NavigationMapItem : areaData.NavigationMapItem;
                                        areaData.Command = patch.Command != 0 ? patch.Command : areaData.Command;
                                        areaData.Reserved = patch.Reserved != null ? patch.Reserved : areaData.Reserved;
                                    }
                                }
                            }
                        }


                        Kh2.SystemData.Arif.Write(stream.SetPosition(0), originalData);
                        break;

                    case "place":
                        var originalPlace = Kh2.Places.Read(stream);
                        var moddedPlace = deserializer.Deserialize<List<Kh2.Places.PlacePatch>>(sourceText);

                        foreach (var place in moddedPlace)
                        {
                            if (place.Index >= 0 && place.Index < originalPlace.Count)
                            {
                                // Update existing entry
                                originalPlace[place.Index].MessageId = place.MessageId;
                                originalPlace[place.Index].Padding = place.Padding;
                            }
                            else if (place.Index == originalPlace.Count)
                            {
                                // Add new entry
                                originalPlace.Add(new Places { MessageId = place.MessageId, Padding = place.Padding });
                            }
                            else
                            {
                                // Expand the list and add the new entry at the specified index
                                while (originalPlace.Count < place.Index)
                                {
                                    originalPlace.Add(new Places { MessageId = 0, Padding = 0 });
                                }
                                originalPlace.Add(new Places { MessageId = place.MessageId, Padding = place.Padding });
                            }
                        }

                        Kh2.Places.Write(stream.SetPosition(0), originalPlace);
                        break;

                    case "soundinfo":
                        var originalSoundInfo = Kh2.Soundinfo.Read(stream);
                        var moddedSoundInfo = deserializer.Deserialize<List<Kh2.Soundinfo.SoundinfoPatch>>(sourceText);

                        foreach (var info in moddedSoundInfo)
                        {
                            while (originalSoundInfo.Count <= info.Index)
                            {
                                originalSoundInfo.Add(new Soundinfo
                                {
                                    Reverb = 0,
                                    Rate = 0,
                                    EnvironmentWAV = 0,
                                    EnvironmentSEB = 0,
                                    EnvironmentNUMBER = 0,
                                    EnvironmentSPOT = 0,
                                    FootstepWAV = 0,
                                    FootstepSORA = 0,
                                    FootstepDONALD = 0,
                                    FootstepGOOFY = 0,
                                    FootstepWORLDFRIEND = 0,
                                    FootstepOTHER = 0
                                });
                            }

                            originalSoundInfo[info.Index].Reverb = info.Reverb;
                            originalSoundInfo[info.Index].Rate = info.Rate;
                            originalSoundInfo[info.Index].EnvironmentWAV = info.EnvironmentWAV;
                            originalSoundInfo[info.Index].EnvironmentSEB = info.EnvironmentSEB;
                            originalSoundInfo[info.Index].EnvironmentNUMBER = info.EnvironmentNUMBER;
                            originalSoundInfo[info.Index].EnvironmentSPOT = info.EnvironmentSPOT;
                            originalSoundInfo[info.Index].FootstepWAV = info.FootstepWAV;
                            originalSoundInfo[info.Index].FootstepSORA = info.FootstepSORA;
                            originalSoundInfo[info.Index].FootstepDONALD = info.FootstepDONALD;
                            originalSoundInfo[info.Index].FootstepGOOFY = info.FootstepGOOFY;
                            originalSoundInfo[info.Index].FootstepWORLDFRIEND = info.FootstepWORLDFRIEND;
                            originalSoundInfo[info.Index].FootstepOTHER = info.FootstepOTHER;
                        }

                        // Write the updated list back to the stream
                        Kh2.Soundinfo.Write(stream.SetPosition(0), originalSoundInfo);
                        break;

                    case "libretto":
                        var originalLibretto = Kh2.Libretto.Read(stream);

                        var patches2 = deserializer.Deserialize<List<Kh2.Libretto.TalkMessagePatch>>(sourceText);

                        foreach (var patch in patches2)
                        {
                            var definition = originalLibretto.Definitions.FirstOrDefault(def => def.TalkMessageId == patch.TalkMessageId);

                            if (definition != null)
                            {
                                definition.Type = patch.Type;

                                var contentList = new List<Libretto.TalkMessageContent>();
                                foreach (var contentPatch in patch.Contents)
                                {
                                    contentList.Add(new Libretto.TalkMessageContent
                                    {
                                        CodeType = contentPatch.CodeType,
                                        Unknown = contentPatch.Unknown,
                                        TextId = contentPatch.TextId
                                    });
                                }
                                originalLibretto.Contents[originalLibretto.Definitions.IndexOf(definition)] = contentList;
                            }
                            else
                            {
                                var newDefinition = new Libretto.TalkMessageDefinition
                                {
                                    TalkMessageId = patch.TalkMessageId,
                                    Type = patch.Type,
                                    ContentPointer = 0 // Will update this later after adding content entries
                                };

                                originalLibretto.Definitions.Add(newDefinition);
                                originalLibretto.Count++;

                                var contentList = new List<Libretto.TalkMessageContent>();
                                foreach (var contentPatch in patch.Contents)
                                {
                                    contentList.Add(new Libretto.TalkMessageContent
                                    {
                                        CodeType = contentPatch.CodeType,
                                        Unknown = contentPatch.Unknown,
                                        TextId = contentPatch.TextId
                                    });
                                }
                                originalLibretto.Contents.Add(contentList);
                            }
                        }

                        stream.Position = 0;
                        Kh2.Libretto.Write(stream, originalLibretto);
                        break;


                    case "memt":
                        var memt = Kh2.SystemData.Memt.Read(stream);
                        var memtEntries = memt.Entries.Cast<Kh2.SystemData.Memt.EntryFinalMix>().ToList();
                        var memtPatches = deserializer.Deserialize<Kh2.SystemData.Memt.MemtPatches>(sourceText);

                        if (memtPatches.MemtEntries != null)
                        {
                            foreach (var patch in memtPatches.MemtEntries)
                            {
                                if (patch.Index < 0)
                                    throw new IndexOutOfRangeException($"Invalid index {patch.Index} for Memt.");

                                if (patch.Index >= memtEntries.Count)
                                {
                                    // Index is beyond current entries, append new entries up to the patch index
                                    while (memtEntries.Count <= patch.Index)
                                    {
                                        memtEntries.Add(new Kh2.SystemData.Memt.EntryFinalMix());
                                    }
                                }

                                var memtEntry = memtEntries[patch.Index];
                                memtEntry.WorldId = patch.WorldId;
                                memtEntry.CheckStoryFlag = patch.CheckStoryFlag;
                                if (!string.IsNullOrWhiteSpace(patch.FlagForWorld))
                                {
                                    if (!worldIndexMap.TryGetValue(patch.FlagForWorld.Replace(" ", "").ToLower(), out var worldId))
                                        throw new Exception($"Unknown world name '{patch.FlagForWorld}' in CheckStoryFlagWorld.");

                                    memtEntry.CheckStoryFlag += (short)(worldId * 1024);
                                }

                                memtEntry.CheckStoryFlagNegation = patch.CheckStoryFlagNegation;
                                if (!string.IsNullOrWhiteSpace(patch.NegationFlagForWorld))
                                {
                                    if (!worldIndexMap.TryGetValue(patch.NegationFlagForWorld.Replace(" ", "").ToLower(), out var worldId))
                                        throw new Exception($"Unknown world name '{patch.NegationFlagForWorld}' in CheckStoryFlagNegationWorld.");

                                    memtEntry.CheckStoryFlagNegation += (short)(worldId * 1024);
                                }
                                memtEntry.CheckArea = patch.CheckArea;
                                memtEntry.Padding = patch.Padding;
                                memtEntry.PlayerSize = patch.PlayerSize;
                                memtEntry.FriendSize = patch.FriendSize;

                                memtEntry.Members = patch.Members.ToArray();
                                memtEntries[patch.Index] = memtEntry;
                            }

                            memt.Entries.Clear();
                            memt.Entries.AddRange(memtEntries);

                            stream.Position = 0;
                            Kh2.SystemData.Memt.Write(stream, memt);
                        }

                        if (memtPatches.MemberIndices != null)
                        {
                            foreach (var patch in memtPatches.MemberIndices)
                            {
                                if (patch.Index < 0 || patch.Index >= memt.MemberIndexCollection.Length)
                                    throw new IndexOutOfRangeException($"Invalid MemberIndices index {patch.Index}.");

                                var memberIndices = memt.MemberIndexCollection[patch.Index];
                                memberIndices.Player = patch.Player;
                                memberIndices.Friend1 = patch.Friend1;
                                memberIndices.Friend2 = patch.Friend2;
                                memberIndices.FriendWorld = patch.FriendWorld;

                                memt.MemberIndexCollection[patch.Index] = memberIndices;
                            }

                            stream.Position = 0;
                            Kh2.SystemData.Memt.Write(stream, memt);
                        }
                        break;

                    //New: Pref listpatches. More rigid as they're mostly offset-based. Can be updated to eventually support addition though.
                    case "fmab":
                        var fmabList = Kh2.SystemData.Fmab.Read(stream);

                        var moddedFmab = deserializer.Deserialize<Kh2.SystemData.Fmab.FmabEntries>(sourceText);

                        foreach (var patch in moddedFmab.Entries)
                        {
                            if (patch.Key >= 0 && patch.Key < fmabList.Count)
                            {
                                fmabList[patch.Key] = patch.Value;
                            }
                        }

                        stream.SetLength(0);
                        Kh2.SystemData.Fmab.Write(stream, fmabList);
                        break;

                    default:
                        break;
                }
            }
        }
        private static void PatchSynth(Context context, List<AssetFile> sources, Stream stream)
        {
            foreach (var source in sources)
            {
                string sourceText = File.ReadAllText(context.GetSourceModAssetPath(source.Name));
                switch (source.Type)
                {
                    case "recipe":
                        var recipeList = Kh2.Mixdata.ReciLP.Read(stream); // Read existing Reci list
                        var moddedRecipes = deserializer.Deserialize<List<Kh2.Mixdata.ReciLP>>(sourceText); // Deserialize modded recipes

                        foreach (var moddedRecipe in moddedRecipes)
                        {
                            var existingRecipe = recipeList.FirstOrDefault(x => x.Id == moddedRecipe.Id);

                            if (existingRecipe != null)
                            {
                                // Update existing recipe in the list
                                recipeList[recipeList.IndexOf(existingRecipe)] = moddedRecipe;

                                // Update other properties as needed
                            }
                            else
                            {
                                // Add new recipe to the list
                                recipeList.Add(moddedRecipe);
                            }
                        }

                        // Write the updated recipe list back to the stream
                        Kh2.Mixdata.ReciLP.Write(stream, recipeList); // Pass IEnumerable<Reci>
                        break;

                    case "condition":
                        var conditionList = Kh2.Mixdata.CondLP.Read(stream);
                        var moddedConditions = deserializer.Deserialize<List<Kh2.Mixdata.CondLP>>(sourceText);

                        foreach (var moddedCondition in moddedConditions)
                        {
                            var existingCondition = conditionList.FirstOrDefault(x => x.TextId == moddedCondition.TextId);

                            if (existingCondition != null)
                            {
                                conditionList[conditionList.IndexOf(existingCondition)] = moddedCondition;

                            }
                            else
                            {
                                conditionList.Add(moddedCondition);
                            }
                        }

                        Kh2.Mixdata.CondLP.Write(stream, conditionList); // Pass IEnumerable<Reci>
                        break;

                    case "level":
                        var levelList = Kh2.Mixdata.LeveLP.Read(stream);
                        var moddedLevels = deserializer.Deserialize<List<Kh2.Mixdata.LeveLP>>(sourceText);

                        foreach (var moddedLevel in moddedLevels)
                        {
                            var existingLevel = levelList.FirstOrDefault(x => x.Title == moddedLevel.Title);

                            if (existingLevel != null)
                            {
                                levelList[levelList.IndexOf(existingLevel)] = moddedLevel;
                            }
                            else
                            {
                                levelList.Add(moddedLevel);
                            }
                        }

                        Kh2.Mixdata.LeveLP.Write(stream, levelList);
                        break;
                }
            }
        }
    }
}
