using System;
using System.Collections.Generic;
using System.IO;

namespace OpenKh.Kh2
{
    class Trsr
    {
        /*/ FORMAT: Looking at the "Entry" struct is sufficient. /*/

        /// <summary>
        /// Specifies how the treasure is obtained. Assigning an "Event" treasure to a chest will break the game in more ways than one.
        /// </summary>
        public enum TreasureType
        {
            Chest,
            Event
        }

        /// <summary>
        /// Specifies where the treasure is obtained. Assigning one treasure to a world other than specified has unknown consequences.
        /// </summary>
        public enum TreasureWorld
        {
            Null,
            DarkRealm,
            TwilightTown,
            Unknown,
            HollowBastion,
            BeastCastle,
            TheUnderworld,
            Agrabah,
            LandofDragons,
            HundredAcreWood,
            PrideLands,
            Atlantica,
            DisneyCastle,
            TimelessRiver,
            HalloweenTown,
            WorldMap,
            PortRoyal,
            SpaceParanoids,
            FinalWorld
        }

        /// <summary>
        /// The treasure entry.
        /// </summary>
        public struct Entry
        {
            public ushort Identifier;
            public ushort Item;
            internal byte TypeID;
            internal byte WorldID;
            public TreasureType Type;
            public TreasureWorld World;
            public byte RoomID;
            public byte RoomChestIndex;
            public ushort EventID;
            public ushort OverallChestIndex;
        }

        /// <summary>
        /// Entries of the parsed TreasureTable.
        /// </summary>
        public List<Entry> Entries = new List<Entry>();

        internal ushort MagicHeader = 3;
        internal ushort EntryCount;

        /// <summary>
        /// Initializes an empty TreasureTable. For use with tools only.
        /// </summary>
        public Trsr()
        {

        }

        /// <summary>
        /// Reads treasure table data. Either as a file or as raw data.
        /// </summary>
        /// <param name="FileName">The location of the file. If null, the function will read the raw data instead.</param>
        /// <param name="FileData">The raw data array of the file. If null, the function will read the file instead.</param>
        public Trsr(string FileName = null, byte[] FileData = null)
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
            if (GeneralReader.ReadUInt16() != MagicHeader)
                throw new InvalidDataException("Invalid Magic Header.");

            EntryCount = GeneralReader.ReadUInt16();

            if (GeneralReader.BaseStream.Length < 4 + 12 * EntryCount)
                throw new IOException("File size is smaller than it is supposed to be. The file is invalid.");

            else if (GeneralReader.BaseStream.Length > 4 + 12 * EntryCount)
                Console.WriteLine("File size is bigger than it is supposed to be. Not necessarily an invalid file.");
            #endregion

            #region Parsing Region
            for (int i = 0; i < EntryCount; i++)
            {
                GeneralReader.BaseStream.Position = 4 + 12 * i;

                Entry NewEntry = new Entry();

                NewEntry.Identifier = GeneralReader.ReadUInt16();
                NewEntry.Item = GeneralReader.ReadUInt16();
                NewEntry.TypeID = GeneralReader.ReadByte();
                NewEntry.WorldID = GeneralReader.ReadByte();
                NewEntry.RoomID = GeneralReader.ReadByte();
                NewEntry.RoomChestIndex = GeneralReader.ReadByte();
                NewEntry.EventID = GeneralReader.ReadUInt16();
                NewEntry.OverallChestIndex = GeneralReader.ReadUInt16();

                try
                {
                    NewEntry.Type = (TreasureType)NewEntry.TypeID;
                    NewEntry.World = (TreasureWorld)NewEntry.WorldID;
                }

                catch (IndexOutOfRangeException)
                {
                    throw new InvalidCastException("Invalid treasure type or world. Please ensure everything is correct on the file.");
                }

                Entries.Add(NewEntry);
            }
            #endregion
        }
    }
}
