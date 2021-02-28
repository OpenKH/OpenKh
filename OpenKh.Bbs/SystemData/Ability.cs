using System;
using System.Collections.Generic;
using System.Text;

namespace OpenKh.Bbs.SystemData
{
    public class Ability
    {
        public enum Type : ushort
        {
            ABILITY_KIND_None = 451,
            ABILITY_KIND_Draw = 452,
            ABILITY_KIND_HpPrizeUp = 453,
            ABILITY_KIND_FpPrizeUp = 454,
            ABILITY_KIND_LuckUp = 455,
            ABILITY_KIND_HpUp = 456,
            ABILITY_KIND_Exp0 = 457,
            ABILITY_KIND_FireUp = 458,
            ABILITY_KIND_BlizzardUp = 459,
            ABILITY_KIND_ThunderUp = 460,
            ABILITY_KIND_CureUp = 461,
            ABILITY_KIND_ItemUp = 462,
            ABILITY_KIND_AttackHaste = 463,
            ABILITY_KIND_MagicHaste = 464,
            ABILITY_KIND_FinishUp = 465,
            ABILITY_KIND_SFinishUp = 466,
            ABILITY_KIND_ComboPlus = 467,
            ABILITY_KIND_AirComboPlus = 468,
            ABILITY_KIND_FireGuard = 469,
            ABILITY_KIND_BlizzardGuard = 470,
            ABILITY_KIND_ThunderGuard = 471,
            ABILITY_KIND_DarkGuard = 472,
            ABILITY_KIND_ReloadBoost = 473,
            ABILITY_KIND_Defender = 474,
            ABILITY_KIND_ExpChance = 475,
            ABILITY_KIND_WalkingExp = 476,
            ABILITY_KIND_DamageAspir = 477,
            ABILITY_KIND_LastLeave = 478,
            ABILITY_KIND_ComboLeave = 479,
            ABILITY_KIND_Libra = 480,
            ABILITY_KIND_LeafVeil = 481,
        }
    }
}
