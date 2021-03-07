using System.Collections.Generic;
using System.IO;
using Xe.BinaryMapper;
using OpenKh.Common.Utils;

namespace OpenKh.Bbs
{
    public class Olo
    {
        private const int MagicCode = 0x4F4C4F40; // Always @OLO

        public class Header
        {
            [Data] public int MagicCode { get; set; }
            [Data] public ushort FileVersion { get; set; }
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
            behavior.Shape = (TriggerShape)BitsUtil.Int.GetBits((int)value, 4, 4);
            behavior.Fire = BitsUtil.Int.GetBit((int)value, 8);
            behavior.Stop = BitsUtil.Int.GetBit((int)value, 9);

            return behavior;
        }

        public static uint MakeTriggerBehavior(TriggerType type, TriggerShape shape, bool Fire, bool Stop)
        {
            uint val = 0;
            val = BitsUtil.Int.SetBits(val, 0, 4, (uint)type);
            val = BitsUtil.Int.SetBits(val, 4, 4, (uint)shape);
            val = BitsUtil.Int.SetBit(val, 8, Fire);
            val = BitsUtil.Int.SetBit(val, 9, Stop);
            return val;
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

        public static uint MakeGroupFlag(AppearType Type, bool Linked, bool AppearOK, bool LinkInvoke, byte Step, bool Fire, byte ID, bool Specified, bool GameTriggerFire, bool MissionFire, bool AllDeadNoAppear, byte GroupID)
        {
            uint val = 0;
            val = BitsUtil.Int.SetBits(val, 0, 4, (uint)Type);
            val = BitsUtil.Int.SetBit(val, 4, Linked);
            val = BitsUtil.Int.SetBit(val, 5, AppearOK);
            val = BitsUtil.Int.SetBit(val, 6, LinkInvoke);
            val = BitsUtil.Int.SetBits(val, 7, 4, Step);
            val = BitsUtil.Int.SetBit(val, 11, Fire);
            val = BitsUtil.Int.SetBits(val, 12, 8, ID);
            val = BitsUtil.Int.SetBit(val, 20, Specified);
            val = BitsUtil.Int.SetBit(val, 21, GameTriggerFire);
            val = BitsUtil.Int.SetBit(val, 22, MissionFire);
            val = BitsUtil.Int.SetBit(val, 23, AllDeadNoAppear);
            val = BitsUtil.Int.SetBits(val, 24, 5, GroupID);

            return val;
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

        public static LayoutInfo GetLayoutInfo(uint value)
        {
            LayoutInfo flag = new LayoutInfo();

            flag.Appear = BitsUtil.Int.GetBit((int)value, 0);
            flag.LoadOnly = BitsUtil.Int.GetBit((int)value, 1);
            flag.Dead = BitsUtil.Int.GetBit((int)value, 2);
            flag.ID = (byte)BitsUtil.Int.GetBits((int)value, 3, 8);
            flag.ModelDisplayOff = BitsUtil.Int.GetBit((int)value, 11);
            flag.GroupID = (byte)BitsUtil.Int.GetBits((int)value, 12, 8);
            flag.NoLoad = BitsUtil.Int.GetBit((int)value, 20);
            flag.NetworkID = (byte)BitsUtil.Int.GetBits((int)value, 21, 8);

            return flag;
        }

        public static uint MakeLayoutInfo(bool Appear, bool LoadOnly, bool Dead, byte ID, bool ModelDisplayOff, byte GroupID, bool NoLoad, byte NetworkID)
        {
            uint val = 0;

            val = BitsUtil.Int.SetBit(val, 0, Appear);
            val = BitsUtil.Int.SetBit(val, 1, LoadOnly);
            val = BitsUtil.Int.SetBit(val, 2, Dead);
            val = BitsUtil.Int.SetBits(val, 3, 8, ID);
            val = BitsUtil.Int.SetBit(val, 11, ModelDisplayOff);
            val = BitsUtil.Int.SetBits(val, 12, 8, GroupID);
            val = BitsUtil.Int.SetBit(val, 20, NoLoad);
            val = BitsUtil.Int.SetBits(val, 21, 8, NetworkID);

            return val;
        }

        public Header header;
        public List<ObjectName> ObjectList = new List<ObjectName>();
        public List<PathName> FileList = new List<PathName>();
        public List<PathName> ScriptList = new List<PathName>();
        public List<ObjectName> MissionNameList = new List<ObjectName>();
        public List<TriggerData> TriggerList = new List<TriggerData>();
        public List<GroupData> GroupList = new List<GroupData>();
        public List<LayoutData> LayoutList = new List<LayoutData>();

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
            olo.LayoutList = new List<LayoutData>();
            for (int i = 0; i < olo.header.GroupDataCount; i++)
            {
                stream.Seek(olo.header.GroupDataOffset + (i * 0x30), SeekOrigin.Begin);
                GroupData data = BinaryMapping.ReadObject<GroupData>(stream);
                stream.Seek(data.ObjectLayoutDataOffset, SeekOrigin.Begin);
                
                for (int j = 0; j < data.ObjectLayoutDataCount; j++)
                {
                    olo.LayoutList.Add(BinaryMapping.ReadObject<LayoutData>(stream));
                }

                olo.GroupList.Add(data);
            }

            return olo;
        }

        public static void Write(Stream stream, Olo olo)
        {
            BinaryMapping.WriteObject<Header>(stream, olo.header);

            for (int i = 0; i < olo.header.SpawnObjectsCount; i++)
            {
                BinaryMapping.WriteObject<ObjectName>(stream, olo.ObjectList[i]);
            }

            for (int i = 0; i < olo.header.FilePathCount; i++)
            {
                BinaryMapping.WriteObject<PathName>(stream, olo.FileList[i]);
            }

            for (int i = 0; i < olo.header.ScriptPathCount; i++)
            {
                BinaryMapping.WriteObject<PathName>(stream, olo.ScriptList[i]);
            }

            for (int i = 0; i < olo.header.MissionNameCount; i++)
            {
                BinaryMapping.WriteObject<ObjectName>(stream, olo.MissionNameList[i]);
            }

            for (int i = 0; i < olo.header.TriggerDataCount; i++)
            {
                BinaryMapping.WriteObject<TriggerData>(stream, olo.TriggerList[i]);
            }

            for (int i = 0; i < olo.header.GroupDataCount; i++)
            {
                BinaryMapping.WriteObject<GroupData>(stream, olo.GroupList[i]);
            }

            for (int j = 0; j < olo.LayoutList.Count; j++)
            {
                BinaryMapping.WriteObject<LayoutData>(stream, olo.LayoutList[j]);
            }
        }
    }
}
