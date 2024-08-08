using System;
using System.Collections.Generic;
using Xe.BinaryMapper;
using System.Linq;
using System.IO;
using System.Text;
using OpenKh.Common;

namespace OpenKh.Bbs
{
    public class Scd
    {
        private const UInt64 MagicCode = 0x4643535342444553;
        private const uint FileVersion = 3;
        private const ushort SSCFVersion = 0x400;

        public class Header
        {
            [Data] public UInt64 MagicCode { get; set; }
            [Data] public uint FileVersion { get; set; }
            [Data] public ushort SSCFVersion { get; set; }
            [Data] public ushort HeaderSize { get; set; }
            [Data] public uint TotalFileSize { get; set; }
            [Data(Count = 7)] public uint[] Padding { get; set; }
        }

        public class TableOffsetHeader
        {
            [Data] public ushort Table0ElementCount { get; set; }
            [Data] public ushort Table1ElementCount { get; set; }
            [Data] public ushort Table2ElementCount { get; set; }
            [Data] public ushort Unk06 { get; set; }
            [Data] public uint Table0Offset { get; set; }
            [Data] public uint Table1Offset { get; set; }
            [Data] public uint Table2Offset { get; set; }
            [Data] public uint Unk14 { get; set; }
            [Data] public uint Unk18 { get; set; }
            [Data] public uint Padding { get; set; }
        }

        public class StreamHeader
        {
            [Data] public uint StreamSize { get; set; }
            [Data] public uint ChannelCount { get; set; }
            [Data] public uint SampleRate { get; set; }
            [Data] public uint Codec { get; set; } //6 = .ogg, everything else is msadpcm
            [Data] public uint LoopStart { get; set; }
            [Data] public uint LoopEnd { get; set; }
            [Data] public uint ExtraDataSize { get; set; }
            [Data] public uint AuxChunkCount { get; set; }
        }

        public class VAGHeader
        {
            [Data] public uint Magic { get; set; }
            [Data] public uint Version { get; set; } // Usually 3
            [Data] public uint Reserved08 { get; set; }
            [Data] public uint DataSize { get; set; }
            [Data] public uint SamplingFrequency { get; set; }
            [Data(Count = 10)] public byte[] Reserved14 { get; set; }
            [Data] public byte ChannelCount { get; set; }
            [Data] public byte Reserved1F { get; set; }
            [Data(Count = 16)] public string Name { get; set; }
        }

        public Header header = new();
        public TableOffsetHeader tableOffsetHeader = new();
        public List<StreamHeader> StreamHeaders = [];
        public List<byte[]> StreamFiles = [];
        public List<byte[]> MediaFiles = [];

        public static Scd Read(Stream stream)
        {
            var scd = new Scd
            {
                header = BinaryMapping.ReadObject<Header>(stream),
                tableOffsetHeader = BinaryMapping.ReadObject<TableOffsetHeader>(stream),
            };

            stream.Seek(scd.tableOffsetHeader.Table1Offset, SeekOrigin.Begin);

            var SoundOffsets = new List<uint>();
            for (var i = 0; i < scd.tableOffsetHeader.Table1ElementCount; i++)
            {
                SoundOffsets.Add(stream.ReadUInt32());
            }

            for (var index = 0; index < SoundOffsets.Count; index++)
            {
                var off = SoundOffsets[index];
                var next = (int)((index == SoundOffsets.Count - 1 ? stream.Length : SoundOffsets[index + 1]) - off);
                stream.Seek(off, SeekOrigin.Begin);
                var streamInfo = BinaryMapping.ReadObject<StreamHeader>(stream);
                scd.StreamHeaders.Add(streamInfo);

                var st = stream.ReadBytes(next - 0x20);
                scd.StreamFiles.Add(st);

                //https://github.com/Leinxad/KHPCSoundTools/blob/main/SCDInfo/Program.cs#L109
                if (streamInfo.Codec == 6)
                {
                    var extradataOffset = 0u;
                    if (streamInfo.AuxChunkCount > 0) extradataOffset += BitConverter.ToUInt32(st.Skip((int)extradataOffset).Take(4).ToArray(), 0);

                    var encryptionKey = st[extradataOffset + 0x02];
                    var seekTableSize = BitConverter.ToUInt32(st.Skip((int)extradataOffset + 0x10).Take(4).ToArray(), 0);
                    var vorbHeaderSize = BitConverter.ToUInt32(st.Skip((int)extradataOffset + 0x14).Take(4).ToArray(), 0);
                
                    var startOffset = extradataOffset + 0x20 + seekTableSize;

                    var decryptedFile = st.ToArray();

                    var endPosition = startOffset + vorbHeaderSize;
                
                    for (var i = startOffset; i < endPosition; i++)
                    {
                        decryptedFile[i] = (byte)(decryptedFile[i]^encryptionKey);
                    }
                    
                    var oggSize = vorbHeaderSize + streamInfo.StreamSize;
                    
                    scd.MediaFiles.Add(decryptedFile.Skip((int)startOffset).Take((int)oggSize).ToArray());
                }
                else scd.MediaFiles.Add(Array.Empty<byte>());
                


                // Convert to VAG Streams.
                /*VAGHeader vagHeader = new VAGHeader();
                vagHeader.ChannelCount = (byte)streamInfo.ChannelCount;
                vagHeader.SamplingFrequency = (byte)streamInfo.SampleRate;
                vagHeader.Version = 3;
                vagHeader.Magic = 0x70474156;
                vagHeader.Name = p.ToString();*/
            }


            return scd;
        }

        public static void Write(Scd scd, Stream stream)
        {

        }
    }
}
