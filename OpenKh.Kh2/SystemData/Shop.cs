using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xe.BinaryMapper;

namespace OpenKh.Kh2.SystemData
{
    public class Shop
    {
        private const uint MagicCode = 0x48535A54;  // TZSH
        private const ushort FileType = 0x07;
        public const uint HeaderSize = 0x10;
        public const uint ShopEntrySize = 0x18;
        public const uint InventoryEntrySize = 0x08;
        public const uint ProductEntrySize = 0x02;

        public class ShopHeader
        {
            [Data] public uint MagicCode { get; set; }
            [Data] public ushort FileType { get; set; }
            [Data] public ushort ShopEntryCount { get; set; }
            [Data] public ushort InventoryEntryCount { get; set; }
            [Data] public ushort ProductEntryCount { get; set; }
            [Data] public uint ValidProductsOffset { get; set; }

            public static ShopHeader Read(Stream stream) => BinaryMapping.ReadObject<ShopHeader>(stream);
            public static void Write(Stream stream, ShopHeader header) => BinaryMapping.WriteObject(stream, header);
        }

        public class ShopEntry
        {
            [Data] public ushort CommandArgument { get; set; }
            [Data] public ushort UnlockMenuFlag { get; set; }
            [Data] public ushort NameID { get; set; }
            [Data] public ushort ShopKeeperEntityID { get; set; }
            [Data] public short PosX { get; set; }
            [Data] public short PosY { get; set; }
            [Data] public short PosZ { get; set; }
            [Data] public byte ExtraInventoryBitMask { get; set; }
            [Data] public byte SoundID { get; set; }
            [Data] public ushort InventoryCount { get; set; }
            [Data] public byte ShopID { get; set; }
            [Data] public byte Unk19 { get; set; }
            [Data] public ushort InventoryOffset { get; set; }
            [Data] public ushort Reserved { get; set; }

            public ShopEntryHelper ToShopEntryHelper(ushort InventoriesBaseOffset) => new ShopEntryHelper()
            {
                CommandArgument = CommandArgument,
                UnlockMenuFlag = UnlockMenuFlag,
                NameID = NameID,
                ShopKeeperEntityID = ShopKeeperEntityID,
                PosX = PosX,
                PosY = PosY,
                PosZ = PosZ,
                ExtraInventoryBitMask = ExtraInventoryBitMask,
                SoundID = SoundID,
                InventoryCount = InventoryCount,
                ShopID = ShopID,
                Unk19 = Unk19,
                InventoryStartIndex = (uint)((InventoryOffset - InventoriesBaseOffset) / InventoryEntrySize),
            };

            public static List<ShopEntry> Read(Stream stream, int count) => BaseList<ShopEntry>.Read(stream, count).ToList();
            public static void Write(Stream stream, IEnumerable<ShopEntry> entries) => BaseList<ShopEntry>.Write(stream, entries);
        }

        public class InventoryEntry
        {
            [Data] public ushort UnlockEventID { get; set; } 
            [Data] public ushort ProductCount { get; set; }
            [Data] public ushort ProductOffset { get; set; }
            [Data] public ushort Reserved { get; set; }

            public InventoryEntryHelper ToInventoryEntryHelper(int InventoryIndex, ushort ProductsBaseOffset) => new InventoryEntryHelper
            {
                InventoryIndex = InventoryIndex,
                UnlockEventID = UnlockEventID,
                ProductCount = ProductCount,
                ProductStartIndex = (uint)((ProductOffset - ProductsBaseOffset) / ProductEntrySize)
            };

            public static List<InventoryEntry> Read(Stream stream, int count) => BaseList<InventoryEntry>.Read(stream, count).ToList();
            public static void Write(Stream stream, IEnumerable<InventoryEntry> entries) => BaseList<InventoryEntry>.Write(stream, entries);
        }

        public class ProductEntry
        {
            [Data] public ushort ItemID { get; set; }

            public ProductEntryHelper ToProductEntryHelper(int ProductIndex) => new ProductEntryHelper
            {
                ProductIndex = ProductIndex,
                ItemID = ItemID
            };

            public static List<ProductEntry> Read(Stream stream, int count) => BaseList<ProductEntry>.Read(stream, count).ToList();
            public static void Write(Stream stream, IEnumerable<ProductEntry> entries) => BaseList<ProductEntry>.Write(stream, entries);
        }

        public List<ShopEntry> ShopEntries { get; set; }
        public List<InventoryEntry> InventoryEntries { get; set; }
        public List<ProductEntry> ProductEntries { get; set; }
        public List<ProductEntry> ValidProductEntries { get; set; }

        public class ShopEntryHelper
        {
            public ushort CommandArgument { get; set; }
            public ushort UnlockMenuFlag { get; set; }
            public ushort NameID { get; set; }
            public ushort ShopKeeperEntityID { get; set; }
            public short PosX { get; set; }
            public short PosY { get; set; }
            public short PosZ { get; set; }
            public byte ExtraInventoryBitMask { get; set; }
            public byte SoundID { get; set; }
            public ushort InventoryCount { get; set; }
            public byte ShopID { get; set; }
            public byte Unk19 { get; set; }
            public uint InventoryStartIndex { get; set; }

            public ShopEntryHelper() { }
            public ShopEntryHelper(ushort commandArgument, ushort unlockMenuFlag, ushort nameID, ushort shopKeeperEntityID, short posX, short posY, short posZ, byte extraInventoryBitMask, byte soundID, ushort inventoryCount, byte shopID, byte unk19, uint inventoryStartIndex)
            {
                CommandArgument = commandArgument;
                UnlockMenuFlag = unlockMenuFlag;
                NameID = nameID;
                ShopKeeperEntityID = shopKeeperEntityID;
                PosX = posX;
                PosY = posY;
                PosZ = posZ;
                ExtraInventoryBitMask = extraInventoryBitMask;
                SoundID = soundID;
                InventoryCount = inventoryCount;
                ShopID = shopID;
                Unk19 = unk19;
                InventoryStartIndex = inventoryStartIndex;
            }
            public ShopEntry ToShopEntry(ushort InventoriesBaseOffset) => new ShopEntry() {
                CommandArgument = CommandArgument,
                UnlockMenuFlag = UnlockMenuFlag,
                NameID = NameID,
                ShopKeeperEntityID = ShopKeeperEntityID,
                PosX = PosX,
                PosY = PosY,
                PosZ = PosZ,
                ExtraInventoryBitMask = ExtraInventoryBitMask,
                SoundID = SoundID,
                InventoryCount = InventoryCount,
                ShopID = ShopID,
                Unk19 = Unk19,
                InventoryOffset = (ushort)(InventoriesBaseOffset + InventoryStartIndex * InventoryEntrySize),
                Reserved = 0
            };
        }

        public class InventoryEntryHelper
        {
            public int InventoryIndex { get; set; }
            public ushort UnlockEventID { get; set; }
            public ushort ProductCount { get; set; }
            public uint ProductStartIndex { get; set; }

            public InventoryEntry ToInventoryEntry(ushort ProductsBaseOffset) => new InventoryEntry()
            {
                UnlockEventID = UnlockEventID,
                ProductCount = ProductCount,
                ProductOffset  = (ushort)(ProductsBaseOffset + ProductStartIndex * ProductEntrySize),
                Reserved = 0
            };
        }

        public class ProductEntryHelper
        {
            public int ProductIndex { get; set; }
            public ushort ItemID { get; set; }

            public ProductEntry ToProductEntry() => new ProductEntry()
            {
                ItemID = ItemID
            };
        }

        public class ShopHelper
        {
            public List<ShopEntryHelper> ShopEntryHelpers { get; set; }
            public List<InventoryEntryHelper> InventoryEntryHelpers { get; set; }
            public List<ProductEntryHelper> ProductEntryHelpers { get; set; }
            public List<ProductEntryHelper> ValidProductEntryHelpers { get; set; }
        }

        public static Shop Read(Stream stream)
        {
            if (!stream.CanRead || !stream.CanSeek)
                throw new InvalidDataException($"Read or seek must be supported.");

            ShopHeader header = ShopHeader.Read(stream);

            if (header.MagicCode != MagicCode)
                throw new InvalidDataException($"Invalid shop magic 0x{header.MagicCode:X8} (expected 0x{MagicCode:X8}).");
            if (header.FileType != FileType)
                throw new InvalidDataException($"Invalid shop file type 0x{header.FileType:X4} (expected 0x{FileType:X4}).");

            var shop = new Shop()
            {
                ShopEntries = ShopEntry.Read(stream, header.ShopEntryCount),
                InventoryEntries = InventoryEntry.Read(stream, header.InventoryEntryCount),
                ProductEntries = ProductEntry.Read(stream, header.ProductEntryCount),
                ValidProductEntries = ProductEntry.Read(stream, (int)((stream.Length - header.ValidProductsOffset) / ProductEntrySize))
            };
            return shop;
        }

        public static void Write(Stream stream, Shop shop)
        {
            if (!stream.CanWrite || !stream.CanSeek)
                throw new InvalidDataException($"Write or seek must be supported.");

            ushort shop_entries_count = (ushort)shop.ShopEntries.Count;
            ushort inventory_entries_count = (ushort)shop.InventoryEntries.Count;
            ushort product_entries_count = (ushort)shop.ProductEntries.Count;
            uint valid_products_offset = HeaderSize + shop_entries_count * ShopEntrySize + inventory_entries_count * InventoryEntrySize + product_entries_count * ProductEntrySize;

            ShopHeader header = new ShopHeader
            {
                MagicCode = MagicCode,
                FileType = FileType,
                ShopEntryCount = shop_entries_count,
                InventoryEntryCount = inventory_entries_count,
                ProductEntryCount = product_entries_count,
                ValidProductsOffset = valid_products_offset
            };
            ShopHeader.Write(stream, header);
            ShopEntry.Write(stream, shop.ShopEntries);
            InventoryEntry.Write(stream, shop.InventoryEntries);
            ProductEntry.Write(stream, shop.ProductEntries);
            ProductEntry.Write(stream, shop.ValidProductEntries);
        }
    }
}
