using System;
using System.Collections.Generic;
using System.Linq;

namespace OpenKh.Kh2
{
    public class MotionSet
    {
        private static List<int> _validMotionValues;
        private static List<int> ValidMotionValues
        {
            get
            {
                if (_validMotionValues == null)
                {
                    _validMotionValues = ((MotionName[])Enum.GetValues(typeof(MotionName)))
                                                                .Select(e => (int)e)
                                                                .ToList();
                }
                return _validMotionValues;
            }
        }
        public static bool IsNamedMotion(int motionId)
        {
            return ValidMotionValues.Contains(motionId);
        }

        public enum MotionName
        {
            IDLE = 0,
            WALK = 1,
            RUN = 2,
            JUMP = 3,
            FALL = 4,
            LAND = 5,
            LINK_IDLE = 6,
            HANG = 7,
            HANG_UP = 8,
            ITEM = 9,
            DAMAGE_S_FRONT_HIGH = 10,
            DAMAGE_S_FRONT_LOW = 11,
            DAMAGE_S_BACK_HIGH = 12,
            DAMAGE_S_BACK_LOW = 13,
            DAMAGE_AIR_FRONT = 14,
            DAMAGE_AIR_BACK = 15,
            DAMAGE_BLOW_FRONT = 16,
            DAMAGE_BLOW_FRONT_LAND = 17,
            DAMAGE_BLOW_BACK = 18,
            DAMAGE_BLOW_BACK_LAND = 19,
            DAMAGE_TORNADO_FRONT = 20,
            DAMAGE_TORNADO_BACK = 21,
            DAMAGE_TORNADO = 22,
            DAMAGE_LARGE_FRONT = 23,
            DAMAGE_LARGE_BACK = 24,
            DAMAGE_RESERVE_3 = 25,
            DAMAGE_RESERVE_4 = 26,
            DAMAGE_RESERVE_5 = 27,
            DAMAGE_RESERVE_6 = 28,
            DAMAGE_RESERVE_7 = 29,
            DAMAGE_01 = 30,
            DAMAGE_02 = 31,
            DAMAGE_03 = 32,
            DAMAGE_04 = 33,
            DAMAGE_05 = 34,
            DAMAGE_06 = 35,
            DAMAGE_07 = 36,
            DAMAGE_08 = 37,
            DAMAGE_09 = 38,
            DAMAGE_10 = 39,
            DAMAGE_BLOW_RECOV = 40,
            REFLECT = 41,
            AUTOGUARD = 42,
            REFLECT_AIR = 43,
            APPEAR = 44,
            LEAVE = 45,
            LEAVE_AIR = 46,
            FOOTWORK = 47,
            TURN_L = 48,
            TURN_R = 49,
            TALK = 50,
            TALK_END = 51,
            CHANGEFORM = 52,
            CHANGEEND = 53,
            STUN_LOOP = 54,
            STUN_END = 55,
            MAGIC_FIRE = 56,
            MAGIC_FIRE_FINISH = 57,
            MAGIC_FIRE_AIR = 58,
            MAGIC_BLIZZARD = 59,
            MAGIC_BLIZZARD_FINISH = 60,
            MAGIC_BLIZZARD_AIR = 61,
            MAGIC_THUNDER = 62,
            MAGIC_THUNDER_FINISH = 63,
            MAGIC_THUNDER_AIR = 64,
            MAGIC_CURE = 65,
            MAGIC_CURE_FINISH = 66,
            MAGIC_CURE_AIR = 67,
            MAGIC_MAGNET = 68,
            MAGIC_MAGNET_FINISH = 69,
            MAGIC_MAGNET_AIR = 70,
            MAGIC_REFLECT = 71,
            MAGIC_REFLECT_FINISH = 72,
            MAGIC_REFLECT_AIR = 73,
            RTN_TURN = 129,
            CALL_SUMMON_END = 130,
            DEAD_LAND = 131,
            DEAD_AIR = 132,
            MEMO_IDLE = 133,
            SUBMENU_IDLE = 134,
            SUBMENU_ACTION1 = 135,
            SUBMENU_ACTION2 = 136,
            CALL_FRIEND = 137,
            CALL_SUMMON = 138,
            MENU_IDLE = 139,
            REFLECT00 = 140,
            REFLECT01 = 141,
            REFLECT02 = 142,
            REFLECT03 = 143,
            REFLECT04 = 144,
            REFLECT05 = 145,
            REFLECT06 = 146,
            REFLECT07 = 147,
            REFLECT08 = 148,
            REFLECT09 = 149,
            REFLECT10 = 150,
            EX000 = 151,
            EX001 = 152,
            EX002 = 153,
            EX003 = 154,
            EX004 = 155,
            EX005 = 156,
            EX006 = 157,
            EX007 = 158,
            EX008 = 159,
            EX009 = 160,
            EX010 = 161,
            EX011 = 162,
            EX012 = 163,
            EX013 = 164,
            EX014 = 165,
            EX015 = 166,
            EX016 = 167,
            EX017 = 168,
            EX018 = 169,
            EX019 = 170,
            EX020 = 171,
            EX021 = 172,
            EX022 = 173,
            EX023 = 174,
            EX024 = 175,
            EX025 = 176,
            EX026 = 177,
            EX027 = 178,
            EX028 = 179,
            EX029 = 180,
            EX030 = 181,
            EX031 = 182,
            EX032 = 183,
            EX033 = 184,
            EX034 = 185,
            EX035 = 186,
            EX036 = 187,
            EX037 = 188,
            EX038 = 189,
            EX039 = 190,
            EX040 = 191,
            EX041 = 192,
            EX042 = 193,
            EX043 = 194,
            EX044 = 195,
            EX045 = 196,
            EX046 = 197,
            EX047 = 198,
            EX048 = 199,
            EX049 = 200,
            EX050 = 201,
            EX051 = 202,
            EX052 = 203,
            EX053 = 204,
            EX054 = 205,
            EX055 = 206,
            EX056 = 207,
            EX057 = 208,
            EX058 = 209,
            EX059 = 210,
            EX060 = 211,
            EX061 = 212,
            EX062 = 213,
            EX063 = 214,
            EX064 = 215,
            EX065 = 216,
            EX066 = 217,
            EX067 = 218,
            EX068 = 219,
            EX069 = 220,
            EX070 = 221,
            EX071 = 222,
            EX072 = 223,
            EX073 = 224,
            EX074 = 225,
            EX075 = 226,
            EX076 = 227,
            EX077 = 228,
            EX078 = 229,
            EX079 = 230,
            EX080 = 231,
            EX081 = 232,
            EX082 = 233,
            EX083 = 234,
            EX084 = 235,
            EX085 = 236,
            EX086 = 237,
            EX087 = 238,
            EX088 = 239,
            EX089 = 240,
            EX090 = 241,
            EX091 = 242,
            EX092 = 243,
            EX093 = 244,
            EX094 = 245,
            EX095 = 246,
            EX096 = 247,
            EX097 = 248,
            EX098 = 249,
            EX099 = 250,
            EX100 = 251,
            RTN_00 = 283,
            RTN_01 = 284,
            RTN_02 = 285,
            RTN_03 = 286,
            RTN_04 = 287,
            RTN_05 = 288,
            RTN_06 = 289,
            RTN_07 = 290,
            RTN_08 = 291,
            RTN_09 = 292,
        }
    
        public static int GetMotionSetIndex(
            IList<Bar.Entry> barEntries, MotionName motionId, bool isBattle, bool hasWeapon)
        {
            const int MaxMotionCountPerAnim = 4;
            var ModeTable = new byte[]
            {
                0x00, 0x01, 0x03, 0x02, 0x01, 0x00, 0x02, 0x03,
                0x02, 0x03, 0x01, 0x00, 0x03, 0x02, 0x00, 0x01
            };

            var modeIndex = isBattle == false ? 2 : 1;
            if (hasWeapon)
                modeIndex ^= 1;

            for (var i = 0; i < MaxMotionCountPerAnim; i++)
            {
                var msetId = ModeTable[modeIndex * 4 + i] + (int)motionId * 4;
                //if (msetId == 0)
                //    continue;
                if (ContainsAnimation(barEntries[msetId]))
                    return msetId;
            }

            return -1;
        }

        private static bool ContainsAnimation(Bar.Entry entry) =>
            entry.Stream.Length > 0;
    }
}
