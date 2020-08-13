using System.Collections.Generic;
using System.IO;
using Xe.BinaryMapper;

namespace OpenKh.Kh2.Battle
{
    public class Bons
    {
        [Data] public byte RewardId { get; set; }
        [Data] public byte CharacterId { get; set; }
        [Data] public byte HpIncrease { get; set; }
        [Data] public byte MpIncrease { get; set; }
        [Data] public byte DriveGaugeUpgrade { get; set; }
        [Data] public byte ItemSlotUpgrade { get; set; }
        [Data] public byte AccessorySlotUpgrade { get; set; }
        [Data] public byte ArmorSlotUpgrade { get; set; }
        [Data] public short BonusItem1 { get; set; }
        [Data] public short BonusItem2 { get; set; }
        [Data] public int Unknown0c { get; set; }

        public override string ToString() =>
            $"HP: {HpIncrease}, MP: {MpIncrease}, ItemSlot: {ItemSlotUpgrade}, " +
            $"Acc.Slot: {AccessorySlotUpgrade}, Bonus 1: {BonusItem1}, Bonus 2: {BonusItem2}";

        public static List<Bons> Read(Stream stream) => BaseTable<Bons>.Read(stream);

        public static void Write(Stream stream, IEnumerable<Bons> items) =>
            BaseTable<Bons>.Write(stream, 2, items);
    }
}
