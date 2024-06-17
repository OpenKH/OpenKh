using OpenKh.Common;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xe.BinaryMapper;

namespace OpenKh.Kh2
{
    // Bianry Archives are used to store subfiles.
    // The previous version for this type of file is "Bar.cs". It dealt with linking through name + entry type and a more refined linking structure was required.
    public class BinaryArchive
    {
        /******************************************
         * Constants
         ******************************************/
        private const string SIGNATURE = "BAR";
        private const int HEADER_SIZE = 0x10;
        private const int ENTRY_SIZE = 0x10;

        /******************************************
         * Properties
         ******************************************/
        public byte ExternalFlags { get; set; }
        public byte Version { get; set; }
        public MotionsetType MSetType { get; set; }
        public List<Entry> Entries { get; set; }
        public List<byte[]> Subfiles {  get; set; }

        /******************************************
         * Constructors
         ******************************************/

        public BinaryArchive()
        {
            Entries = new List<Entry>();
            Subfiles = new List<byte[]>();
        }

        /******************************************
         * Functions - Static
         ******************************************/

        // Creates the BinaryArchive from the given Stream. The final position is the same as the initial position.
        public static BinaryArchive Read(Stream barStream)
        {
            BinaryArchive binaryArchive = new BinaryArchive();

            int initialPosition = (int)barStream.Position;

            // Read Header
            HeaderBinary header = BinaryMapping.ReadObject<HeaderBinary>(barStream);
            binaryArchive.ExternalFlags = (byte)((header.ExternalFlagsAndVersion & 0xF0) >> 4);
            binaryArchive.Version = (byte)(header.ExternalFlagsAndVersion & 0x0F);
            binaryArchive.MSetType = (MotionsetType)header.MotionSetType;

            // Read Entries
            List<EntryBinary> entryBinaries = new List<EntryBinary>();
            Dictionary<int, int> fileOffsetSize = new Dictionary<int, int>();

            for (int i = 0; i < header.EntryCount; i++)
            {
                entryBinaries.Add(BinaryMapping.ReadObject<EntryBinary>(barStream));
                EntryBinary entryBinary = entryBinaries[i];
                if(entryBinary.Size == 0)
                {
                    entryBinary.Offset = -1;
                }

                if (!fileOffsetSize.ContainsKey(entryBinary.Offset) && entryBinary.Offset != -1)
                {
                    fileOffsetSize.Add(entryBinary.Offset, entryBinary.Size);
                }

                Entry entry = new Entry();
                entry.Type = (EntryType)entryBinary.Type;
                entry.Name = entryBinary.Name;
                entry.Link = entryBinary.Size == 0 ? -1 : fileOffsetSize.Keys.ToList().IndexOf(entryBinary.Offset);
                binaryArchive.Entries.Add(entry);
            }

            // Read SubFiles
            foreach (int fileOffset in fileOffsetSize.Keys)
            {
                byte[] fileBytes = new byte[fileOffsetSize[fileOffset]];
                barStream.Position = initialPosition + fileOffset;
                barStream.Read(fileBytes, 0, fileOffsetSize[fileOffset]);
                binaryArchive.Subfiles.Add(fileBytes);
            }

            barStream.Position = initialPosition;

            return binaryArchive;
        }
        public static BinaryArchive Read(byte[] binaryFile)
        {
            MemoryStream memStream = new MemoryStream(binaryFile);
            return Read(memStream);
        }

        public static bool IsValid(Stream stream)
        {
            if (stream.Length < 4)
            {
                return false;
            }

            string signatureBytes = System.Text.Encoding.ASCII.GetString(stream.ReadBytes(3));
            stream.Position -= 3;

            if (signatureBytes == SIGNATURE)
            {
                return true;
            }
            return false;
        }

        /******************************************
         * Functions - Local
         ******************************************/

        // Returns the BinaryArchive as a byte array
        public byte[] getAsByteArray()
        {
            RemoveUnlinkedSubfiles();

            using MemoryStream fileStream = new MemoryStream();

            // Header
            HeaderBinary headerBinary = new HeaderBinary();
            headerBinary.Signature = SIGNATURE;
            headerBinary.ExternalFlagsAndVersion = (byte)((ExternalFlags << 4) | (Version & 0x0F));
            headerBinary.EntryCount = Entries.Count;
            headerBinary.Address = 0;
            headerBinary.MotionSetType = (int)MSetType;
            BinaryMapping.WriteObject(fileStream, headerBinary);

            // SubFiles
            fileStream.Position += Entries.Count * ENTRY_SIZE;
            List<int> offsets = new List<int>();
            foreach(byte[] subfile in Subfiles)
            {
                offsets.Add((int)fileStream.Position);
                fileStream.Write(subfile, 0, subfile.Length);

                // Padding to 16
                byte excess = (byte)(fileStream.Position % 16);
                if(excess > 0)
                {
                    for (int i = 0; i < 16 - excess; i++)
                    {
                        fileStream.WriteByte(0);
                    }
                }
            }

            // Entries
            List<int> usedSubFiles = new List<int>();
            for (int i = 0; i < Entries.Count; i++)
            {
                Entry entry = Entries[i];
                EntryBinary entryBinary = new EntryBinary();
                entryBinary.Type = (short)entry.Type;
                entryBinary.Name = entry.Name;
                entryBinary.Offset = entry.Link == -1 ? -1 : offsets[entry.Link];
                entryBinary.Size = entry.Link == -1 ? 0 : Subfiles[entry.Link].Length;

                if (!usedSubFiles.Contains(entry.Link))
                {
                    entryBinary.Flag = 0;
                    usedSubFiles.Add(entry.Link);
                }
                else
                {
                    entryBinary.Flag = 1;
                }

                fileStream.Position = HEADER_SIZE + (i * ENTRY_SIZE);
                BinaryMapping.WriteObject(fileStream, entryBinary);
            }

            return fileStream.ToArray();
        }

        public void RemoveUnlinkedSubfiles()
        {
            List<int> links = new List<int>();
            foreach(Entry entry in Entries)
            {
                if(entry.Link != -1 && !links.Contains(entry.Link))
                {
                    links.Add(entry.Link);
                }
            }
            links.Sort();

            Dictionary<int,int> newIndices = new Dictionary<int,int>();
            for(int i = 0; i < links.Count; i++)
            {
                if (links[i] != i)
                {
                    newIndices.Add(links[i], i);
                }
            }

            List<int> indicesToDelete = new List<int>();
            for (int i = 0; i < Subfiles.Count; i++)
            {
                if (!links.Contains(i))
                {
                    indicesToDelete.Add(i);
                }
            }

            for (int i = Subfiles.Count; i >= 0; i--)
            {
                if (indicesToDelete.Contains(i))
                {
                    Subfiles.RemoveAt(i);
                }
            }

            foreach (Entry entry in Entries)
            {
                if (newIndices.ContainsKey(entry.Link))
                {
                    entry.Link = newIndices[entry.Link];
                }
            }
        }

        /******************************************
         * Subclasses
         ******************************************/
        public class Entry
        {
            public EntryType Type { get; set; }
            public int Link { get; set; } // Index of the subfile it links to
            private string _name;
            public string Name
            {
                get
                {
                    return _name;
                }
                set
                {
                    value += "\0\0\0\0";
                    _name = value.Substring(0, 4);
                }
            }

            public Entry()
            {
                Name = "\0\0\0\0";
                Link = -1;
            }

            // For visualizing easier in Debug Locals
            public override string ToString()
            {
                return "[" + Name + "] Link: " + Link + "; Type: " + Type;
            }
        }

        /******************************************
         * Enums
         ******************************************/
        public enum EntryType //2b
        {
            Dummy = 0,
            Binary = 1,
            List = 2,
            Bdx = 3,
            Model = 4,
            DrawOctalTree = 5,
            CollisionOctalTree = 6,
            ModelTexture = 7,
            Dpx = 8,
            Motion = 9,
            Tim2 = 10,
            CameraOctalTree = 11,
            AreaDataSpawn = 12,
            AreaDataScript = 13,
            FogColor = 14,
            ColorOctalTree = 15,
            MotionTriggers = 16,
            Anb = 17,
            Pax = 18,
            MapCollision2 = 19,
            Motionset = 20,
            BgObjPlacement = 21,
            Event = 22,
            ModelCollision = 23,
            Imgd = 24,
            Seqd = 25,
            Layout = 28,
            Imgz = 29,
            AnimationMap = 30,
            Seb = 31,
            Wd = 32,
            Unknown33,
            IopVoice = 34,
            RawBitmap = 36,
            MemoryCard = 37,
            WrappedCollisionData = 38,
            Unknown39 = 39,
            Unknown40 = 40,
            Unknown41 = 41,
            Minigame = 42,
            JimiData = 43,
            Progress = 44,
            Synthesis = 45,
            BarUnknown = 46,
            Vibration = 47,
            Vag = 48,
        }
        public enum MotionsetType //4b
        {
            Default = 0,
            Player = 1,
            Raw = 2
        }

        /******************************************
         * Helper classes for reading and writing
         ******************************************/
        // struct BINARC
        public class HeaderBinary
        {
            [Data(Count = 3)] public string Signature { get; set; } // BAR
            [Data] public byte ExternalFlagsAndVersion { get; set; } // 4b external flags; 4b Version
            [Data] public int EntryCount { get; set; }
            [Data] public int Address { get; set; } // Always 0 unless ingame
            [Data] public int MotionSetType { get; set; } // 30b Replace; 2b Flag (MotionSet Type)
        }
        // struct INFO
        public class EntryBinary
        {
            [Data] public short Type { get; set; }
            [Data] public short Flag { get; set; } // Bitflag
            [Data(Count = 4)] public string Name { get; set; }
            [Data] public int Offset { get; set; }
            [Data] public int Size { get; set; }
        }
    }
}
