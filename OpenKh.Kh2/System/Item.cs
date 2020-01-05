using System.Collections.Generic;
using System.IO;
using Xe.BinaryMapper;

namespace OpenKh.Kh2.System
{
    public class Item
    {
        public enum Type : ushort // Declaring this as a byte is a mistake you do NOT want to make. Trust me, I have been there.
        {
            Consumable = 0x00,
            CampItem = 0x01,
            MegaConsumbale = 0x0200,
            MegaCamp = 0x0201,
            Keyblade = 0x02,
            Staff = 0x03,
            Shield = 0x04,
            AladdinWeapon = 0x05, 
            AuronWeapon = 0x06,
            BeastWeapon = 0x07,
            JackWeapon = 0x08,
            MulanWeapon = 0x09,
            RikuWeapon = 0x0A,
            SimbaWeapon = 0x0B, 
            JackSparrowWeapon = 0x0C,
            TronWeapon = 0x0D,
            Armor = 0x0E,
            Accessory = 0x0F,
            Gem = 0x10,
            KeyItem = 0x11,
            Magic = 0x12,
            Ability = 0x13,
            Summon = 0x0114,
            Form = 0x0115,
            Map = 0x0116,
            Report = 0x117,
        }

        public enum Rank : byte // Haven't seen rank declared in this table.
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
            [Data] public ushort Flag0 { get; set; } // Dependent on Type, can be a ushort or a byte array depending on type.
            [Data] public ushort Flag1 { get; set; } // Same as Flag0.
            [Data] public ushort NameID { get; set; } // Applies some weird math which I shall note later.
            [Data] public ushort DescriptionID { get; set; } // Same as NameID.
            [Data] public short ShopBuy { get; set; }
            [Data] public short ShopSell { get; set; }
            [Data] public ushort Command { get; set; }
            [Data] public ushort Slot { get; set; } // Weird in it's own way. Always "0" for Abilities.
            [Data] public short Picture { get; set; } // Up to 999.
            [Data] public byte DropContainer { get; set; } // Uncertain, needs verification
            [Data] public byte Icon { get; set; }
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
            [Data] public byte FireVulnerability { get; set; } // These show "Vulnerability"
            [Data] public byte IceVulnerability  { get; set; } // Not "Resistance"
            [Data] public byte LightningVulnerability  { get; set; } // They are in percentages
            [Data] public byte DarkVulnerability  { get; set; } // "0x64" to "0x00"
            [Data] public byte Unknown0d { get; set; }
            [Data] public byte GeneralVulnerability  { get; set; }
            [Data] public byte Unknown { get; set; }
        }

        private class SubItemReader<T>
        {
            [Data] public int Id { get; set; }
            [Data] public int Count { get => Items.TryGetCount(); set => Items = Items.CreateOrResize(value); }
            [Data] public List<T> Items { get; set; }

            static SubItemReader() => BinaryMapping.SetMemberLengthMapping<SubItemReader<T>>(nameof(Items), (o, m) => o.Count);

            public static SubItemReader<T> Read(Stream stream) => BinaryMapping.ReadObject<SubItemReader<T>>(stream);

            public void Write(Stream stream) => BinaryMapping.WriteObject(stream, this);
        }

        [Data] public List<Entry> Items1 { get; set; }
        [Data] public List<Stat> Items2 { get; set; }

        public static Item Read(Stream stream)
        {
            stream.Position = 0;
            var one = SubItemReader<Entry>.Read(stream);
            var two = SubItemReader<Stat>.Read(stream);

            return new Item
            {
                Items1 = one.Items,
                Items2 = two.Items
            };
        }

        public void Write(Stream stream)
        {
            new SubItemReader<Entry>
            {
                Id = 6,
                Items = Items1
            }.Write(stream);

            new SubItemReader<Stat>
            {
                Id = 0,
                Items = Items2
            }.Write(stream);
        }
    }
}
