using System.Collections.Generic;
using System.IO;
using Xe.BinaryMapper;

namespace OpenKh.Bbs
{
    public class Olo
    {
        public Dictionary<string, string> SpawnObjectList = new Dictionary<string, string>()
        {
            {"b01ex00", "Mysterious Figure"},
            {"b20ex00", "Eraqus"},
            {"b40ex00", "Master Xehanort"},

            {"g01ex00", "Savepoint"},
            {"g02ex00", "Examine Actor (To place on static object)"},
            {"g03ex00", "Crown (Puzzle)"},

            {"p01ex00", "Ventus (PC)"},
            {"p02ex00", "Aqua (PC)"},
            {"p03ex00", "Terra (PC)"},
            {"p11ex00", "Ventus Armor (PC)"},
            {"p12ex00", "Aqua Armor (PC)"},
            {"p13ex00", "Terra Armor (PC)"},
            {"p41ex00", "Ventus Armor Helmetless (PC)"},
            {"p42ex00", "Aqua Armor Helmetless (PC)"},
            {"p43ex00", "Terra Armor Helmetless (PC)"},

            {"n10ex00", "Moogle Shop 1"},
            {"n11ex00", "Moogle Shop 2"},
            {"n12ex00", "Moogle Shop 3"},
            {"n13ex00", "Moogle Shop 4"},
        };

        private const int MagicCode = 0x4F4C4F40; // Always @OLO

        public class Header
        {
            [Data] public int MagicCode { get; set; }
            [Data] public ushort FileVersion { get; set; } // Pretty much always 5.
            [Data] public ushort Flag { get; set; }
            [Data] public uint SpawnObjectsCount { get; set; }
            [Data] public uint SpawnObjectsOffset { get; set; }
            [Data] public uint FilePathCount { get; set; }
            [Data] public uint FilePathOffset { get; set; }
            [Data] public uint ScriptPathCount { get; set; }
            [Data] public uint ScriptPathOffset { get; set; }
            [Data] public uint MissionNameCount { get; set; }
            [Data] public uint MissionNameOffset { get; set; }
            [Data] public uint TriggerDataCount { get; set; }
            [Data] public uint TriggerDataOffset { get; set; }
            [Data] public uint GroupDataCount { get; set; }
            [Data] public uint GroupDataOffset { get; set; }
            [Data] public uint padding1 { get; set; }
            [Data] public uint padding2 { get; set; }
        }

        public enum OLOFlag
        {
            FLAG_NONE = 0,
            FLAG_ENEMY = 1,
            FLAG_GIMMICK = 2,
            FLAG_NPC = 4,
            FLAG_PLAYER = 8,
            FLAG_EVENT_TRIGGER = 16,
        }

        public class ObjectName
        {
            [Data(Count = 16)] public string Name { get; set; }
        }

        public class PathName
        {
            [Data (Count = 32)] public string Name { get; set; }
        }

        public class TriggerData
        {
            [Data] public float PositionX { get; set; }
            [Data] public float PositionY { get; set; }
            [Data] public float PositionZ { get; set; }
            [Data] public float ScaleX { get; set; }
            [Data] public float ScaleY { get; set; }
            [Data] public float ScaleZ { get; set; }
            [Data] public uint ID { get; set; }
            [Data] public uint Behavior { get; set; }
            [Data] public ushort Param1 { get; set; }
            [Data] public ushort Param2 { get; set; }
            [Data] public uint CTDid { get; set; }
            [Data] public uint TypeRef { get; set; }
            [Data] public float Yaw { get; set; }
        }

        public enum TriggerType
        {
            SCENE_JUMP,
            APPEAR_ENEMY,
            BEGIN_GIMMICK,
            BEGIN_EVENT,
            DESTINATION,
            MESSAGE,
            MISSION
        }

        public enum TriggerShape
        {
            SHAPE_BOX,
            SHAPE_SPHERE,
            SHAPE_CYLINDER
        }

        public class TriggerBehavior
        {
            [Data] public TriggerType Type { get; set; }
            [Data] public TriggerShape Shape { get; set; }
            [Data] public byte Fire { get; set; }
            [Data] public byte Stop { get; set; }
            [Data] public uint Padding { get; set; }
        }

        public class GroupData
        {
            [Data] public float CenterX { get; set; }
            [Data] public float CenterY { get; set; }
            [Data] public float CenterZ { get; set; }
            [Data] public uint TriggerID { get; set; }
            [Data] public uint Flag { get; set; }
            [Data] public float AppearParameter { get; set; }
            [Data] public uint NextGroupDataOffset { get; set; }
            [Data] public float DeadRate { get; set; }
            [Data] public ushort GameTrigger { get; set; }
            [Data] public byte MissionParameter { get; set; }
            [Data] public byte UnkParameter { get; set; }
            [Data] public uint ObjectLayoutDataCount { get; set; }
            [Data] public uint ObjectLayoutDataOffset { get; set; }
        }

        public enum AppearType
        {
            APPEAR_NONE,
            APPEAR_TYPE_PLAYER_DISTANCE,
            APPEAR_TYPE_SPECIFIED,
            APPEAR_TYPE_NPC_DISTANCE
        }

        public class GroupFlag
        {
            [Data] public AppearType Type { get; set; }
            [Data] public bool Linked { get; set; }
            [Data] public bool AppearOK { get; set; }
            [Data] public bool LinkInvoke { get; set; }
            [Data] public byte Step { get; set; }
            [Data] public bool Fire { get; set; }
            [Data] public byte ID { get; set; }
            [Data] public bool Specified { get; set; }
            [Data] public bool GameTriggerFire { get; set; }
            [Data] public bool MissionFire { get; set; }
            [Data] public bool AllDeadNoAppear { get; set; }
            [Data] public bool GroupID { get; set; }
        }

        public class LayoutData
        {
            [Data (Count = 16)] public string ObjectName { get; set; }
            [Data] public float PositionX { get; set; }
            [Data] public float PositionY { get; set; }
            [Data] public float PositionZ { get; set; }
            [Data] public float RotationX { get; set; }
            [Data] public float RotationY { get; set; }
            [Data] public float RotationZ { get; set; }
            [Data] public float Height { get; set; }
            [Data] public uint LayoutInfo { get; set; }
            [Data] public uint UniqueID { get; set; }
            [Data] public ushort Parameter5 { get; set; }
            [Data] public ushort Parameter6 { get; set; }
            [Data] public ushort Parameter7 { get; set; }
            [Data] public ushort Parameter8 { get; set; }
            [Data] public uint MessageID { get; set; }
            [Data (Count = 32)] public string PathName { get; set; }
            [Data(Count = 32)] public string ScriptName { get; set; }
            [Data(Count = 16)] public string MissionLabel { get; set; }
        }

        public class LayoutInfo
        {
            [Data] public bool Appear { get; set; }
            [Data] public bool LoadOnly { get; set; }
            [Data] public bool Dead { get; set; }
            [Data] public byte ID { get; set; }
            [Data] public bool ModelDisplayOff { get; set; }
            [Data] public byte GroupID { get; set; }
            [Data] public bool NoLoad { get; set; }
            [Data] public byte NetworkID { get; set; }
        }

        public Header header;
        public List<ObjectName> ObjectList = new List<ObjectName>();
        public List<PathName> FileList = new List<PathName>();
        public List<PathName> ScriptList = new List<PathName>();
        public List<string> MissionNameList = new List<string>();
        public List<TriggerData> TriggerList = new List<TriggerData>();
        public List<GroupData> GroupList = new List<GroupData>();

        public static bool IsValid(Stream stream)
        {
            var prevPosition = stream.Position;
            var magicCode = new BinaryReader(stream).ReadInt32();
            stream.Position = prevPosition;

            return magicCode == MagicCode;
        }

        public static Olo Read(Stream stream)
        {
            Olo olo = new Olo();

            return olo;
        }
    }
}
