using System.Collections.Generic;
using System.IO;
using Xe.BinaryMapper;

namespace OpenKh.Kh2.SystemData
{
    public class Item
    {
        public enum Type : byte
        {
            Consumable,
            Boost,
            Keyblade,
            Staff,
            Shield,
            PingWeapon,
            AuronWeapon,
            BeastWeapon,
            JackWeapon,
            DummyWeapon,
            RikuWeapon,
            SimbaWeapon,
            JackSparrowWeapon,
            TronWeapon,
            Armor,
            Accessory,
            Synthesis,
            Recipe,
            Magic,
            Ability,
            Summon,
            Form,
            Map,
            Report,
        }

        public enum Rank : byte
        {
            C,
            B,
            A,
            S
        }

        public class Entry
        {
            [Data] public ushort Id { get; set; }
            [Data] public Type Type { get; set; }
            [Data] public byte Flag0 { get; set; }
            [Data] public byte Flag1 { get; set; }
            [Data] public Rank Rank { get; set; }
            [Data] public ushort StatEntry { get; set; }
            [Data] public ushort Name { get; set; }
            [Data] public ushort Description { get; set; }
            [Data] public ushort ShopBuy { get; set; }
            [Data] public ushort ShopSell { get; set; }
            [Data] public ushort Command { get; set; }
            [Data] public ushort Slot { get; set; }
            [Data] public short Picture { get; set; }
            [Data] public byte Icon1 { get; set; }
            [Data] public byte Icon2 { get; set; }
            public ushort InsertBefore { get; set; } = 0; // Default to 0, meaning append. Only used for Mod Manager.
            public ushort InsertAfter {get; set; } = 0;
        }

        public class Stat
        {
            [Data] public ushort Id { get; set; }
            [Data] public ushort Ability { get; set; }
            [Data] public byte Attack { get; set; }
            [Data] public byte Magic { get; set; }
            [Data] public byte Defense { get; set; }
            [Data] public byte AbilityPoints { get; set; }
            [Data] public byte Unknown08 { get; set; }
            [Data] public byte FireResistance { get; set; }
            [Data] public byte IceResistance { get; set; }
            [Data] public byte LightningResistance { get; set; }
            [Data] public byte DarkResistance { get; set; }
            [Data] public byte Unknown0d { get; set; }
            [Data] public byte GeneralResistance { get; set; }
            [Data] public byte Unknown { get; set; }
        }

        [Data] public List<Entry> Items { get; set; }
        [Data] public List<Stat> Stats { get; set; }

        public static Item Read(Stream stream)
        {
            stream.Position = 0;
            var one = BaseTable<Entry>.Read(stream);
            var two = BaseTable<Stat>.Read(stream);

            return new Item
            {
                Items = one,
                Stats = two
            };
        }

        public void Write(Stream stream)
        {
            BaseTable<Entry>.Write(stream, 6, Items);
            BaseTable<Stat>.Write(stream, 0, Stats);
        }
    }
}
