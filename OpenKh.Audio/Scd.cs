using System;
using System.Collections.Generic;
using Xe.BinaryMapper;
using System.Linq;
using System.IO;
using OpenKh.Common;

namespace OpenKh.Audio
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
            [Data] public Codec Codec { get; set; }
            [Data] public uint LoopStart { get; set; }
            [Data] public uint LoopEnd { get; set; }
            [Data] public uint ExtraDataSize { get; set; }
            [Data] public uint AuxChunkCount { get; set; }

            public byte[] ExtraData { get; set; }
            public byte[] Data { get; set; }
        }

        public enum Codec : uint
        {
            Pcm = 0x1,
            PsAdpcm = 0x3,
            OggVorbis = 0x6,
            Mpeg = 0x7,
            MsAdpcm = 0xc,
            DspAdpcm = 0xa,
            Xma2 = 0xb,
            DspAdpcm2 = 0x15,
            Atrac9 = 0x16
        }

        public static Header header = new Header();
        public static TableOffsetHeader tableOffsetHeader = new TableOffsetHeader();
        public static List<StreamHeader> StreamFiles = new List<StreamHeader>();

        public static Scd Read(Stream stream)
        {
            Scd scd = new();

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

                streamInfo.ExtraData = stream.ReadBytes((int)streamInfo.ExtraDataSize);
                streamInfo.Data = stream.ReadBytes((int)streamInfo.StreamSize);


                StreamFiles.Add(streamInfo);
            }
            return scd;
        }

        public static void Write(Scd scd, Stream stream)
        {

        }
    }
}
