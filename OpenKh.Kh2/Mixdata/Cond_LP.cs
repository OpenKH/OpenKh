using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xe.BinaryMapper;

namespace OpenKh.Kh2.Mixdata
{
    public class CondLP 
    //LP for Listpatch version of the file. Currently the BaseMixdata Read/Write seems to not work with reading/writing these files? Tests seemed to fail.
    //LP versions of the files exist so as to not ruin the original, cleaner version of the code, however if the original version of the code is used for the Listpatch then current mods wouldn't break.
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

        [Data] public ushort TextId { get; set; }
        [Data] public short Reward { get; set; } //either item from 03system or shop upgrades
        [Data] public RewardType Type { get; set; }
        [Data] public byte MaterialType { get; set; }
        [Data] public byte MaterialRank { get; set; }
        [Data] public CollectionType ItemCollect { get; set; }
        [Data] public short Count { get; set; }
        [Data] public short ShopUnlock { get; set; }

        public static List<CondLP> Read(Stream stream)
        {
            var condLPList = new List<CondLP>();
            using (var reader = new BinaryReader(stream, System.Text.Encoding.Default, true))
            {
                int magicCode = reader.ReadInt32();
                int version = reader.ReadInt32();
                int count = reader.ReadInt32();
                reader.ReadInt32(); // Skip padding

                for (int i = 0; i < count; i++)
                {
                    var condLP = new CondLP
                    {
                        TextId = reader.ReadUInt16(),
                        Reward = reader.ReadInt16(),
                        Type = (CondLP.RewardType)reader.ReadByte(),
                        MaterialType = reader.ReadByte(),
                        MaterialRank = reader.ReadByte(),
                        ItemCollect = (CondLP.CollectionType)reader.ReadByte(),
                        Count = reader.ReadInt16(),
                        ShopUnlock = reader.ReadInt16()
                    };
                    condLPList.Add(condLP);
                }
            }
            return condLPList;
        }
        public static void Write(Stream stream, List<CondLP> condLPList)
        {
            stream.Position = 0;
            using (var writer = new BinaryWriter(stream, System.Text.Encoding.Default, true))
            {
                writer.Write(MagicCode);
                writer.Write(2); // Version number, hardcoded for example
                writer.Write(condLPList.Count);
                writer.Write(0); // Padding

                foreach (var condLP in condLPList)
                {
                    writer.Write(condLP.TextId);
                    writer.Write(condLP.Reward);
                    writer.Write((byte)condLP.Type);
                    writer.Write(condLP.MaterialType);
                    writer.Write(condLP.MaterialRank);
                    writer.Write((byte)condLP.ItemCollect);
                    writer.Write(condLP.Count);
                    writer.Write(condLP.ShopUnlock);
                }
            }
        }
    }
}
