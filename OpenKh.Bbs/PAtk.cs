using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Xe.BinaryMapper;
using System.Linq;
using OpenKh.Common;

namespace OpenKh.Bbs
{
    public class PAtk
    {
        public class PAtkData
        {
            [Data] public short frPlayEnd { get; set; }
            [Data] public short EffectGroup { get; set; }
            [Data] public ushort Flag { get; set; }
            [Data] public ushort Dummy1 { get; set; }
            [Data] public byte Animation1 { get; set; }
            [Data] public byte Animation2 { get; set; }
            [Data] public byte Animation3 { get; set; }
            [Data] public byte Animation4 { get; set; }
            [Data] public byte frComboEnable { get; set; }
            [Data] public byte frChangeEnable { get; set; }
            [Data] public byte SEGroup { get; set; }
            [Data] public byte Dummy2 { get; set; }
            [Data] public byte GroupAttack1 { get; set; }
            [Data] public byte GroupAttack2 { get; set; }
            [Data] public byte GroupAttack3 { get; set; }
            [Data] public byte GroupAttack4 { get; set; }
            [Data] public byte frTrigger1 { get; set; }
            [Data] public byte frTrigger2 { get; set; }
            [Data] public byte frTrigger3 { get; set; }
            [Data] public byte frTrigger4 { get; set; }
            [Data] public byte Bullet { get; set; }
            [Data] public byte Camera { get; set; }
            [Data] public byte AttackPower { get; set; }
            [Data] public byte AttackAttribute { get; set; }
            [Data] public byte frMarkStart { get; set; }
            [Data] public byte frMarkEnd { get; set; }
            [Data] public byte frMoveStart { get; set; }
            [Data] public byte frMoveEnd { get; set; }
            [Data] public byte MaximumDistance { get; set; }
            [Data] public byte Translation { get; set; }
            [Data] public byte Range { get; set; }
            [Data] public byte Speed { get; set; }
            [Data] public byte Rate { get; set; }
            [Data] public byte ExDash { get; set; }
            [Data] public byte ExRise { get; set; }
            [Data] public byte Dummy3 { get; set; }
        }

        public static List<PAtkData> Read(Stream stream)
        {
            return Enumerable.Range(0, (int)(stream.Length / 0x28))
                .Select(_ => BinaryMapping.ReadObject<PAtkData>(stream))
                .ToList();
        }

        public static void Write(Stream stream, IEnumerable<PAtkData> AttackData)
        {
            var list = AttackData.ToList();

            foreach (var item in list)
                BinaryMapping.WriteObject(stream, item);
        }
    }
}
