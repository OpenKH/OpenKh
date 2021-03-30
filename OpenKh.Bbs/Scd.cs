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
            [Data] public uint Codec { get; set; }
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

        public static Header header = new Header();
        public static TableOffsetHeader tableOffsetHeader = new TableOffsetHeader();
        public static List<byte[]> StreamFiles = new List<byte[]>();

        public static Scd Read(Stream stream)
        {
            Scd scd = new Scd();

            header = BinaryMapping.ReadObject<Header>(stream);
            tableOffsetHeader = BinaryMapping.ReadObject<TableOffsetHeader>(stream);

            stream.Seek(tableOffsetHeader.Table1Offset, SeekOrigin.Begin);

            List<uint> SoundOffsets = new List<uint>();
            for (int i = 0; i < tableOffsetHeader.Table1ElementCount; i++)
            {
                SoundOffsets.Add(stream.ReadUInt32());
            }

            foreach(uint off in SoundOffsets)
            {
                stream.Seek(off, SeekOrigin.Begin);
                var streamInfo = BinaryMapping.ReadObject<StreamHeader>(stream);

                byte[] st = stream.ReadBytes((int)streamInfo.StreamSize);
                StreamFiles.Add(st);

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
