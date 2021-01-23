using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xe.BinaryMapper;

namespace OpenKh.Kh2.Mixdata
{
    public class Cond
    {
        private const int MagicCode = 0x4F43494D;

        public enum RewardType
        {
            Item = 0,
            ShopUpgrade = 1
        }

        public enum CollectionType
        {
            Stack = 0,
            Unique = 1
        }

        [Data] public ushort Id { get; set; }
        [Data] public short Reward { get; set; } //either item from 03system or shop upgrades
        [Data] public RewardType Type { get; set; }
        [Data] public byte MaterialType { get; set; }
        [Data] public byte MaterialRank { get; set; }
        [Data] public CollectionType ItemCollect { get; set; }
        [Data] public short Count { get; set; }
        [Data] public short ShopUnlock { get; set; }

        public List<Cond> Read(Stream stream) => BaseMixdata<Cond>.Read(stream).Items;
        public void Write(Stream stream, int version, IEnumerable<Cond> items) => BaseMixdata<Cond>.Write(stream, MagicCode, version, items.ToList());
    }
}
