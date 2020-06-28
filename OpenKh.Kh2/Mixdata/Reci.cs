using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xe.BinaryMapper;

namespace OpenKh.Kh2.Mixdata
{
    public class Reci
    {
        private const int MagicCode = 0x4552494D;

        [Data] public ushort Id { get; set; } //03system -> item
        [Data] public byte Unk2 { get; set; }
        [Data] public byte Unk3 { get; set; }
        [Data] public ushort Item { get; set; }
        [Data] public ushort UpgradedItem { get; set; }
        [Data] public ushort Ingredient1 { get; set; }
        [Data] public ushort Ingredient1Amount { get; set; }
        [Data] public ushort Ingredient2 { get; set; }
        [Data] public ushort Ingredient2Amount { get; set; }
        [Data] public ushort Ingredient3 { get; set; }
        [Data] public ushort Ingredient3Amount { get; set; }
        [Data] public ushort Ingredient4 { get; set; }
        [Data] public ushort Ingredient4Amount { get; set; }
        [Data] public ushort Ingredient5 { get; set; }
        [Data] public ushort Ingredient5Amount { get; set; }
        [Data] public ushort Ingredient6 { get; set; }
        [Data] public ushort Ingredient6Amount { get; set; }

        public List<Reci> Read(Stream stream) => BaseMixdata<Reci>.Read(stream).Items;
        public void Write(Stream stream, int version, IEnumerable<Reci> items) => BaseMixdata<Reci>.Write(stream, MagicCode, version, items.ToList());
    }
}
