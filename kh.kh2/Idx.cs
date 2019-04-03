using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Xe.BinaryMapper;

namespace kh.kh2
{
    public class Idx
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
            [Data] public ushort Info { get; set; }

            /// <summary>
            /// Offset of the file in the archive, divided by the ISO block size
            /// </summary>
            [Data] public uint Offset { get; set; }

            /// <summary>
            /// Length of the file in bytes
            /// </summary>
            [Data] public uint Length { get; set; }

            /// <summary>
            /// ISO blocks used in the ISO.
            /// </summary>
            public uint BlockLength
            {
                get => (uint)(Info & MaxBlockLength);
                set
                {
                    if (value > MaxBlockLength)
                        throw new ArgumentOutOfRangeException(nameof(BlockLength), $"Cannot exceeds {MaxBlockLength}");

                    Info = (ushort)(value | (Info & 0xC000U));
                }
            }

            /// <summary>
            /// If the file is compressed.
            /// </summary>
            public bool IsCompressed
            {
                get => (Info & IsCompressedFlag) != 0;
                set
                {
                    if (value)
                        Info |= IsCompressedFlag;
                    else
                        Info = (ushort)(Info & ~IsCompressedFlag);
                }
            }

            /// <summary>
            /// If the file is not loaded in RAM, but streamed.
            /// </summary>
            public bool IsStreamed
            {
                get => (Info & IsStreamedFlag) != 0;
                set
                {
                    if (value)
                        Info |= IsStreamedFlag;
                    else
                        Info = (ushort)(Info & ~IsStreamedFlag);
                }
            }
        }

        [Data] public int Length
        {
            get => Items.TryGetCount();
            set => Items = Items.CreateOrResize(value);
        }

        [Data] public List<Entry> Items { get; set; }

        public static Idx Read(Stream stream)
        {
            BinaryMapping.SetMemberLengthMapping<Idx>(nameof(Items), (o, m) => o.Length);
            return BinaryMapping.ReadObject<Idx>(stream);
        }

        public void Write(Stream stream) => BinaryMapping.WriteObject<Idx>(stream, this);

        public static uint GetHash32(string text) => GetHash32(Encoding.UTF8.GetBytes(text));

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

        public static ushort GetHash16(string text) => GetHash16(Encoding.UTF8.GetBytes(text));

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
