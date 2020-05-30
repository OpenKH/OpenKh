using OpenKh.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Xe.BinaryMapper;

namespace OpenKh.Kh2
{
    public partial class Idx
    {
        public class Entry
        {
            public const int MaxBlockLength = 0x3FFF;
            private const int IsCompressedFlag = 0x4000;
            private const int IsStreamedFlag = 0x8000;

            /// <summary>
            /// 32-bit hash of the file name.
            /// Paired with Hash2 to reduce the hash collisions.
            /// </summary>
            [Data] public uint Hash32 { get; set; }

            /// <summary>
            /// 16-bit hash of the file name.
            /// Paired with Hash1 to reduce the hash collisions.
            /// </summary>
            [Data] public ushort Hash16 { get; set; }

            /// <summary>
            /// Additional info and files
            /// </summary>
            [Data] public ushort BlockDescription { get; set; }

            /// <summary>
            /// Offset of the file in the archive, divided by the ISO block size
            /// </summary>
            [Data] public int Offset { get; set; }

            /// <summary>
            /// Length of the file in bytes
            /// </summary>
            [Data] public int Length { get; set; }

            /// <summary>
            /// ISO blocks used in the ISO.
            /// </summary>
            public int BlockLength
            {
                get => BlockDescription & MaxBlockLength;
                set
                {
                    if (value > MaxBlockLength)
                        throw new ArgumentOutOfRangeException(nameof(BlockLength), $"Cannot exceeds {MaxBlockLength}");

                    BlockDescription = (ushort)((ushort)value | (BlockDescription & 0xC000U));
                }
            }

            /// <summary>
            /// If the file is compressed.
            /// </summary>
            public bool IsCompressed
            {
                get => (BlockDescription & IsCompressedFlag) != 0;
                set
                {
                    if (value)
                        BlockDescription |= IsCompressedFlag;
                    else
                        BlockDescription = (ushort)(BlockDescription & ~IsCompressedFlag);
                }
            }

            /// <summary>
            /// If the file is not loaded in RAM, but streamed.
            /// </summary>
            public bool IsStreamed
            {
                get => (BlockDescription & IsStreamedFlag) != 0;
                set
                {
                    if (value)
                        BlockDescription |= IsStreamedFlag;
                    else
                        BlockDescription = (ushort)(BlockDescription & ~IsStreamedFlag);
                }
            }
        }

        private Dictionary<uint, Entry> dictionaryEntries;

        [Data] public int Length
        {
            get => Items.TryGetCount();
            set => Items = Items.CreateOrResize(value);
        }

        [Data] public List<Entry> Items { get; set; }

        public static bool IsValid(Stream stream) =>
            stream.Length == new BinaryReader(stream.SetPosition(0)).ReadInt32() * 0x10 + 4;

        /// <summary>
        /// Deserialize an IDX from a stream
        /// </summary>
        /// <param name="stream">Readable stream where the IDX has been serialized.</param>
        /// <returns></returns>
        public static Idx Read(Stream stream)
        {
            BinaryMapping.SetMemberLengthMapping<Idx>(nameof(Items), (o, m) => o.Length);
            return BinaryMapping.ReadObject<Idx>(stream.SetPosition(0));
        }

        /// <summary>
        /// Serialize an IDX to a stream
        /// </summary>
        /// <param name="stream">Writable stream that will contain the IDX data</param>
        public void Write(Stream stream) => BinaryMapping.WriteObject(stream.SetPosition(0), this);

        /// <summary>
        /// Try to get an entry form a file name 
        /// </summary>
        /// <param name="name">file name to search</param>
        /// <param name="entry">Found entry</param>
        /// <returns>Return true if the entry has been found</returns>
        public bool TryGetEntry(string name, out Entry entry)
        {
            var hash32 = GetHash32(name);
            var hash16 = GetHash16(name);

            var dictionaryEntries = GetDictionaryEntries();
            if (dictionaryEntries.TryGetValue(hash32, out entry))
            {
                if (entry.Hash16 == hash16)
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Create a copy of the merge between the current IDX and the specified IDX.
        /// </summary>
        /// <param name="idx">Idx to merge in</param>
        /// <returns></returns>
        public Idx Merge(Idx idx)
        {
            var dictionaryEntries = Items
                .ToDictionary(x => x.Hash32, x => x);

            foreach (var item in idx.Items)
            {
                if (!dictionaryEntries.ContainsKey(item.Hash32))
                {
                    dictionaryEntries.Add(item.Hash32, item);
                }
            }

            return new Idx
            {
                Items = dictionaryEntries
                    .Values
                    .OrderBy(x => x.Hash32)
                    .ToList()
            };
        }

        private Dictionary<uint, Entry> GetDictionaryEntries()
        {
            if (dictionaryEntries == null)
                dictionaryEntries = Items.ToDictionary(x => x.Hash32, x => x);

            return dictionaryEntries;
        }

        /// <summary>
        /// Calculate an hash32 from a name
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static uint GetHash32(string text) => GetHash32(Encoding.UTF8.GetBytes(text));

        /// <summary>
        /// Calculate an hash32 from data
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static uint GetHash32(byte[] data)
        {
            int c = -1;
            for (int i = 0; i < data.Length; i++)
            {
                c ^= data[i] << 24;
                for (int j = 8; j > 0; j--)
                {
                    if (c < 0)
                        c = (c << 1) ^ 0x4C11DB7;
                    else
                        c <<= 1;
                }
            }

            return (uint)~c;
        }

        /// <summary>
        /// Calculate an hash16 from a name
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static ushort GetHash16(string text) => GetHash16(Encoding.UTF8.GetBytes(text));

        /// <summary>
        /// Calculate an hash16 from data
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static ushort GetHash16(byte[] data)
        {
            int s1 = -1;
            for (int i = data.Length - 1; i >= 0; i--)
            {
                s1 = (s1 ^ (data[i] << 8)) & 0xFFFF;
                for (int j = 8; j > 0; j--)
                {
                    if ((s1 & 0x8000) != 0)
                        s1 = ((s1 << 1) ^ 0x1021) & 0xFFFF;
                    else
                        s1 <<= 1;
                }
            }
            return (ushort)~s1;
        }
    }
}
