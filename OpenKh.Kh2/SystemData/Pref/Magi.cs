using System.Collections.Generic;
using System.IO;
using Xe.BinaryMapper;

namespace OpenKh.Kh2.SystemData
{
    public class Magi
    {
        //[Data] public uint Id { get; set; }
        [Data] public float FireRadius { get; set; }
        [Data] public float FireHeight { get; set; }
        [Data] public float FireTime { get; set; }
        [Data] public float BlizzardFadeTime { get; set; }
        [Data] public float BlizzardTime { get; set; }
        [Data] public float BlizzardSpeed { get; set; }
        [Data] public float BlizzardRadius { get; set; }
        [Data] public float BlizzardHitBack { get; set; }
        [Data] public float ThunderNoTargetDistance { get; set; }
        [Data] public float ThunderBorderHeight { get; set; }
        [Data] public float ThunderCheckHeight { get; set; }
        [Data] public float ThunderBurstRadius { get; set; }
        [Data] public float ThunderHeight { get; set; }
        [Data] public float ThunderRadius { get; set; }
        [Data] public float ThunderAttackWait { get; set; }
        [Data] public float ThunderTime { get; set; }
        [Data] public float CureRange { get; set; }
        [Data] public float MagnetMinYOffset { get; set; }
        [Data] public float MagnetLargeTime { get; set; }
        [Data] public float MagnetStayTime { get; set; }
        [Data] public float MagnetSmallTime { get; set; }
        [Data] public float MagnetRadius { get; set; }
        [Data] public float ReflectRadius { get; set; }
        [Data] public float ReflectLaserTime { get; set; }
        [Data] public float ReflectFinishTime { get; set; }
        [Data] public float ReflectLv1Radius { get; set; }
        [Data] public float ReflectLv1Height { get; set; }
        [Data] public int ReflectLv2Count { get; set; }
        [Data] public float ReflectLv2Radius { get; set; }
        [Data] public float ReflectLv2Height { get; set; }
        [Data] public int ReflectLv3Count { get; set; }
        [Data] public float ReflectLv3Radius { get; set; }
        [Data] public float ReflectLv3Height { get; set; }

    }
}
