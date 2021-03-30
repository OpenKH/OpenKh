using System;
using System.Collections.Generic;
using System.Text;
using Xe.BinaryMapper;
using System.IO;
using System.Linq;
using OpenKh.Common.Utils;

namespace OpenKh.Bbs
{
    public class Mission
    {
        private const int MagicCode = 0x53494D;
        private const ushort FileVersion = 3;

        public class Header
        {
            [Data] public int MagicCode { get; set; }
            [Data] public ushort FileVersion { get; set; }
            [Data] public ushort DataCount { get; set; }
        }

        public class MissionData
        {
            [Data(Count = 12)] public string MissionName { get; set; }
            [Data] public uint InformationCTD  { get; set; }
            [Data(Count = 12)] public string StartEXA { get; set; }
            [Data(Count = 12)] public string Script { get; set; }
            [Data(Count = 12)] public string SuccessEXA { get; set; }
            [Data(Count = 12)] public string FailureEXA { get; set; }
            [Data] public uint PauseInformationCTD { get; set; }
            [Data] public MissionKind Kind { get; set; }
            [Data] public byte Flag { get; set; }
            [Data] public byte Bonus1 { get; set; }
            [Data] public byte Bonus2 { get; set; }
            [Data] public ushort BonusParam1 { get; set; }
            [Data] public ushort BonusParam2 { get; set; }
            [Data] public byte Navigation { get; set; }
            [Data] public byte Present1 { get; set; }
            [Data] public byte Present2 { get; set; }
            [Data] public GeneralPathBit GeneralPath { get; set; }
            [Data] public ushort PresentParam1 { get; set; }
            [Data] public ushort PresentParam2 { get; set; }
            [Data] public byte HPRecovery { get; set; }
            [Data] public byte ExtraRecovery { get; set; }
            [Data] public byte Padding1 { get; set; }
            [Data] public byte Padding2 { get; set; }
        }

        public enum MissionKind : sbyte
        {
            MISSION_NONE = -1,
            DEAD_ALL = 0,
            BOSS_DEAD,
            SERCH_MIDGET,
            ESCAPE_FOREST,
            GUARD_CINDERELLA_0,
            GUARD_CINDERELLA_1,
            GUARD_JAQ,
            JOINT_STRUGGLE_PHILLIP_0,
            JOINT_STRUGGLE_PHILLIP_1,
            BTL_MDRAGON_PHILLIP,
            BTL_VANITAS_KING,
            BTL_DEAD_ALL_KING,
            BTL_DEAD_ALL_HERCULES ,
            BTL_DEAD_ALL_ZACKS_HERCULES,
            BTL_DEAD_ALL_STITCH,
            BTL_PARTS_AQ_VE,
            BTL_PARTS_VE_TE,
            BTL_PARTS_AQ_TE,
            BTL_ROCK_TITAN_HERCULES,
            BTL_METAL_STITCH,
            BTL_IRON_AYA_RAGNA,
            MINIGAME_POT_BREAK_0,
            MINIGAME_POT_BREAK_1,
            BTL_GUNTU_STITCH,
            MINIGAME_RIDE_BATTLE,
            PARTITION_CLOSE,
            BTL_MANY_ENEMY_HERCULES,
            BTL_MANY_ENEMY,
            FIGHT_RALLY,
            MINIGAME_RIDE_RACE_EVENT,
            MINIGAME_RIDE_RACE_A,
            MINIGAME_RIDE_RACE_B,
            MINIGAME_RIDE_RACE_C,
            MINIGAME_RIDE_BATTLE_NMAP,
            MINIGAME_RIDE_BATTLE_METAMO,
            MINIGAME_ICE_MISSION,
            MINIGAME_ICE_M1_NORMAL,
            MINIGAME_ICE_M1_MANIACK,
            MINIGAME_ICE_M2_NORMAL,
            MINIGAME_ICE_M2_MANIACK,
            MINIGAME_BALL_MISSION,
            MINIGAME_BALL_VS_DEBU,
            MINIGAME_BALL_VS_TIP,
            MINIGAME_BALL_VS_DARK,
            MINIGAME_BALL_VS_JAST,
            MINIGAME_BALL_VS_HIDE,
            MINIGAME_POT_BREAK_2,
            BOSS_DEAD_TIMEov_or_HPxx,
            UNK1,
            UNK2,
            TUTORIAL_MOVE_AND_CAMERA,
            TUTORIAL_NORMAL_ATTACK,
            TUTORIAL_D_LINK,
            TUTORIAL_GUARD,
            TUTORIAL_CMD_GAUGE,
            TUTORIAL_SHOOT_LOCK,
            TUTORIAL_TEST_SELECT_CHARA,
            TUTORIAL_DUMMY,
            TUTORIAL_BTL_DICE,
            TUTORIAL_SAVE_POINT,
            TUTORIAL_PLAYER_SELECT,
            TUTORIAL_TEST_BTL_TERA,
            TUTORIAL_TEST_BTL_AQUA,
            TUTORIAL_TEST_BTL_VEN,
            TUTORIAL_MAX,
            BOSS_DEAD_EXT_METAMOL = 100,
            BOSS_DEAD_EXT_PAN,
            BOSS_DEAD_EXT_VANITUS,
            BOSS_DEAD_EXT_VAN_VEN,
            BOSS_DEAD_EXT_XEA_VAN,
            BOSS_DEAD_TERA_ZEA,
            MINIGAME_ICE_M1_SPECIAL,
            MINIGAME_ICE_M2_SPECIAL,
            LIGHT_BALL,
            MINIGAME_ICE_M3_NORMAL,
            MINIGAME_ICE_M3_MANIACK,
            MINIGAME_ICE_M3_SPECIAL,
            MINIGAME_ICE_M4_NORMAL,
            MINIGAME_ICE_M4_MANIACK,
            MINIGAME_ICE_M4_SPECIAL,
            MINIGAME_ICE_M5_NORMAL,
            MINIGAME_ICE_M5_MANIACK,
            MINIGAME_ICE_M5_SPECIAL,
            MINIGAME_RIDE_RACE_D,
            FM_MISSION_ILLUSION_A,
            FM_MISSION_ILLUSION_B,
            FM_MISSION_ILLUSION_C,
            FM_MISSION_ILLUSION_D,
            FM_MISSION_ILLUSION_E,
            FM_MISSION_ILLUSION_F,
            FM_MISSION_ILLUSION_G,
            FM_MISSION_ILLUSION_H,
            FM_MISSION_ILLUSION_I,
        }

        public enum GeneralPathBit : byte
        {
            GENERAL_PATH_NONE = 0,
            GENERAL_PATH_START = 1,
            GENERAL_PATH_END = 2
        }

        public static bool IsValid(Stream stream)
        {
            var prevPosition = stream.Position;
            var magicCode = new BinaryReader(stream).ReadInt32();
            stream.Position = prevPosition;

            return magicCode == MagicCode;
        }

        public static IEnumerable<MissionData> Read(Stream stream)
        {
            Header head = BinaryMapping.ReadObject<Header>(stream);

            return Enumerable.Range(0, head.DataCount)
                .Select(x => BinaryMapping.ReadObject<MissionData>(stream))
                .ToArray();
        }

        public static void Write(Stream stream, IEnumerable<MissionData> data)
        {
            Header head = new Header();
            head.MagicCode = MagicCode;
            head.FileVersion = FileVersion;
            head.DataCount = (ushort)data.Count();
            BinaryMapping.WriteObject<Header>(stream, head);

            foreach(MissionData miss in data)
            {
                BinaryMapping.WriteObject<MissionData>(stream, miss);
            }
        }
    }
}
