using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xe.BinaryMapper;

namespace OpenKh.Kh2.Mixdata
{
    public class ReciLP
    {
        public const int MagicCode = 0x4552494D;

        public enum UnlockType
        {
            Recipe = 0,
            FreeDevelopment1 = 1,
            FreeDevelopment2 = 2,
            FreeDevelopment3 = 3,
        }

        [Data] public ushort Id { get; set; } //03system -> item
        [Data] public UnlockType Unlock { get; set; }
        [Data] public byte Rank { get; set; }
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

        public static List<ReciLP> Read(Stream stream)
        {
            var recipes = new List<ReciLP>();
            using (var reader = new BinaryReader(stream, System.Text.Encoding.Default, true))
            {
                int magicCode = reader.ReadInt32();
                int version = reader.ReadInt32();
                int count = reader.ReadInt32();
                reader.ReadInt32(); // Skip padding

                for (int i = 0; i < count; i++)
                {
                    var recipe = new ReciLP
                    {
                        Id = reader.ReadUInt16(),
                        Unlock = (UnlockType)reader.ReadByte(),
                        Rank = reader.ReadByte(),
                        Item = reader.ReadUInt16(),
                        UpgradedItem = reader.ReadUInt16(),
                        Ingredient1 = reader.ReadUInt16(),
                        Ingredient1Amount = reader.ReadUInt16(),
                        Ingredient2 = reader.ReadUInt16(),
                        Ingredient2Amount = reader.ReadUInt16(),
                        Ingredient3 = reader.ReadUInt16(),
                        Ingredient3Amount = reader.ReadUInt16(),
                        Ingredient4 = reader.ReadUInt16(),
                        Ingredient4Amount = reader.ReadUInt16(),
                        Ingredient5 = reader.ReadUInt16(),
                        Ingredient5Amount = reader.ReadUInt16(),
                        Ingredient6 = reader.ReadUInt16(),
                        Ingredient6Amount = reader.ReadUInt16()
                    };
                    recipes.Add(recipe);
                }
            }
            return recipes;
        }

        public static void Write(Stream stream, List<ReciLP> recipes)
        {
            stream.Position = 0;
            using (var writer = new BinaryWriter(stream, System.Text.Encoding.Default, true))
            {
                writer.Write(MagicCode);
                writer.Write(2); // Version number, hardcoded for example
                writer.Write(recipes.Count);
                writer.Write(0); // Padding

                foreach (var recipe in recipes)
                {
                    writer.Write(recipe.Id);
                    writer.Write((byte)recipe.Unlock);
                    writer.Write(recipe.Rank);
                    writer.Write(recipe.Item);
                    writer.Write(recipe.UpgradedItem);
                    writer.Write(recipe.Ingredient1);
                    writer.Write(recipe.Ingredient1Amount);
                    writer.Write(recipe.Ingredient2);
                    writer.Write(recipe.Ingredient2Amount);
                    writer.Write(recipe.Ingredient3);
                    writer.Write(recipe.Ingredient3Amount);
                    writer.Write(recipe.Ingredient4);
                    writer.Write(recipe.Ingredient4Amount);
                    writer.Write(recipe.Ingredient5);
                    writer.Write(recipe.Ingredient5Amount);
                    writer.Write(recipe.Ingredient6);
                    writer.Write(recipe.Ingredient6Amount);
                }
            }
        }


    }
}
