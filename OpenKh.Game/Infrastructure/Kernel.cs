using OpenKh.Common;
using OpenKh.Common.Archives;
using OpenKh.Engine;
using OpenKh.Engine.Extensions;
using OpenKh.Engine.Renders;
using OpenKh.Game.Debugging;
using OpenKh.Kh2;
using OpenKh.Kh2.Battle;
using OpenKh.Kh2.Contextes;
using OpenKh.Kh2.Extensions;
using OpenKh.Kh2.System;
using System;
using System.Collections.Generic;
using System.IO;

namespace OpenKh.Game.Infrastructure
{
    public class Kernel : ILanguage
    {
        private readonly int _regionId;

        public bool IsFinalMix { get; }
        public bool IsReMix { get; }
        public int RegionId
        {
            get => Config.RegionId == -1 ? _regionId : Config.RegionId;
            set => Config.RegionId = value;
        }
        public string Language
        {
            get
            {
                int languageId;
                if (RegionId == Constants.RegionFinalMix) // Final mix should load JP assets
                    languageId = 0;
                else if (RegionId == 2) // UK region should load US assets
                    languageId = 1;
                else
                    languageId = RegionId;

                return Constants.Regions[languageId];
            }
        }

        public string Region => Constants.Regions[RegionId];
        public IDataContent DataContent { get; }
        public FontContext FontContext { get; }
        public RenderingMessageContext SystemMessageContext { get; set; }
        public RenderingMessageContext EventMessageContext { get; set; }
        public Kh2MessageProvider MessageProvider { get; }
        public List<Objentry> ObjEntries { get; }
        public Dictionary<string, List<Place>> Places { get; }
        public List<Ftst.Entry> Ftst { get; private set; }
        public Item Item { get; private set; }
        public List<Trsr> Trsr { get; private set; }
        public List<Fmlv.Level> Fmlv { get; private set; }
        public List<Lvup.PlayableCharacter> Lvup { get; private set; }

        public Kernel(IDataContent dataContent)
        {
            Log.Info("Initialize kernel");
            DataContent = dataContent;

            FontContext = new FontContext();
            MessageProvider = new Kh2MessageProvider();
            _regionId = DetectRegion(dataContent);
            Log.Info($"Region={Region} Language={Language}");

            IsReMix = IsReMixFileExists(dataContent, Region);
            Log.Info($"ReMIX={IsReMix}");

            IsFinalMix = IsReMix || RegionId == Constants.RegionFinalMix;
            Log.Info($"Final Mix={IsFinalMix}");

            // Load files in the same order as KH2 does
            ObjEntries = LoadFile("00objentry.bin", stream => Objentry.Read(stream));
            // 00progress
            LoadSystem("03system.bin");
            LoadBattle("00battle.bin");
            // 00common
            // 00worldpoint
            // 07localset
            // 12soundinfo
            // 14mission

            LoadFontImage($"msg/{Language}/fontimage.bar");
            LoadFontInfo($"msg/{Language}/fontinfo.bar");
            Places = LoadFile($"msg/{Language}/place.bin", stream => Place.Read(stream));
            LoadMessage("sys");
            // 15jigsaw

            if (Language == "jp" && Config.EnforceInternationalTextEncoding == false)
            {
                Log.Info($"Use Japanese text encoding");
                SystemMessageContext = FontContext.ToKh2JpSystemTextContext();
                EventMessageContext = FontContext.ToKh2JpEventTextContext();
            }
            else
            {
                Log.Info($"Use International text encoding");
                SystemMessageContext = FontContext.ToKh2EuSystemTextContext();
                EventMessageContext = FontContext.ToKh2EuEventTextContext();
            }
            MessageProvider.Encoder = SystemMessageContext.Encoder;
        }

        public string GetMapFileName(int worldIndex, int placeIndex) => IsReMix
            ? $"map/{Constants.WorldIds[worldIndex]}{placeIndex:D02}.map"
            : $"map/{Language}/{Constants.WorldIds[worldIndex]}{placeIndex:D02}.map";

        private T LoadFile<T>(string fileName, Func<Stream, T> action)
        {
            using var stream = DataContent.FileOpen(fileName);
            return action(stream);
        }

        private void LoadSystem(string fileName)
        {
            var bar = DataContent.FileOpen(fileName).Using(stream => Bar.Read(stream));

            Ftst = bar.ForEntry("ftst", Bar.EntryType.List, Kh2.System.Ftst.Read);
            Item = bar.ForEntry("item", Bar.EntryType.List, Kh2.System.Item.Read);
            Trsr = bar.ForEntry("tsrs", Bar.EntryType.List, Kh2.System.Trsr.Read);
        }

        private void LoadBattle(string fileName)
        {
            var bar = DataContent.FileOpen(fileName).Using(stream => Bar.Read(stream));

            Fmlv = bar.ForEntry("fmlv", Bar.EntryType.List, Kh2.Battle.Fmlv.Read);
            Lvup = bar.ForEntry("lvup", Bar.EntryType.List, Kh2.Battle.Lvup.Read)?.Characters;
        }

        private void LoadFontInfo(string fileName)
        {
            var bar = DataContent.FileOpen(fileName).Using(Bar.Read);
            FontContext.Read(bar);
        }

        private void LoadFontImage(string fileName) =>
            LoadFontInfo(fileName);

        private void LoadMessage(string worldId)
        {
            var messageBar = DataContent.FileOpen($"msg/{Language}/sys.bar")
                .Using(stream => Bar.Read(stream));

            MessageProvider.Load(messageBar.ForEntry(worldId, Bar.EntryType.List, Msg.Read));
        }

        private static int DetectRegion(IDataContent dataContent)
        {
            for (var i = 0; i < Constants.Regions.Length; i++)
            {
                var testFileName = $"menu/{Constants.Regions[i]}/title.2ld";
                if (dataContent.FileExists(testFileName))
                {
                    Log.Info($"Region ID candidate: {i}");
                    return i;
                }
            }

            throw new Exception("Unable to detect any region for the game. Some files are potentially missing.");
        }

        public static bool IsReMixFileExists(IDataContent dataContent, string region)
        {
            var testFileName = $"menu/{region}/titlejf.2ld";
            return dataContent.FileExists(testFileName);
        }

        public static bool IsReMixFileHasHdAssetHeader(IDataContent dataContent, string region)
        {
            var testFileName = $"menu/{region}/titlejf.2ld";
            var stream = dataContent.FileOpen(testFileName);
            if (stream == null)
                return false;

            using (stream)
                return HdAsset.IsValid(stream);
        }
    }
}
