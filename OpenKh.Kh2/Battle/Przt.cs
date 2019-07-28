using System.Collections.Generic;
using System.IO;
using Xe.BinaryMapper;

namespace OpenKh.Kh2.Battle
{
    public class Przt
    {
        private class Header
        {
            [Data] public int MagicCode { get; set; }
            [Data] public int Count { get => Items.TryGetCount(); set => Items = Items.CreateOrResize(value); }
            [Data] public List<ItemDrops> Items { get; set; }
        }

        private readonly Header _header;

        public List<ItemDrops> Drops => _header.Items;

        public class ItemDrops
        {
            [Data] public byte Index { get; set; }
            [Data] public byte Unknown01 { get; set; }
            [Data] public byte Unknown02 { get; set; }
            [Data] public byte Unknown03 { get; set; }
            [Data] public byte Unknown04 { get; set; }
            [Data] public byte Unknown05 { get; set; }
            [Data] public byte Unknown06 { get; set; }
            [Data] public byte Unknown07 { get; set; }
            [Data] public byte Unknown08 { get; set; }
            [Data] public byte Unknown09 { get; set; }
            [Data] public byte Unknown0a { get; set; }
            [Data] public byte Unknown0b { get; set; }
            [Data] public short DropItem1 { get; set; }
            [Data] public short DropItem1Percentage { get; set; }
            [Data] public short DropItem2 { get; set; }
            [Data] public short DropItem2Percentage { get; set; }
            [Data] public short DropItem3 { get; set; }
            [Data] public short DropItem3Percentage { get; set; }
        }

        public Przt(Stream stream)
        {
            BinaryMapping.SetMemberLengthMapping<Header>(nameof(Header.Items), (o, m) => o.Count);
            _header = BinaryMapping.ReadObject<Header>(stream);
        }

        public void Write(Stream stream) => BinaryMapping.WriteObject(stream, _header);
    }
}
