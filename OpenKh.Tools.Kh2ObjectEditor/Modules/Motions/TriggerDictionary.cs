using System.Collections.Generic;

namespace OpenKh.Tools.Kh2ObjectEditor.Modules.Motions
{
    public class TriggerDictionary
    {
        public static Dictionary<byte, string> Range = new Dictionary<byte, string>
        {
{0, "[0] FLAG: Grounded"}, // srk: interpolates animation back to idle
{1, "[1] FLAG: Air"}, // srk: lock animation
{2, "[2] _FLAG: Interpolate to idle"}, // wait for input
{3, "[3] FLAG: No gravity"},
{4, "[4] RC hitbox enabled"},
{5, "[5] _COLLISION_FLAG::disable"},
{6, "[6] _RECOM_FLAG::enable"},
{7, "[7] _RECOM_FLAG::disable"},
{8, "[8] <DEBUG print>"},
{9, "[9] Jump/Land"}, // srk: AI must play this animation for as long as this timer lasts
{10, "[10] Attack hitbox"}, // Also used by enemies. EG: LW dash 1st hit (2nd and 3rd are #33)
{11, "[11] State: Allows Combo"},
{12, "[12] _State: ???"}, // srk: Plays Keyblade trail effect
{13, "[13] _State: ???"}, // srk: glide helper
{14, "[14] State: Allows RC"},
{15, "[15] _State: ???"}, // srk: attack label
{16, "[16] _???"}, // srk: Enhanced movement
{17, "[17] _???"}, // srk: AI check's target position then determines if it can continue its combo or quits
{18, "[18] _???"}, // srk: AI check's target position then determines if it can continue its combo or quits
{19, "[19] _???"}, // srk: Disable invincibility
{20, "[20] _ReactionCommand"}, // srk: shows RC on a certain bone
{21, "[21] _State: ???"},
{22, "[22] Turn to target"},
{23, "[23] Texture Animation"},
{24, "[24] _unlabelled240 (Disable grav, keep kinetics)"},
{25, "[25] _AnmatrCommand"}, // srk: Shows a RC on a certain bone from another model
{26, "[26] _AnmatrCommand"},
{27, "[27] Hitbox off"},
{28, "[28] Turn to lock"},
{29, "[29] State: Can't leave ground"},
{30, "[30] _State: Freeze animation"},
{31, "[31] _State: ???"}, // srk: vibration
{32, "[32] _State: ???"},
{33, "[33] Attack Hitbox (Combo)"}, // Enemy's combo for Once More
{34, "[34] _State: ???"},
{35, "[35] _State: Allow Combo (Magic)"}, // srk: Allows magic of [value] to combo. 56-58 fire, 59-61 blizzard, 62-64 thunder, 65-67 cure, 68-70 magnet, 71-73 reflect. Values on 00battle/magc
{36, "[36] Pattern enable"},// Enables a pattern on the entity. Leads to an entry in the PATN table in 00battle.
{37, "[37] Pattern disable"}, // Disables a pattern applied to the entity.
{38, "[38] _State: ???"},
{39, "[39] _State: ???"},
{40, "[40] _State: ???"},
{41, "[41] _State: ???"}, // srk: Allows movement while animation is playing
{42, "[42] _State: ???"},
{43, "[43] _State: ???"},
{44, "[44] _State: ???"}, // srk: Not movable without damage
{45, "[45] _State: ???"},
{46, "[46] _State: ???"},
{47, "[47] _State: ???"},
{48, "[48] _State: ???"},
{49, "[49] State: Maintain motion on ground leave"}, // Keeps the motion going after the entity has left the ground
{50, "[50] State: Allow Combo Finisher"},
{51, "[51] _play_singleton_se"},
{52, "[52] _State: ???"},
{53, "[53] _State: ???"}
        };
        public static Dictionary<byte, string> Frame = new Dictionary<byte, string>
        {
{0, "[0] Action: Jump"},
{1, "[1] Trigger APDX Effect"},
{2, "[2] Play footstep sound"},
{3, "[3] Action: Jump (Dusk)"}, // Motion slot 628
{4, "[4] _OBJ::texanm_start"},
{5, "[5] _OBJ::texanm_stop"},
{6, "[6] Use Item"},
{7, "[7] _LIMIT::AnmatrCallback"}, // srk: AI activates a special projectile or alternate effect such as slowing down
{8, "[8] Play sound"},
{9, "[9] _VariousTrigger 1"}, // Fat Bandit: start flamethrower; No params - Coded in AI?
{10, "[10] _VariousTrigger 2"}, // Fat Bandit: finish flamethrower; No params - Coded in AI?
{11, "[11] _VariousTrigger 4"},
{12, "[12] _VariousTrigger 8"},
{13, "[13] Play Voice"}, // Check 00battle/vbtl
{14, "[14] Play Voice"},
{15, "[15] Turn to target"}, //srk: plays VAG lopVoice from AFM
{16, "[16] _DisableCommandTime"}, // srk: keyblade pop
{17, "[17] Cast Magic"}, // srk: Allows magic of [value] to combo. 56-58 fire, 59-61 blizzard, 62-64 thunder, 65-67 cure, 68-70 magnet, 71-73 reflect. Values on 00battle/magc
{18, "[18] <UNUSED>"},
{19, "[19] Trigger footstep effect"},
{20, "[20] <UNUSED>"},
{21, "[21] _Turn to lockon"},
{22, "[22] _Show weapon"},
{23, "[23] _Fade Out"},
{24, "[24] _Fade In"},
{25, "[25] _pVTable"},
{26, "[26] _set_parts_color"}, // srk: makes model disappear
{27, "[27] _reset_parts_color"}, // srk: makes model appear
{28, "[28] Revenge Check"},
{29, "[29] _Weapon appear"},
{30, "[30] _LIMIT:PlayVoice"},
{31, "[31] Controller vibration"},
{32, "[32] <UNUSED>"},
{33, "[33] <UNUSED>"},
{34, "[34] Quick Run Check"},
{35, "[35] _MotionStart"}
        };
    }
}
