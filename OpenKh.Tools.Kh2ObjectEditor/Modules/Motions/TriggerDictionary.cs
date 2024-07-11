using System.Collections.Generic;

namespace OpenKh.Tools.Kh2ObjectEditor.Modules.Motions
{
    public class TriggerDictionary
    {
        public static Dictionary<byte, string> Range = new Dictionary<byte, string>
        {
{0, "[0] STATE: Grounded"},
{1, "[1] STATE: Air"},
{2, "[2] STATE: Blend to idle"},
{3, "[3] STATE: No gravity"},
{4, "[4] Enable collision"},
{5, "[5] Disable collision"},
{6, "[6] Enable conditional RC"},
{7, "[7] Disable conditional RC"}, // Unused
{8, "[8] <DEBUG>"}, // Unused
{9, "[9] STATE: Jump/Land"},
{10, "[10] Attack hitbox"},
{11, "[11] STATE: Allow combo"},
{12, "[12] STATE: Weapon swing (Display trail)"},
{13, "[13] _STATE: ???"},
{14, "[14] STATE: Allow RC"},
{15, "[15] _STATE: ???"}, // Unused; srk: attack label
{16, "[16] AI: Special movement"},
{17, "[17] _AI: ???"}, // srk: AI check's target position then determines if it can continue its combo or quits
{18, "[18] _AI: ???"}, // srk: AI check's target position then determines if it can continue its combo or quits
{19, "[19] AI: Disable invincibility"},
{20, "[20] Reaction Command (Self)"},
{21, "[21] _STATE: ???"},
{22, "[22] ACTION: Turn to target"},
{23, "[23] Texture Animation"},
{24, "[24] _STATE: No gravity, keep kinetics"},
{25, "[25] _STATE: AnmatrCommand"},
{26, "[26] _STATE: AnmatrCommand 2"},
{27, "[27] STATE: Hitbox off"},
{28, "[28] ACTION: Turn to lock"},
{29, "[29] STATE: Can't leave ground"},
{30, "[30] _STATE: Freeze animation"},
{31, "[31] _STATE: ???"}, // srk: vibration
{32, "[32] _STATE: ???"},
{33, "[33] Attack hitbox (Combo)"}, // Enemy's combo for Once More
{34, "[34] _STATE: ???"},
{35, "[35] STATE: Allow combo (Magic)"}, // Allows combo if Param1 = MOTION.Id; srk: Allows magic of [value] to combo. 56-58 fire, 59-61 blizzard, 62-64 thunder, 65-67 cure, 68-70 magnet, 71-73 reflect. Values on 00battle/magc
{36, "[36] Pattern enable"}, // Enables a pattern on the entity. Leads to an entry in the PATN table in 00battle.
{37, "[37] Pattern disable"}, // Disables a pattern applied to the entity.
{38, "[38] _STATE: Allow drop from on top"}, // Allows Sora to drop from top of entity
{39, "[39] _STATE: ???"},
{40, "[40] _STATE: ???"},
{41, "[41] _STATE: Allow movement"},
{42, "[42] PHYSICS: Keep momentum & restrict movement"},
{43, "[43] <NOT CODED>"},
{44, "[44] PHYSICS: Immovable by friction"},
{45, "[45] _ACTION: ???"},
{46, "[46] _ACTION: <UNUSED>"},
{47, "[47] ACTION: Rotate towards movement direction"},
{48, "[48] <NOT CODED>"},
{49, "[49] ACTION: Maintain motion on ground leave"}, // Keeps the motion going after the entity has left the ground
{50, "[50] ACTION: Allow combo finisher"},
{51, "[51] Play singleton sound effect"},
{52, "[52] ACTION: Stop actions"},
{53, "[53] _ACTION: ???"}
        };
        public static Dictionary<byte, string> Frame = new Dictionary<byte, string>
        {
{0, "[0] Action: Jump"},
{1, "[1] Trigger effect caster"},
{2, "[2] Play footstep sound"},
{3, "[3] Action: Jump (Dusk)"}, // Motion slot 628
{4, "[4] Texture animation start"},
{5, "[5] Texture animation stop"},
{6, "[6] Use item"},
{7, "[7] Game Effect"}, // srk: AI activates a special projectile or alternate effect such as slowing down
{8, "[8] Play sound effect"},
{9, "[9] _VariousTrigger 1"}, // Fat Bandit: start flamethrower; No params - Coded in AI?
{10, "[10] _VariousTrigger 2"}, // Fat Bandit: finish flamethrower; No params - Coded in AI?
{11, "[11] _VariousTrigger 4"},
{12, "[12] _VariousTrigger 8"},
{13, "[13] Play voice"},
{14, "[14] Play voice"},
{15, "[15] Turn to target"}, //srk: plays VAG lopVoice from AFM
{16, "[16] _DisableCommandTime"}, // srk: keyblade pop
{17, "[17] Cast magic"}, // srk: Allows magic of [value] to combo. 56-58 fire, 59-61 blizzard, 62-64 thunder, 65-67 cure, 68-70 magnet, 71-73 reflect. Values on 00battle/magc
{18, "[18] <NOT CODED>"},
{19, "[19] Footstep effect"},
{20, "[20] <NOT CODED>"},
{21, "[21] _Turn to lockon"},
{22, "[22] Make the weapon appear"},
{23, "[23] Fade Out"},
{24, "[24] Fade In"},
{25, "[25] _Call entity function"},
{26, "[26] Set mesh color grey"},
{27, "[27] Reset mesh color"},
{28, "[28] Revenge Check"},
{29, "[29] Make the weapon appear with effect"},
{30, "[30] _LIMIT:PlayVoice"},
{31, "[31] Controller vibration"},
{32, "[32] <NOT CODED>"},
{33, "[33] <NOT CODED>"},
{34, "[34] Quick run check"},
{35, "[35] Transition to fall if on air"}
        };
    }
}
