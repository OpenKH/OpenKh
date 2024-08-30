using System.Collections.Generic;
using System.IO;
using Xe.BinaryMapper;

namespace OpenKh.Kh2.SystemData
{
    public class Sstm
    {
        //[Data] public uint Id { get; set; }
        [Data] public float CeilingStop { get; set; }
        [Data] public float CeilingDisableCommandTime { get; set; }
        [Data] public float HangRangeH { get; set; }
        [Data] public float HangRangeL { get; set; }
        [Data] public float HangRangeXZ { get; set; }
        [Data] public float FallMax { get; set; }
        [Data] public float BlowBrakeXZ { get; set; }
        [Data] public float BlowMinXZ { get; set; }
        [Data] public float BlowBrakeUp { get; set; }
        [Data] public float BlowUp { get; set; }
        [Data] public float BlowSpeed { get; set; }
        [Data] public float BlowToHitBack { get; set; }
        [Data] public float HitBack { get; set; }
        [Data] public float HitBackSmall { get; set; }
        [Data] public float HitBackToJump { get; set; }
        [Data] public float FlyBlowBrake { get; set; }
        [Data] public float FlyBlowStop { get; set; }
        [Data] public float FlyBlowUpAdjust { get; set; }
        [Data] public float MagicJump { get; set; }
        [Data] public float LockOnRange { get; set; }
        [Data] public float LockOnReleaseRange { get; set; }
        [Data] public float StunRecovery { get; set; }
        [Data] public float StunRecoveryHp { get; set; }
        [Data] public float StunRelax { get; set; }
        [Data] public float DriveEnemy { get; set; }
        [Data] public float ChangeTimeEnemy { get; set; }
        [Data] public float DriveTime { get; set; }
        [Data] public float DriveTimeRelax { get; set; }
        [Data] public float ChangeTimeAddRate { get; set; }
        [Data] public float ChangeTimeSubRate { get; set; }
        [Data] public float MpDriveRate { get; set; }
        [Data] public float MpToMpDrive { get; set; }
        [Data] public float SummonTimeRelax { get; set; }
        [Data] public float SummonPrayTime { get; set; }
        [Data] public float SummonPrayTimeSkip { get; set; }
        [Data] public int AntiFormDriveCount { get; set; }
        [Data] public int AntiFormSubCount { get; set; }
        [Data] public float AntiFormDamageRate { get; set; }
        [Data] public float FinalFormRate { get; set; }
        [Data] public float FinalFormMulRate { get; set; }
        [Data] public float FinalFormMaxRate { get; set; }
        [Data] public int FinalFormSubCount { get; set; }
        [Data] public float AttackDistanceToSpeed { get; set; }
        [Data] public float AlCarpetDashInner { get; set; }
        [Data] public float AlCarpetDashDelay { get; set; }
        [Data] public float AlCarpetDashAcceleration { get; set; }
        [Data] public float AlCarpetDashBrake { get; set; }
        [Data] public float LkDashDriftInner { get; set; }
        [Data] public float LkDashDriftTime { get; set; }
        [Data] public float LkDashAccelerationDrift { get; set; }
        [Data] public float LkDashAccelerationStop { get; set; }
        [Data] public float LkDashDriftSpeed { get; set; }
        [Data] public float LkMagicJump { get; set; }
        [Data] public float MickeyChargeWait { get; set; }
        [Data] public float MickeyDownRate { get; set; }
        [Data] public float MickeyMinRate { get; set; }
        [Data] public float LmSwimSpeed { get; set; }
        [Data] public float LmSwimControl { get; set; }
        [Data] public float LmSwimAcceleration { get; set; }
        [Data] public float LmDolphinAcceleration { get; set; }
        [Data] public float LmDolphinSpeedMax { get; set; }
        [Data] public float LmDolphinSpeedMin { get; set; }
        [Data] public float LmDolphinSpeedMaxDistance { get; set; }
        [Data] public float LmDolphinSpeedMinDistance { get; set; }
        [Data] public float LmDolphinRotationMax { get; set; }
        [Data] public float LmDolphinRotationDistance { get; set; }
        [Data] public float LmDolphinRotationMaxDistance { get; set; }
        [Data] public float LmDolphinDistanceToTime { get; set; }
        [Data] public float LmDolphinTimeMax { get; set; }
        [Data] public float LmDolphinTimeMin { get; set; }
        [Data] public float LmDolphinNearSpeed { get; set; }
        [Data] public int DriveBerserkAttack { get; set; }
        [Data] public float MpHaste { get; set; }
        [Data] public float MpHastera { get; set; }
        [Data] public float MpHastega { get; set; }
        [Data] public float DrawRange { get; set; }
        [Data] public int ComboDamageUp { get; set; }
        [Data] public int ReactionDamageUp { get; set; }
        [Data] public float DamageDrive { get; set; }
        [Data] public float DriveBoost { get; set; }
        [Data] public float FormBoost { get; set; }
        [Data] public float ExpChance { get; set; }
        [Data] public int Defender { get; set; }
        [Data] public int ElementUp { get; set; }
        [Data] public float DamageAspir { get; set; }
        [Data] public float HyperHeal { get; set; }
        [Data] public float CombinationBoost { get; set; }
        [Data] public float PrizeUp { get; set; }
        [Data] public float LuckUp { get; set; }
        [Data] public int ItemUp { get; set; }
        [Data] public float AutoHeal { get; set; }
        [Data] public float SummonBoost { get; set; }
        [Data] public float DriveConvert { get; set; }
        [Data] public float DefenseMaster { get; set; }
        [Data] public int DefenseMasterRatio { get; set; }

        public class SstmPatch
        {
            public Dictionary<string, float> Properties { get; set; } = new Dictionary<string, float>();
        }
		
		//Read/Write doesn't currently work. 
        public static List<Sstm> Read(Stream stream) => BaseTableSstm<Sstm>.Read(stream);

        public static void Write(Stream stream, IEnumerable<Sstm> entries) => BaseTableSstm<Sstm>.Write(stream, entries);

    }

}
