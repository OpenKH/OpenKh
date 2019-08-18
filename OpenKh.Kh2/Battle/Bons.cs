using System.Collections.Generic;
using System.IO;
using Xe.BinaryMapper;

namespace OpenKh.Kh2.Battle
{
    public class Bons
    {
        private class Header
        {
            [Data] public int MagicCode { get; set; }
            [Data] public int Count { get => Items.TryGetCount(); set => Items = Items.CreateOrResize(value); }
            [Data] public List<BonusLevel> Items { get; set; }
        }

        private readonly Header _header;

        public List<BonusLevel> BonusLevels => _header.Items;

        public class BonusLevel
        {
            [Data] public byte HpIncrease { get; set; }
            [Data] public byte MpIncrease { get; set; }
            [Data] public byte Unknown02 { get; set; }
            [Data] public byte Unknown03 { get; set; }
            [Data] public byte DriveGaugeUpgrade { get; set; }
            [Data] public byte ItemSlotUpgrade { get; set; }
            [Data] public byte AccessorySlotUpgrade { get; set; }
            [Data] public byte ArmorSlotUpgrade { get; set; }
            [Data] public short BonusItem1 { get; set; }
            [Data] public short BonusItem2 { get; set; }
            [Data] public int Unknown0c { get; set; }

            public override string ToString() =>
                $"HP: {HpIncrease}, MP: {MpIncrease}, AP?: {Unknown03}, ItemSlot: {ItemSlotUpgrade}, " +
                $"Acc.Slot: {AccessorySlotUpgrade}, Bonus 1: {BonusItem1}, Bonus 2: {BonusItem2}";
        }

        public Bons(Stream stream)
        {
            BinaryMapping.SetMemberLengthMapping<Header>(nameof(Header.Items), (o, m) => o.Count);
            _header = BinaryMapping.ReadObject<Header>(stream);
        }

        public void Write(Stream stream) => BinaryMapping.WriteObject(stream, _header);
    }
}
