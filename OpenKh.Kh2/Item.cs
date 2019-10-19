using System;
using System.Collections.Generic;
using System.IO;

namespace OpenKh.Kh2
{
    public class Item
    {
        /*/ FORMAT ItemTable:
         * ushort Identifier;
         * ushort TypeShort;
         * byte[2] Flags;
         * ushort StatEntry;
         * ushort[2] ShopValues;
         * ushort[2] Strings;
         * ushort Command;
         * ushort Slot;
         * short Picture;
         * byte[2] Icon;
        /*/

        /*/ FORMAT StatTable:
         * ushort Identifier;
         * ushort Ability;
         * byte[4] GeneralValues; => Attack, Magic, Defense, AP
         * byte UnknownByte;
         * byte[4] ResistancePercentages; => Fire, Ice, Lightning, Dark
         * byte UnknownByte;
         * byte GeneralResistance;
        /*/

        /// <summary>
        /// The item's specific type. 
        /// </summary>
        public enum ItemType
        {
            Normal = 0,
            Special = 256,
            Mega = 512
        }

        /// <summary>
        /// The item's category. Specifies if it is a Keyblade, Rapier, Potion, etc.
        /// </summary>
        public enum ItemCategory
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

        /// <summary>
        /// The item entry itself. Used by the game to know the item's technical properties.
        /// </summary>
        public struct ItemEntry
        {
            public ushort Identifier;
            internal ushort TypeShort;
            public List<byte> Flags;
            public ushort StatEntry;
            public List<ushort> ShopValues;
            public List<ushort> Strings;
            public ushort Command;
            public ushort Slot;
            public short Picture;
            public List<byte> Icon;
            public ItemType Type;
            public ItemCategory Category;
        }

        /// <summary>
        /// The item's stat entry. Used by the game to know the item's stats.
        /// </summary>
        public struct StatEntry
        {
            public ushort Identifier;
            public ushort Ability;
            public List<byte> GeneralValues;
            internal byte Unknown01;
            public List<byte> ResistanceValues;
            internal byte Unknown02;
        }

        /// <summary>
        /// Entries from the first sub-table of the parsed ItemTable.
        /// </summary>
        public List<ItemEntry> ItemEntries = new List<ItemEntry>();

        /// <summary>
        /// Entries from the second sub-table of the parsed ItemTable.
        /// </summary>
        public List<StatEntry> StatEntries = new List<StatEntry>();

        internal int MagicHeader = 2;
        internal int ItemCount;
        internal int StatsCount;

        /// <summary>
        /// Initializes an empty item table. Ought to be interesting.
        /// </summary>
        public Item()
        {

        }

        /// <summary>
        /// Reads item table data. Either as a file or as raw data.
        /// </summary>
        /// <param name="FileName">The location of the file. If null, the function will read the raw data instead.</param>
        /// <param name="FileData">The raw data array of the file. If null, the function will read the file instead.</param>
        public Item(string FileName = null, byte[] FileData = null)
        {
            BinaryReader GeneralReader = null;

            if (FileName != null && FileData != null)
                throw new IOException("Only one parameter should be used, other should remain null.");

            else if (FileName != null)
                GeneralReader = new BinaryReader(new FileStream(FileName, FileMode.Open));

            else if (FileData != null)
                GeneralReader = new BinaryReader(new MemoryStream(FileData));

            else
                throw new IOException("Both parameters are null.");

            #region File Check-Up
            if (GeneralReader.ReadUInt32() != MagicHeader)
                throw new InvalidDataException("Invalid Magic Header.");

            ItemCount = GeneralReader.ReadInt32();
            GeneralReader.BaseStream.Position = 8 + 24 * ItemCount;

            if (GeneralReader.ReadUInt32() != 0)
                throw new IOException("Start of the Property Table cannot be located. The file is invalid.");

            StatsCount = GeneralReader.ReadInt32();
            GeneralReader.BaseStream.Position += 16 * StatsCount;

            if (GeneralReader.BaseStream.Position != GeneralReader.BaseStream.Length)
                Console.WriteLine("The file length is incorrect. Not necessarily an invalid file.");
            #endregion

            #region Parsing Region
            for (int i = 0; i < ItemCount; i++)
            {
                List<ushort> TemporaryList = null;

                GeneralReader.BaseStream.Position = 8 + 24 * i;

                ItemEntry NewEntry = new ItemEntry();

                NewEntry.Identifier = GeneralReader.ReadUInt16();
                NewEntry.TypeShort = GeneralReader.ReadUInt16();
                NewEntry.Flags = new List<byte>(GeneralReader.ReadBytes(2));
                NewEntry.StatEntry = GeneralReader.ReadUInt16();

                TemporaryList = new List<ushort>();
                TemporaryList.Add(BitConverter.ToUInt16(GeneralReader.ReadBytes(2), 0));
                TemporaryList.Add(BitConverter.ToUInt16(GeneralReader.ReadBytes(2), 0));
                NewEntry.Strings = TemporaryList;

                TemporaryList = new List<ushort>();
                TemporaryList.Add(BitConverter.ToUInt16(GeneralReader.ReadBytes(2), 0));
                TemporaryList.Add(BitConverter.ToUInt16(GeneralReader.ReadBytes(2), 0));
                NewEntry.ShopValues = TemporaryList;

                NewEntry.Command = GeneralReader.ReadUInt16();
                NewEntry.Slot = GeneralReader.ReadUInt16();
                NewEntry.Picture = GeneralReader.ReadInt16();
                NewEntry.Icon = new List<byte>(GeneralReader.ReadBytes(2));

                for (int z = 256; z < 1024; z = z ^ 2)
                {
                    if ((NewEntry.TypeShort & z) == z)
                        NewEntry.Type = (ItemType)z;
                    
                    else if (z == 512)
                        NewEntry.Type = ItemType.Normal;
                }

                for (int z = 0; z < 23; z++)
                {
                    if ((NewEntry.TypeShort & z) == z)
                        NewEntry.Category = (ItemCategory)z;

                    else if (z == 22)
                        throw new InvalidDataException("Invalid Item Type.");
                }

                ItemEntries.Add(NewEntry);
            }

            for (int i = 0; i < StatsCount; i++)
            {
                GeneralReader.BaseStream.Position = (8 + 24 * ItemCount) + 8 + 16 * i;

                StatEntry NewEntry = new StatEntry();

                NewEntry.Identifier = GeneralReader.ReadUInt16();
                NewEntry.Ability = GeneralReader.ReadUInt16();
                NewEntry.GeneralValues = new List<byte>(GeneralReader.ReadBytes(4));
                NewEntry.Unknown01 = GeneralReader.ReadByte();
                NewEntry.ResistanceValues = new List<byte>(GeneralReader.ReadBytes(4));
                NewEntry.Unknown02 = GeneralReader.ReadByte();
                NewEntry.ResistanceValues.Add(GeneralReader.ReadByte());

                for (int z = 0; z < 5; z++)
                {
                    if (NewEntry.ResistanceValues[z] == 0)
                        NewEntry.ResistanceValues[z] = 100;

                    else
                        NewEntry.ResistanceValues[z] = (byte)(NewEntry.ResistanceValues[z] % 100);
                }

                StatEntries.Add(NewEntry);
            }
            #endregion
        }
    }
}
