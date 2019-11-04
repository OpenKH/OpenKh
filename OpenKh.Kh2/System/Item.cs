using System.Collections.Generic;
using System.IO;
using Xe.BinaryMapper;

namespace OpenKh.Kh2.System
{
    public class Item
    {
        public enum Type : byte
        {
        }

        public enum Category : byte
        {
            Item,
            MenuItem,
            SoraKeyblade,
            Staff,
            Shield,
            Scimitar,
            Fangs,
            BoneHand,
            Sword,
            RikuKeyblade,
            Claws,
            Rapier,
            DataDisc,
            Armor,
            Accessory,
            Gem,
            KeyItem,
            Magic,
            Ability,
            Summon,
            Form,
            Map,
            Report
        }

        public class Entry
        {
            [Data] public ushort Id { get; set; }
            [Data] public ushort Type { get; set; }
            [Data] public byte Flag1 { get; set; }
            [Data] public byte Flag2 { get; set; }
            [Data] public ushort StatEntry { get; set; }
            [Data] public ushort Name { get; set; }
            [Data] public ushort Description { get; set; }
            [Data] public ushort ShopValue1 { get; set; }
            [Data] public ushort ShopValue2 { get; set; }
            [Data] public ushort Command { get; set; }
            [Data] public ushort Slot { get; set; }
            [Data] public short Picture { get; set; }
            [Data] public byte Icon1 { get; set; }
            [Data] public byte Icon2 { get; set; }
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
