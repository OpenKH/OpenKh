using System.Collections.Generic;
using System.IO;
using Xe.BinaryMapper;

namespace OpenKh.Kh2.SystemData
{
    public class Plyr
    {
        [Data] public float AttackYOffset { get; set; }
        [Data] public float AttackRadius { get; set; }
        [Data] public float AttackMinHeight { get; set; }
        [Data] public float AttackMaxHeight { get; set; }
        [Data] public float AttackVAngle { get; set; }
        [Data] public float AttackVRange { get; set; }
        [Data] public float AttackSRange { get; set; }
        [Data] public float AttackHAngle { get; set; }
        [Data] public float AttackUMinHeight { get; set; }
        [Data] public float AttackUMaxHeight { get; set; }
        [Data] public float AttackURange { get; set; }
        [Data] public float AttackJFront { get; set; }
        [Data] public float AttackAirMinHeight { get; set; }
        [Data] public float AttackAirMaxHeight { get; set; }
        [Data] public float AttackAirBigHeightOffset { get; set; }
        [Data] public float AttackUV0 { get; set; }
        [Data] public float AttackJV0 { get; set; }
        [Data] public float AttackFirstV0 { get; set; }
        [Data] public float AttackComboV0 { get; set; }
        [Data] public float AttackFinishV0 { get; set; }
        [Data] public float BlowRecoveryH { get; set; }
        [Data] public float BlowRecoveryV { get; set; }
        [Data] public float BlowRecoveryTime { get; set; }
        [Data] public float AutoLockonRange { get; set; }
        [Data] public float AutoLockonMinHeight { get; set; }
        [Data] public float AutoLockonMaxHeight { get; set; }
        [Data] public float AutoLockonTime { get; set; }
        [Data] public float AutoLockonHeightAdjust { get; set; }
        [Data] public float AutoLockonInnerAdjust { get; set; }
    }
}
