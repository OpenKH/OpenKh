using System;
using System.Collections.Generic;
using System.IO;

namespace OpenKh.Kh2.Battle
{
    /// <summary>
    /// Unfinished
    /// </summary>
    public class Atkp
    {
        public enum AttackType : byte
        {
            NormalAttack = 0,
            PierceArmor = 1,
            Guard = 2,
            SGuard = 3,
            Special = 4,
            Cure = 5,
            CCure = 6,
        }

        [Flags]
        public enum AttackFlags : byte
        {
            BGHit = 0x1,
            LimitPAX = 0x2,
            Land = 0x4,
            CapturePAX = 0x8,
            ThankYou = 0x10,
            KillBoss = 0x20,
        }

        public enum Refact : byte
        {
            Reflect = 0,
            Guard = 1,
            Nothing = 2,
        }

        public enum TrReaction : byte
        {
            Attack = 0,
            Charge = 1,
            Crash = 2,
            Wall = 3,
        }

        [Flags]
        public enum AttackKind : byte
        {
            ComboFinisher = 0x1,
            AirComboFinisher = 0x2,
            ReactionCommand = 0x4,
        }

        public ushort SubId { get; set; }
        public ushort Id { get; set; }
        public AttackType Type { get; set; }
        public byte CriticalAdjust { get; set; }
        public ushort Power { get; set; }
        public byte Team { get; set; }
        public byte Element { get; set; }
        public byte EnemyReaction { get; set; }
        public byte EffectOnHit{ get; set; }
        public short KnockbackStrength1 { get; set; }
        public short KnockbackStrength2 { get; set; }
        public short Unknown { get; set; }
        public byte Flags { get; set; }
        public Refact RefactSelf { get; set; }
        public Refact RefactOther { get; set; }
        public byte ReflectedMotion { get; set; }
        public short ReflectHitBack { get; set; }
        public int ReflectAction { get; set; }
        public int ReflectHitSound { get; set; }
        public ushort ReflectRC { get; set; }
        public byte ReflectRange { get; set; }
        public sbyte ReflectAngle { get; set; }
        public byte DamageEffect { get; set; }
        public byte Switch { get; set; }
        public ushort Interval { get; set; }
        public byte FloorCheck { get; set; }
        public byte DriveDrain { get; set; }
        public byte RevengeDamage { get; set; }
        public TrReaction AttackTrReaction { get; set; }
        public byte ComboGroup { get; set; }
        public byte RandomEffect { get; set; }
        public byte Kind { get; set; }
        public byte HpDrain { get; set; }

        public static List<Atkp> Read(Stream stream) => BaseTable<Atkp>.Read(stream);

        public static void Write(Stream stream, IEnumerable<Atkp> items) =>
            BaseTable<Atkp>.Write(stream, 6, items);
    }
}
