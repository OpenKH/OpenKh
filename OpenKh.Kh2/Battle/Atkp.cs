using System;
using System.Collections.Generic;
using System.IO;
using Xe.BinaryMapper;

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

        [Data] public ushort SubId { get; set; }
        [Data] public ushort Id { get; set; }
        [Data] public AttackType Type { get; set; }
        [Data] public byte CriticalAdjust { get; set; }
        [Data] public ushort Power { get; set; }
        [Data] public byte Team { get; set; }
        [Data] public byte Element { get; set; }
        [Data] public byte EnemyReaction { get; set; }
        [Data] public byte EffectOnHit { get; set; }
        [Data] public short KnockbackStrength1 { get; set; }
        [Data] public short KnockbackStrength2 { get; set; }
        [Data] public short Unknown { get; set; }
        [Data] public AttackFlags Flags { get; set; }
        [Data] public Refact RefactSelf { get; set; }
        [Data] public Refact RefactOther { get; set; }
        [Data] public byte ReflectedMotion { get; set; }
        [Data] public short ReflectHitBack { get; set; }
        [Data] public int ReflectAction { get; set; }
        [Data] public int ReflectHitSound { get; set; }
        [Data] public ushort ReflectRC { get; set; }
        [Data] public byte ReflectRange { get; set; }
        [Data] public sbyte ReflectAngle { get; set; }
        [Data] public byte DamageEffect { get; set; }
        [Data] public byte Switch { get; set; }
        [Data] public ushort Interval { get; set; }
        [Data] public byte FloorCheck { get; set; }
        [Data] public byte DriveDrain { get; set; }
        [Data] public byte RevengeDamage { get; set; }
        [Data] public TrReaction AttackTrReaction { get; set; }
        [Data] public byte ComboGroup { get; set; }
        [Data] public byte RandomEffect { get; set; }
        [Data] public AttackKind Kind { get; set; }
        [Data] public byte HpDrain { get; set; }
        private static byte[] endBytes = new byte[]
        {
                                                            0x66, 0x6F, 0x6F, 0x74, 0x77, 0x6F, 0x72, 0x6B,
            0x00, 0x6D, 0x6F, 0x64, 0x65, 0x5F, 0x72, 0x65, 0x76, 0x65, 0x6E, 0x67, 0x65, 0x00, 0x72, 0x65,
            0x66, 0x5F, 0x62, 0x6C, 0x6F, 0x77, 0x00, 0x64, 0x6F, 0x77, 0x6E, 0x00, 0x73, 0x74, 0x75, 0x6E,
            0x00, 0x66, 0x69, 0x67, 0x68, 0x74, 0x00, 0x66, 0x6F, 0x6F, 0x74, 0x77, 0x6F, 0x72, 0x6B, 0x5F,
            0x73, 0x65, 0x61, 0x72, 0x63, 0x68, 0x31, 0x00, 0x66, 0x6F, 0x6F, 0x74, 0x77, 0x6F, 0x72, 0x6B,
            0x5F, 0x73, 0x65, 0x61, 0x72, 0x63, 0x68, 0x32, 0x00, 0x69, 0x64, 0x6C, 0x65, 0x00, 0x61, 0x74,
            0x6B, 0x5F, 0x72, 0x69, 0x6E, 0x67, 0x5F, 0x65, 0x6E, 0x64, 0x00, 0x73, 0x65, 0x6C, 0x66, 0x5F,
            0x72, 0x65, 0x66, 0x00, 0x63, 0x68, 0x61, 0x6E, 0x67, 0x65, 0x00, 0x66, 0x6F, 0x6F, 0x74, 0x77,
            0x6F, 0x72, 0x6B, 0x5F, 0x61, 0x69, 0x72, 0x00, 0x6D, 0x6F, 0x76, 0x65, 0x5F, 0x64, 0x6F, 0x77,
            0x6E, 0x00, 0x74, 0x68, 0x69, 0x6E, 0x6B, 0x00, 0x6D, 0x6F, 0x64, 0x65, 0x5F, 0x62, 0x61, 0x74,
            0x74, 0x6C, 0x65, 0x5F, 0x62, 0x6F, 0x73, 0x73, 0x00, 0x72, 0x65, 0x66, 0x5F, 0x73, 0x68, 0x6F,
            0x75, 0x74, 0x00, 0x6D, 0x6F, 0x64, 0x65, 0x5F, 0x62, 0x61, 0x74, 0x74, 0x6C, 0x65, 0x00, 0x6D,
            0x6F, 0x64, 0x65, 0x5F, 0x72, 0x65, 0x76, 0x65, 0x6E, 0x67, 0x65, 0x5F, 0x62, 0x6F, 0x73, 0x73,
            0x00, 0x73, 0x68, 0x65, 0x6C, 0x6C, 0x5F, 0x72, 0x65, 0x66, 0x00, 0x70, 0x75, 0x6E, 0x63, 0x68,
            0x5F, 0x68, 0x69, 0x74, 0x00, 0x66, 0x61, 0x6C, 0x6C, 0x5F, 0x72, 0x65, 0x66, 0x00, 0x70, 0x72,
            0x65, 0x73, 0x73, 0x5F, 0x72, 0x65, 0x66, 0x00, 0x73, 0x68, 0x6F, 0x63, 0x6B, 0x5F, 0x64, 0x6F,
            0x77, 0x6E, 0x00, 0x62, 0x61, 0x6C, 0x6C, 0x5F, 0x6C, 0x65, 0x61, 0x76, 0x65, 0x00, 0x72, 0x65,
            0x66, 0x5F, 0x6D, 0x6F, 0x76, 0x65, 0x00, 0x63, 0x72, 0x61, 0x73, 0x68, 0x00
        };

        public static List<Atkp> Read(Stream stream) => BaseTable<Atkp>.Read(stream);

        //New ATKP: Extra byte addition is no longer hardcoded to add to a specific position.
        //Rather, they're now added at the end.
        //The bytes seem to not be important, the strings resemble ones found in AI.
        //However, to prevent messing with the ATKP offset unless explicity adding attack hitboxes, they'll be appended to the end of the file.
        public static void Write(Stream stream, IEnumerable<Atkp> items)
        {
            // Get the initial length of the stream
            long initialLength = stream.Length;

            // Write the items to the stream
            BaseTable<Atkp>.Write(stream, 6, items);

            // Check if the stream length has increased
            if (stream.Length > initialLength)
            {

                // Seek to the end of the stream
                stream.Seek(0, SeekOrigin.End);

                // Append the bytes to the end of the stream
                stream.Write(endBytes, 0, endBytes.Length);
            }
            if (stream.Length == initialLength)
            {
                // Seek to the end of the stream
                stream.Seek(0, SeekOrigin.End);
            }
            //Currently if you're just editing hitboxes, nothing changes. No offset differences occur.
            //If you add one hitbox, it overwrites endbytes until it reaches the end of the file, and starts appending new bytes AND endbytes after.

            else
            {
                // If no new data was written, do nothing
            }
        }
    }
}
