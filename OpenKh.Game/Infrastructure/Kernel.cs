using OpenKh.Common;
using OpenKh.Common.Archives;
using OpenKh.Engine;
using OpenKh.Engine.Extensions;
using OpenKh.Engine.Renders;
using OpenKh.Common;
using OpenKh.Kh2;
using OpenKh.Kh2.Battle;
using OpenKh.Kh2.Contextes;
using OpenKh.Kh2.Extensions;
using OpenKh.Kh2.SaveData;
using OpenKh.Kh2.SystemData;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

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
        public string Language => Constants.Languages[RegionId];
        public string Region => Constants.Regions[RegionId];
        public IDataContent DataContent { get; }
        public FontContext FontContext { get; }
        public RenderingMessageContext SystemMessageContext { get; set; }
        public RenderingMessageContext EventMessageContext { get; set; }
        public Kh2MessageProvider MessageProvider { get; }

        public ISaveData SaveData { get; private set; }
        public List<Objentry> ObjEntries { get; }
        public Dictionary<string, List<Place>> Places { get; }

        public bool DebugMode { get; set; }

        public int World
        {
            get => SaveData.WorldId;
            set => SaveData.WorldId = (byte)value;
        }
        public int Area
        {
            get => SaveData.RoomId;
            set => SaveData.RoomId = (byte)value;
        }
        public int Entrance
        {
            get => SaveData.SpawnId;
            set => SaveData.SpawnId = (byte)value;
        }

        public int SpawnMap => GetPlaceScript()?.Map ?? 0;
        public int SpawnBtl => GetPlaceScript()?.Battle ?? 0;
        public int SpawnEvt => GetPlaceScript()?.Event ?? 0;
        public string SpawnName
        {
            get
            {
                var map = SpawnMap;
                var btl = SpawnBtl;
                var evt = SpawnEvt;
                var strMap = map >= 0x19 ? map.ToString() : $"{(char)(map + 0x41)}";
                var strBtl = btl >= 0x19 ? map.ToString() : $"{(char)(btl + 0x41)}";
                var strEvt = evt >= 0x19 ? map.ToString() : $"{(char)(evt + 0x41)}";
                return $"{strMap}/{strBtl}/{strEvt}";
            }
        }

        // System
        public List<List<Arif>> AreaInfo { get; private set; }
        public List<Ftst.Entry> Ftst { get; private set; }
        public Item Item { get; private set; }
        public Memt MemberTable { get; private set; }
        public List<Trsr> Trsr { get; private set; }

        // Battle
        public List<Fmlv.Level> Fmlv { get; private set; }
        public List<Lvup.PlayableCharacter> Lvup { get; private set; }

        // 00worldpoint
        public List<WorldPoint> WorldPoints { get; private set; }

        public Kernel(IDataContent dataContent)
        {
            Log.Info("Initialize kernel");
            DataContent = dataContent;

            FontContext = new FontContext();
            MessageProvider = new Kh2MessageProvider();
            _regionId = DetectRegion(dataContent);
            Log.Info("Region={0} Language={1}", Region, Language);

            IsReMix = IsReMixFileExists(dataContent, Region);
            Log.Info("ReMIX={0}", IsReMix);

            IsFinalMix = IsReMix || RegionId == (int)Constants.RegionId.FinalMix;
            Log.Info("Final Mix={0}", IsFinalMix);

            // Load files in the same order as KH2 does
            ObjEntries = LoadFile("00objentry.bin", stream => Objentry.Read(stream));
            // 00progress
            LoadSystem("03system.bin");
            LoadBattle("00battle.bin");
            // 00common
            LoadWorldPoint("00worldpoint.bin");
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
            SaveData = new SaveFinalMix()
            {
                WorldId = 2,
                RoomId = 0,
                SpawnId = 0,
                Characters = Enumerable.Range(0, 13).Select(x => new CharacterFinalMix
                {
                    Abilities = Enumerable.Range(0, 80).Select(x => (ushort)0).ToArray()
                }).ToArray(),
                DriveForms = Enumerable.Range(0, 10).Select(x => new DriveFormFinalMix
                {
                    Abilities = Enumerable.Range(0, 24).Select(x => (ushort)0).ToArray()
                }).ToArray(),
            };
        }

        public string GetMapFileName(int worldIndex, int placeIndex) => IsReMix
            ? $"map/{Constants.WorldIds[worldIndex]}{placeIndex:D02}.map"
            : $"map/{Language}/{Constants.WorldIds[worldIndex]}{placeIndex:D02}.map";

        public void LoadSaveData(string fileName)
        {
            var savePath = Path.Combine(Config.SavePath, fileName);
            Log.Info("Attempting to load save {0}...", savePath);
            if (File.Exists(savePath))
                File.OpenRead(savePath).Using(LoadSaveData);
            else
                Log.Warn("Save {0} not found", savePath);
        }

        public bool LoadSaveData(Stream stream)
        {
            if (!SaveDataFactory.IsValid(stream))
            {
                Log.Warn("Specified save game is not valid");
                return false;
            }

            SaveData = SaveDataFactory.Read(stream);
            Log.Info($"Save read successful. IsFinalMix={SaveData.IsFinalMix}");
            return true;
        }

        private T LoadFile<T>(string fileName, Func<Stream, T> action)
        {
            using var stream = DataContent.FileOpen(fileName);
            return action(stream);
        }

        private void LoadSystem(string fileName)
        {
            var bar = DataContent.FileOpen(fileName).Using(stream => Bar.Read(stream));

            AreaInfo = bar.ForEntry("arif", Bar.EntryType.List, Kh2.SystemData.Arif.Read);
            Ftst = bar.ForEntry("ftst", Bar.EntryType.List, Kh2.SystemData.Ftst.Read);
            Item = bar.ForEntry("item", Bar.EntryType.List, Kh2.SystemData.Item.Read);
            MemberTable = bar.ForEntry("memt", Bar.EntryType.List, Kh2.SystemData.Memt.Read);
            Trsr = bar.ForEntry("tsrs", Bar.EntryType.List, Kh2.SystemData.Trsr.Read);
        }

        private void LoadBattle(string fileName)
        {
            var bar = DataContent.FileOpen(fileName).Using(stream => Bar.Read(stream));

            Fmlv = bar.ForEntry("fmlv", Bar.EntryType.List, Kh2.Battle.Fmlv.Read);
            Lvup = bar.ForEntry("lvup", Bar.EntryType.List, Kh2.Battle.Lvup.Read)?.Characters;
        }

        private void LoadWorldPoint(string fileName)
        {
            WorldPoints = DataContent.FileOpen(fileName).Using(stream => WorldPoint.Read(stream));
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
                    Log.Info("Region ID candidate: {0}", i);
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

        public int GetRealObjectId(int objectId)
        {
            const int PLAYER = 0x236;
            const int FRIEND_1 = 0x237;
            const int FRIEND_2 = 0x238;
            const int ACTOR_SORA = 0x23B;
            const int ACTOR_SORA_H = 0x23C;

            if (IsFinalMix)
            {
                switch (objectId)
                {
                    case PLAYER:
                    case ACTOR_SORA:
                        return GetRealObjectId(objectId, (int)MemberFinalMix.Sora);
                    case FRIEND_1:
                        return GetRealObjectId(objectId, (int)MemberFinalMix.Donald);
                    case FRIEND_2:
                        return GetRealObjectId(objectId, (int)MemberFinalMix.Goofy);
                    case ACTOR_SORA_H:
                        return GetRealObjectId(objectId, (int)MemberFinalMix.SoraHighPoly);
                    default:
                        return objectId;
                }
            }
            else
            {
                switch (objectId)
                {
                    case PLAYER:
                    case ACTOR_SORA:
                        return GetRealObjectId(objectId, (int)MemberVanilla.Sora);
                    case FRIEND_1:
                        return GetRealObjectId(objectId, (int)MemberVanilla.Donald);
                    case FRIEND_2:
                        return GetRealObjectId(objectId, (int)MemberVanilla.Goofy);
                    case ACTOR_SORA_H:
                        return GetRealObjectId(objectId, (int)MemberVanilla.SoraHighPoly);
                    default:
                        return objectId;
                }
            }
        }

        private int GetRealObjectId(int objectId, int memberIndex)
        {
            if ((MemberTable?.Entries?.Count ?? 0) == 0)
                return objectId;

            var defaultMemberTableEntry = MemberTable.Entries[0];
            var memberTableEntry = MemberTable.Entries
                .FirstOrDefault(x => x.WorldId == World);

            if (memberIndex >= memberTableEntry.Members.Length)
                return objectId;

            if (memberTableEntry.Members[memberIndex] != 0)
                return memberTableEntry.Members[memberIndex];

            if (defaultMemberTableEntry.Members[memberIndex] != 0)
                return defaultMemberTableEntry.Members[memberIndex];

            return objectId;
        }

        private IPlaceScript GetPlaceScript()
        {
            var index = World * Constants.MaxAreaPerWorldCount + Area;
            if (SaveData == null ||
                Area >= Constants.MaxAreaPerWorldCount ||
                index >= SaveData.PlaceScripts.Length)
                return null;

            return SaveData.PlaceScripts[index];
        }
    }
}
