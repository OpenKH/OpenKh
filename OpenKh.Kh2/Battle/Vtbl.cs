using System.Collections.Generic;
using System.IO;
using Xe.BinaryMapper;

namespace OpenKh.Kh2.Battle
{
    public class Vtbl
    {
        private class Header
        {
            [Data] public int MagicCode { get; set; }
            [Data] public int Count { get => Items.TryGetCount(); set => Items = Items.CreateOrResize(value); }
            [Data] public List<RandomizationTable> Items { get; set; }
        }

        private readonly Header _header;

        public List<RandomizationTable> RandomizationTables => _header.Items;

        public class RandomizationTable
        {
            [Data] public byte CharacterId { get; set; }
            [Data] public byte ActionId { get; set; }
            [Data] public byte Priority { get; set; }
            [Data] public byte Unknown03 { get; set; } //Padding?
            [Data(offset: 4, count: 5)] public List<Voice> Voices { get; set; }

            public class Voice
            {
                [Data] public sbyte VsbIndex { get; set; }
                [Data] public sbyte Weight { get; set; } //(0 = normal random; 100 = guaranteed run)
            }
        }

        public Vtbl(Stream stream)
        {
            BinaryMapping.SetMemberLengthMapping<Header>(nameof(Header.Items), (o, m) => o.Count);
            _header = BinaryMapping.ReadObject<Header>(stream);
        }

        public void Write(Stream stream)
        {
            BinaryMapping.WriteObject(stream, _header);
        }
    }
}
