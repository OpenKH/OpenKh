using OpenKh.Common;
using OpenKh.Common.Utils;
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Numerics;
using Xe.BinaryMapper;
using System.Linq;

namespace OpenKh.Bbs
{
    public class Epd
    {
        private const uint MagicCode = 0x40455044;
        private const uint version = 9;

        public class Header
        {
            [Data] public uint MagicCode { get; set; }
            [Data] public uint version { get; set; }
        }

        public class GeneralParameters
        {
            [Data] public uint StatusAilmentsFlag { get; set; }
            [Data] public float Health { get; set; }
            [Data] public float ExperienceMultiplier { get; set; }
            [Data] public uint Size { get; set; }
            [Data] public float PhysicalDamageMultiplier { get; set; }
            [Data] public float FireDamageMultiplier { get; set; }
            [Data] public float IceDamageMultiplier { get; set; }
            [Data] public float ThunderDamageMultiplier { get; set; }
            [Data] public float DarknessDamageMultiplier { get; set; }
            [Data] public float NonElementalDamageMultiplier { get; set; }
        }

        public struct StatusAilment
        {
            public bool bFly;
            public bool bSmallDamageReaction;
            public bool bSmallDamageReactionOnly;
            public bool bHitback;
            public bool dummy4;
            public bool dummy5;
            public bool dummy6;
            public bool dummy7;
            public bool dummy8;
            public bool dummy9;
            public bool bPoison;
            public bool bSlow;
            public bool bStop;
            public bool bBind;
            public bool bFaint;
            public bool bFreeze;
            public bool bBurn;
            public bool bConfuse;
            public bool bBlind;
            public bool bDeath;
            public bool bZeroGravity;
            public bool bMini;
            public bool bMagnet;
            public bool bDegen;
            public bool bSleep;
            public bool dummy25;
            public bool dummy26;
            public bool dummy27;
            public bool dummy28;
            public bool dummy29;
            public bool dummy30;
            public bool dummy31;
        }

        public class OtherParameters
        {
            [Data] public ushort DamageCeiling { get; set; }
            [Data] public ushort DamageFloor { get; set; }
            [Data] public float fWeight { get; set; }
            [Data] public uint EffectivenessFlag { get; set; }
            [Data] public sbyte PrizeBoxProbability { get; set; }
            [Data(Count = 3)] public byte[] padding { get; set; }
            [Data] public uint TechniqueParameterCount { get; set; }
            [Data] public uint TechniqueParameterOffset { get; set; }
            [Data] public uint DropItemsCount { get; set; }
            [Data] public uint DropItemsOffset { get; set; }
            [Data] public uint ExtraParametersCount { get; set; }
            [Data] public uint ExtraParametersOffset { get; set; }
        }

        public struct EffectivenessFlag
        {
            public uint Poison;
            public uint Stop;
            public uint Bind;
            public uint Faint;
            public uint Blind;
            public uint Mini;
        }

        public class TechniqueParameters
        {
            [Data] public float TechniquePowerCorrection { get; set; }
            [Data] public byte TechniqueNumber { get; set; }
            [Data] public byte TechniqueKind { get; set; }
            [Data] public byte TechniqueAttribute { get; set; }
            [Data] public byte SuccessRate { get; set; }
        }

        public class DropParameters
        {
            [Data] public uint ItemIndex { get; set; }
            [Data] public ushort ItemCount { get; set; }
            [Data] public ushort Probability { get; set; }
        }

        public enum DropKind
        {
            ITEM_KIND_HP_SMALL = 0,
            ITEM_KIND_HP_BIG = 1,
            ITEM_KIND_MUNNY_SMALL = 2,
            ITEM_KIND_MUNNY_MIDDLE = 3,
            ITEM_KIND_MUNNY_BIGL = 4,
            ITEM_KIND_FOCUS_SMALL = 5,
            ITEM_KIND_FOCUS_BIG = 6,
            ITEM_KIND_DRAINMIST = 7,
            ITEM_KIND_D_LINK = 8,
        }

        public class ExtraParameters
        {
            [Data(Count = 12)] public string ParameterName { get; set; }
            [Data] public float ParameterValue { get; set; }
        }

        public enum AttackAttribute
        {
            ATK_ATTR_NONE = 0,
            ATK_ATTR_PHYSICAL = 1,
            ATK_ATTR_FIRE = 2,
            ATK_ATTR_ICE = 3,
            ATK_ATTR_THUNDER = 4,
            ATK_ATTR_DARK = 5,
            ATK_ATTR_ZERO = 6,
            ATK_ATTR_SPECIAL = 7
        }

        public enum AttackKind
        {
            ATK_KIND_NONE = 0,
            ATK_KIND_DMG_SMALL = 1,
            ATK_KIND_DMG_BIG = 2,
            ATK_KIND_DMG_BLOW = 3,
            ATK_KIND_DMG_TOSS = 4,
            ATK_KIND_DMG_BEAT = 5,
            ATK_KIND_DMG_FLICK = 6,
            ATK_KIND_POISON = 7,
            ATK_KIND_SLOW = 8,
            ATK_KIND_STOP = 9,
            ATK_KIND_BIND = 10,
            ATK_KIND_FAINT = 11,
            ATK_KIND_FREEZE = 12,
            ATK_KIND_BURN = 13,
            ATK_KIND_CONFUSE = 14,
            ATK_KIND_BLIND = 15,
            ATK_KIND_DEATH = 16,
            ATK_KIND_KILL = 17,
            ATK_KIND_CAPTURE = 18,
            ATK_KIND_MAGNET = 19,
            ATK_KIND_ZEROGRAVITY = 20,
            ATK_KIND_AERO = 21,
            ATK_KIND_TORNADO = 22,
            ATK_KIND_DEGENERATOR = 23,
            ATK_KIND_WITHOUT = 24,
            ATK_KIND_EAT = 25,
            ATK_KIND_TREASURERAID = 26,
            ATK_KIND_SLEEPINGDEATH = 27,
            ATK_KIND_SLEEP = 28,
            ATK_KIND_MAGNET_MUNNY = 29,
            ATK_KIND_MAGNET_HP = 30,
            ATK_KIND_MAGNET_FOCUS = 31,
            ATK_KIND_MINIMUM = 32,
            ATK_KIND_QUAKE = 33,
            ATK_KIND_RECOVER = 34,
            ATK_KIND_DISCOMMAND = 35,
            ATK_KIND_DISPRIZE_M = 36,
            ATK_KIND_DISPRIZE_H = 37,
            ATK_KIND_DISPRIZE_F = 38,
            ATK_KIND_DETONE = 39,
            ATK_KIND_GM_BLOW = 40,
            ATK_KIND_BLAST = 41,
            ATK_KIND_MAGNESPIRAL = 42,
            ATK_KIND_GLACIALARTS = 43,
            ATK_KIND_TRANSCENDENCE = 44,
            ATK_KIND_VENGEANCE = 45,
            ATK_KIND_MAGNEBREAKER = 46,
            ATK_KIND_MAGICIMPULSE_CF = 47,
            ATK_KIND_MAGICIMPULSE_CFB = 48,
            ATK_KIND_MAGICIMPULSE_CFBB = 49,
            ATK_KIND_DMG_RISE = 50,
            ATK_KIND_STUMBLE = 51,
            ATK_KIND_MOUNT = 52,
            ATK_KIND_IMPRISONMENT = 53,
            ATK_KIND_SLOWSTOP = 54,
            ATK_KIND_GATHERING = 55,
            ATK_KIND_EXHAUSTED = 56
        }

        public static StatusAilment GetStatusAilment(Epd epd)
        {
            StatusAilment stat = new StatusAilment();
            stat.bFly = BitsUtil.Int.GetBit((int)epd.generalParameters.StatusAilmentsFlag, 0);
            stat.bSmallDamageReaction = BitsUtil.Int.GetBit((int)epd.generalParameters.StatusAilmentsFlag, 1);
            stat.bSmallDamageReactionOnly = BitsUtil.Int.GetBit((int)epd.generalParameters.StatusAilmentsFlag, 2);
            stat.bHitback = BitsUtil.Int.GetBit((int)epd.generalParameters.StatusAilmentsFlag, 3);
            stat.bPoison = BitsUtil.Int.GetBit((int)epd.generalParameters.StatusAilmentsFlag, 10);
            stat.bSlow = BitsUtil.Int.GetBit((int)epd.generalParameters.StatusAilmentsFlag, 11);
            stat.bStop = BitsUtil.Int.GetBit((int)epd.generalParameters.StatusAilmentsFlag, 12);
            stat.bBind = BitsUtil.Int.GetBit((int)epd.generalParameters.StatusAilmentsFlag, 13);
            stat.bFaint = BitsUtil.Int.GetBit((int)epd.generalParameters.StatusAilmentsFlag, 14);
            stat.bFreeze = BitsUtil.Int.GetBit((int)epd.generalParameters.StatusAilmentsFlag, 15);
            stat.bBurn = BitsUtil.Int.GetBit((int)epd.generalParameters.StatusAilmentsFlag, 16);
            stat.bConfuse = BitsUtil.Int.GetBit((int)epd.generalParameters.StatusAilmentsFlag, 17);
            stat.bBlind = BitsUtil.Int.GetBit((int)epd.generalParameters.StatusAilmentsFlag, 18);
            stat.bDeath = BitsUtil.Int.GetBit((int)epd.generalParameters.StatusAilmentsFlag, 19);
            stat.bZeroGravity = BitsUtil.Int.GetBit((int)epd.generalParameters.StatusAilmentsFlag, 20);
            stat.bMini = BitsUtil.Int.GetBit((int)epd.generalParameters.StatusAilmentsFlag, 21);
            stat.bMagnet = BitsUtil.Int.GetBit((int)epd.generalParameters.StatusAilmentsFlag, 22);
            stat.bDegen = BitsUtil.Int.GetBit((int)epd.generalParameters.StatusAilmentsFlag, 23);
            stat.bSleep = BitsUtil.Int.GetBit((int)epd.generalParameters.StatusAilmentsFlag, 24);

            return stat;
        }

        public static uint GetStatusAilmentFromStates(bool Fly, bool SmallDamage, bool SmallDamageOnly, bool Hitback, bool Poison, bool Slow,
                                                      bool Stop, bool Bind, bool Faint, bool Freeze, bool Burn, bool Confuse, bool Blind, bool Death,
                                                      bool ZeroGravity, bool Mini, bool Magnet, bool Degen, bool Sleep)
        {
            uint AilmentFlag = 0;

            AilmentFlag = BitsUtil.Int.SetBit(AilmentFlag, 0, Fly);
            AilmentFlag = BitsUtil.Int.SetBit(AilmentFlag, 1, SmallDamage);
            AilmentFlag = BitsUtil.Int.SetBit(AilmentFlag, 2, SmallDamageOnly);
            AilmentFlag = BitsUtil.Int.SetBit(AilmentFlag, 3, Hitback);
            AilmentFlag = BitsUtil.Int.SetBit(AilmentFlag, 10, Poison);
            AilmentFlag = BitsUtil.Int.SetBit(AilmentFlag, 11, Slow);
            AilmentFlag = BitsUtil.Int.SetBit(AilmentFlag, 12, Stop);
            AilmentFlag = BitsUtil.Int.SetBit(AilmentFlag, 13, Bind);
            AilmentFlag = BitsUtil.Int.SetBit(AilmentFlag, 14, Faint);
            AilmentFlag = BitsUtil.Int.SetBit(AilmentFlag, 15, Freeze);
            AilmentFlag = BitsUtil.Int.SetBit(AilmentFlag, 16, Burn);
            AilmentFlag = BitsUtil.Int.SetBit(AilmentFlag, 17, Confuse);
            AilmentFlag = BitsUtil.Int.SetBit(AilmentFlag, 18, Blind);
            AilmentFlag = BitsUtil.Int.SetBit(AilmentFlag, 19, Death);
            AilmentFlag = BitsUtil.Int.SetBit(AilmentFlag, 20, ZeroGravity);
            AilmentFlag = BitsUtil.Int.SetBit(AilmentFlag, 21, Mini);
            AilmentFlag = BitsUtil.Int.SetBit(AilmentFlag, 22, Magnet);
            AilmentFlag = BitsUtil.Int.SetBit(AilmentFlag, 23, Degen);
            AilmentFlag = BitsUtil.Int.SetBit(AilmentFlag, 24, Sleep);

            return AilmentFlag;
        }

        public static uint GetEffectivenessFlagFromStates(uint Poison, uint Stop, uint Bind, uint Faint, uint Blind, uint Minimum)
        {
            uint Effectiveness = 0;
            Effectiveness = BitsUtil.Int.SetBits(Effectiveness, 0, 2, Poison);
            Effectiveness = BitsUtil.Int.SetBits(Effectiveness, 2, 2, Stop);
            Effectiveness = BitsUtil.Int.SetBits(Effectiveness, 4, 2, Bind);
            Effectiveness = BitsUtil.Int.SetBits(Effectiveness, 6, 2, Faint);
            Effectiveness = BitsUtil.Int.SetBits(Effectiveness, 8, 2, Blind);
            Effectiveness = BitsUtil.Int.SetBits(Effectiveness, 10, 2, Minimum);

            return Effectiveness;
        }

        public static EffectivenessFlag GetEffectivenessFlag(Epd epd)
        {
            EffectivenessFlag flag = new EffectivenessFlag();
            int val = (int)epd.otherParameters.EffectivenessFlag;
            flag.Poison = (uint)BitsUtil.Int.GetBits(val, 0, 2);
            flag.Stop = (uint)BitsUtil.Int.GetBits(val, 2, 2);
            flag.Bind = (uint)BitsUtil.Int.GetBits(val, 4, 2);
            flag.Faint = (uint)BitsUtil.Int.GetBits(val, 6, 2);
            flag.Blind = (uint)BitsUtil.Int.GetBits(val, 8, 2);
            flag.Mini = (uint)BitsUtil.Int.GetBits(val, 10, 2);

            return flag;
        }

        public Header header;
        public GeneralParameters generalParameters;
        public List<char[]> AnimationList = new List<char[]>();
        public OtherParameters otherParameters;
        public List<TechniqueParameters> techniqueParameters = new List<TechniqueParameters>();
        public List<DropParameters> dropParameters = new List<DropParameters>();
        public List<ExtraParameters> extraParameters = new List<ExtraParameters>();

        public static Epd Read(Stream stream)
        {
            Epd epd = new Epd();
            epd.header = BinaryMapping.ReadObject<Header>(stream);
            epd.generalParameters = BinaryMapping.ReadObject<GeneralParameters>(stream);
            BinaryReader r = new BinaryReader(stream);

            for (int i = 0; i < 18; i++)
            {
                char[] animName = r.ReadChars(4);
                epd.AnimationList.Add(animName);
            }
            stream.Seek(8, SeekOrigin.Current);


            epd.otherParameters = BinaryMapping.ReadObject<OtherParameters>(stream);

            stream.Seek(epd.otherParameters.TechniqueParameterOffset, SeekOrigin.Begin);
            for (int t = 0; t < epd.otherParameters.TechniqueParameterCount; t++)
            {
                epd.techniqueParameters.Add(BinaryMapping.ReadObject<TechniqueParameters>(stream));
            }

            stream.Seek(epd.otherParameters.DropItemsOffset, SeekOrigin.Begin);
            for (int t = 0; t < epd.otherParameters.DropItemsCount; t++)
            {
                epd.dropParameters.Add(BinaryMapping.ReadObject<DropParameters>(stream));
            }

            stream.Seek(epd.otherParameters.ExtraParametersOffset, SeekOrigin.Begin);
            for (int t = 0; t < epd.otherParameters.ExtraParametersCount; t++)
            {
                epd.extraParameters.Add(BinaryMapping.ReadObject<ExtraParameters>(stream));
            }

            return epd;
        }

        public static void Write(Stream stream, Epd epd)
        {
            BinaryMapping.WriteObject<Header>(stream, epd.header);
            BinaryMapping.WriteObject<GeneralParameters>(stream, epd.generalParameters);
            BinaryWriter w = new BinaryWriter(stream);

            foreach (char[] anim in epd.AnimationList)
            {
                w.Write(anim);
            }

            stream.Write((uint)0);
            stream.Write((uint)0);

            BinaryMapping.WriteObject<OtherParameters>(stream, epd.otherParameters);

            foreach (TechniqueParameters param in epd.techniqueParameters)
            {
                BinaryMapping.WriteObject<TechniqueParameters>(stream, param);
            }

            foreach (DropParameters param in epd.dropParameters)
            {
                BinaryMapping.WriteObject<DropParameters>(stream, param);
            }

            foreach (ExtraParameters param in epd.extraParameters)
            {
                BinaryMapping.WriteObject<ExtraParameters>(stream, param);
            }
        }

        public static bool IsValid(Stream stream) =>
           stream.Length >= 0x10 &&
           stream.SetPosition(0).ReadUInt32() == MagicCode &&
           stream.SetPosition(4).ReadUInt32() == version;
    }
}
