using OpenKh.Common.Utils;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using Xe.BinaryMapper;

namespace OpenKh.Kh2
{
    public class Objentry
    {
        public enum Type : byte
        {
            [Description("Player")]
            PLAYER = 0x0,
            [Description("Party Member")]
            FRIEND = 0x1,
            [Description("NPC")]
            NPC = 0x2,
            [Description("Boss")]
            BOSS = 0x3,
            [Description("Normal Enemy")]
            ZAKO = 0x4,
            [Description("Weapon")]
            WEAPON = 0x5,
            [Description("Placeholder (?)")]
            E_WEAPON = 0x6,
            [Description("Save Point")]
            SP = 0x7,
            [Description("Neutral")]
            F_OBJ = 0x8,
            [Description("Out of Party Member")]
            BTLNPC = 0x9,
            [Description("Chest")]
            TREASURE = 0xA,
            [Description("Moogle (?)")]
            SUBMENU = 0xB,
            [Description("Large Boss")]
            L_BOSS = 0xC,
            [Description]
            G_OBJ = 0xD,
            [Description]
            MEMO = 0xE,
            [Description]
            RTN = 0xF,
            [Description]
            MINIGAME = 0x10,
            [Description("World Map Object")]
            WORLDMAP = 0x11,
            [Description("Drop Item Container")]
            PRIZEBOX = 0x12,
            [Description("Summon")]
            SUMMON = 0x13,
            [Description("Shop")]
            SHOP = 0x14,
            [Description("Normal Enemy 2")]
            L_ZAKO = 0x15,
            [Description("Crowd Spawner")]
            MASSEFFECT = 0x16,
            [Description]
            E_OBJ = 0x17,
            [Description("Puzzle Piece")]
            JIGSAW = 0x18,
        }

        public enum TargetType : byte
        {
            M = 0,
            S = 1,
            L = 2
        }

        public enum ShadowSize : byte
        {
            NoShadow = 0,
            SmallShadow = 1,
            MiddleShadow = 2,
            LargeShadow = 3,
            SmallMovingShadow = 4,
            MiddleMovingShadow = 5,
            LargeMovingShadow = 6
        }

        public enum Form : byte
        {
            [Description("Sora / Roxas")]
            SoraRoxasDefault = 0x0,
            [Description("Valor Form")]
            ValorForm = 0x1,
            [Description("Wisdom Form")]
            WisdomForm = 0x2,
            [Description("Limit Form")]
            LimitForm = 0x3,
            [Description("Master Form")]
            MasterForm = 0x4,
            [Description("Final Form")]
            FinalForm = 0x5,
            [Description("Anti Form")]
            AntiForm = 0x6,
            [Description("Lion King Sora")]
            LionKingSora = 0x7,
            [Description("Atlantica Sora")]
            AtlanticaSora = 0x8,
            [Description("Sora on Carpet")]
            SoraCarpet = 0x9,
            [Description("Roxas Dual-Wield")]
            RoxasDualWield = 0xA,
            [Description("Default (used on Enemies)")]
            Default = 0xB,
            [Description("Sora in Cube / Card Form")]
            CubeCardForm = 0xC,
        }

        [Data] public uint ObjectId { get; set; }
        [Data] public Type ObjectType { get; set; }
        [Data] public byte SubType { get; set; }
        [Data] public byte DrawPriority { get; set; }
        [Data] public byte WeaponJoint { get; set; }
        [Data(Count = 32)] public string ModelName { get; set; }
        [Data(Count = 32)] public string AnimationName { get; set; }
        [Data] public ushort Flags { get; set; }
        [Data] public TargetType ObjectTargetType { get; set; }
        [Data] public byte Padding { get; set; }
        [Data] public ushort NeoStatus { get; set; }
        [Data] public ushort NeoMoveset { get; set; }
        [Data] public float Weight { get; set; }
        [Data] public byte SpawnLimiter { get; set; }
        [Data] public byte Page { get; set; }
        [Data] public ShadowSize ObjectShadowSize { get; set; }
        [Data] public Form ObjectForm { get; set; }
        [Data] public ushort SpawnObject1 { get; set; }
        [Data] public ushort SpawnObject2 { get; set; }
        [Data] public ushort SpawnObject3 { get; set; }
        [Data] public ushort SpawnObject4 { get; set; }

        public bool NoApdx
        {
            get => BitsUtil.Int.GetBit(Flags, 0);
            set => Flags = (ushort)BitsUtil.Int.SetBit(Flags, 0, value);
        }

        public bool Before
        {
            get => BitsUtil.Int.GetBit(Flags, 1);
            set => Flags = (ushort)BitsUtil.Int.SetBit(Flags, 1, value);
        }

        public bool FixColor
        {
            get => BitsUtil.Int.GetBit(Flags, 2);
            set => Flags = (ushort)BitsUtil.Int.SetBit(Flags, 2, value);
        }

        public bool Fly
        {
            get => BitsUtil.Int.GetBit(Flags, 3);
            set => Flags = (ushort)BitsUtil.Int.SetBit(Flags, 3, value);
        }

        public bool Scissoring
        {
            get => BitsUtil.Int.GetBit(Flags, 4);
            set => Flags = (ushort)BitsUtil.Int.SetBit(Flags, 4, value);
        }

        public bool IsPirate
        {
            get => BitsUtil.Int.GetBit(Flags, 5);
            set => Flags = (ushort)BitsUtil.Int.SetBit(Flags, 5, value);
        }

        public bool WallOcclusion
        {
            get => BitsUtil.Int.GetBit(Flags, 6);
            set => Flags = (ushort)BitsUtil.Int.SetBit(Flags, 6, value);
        }

        public bool Hift
        {
            get => BitsUtil.Int.GetBit(Flags, 7);
            set => Flags = (ushort)BitsUtil.Int.SetBit(Flags, 7, value);
        }

        public override string ToString()
        {
            return ModelName;
        }

        public static List<Objentry> Read(Stream stream) => BaseTable<Objentry>.Read(stream);
        public static void Write(Stream stream, IEnumerable<Objentry> entries) =>
            BaseTable<Objentry>.Write(stream, 3, entries);
    }
}
