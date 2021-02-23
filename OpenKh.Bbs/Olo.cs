using System.Collections.Generic;
using System.IO;
using Xe.BinaryMapper;
using OpenKh.Common.Utils;

namespace OpenKh.Bbs
{
    public class Olo
    {
        public static Dictionary<string, string> SpawnObjectList = new Dictionary<string, string>()
        {
            {"b01ex00", "Mysterious Figure"},
            {"b02ex00", "Red Eyes"},
            {"b10ex00", "Vanitas"},
            {"b10ex01", "Vanitas (Low AI)"},
            {"b10ex02", "Vanitas (Final Battle)"},
            {"b11ex00", "Vanitas (Unmasked)"},
            {"b12ex00", "Vanitas Lingering Spirit"},
            {"b12ex01", "Vanitas Lingering Spirit Clone"},
            {"b20ex00", "Eraqus"},
            {"b30ex00", "Braig (Scarred)"},
            {"b31ex00", "Braig Bullets"},
            {"b32ex00", "Braig"},
            {"b40ex00", "Master Xehanort"},
            {"b50ex00", "Terra-Xehanort"},
            {"b51ex00", "Terra-Xehanort 2"},
            {"b52ex00", "Terra-Xehanort + Guardian"},
            {"b53ex00", "Terra-Xehanort's No Name"},
            {"b54ex00", "Terra-Xehanort's No Name Keychain"},
            {"b55ex00", "Terra-Xehanort's Ultima Cannon"},
            {"b56ex00", "Terra-Xehanort's Guardian"},
            {"b60ex00", "Ventus (Boss)"},
            {"b61ex00", "Ventus Weapon"},
            {"b62ex00", "Armor Ventus (Boss)"},
            {"b63ex00", "Ventus=Vanitas (Boss)"},

            {"b01vs00", "Mimic Master (Boss)"},

            {"g01ex00", "Savepoint"},
            {"g02ex00", "Examine Actor (To place on static object)"},
            {"g03ex00", "Crown (Puzzle)"},
            {"g04ex00", "Jellyshade Swam"},
            {"g05ex00", "Teleporter"},
            {"g10ex00", "Invisible Wall"},
            {"g20ex00", "g20ex00"},

            {"g01jb00", "Small Chest (JB)"},
            {"g02jb00", "Large Chest (JB)"},
            {"g03jb00", "Large Chest (Debug)"},
            {"g10jb00", "Plume of Darkness"},
            {"g11jb00", "Sphere of Darkness"},
            {"g12jb00", "g12jb00"},
            {"g13jb00", "g13jb00"},
            {"g14jb00", "g14jb00"},
            {"g15jb00", "g15jb00"},
            {"g16jb00", "g16jb00"},
            {"g17jb00", "g17jb00"},
            {"g18jb00", "g18jb00"},
            {"g19jb00", "g19jb00"},
            {"g20jb00", "g20jb00"},
            {"g21jb00", "g21jb00"},
            {"g22jb00", "g22jb00"},
            {"g23jb00", "g23jb00"},

            {"g01kg00", "Small Chest (KG)"},
            {"g02kg00", "Large Chest (KG)"},
            {"g10kg00", "Tornado (KG)"},
            {"g11kg00", "Tornado Base (KG)"},
            {"g12kg00", "Untextured Triangle"},
            {"g13kg00", "g13kg00 (KG)"},

            {"g01pp00", "g01pp00"},
            {"g20pp00", "g20pp00"},

            {"m01ex00", "Flood"},
            {"m02ex00", "Scrapper"},
            {"m03ex00", "Bruiser"},
            {"m04ex00", "Red Hot Chili"},
            {"m05ex00", "Monotrucker"},
            {"m06ex00", "Thornbite"},
            {"m07ex00", "Shoegazer"},
            {"m08ex00", "Spiderchest"},
            {"m09ex00", "Archraven"},
            {"m10ex00", "Hareraiser"},
            {"m11ex00", "Jellyshade"},
            {"m12ex00", "Tank Toppler"},
            {"m13ex00", "Vile Phial"},
            {"m14ex00", "Sonic Blaster"},
            {"m15ex00", "Triple Wrecker"},
            {"m16ex00", "Wild Bruiser"},
            {"m17ex00", "Blue Sea Salt"},
            {"m18ex00", "Yellow Mustard"},
            {"m19ex00", "Mandrake"},
            {"m20ex00", "Buckle Bruiser"},

            {"m40ex00", "Shadow"},
            {"m41ex00", "Neoshadow"},
            {"m42ex00", "Darkball"},

            {"m54ex00", "Ringer Pot"},


            {"n10ex00", "Moogle Shop 1"},
            {"n11ex00", "Moogle Shop 2"},
            {"n12ex00", "Moogle Shop 3"},
            {"n13ex00", "Moogle Shop 4"},

            {"p01ex00", "Ventus (PC)"},
            {"p02ex00", "Aqua (PC)"},
            {"p03ex00", "Terra (PC)"},
            {"p11ex00", "Ventus Armor (PC)"},
            {"p12ex00", "Aqua Armor (PC)"},
            {"p13ex00", "Terra Armor (PC)"},
            {"p41ex00", "Ventus Armor Helmetless (PC)"},
            {"p42ex00", "Aqua Armor Helmetless (PC)"},
            {"p43ex00", "Terra Armor Helmetless (PC)"},

            
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
            [Data] public bool Fire { get; set; }
            [Data] public bool Stop { get; set; }
            [Data] public uint Padding { get; set; }
        }

        public static TriggerBehavior GetTriggerBehavior(uint value)
        {
            TriggerBehavior behavior = new TriggerBehavior();
            behavior.Type = (TriggerType)BitsUtil.Int.GetBits((int)value, 0, 4);
            behavior.Shape = (TriggerShape)BitsUtil.Int.GetBits((int)value, 4, 8);
            behavior.Fire = BitsUtil.Int.GetBit((int)value, 8);
            behavior.Stop = BitsUtil.Int.GetBit((int)value, 9);

            return behavior;
        }

        public class GroupData
        {
            [Data] public float CenterX { get; set; }
            [Data] public float CenterY { get; set; }
            [Data] public float CenterZ { get; set; }
            [Data] public float Radius { get; set; }
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
            [Data] public List<LayoutData> LayoutList { get; set; }
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
            [Data] public byte GroupID { get; set; }
        }

        public static GroupFlag GetGroupFlag(uint value)
        {
            GroupFlag flag = new GroupFlag();

            flag.Type = (AppearType)BitsUtil.Int.GetBits((int)value, 0, 4);
            flag.Linked = BitsUtil.Int.GetBit((int)value, 4);
            flag.AppearOK = BitsUtil.Int.GetBit((int)value, 5);
            flag.LinkInvoke = BitsUtil.Int.GetBit((int)value, 6);
            flag.Step = (byte)BitsUtil.Int.GetBits((int)value, 7, 4);
            flag.Fire = BitsUtil.Int.GetBit((int)value, 11);
            flag.ID = (byte)BitsUtil.Int.GetBits((int)value, 12, 8);
            flag.Specified = BitsUtil.Int.GetBit((int)value, 20);
            flag.GameTriggerFire = BitsUtil.Int.GetBit((int)value, 21);
            flag.MissionFire = BitsUtil.Int.GetBit((int)value, 22);
            flag.AllDeadNoAppear = BitsUtil.Int.GetBit((int)value, 23);
            flag.GroupID = (byte)BitsUtil.Int.GetBits((int)value, 24, 5);

            return flag;
        }

        public class LayoutData
        {
            [Data] public uint ObjectNameOffset { get; set; }
            [Data] public float PositionX { get; set; }
            [Data] public float PositionY { get; set; }
            [Data] public float PositionZ { get; set; }
            [Data] public float RotationX { get; set; }
            [Data] public float RotationY { get; set; }
            [Data] public float RotationZ { get; set; }
            [Data] public float Height { get; set; }
            [Data] public uint LayoutInfo { get; set; }
            [Data] public uint UniqueID { get; set; }
            [Data] public ushort Parameter1 { get; set; }
            [Data] public ushort Parameter2 { get; set; }
            [Data] public ushort Parameter3 { get; set; }
            [Data] public ushort Trigger { get; set; }
            [Data] public float Parameter5 { get; set; }
            [Data] public float Parameter6 { get; set; }
            [Data] public float Parameter7 { get; set; }
            [Data] public float Parameter8 { get; set; }
            [Data] public int MessageID { get; set; }
            [Data] public uint PathNameOffset { get; set; }
            [Data] public uint ScriptNameOffset { get; set; }
            [Data] public uint MissionLabelOffset { get; set; }
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
        public List<ObjectName> MissionNameList = new List<ObjectName>();
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
            olo.header = BinaryMapping.ReadObject<Header>(stream);

            olo.ObjectList = new List<ObjectName>();
            stream.Seek(olo.header.SpawnObjectsOffset, SeekOrigin.Begin);
            for(int i = 0; i < olo.header.SpawnObjectsCount; i++)
            {
                olo.ObjectList.Add(BinaryMapping.ReadObject<ObjectName>(stream));
            }

            olo.FileList = new List<PathName>();
            stream.Seek(olo.header.FilePathOffset, SeekOrigin.Begin);
            for (int i = 0; i < olo.header.FilePathCount; i++)
            {
                olo.FileList.Add(BinaryMapping.ReadObject<PathName>(stream));
            }

            olo.ScriptList = new List<PathName>();
            stream.Seek(olo.header.ScriptPathOffset, SeekOrigin.Begin);
            for (int i = 0; i < olo.header.ScriptPathCount; i++)
            {
                olo.FileList.Add(BinaryMapping.ReadObject<PathName>(stream));
            }

            olo.MissionNameList = new List<ObjectName>();
            stream.Seek(olo.header.MissionNameOffset, SeekOrigin.Begin);
            for (int i = 0; i < olo.header.MissionNameCount; i++)
            {
                olo.MissionNameList.Add(BinaryMapping.ReadObject<ObjectName>(stream));
            }

            olo.TriggerList = new List<TriggerData>();
            stream.Seek(olo.header.TriggerDataOffset, SeekOrigin.Begin);
            for (int i = 0; i < olo.header.TriggerDataCount; i++)
            {
                olo.TriggerList.Add(BinaryMapping.ReadObject<TriggerData>(stream));
            }

            olo.GroupList = new List<GroupData>();
            stream.Seek(olo.header.GroupDataOffset, SeekOrigin.Begin);
            for (int i = 0; i < olo.header.GroupDataCount; i++)
            {
                GroupData data = BinaryMapping.ReadObject<GroupData>(stream);
                data.LayoutList = new List<LayoutData>();

                stream.Seek(data.ObjectLayoutDataOffset, SeekOrigin.Begin);
                
                for (int j = 0; j < data.ObjectLayoutDataCount; j++)
                {
                    data.LayoutList.Add(BinaryMapping.ReadObject<LayoutData>(stream));
                }

                olo.GroupList.Add(data);
            }

            return olo;
        }
    }
}
