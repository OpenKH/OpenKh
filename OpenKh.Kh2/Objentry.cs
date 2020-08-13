using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
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

        public enum CommandMenuOptions : byte
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
            [Description("Magic, Drive, Party and Limit commands are greyed out")]
            Unk08 = 0x8,
            [Description("Drive, Party and Limit commands are greyed out (not used ingame)")]
            Unk09 = 0x9,
            [Description("Roxas Dual-Wield")]
            RoxasDualWield = 0xA,
            [Description("Only Attack and Summon commands are available")]
            Default = 0xB,
            [Description("Sora in Cube / Card Form (Luxord battle, not used ingame)")]
            CubeCardForm = 0xC,
        }

        [Data] public ushort ObjectId { get; set; }
        [Data] public ushort Unknown02 { get; set; } // has something to do with if the obj is rendered or not, NO: ObjectId is actually an uint, but it's bitshifted afterwards?! see z_un_003216b8
        [Data] public Type ObjectType { get; set; }
        [Data] public byte Unknown05{ get; set; } //padding? isn't used ingame
        [Data] public byte Unknown06 { get; set; }
        [Data] public byte WeaponJoint { get; set; }
        [Data(Count = 32)] public string ModelName { get; set; }
        [Data(Count = 32)] public string AnimationName { get; set; }
        [Data] public ushort Unknown48 { get; set; } //z_un_00169398 (00169840)
        [Data] public ushort Unknown4a { get; set; }
        [Data] public ushort NeoStatus { get; set; }
        [Data] public ushort NeoMoveset { get; set; }
        [Data] public ushort Unknown50 { get; set; } // some kind of floating point calculation? z_un_0016a0a0
        [Data] public short Weight { get; set; }
        [Data] public byte SpawnLimiter { get; set; }
        [Data] public byte Unknown55 { get; set; } // padding?
        [Data] public byte Unknown56{ get; set; }
        [Data] public CommandMenuOptions CommandMenuOption { get; set; }
        [Data] public ushort SpawnObject1 { get; set; }
        [Data] public ushort SpawnObject2 { get; set; }
        [Data] public ushort SpawnObject3 { get; set; }
        [Data] public ushort Unknown5e { get; set; }

        public static List<Objentry> Read(Stream stream) => BaseTable<Objentry>.Read(stream);
        public static void Write(Stream stream, IEnumerable<Objentry> entries) =>
            BaseTable<Objentry>.Write(stream, 3, entries);
    }
}
