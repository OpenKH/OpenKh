using System;
using System.Collections.Generic;
using Xe.BinaryMapper;
using System.Linq;
using System.IO;
using OpenKh.Common;

namespace OpenKh.Audio
{
    public partial class Scd
    {
        private const ulong MagicCode = 0x4643535342444553;
        private const uint FileVersion = 3;
        private const ushort SSCFVersion = 0x400;
        private const int Alignment = 16;

        public class Header
        {
            [Data] public ulong MagicCode { get; set; }
            [Data] public uint FileVersion { get; set; }
            [Data] public ushort SSCFVersion { get; set; }
            [Data] public ushort HeaderSize { get; set; }
            [Data] public uint TotalFileSize { get; set; }
            [Data(Count = 7)] public uint[] Padding { get; set; }
        }

        public class TableOffsetHeader
        {
            //renamed table numbers according to vgmstream's source code
            [Data] public ushort Table1ElementCount { get; set; } //also for table 4
            [Data] public ushort Table2ElementCount { get; set; }
            [Data] public ushort Table3ElementCount { get; set; }
            [Data] public ushort Unk06 { get; set; }
            [Data] public uint Table2Offset { get; set; }
            [Data] public uint Table3Offset { get; set; }
            [Data] public uint Table4Offset { get; set; }
            [Data] public uint Unk14 { get; set; }
            [Data] public uint Table5Offset { get; set; } //strange
            [Data] public uint Padding { get; set; }
        }

        /* Header sequence:
         * Table1
         * Table2
         * Table3
         * Table4
         * Table5
         * 
         * In-file sequence:
         * Table4
         * Table1
         * Table2
         * Table5
         * Table3
         */

        public static List<uint> Table1Offsets { get; set; } //Info entries (if SSCF version 4 sound entries have names)
        public static List<uint> Table2Offsets { get; set; }
        public static List<uint> Table3Offsets { get; set; } //Sound entries
        public static List<uint> Table4Offsets { get; set; }
        public static List<uint> Table5Offsets { get; set; }

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

            Table1Offsets = ReadTableOffsets(stream, tableOffsetHeader.Table1ElementCount).ToList();
            
            stream.Position = tableOffsetHeader.Table2Offset;
            Table2Offsets = ReadTableOffsets(stream, tableOffsetHeader.Table2ElementCount).ToList();
            
            stream.Position = tableOffsetHeader.Table3Offset;
            Table3Offsets = ReadTableOffsets(stream, tableOffsetHeader.Table3ElementCount).ToList();

            stream.Position = tableOffsetHeader.Table4Offset;
            Table4Offsets = ReadTableOffsets(stream, tableOffsetHeader.Table1ElementCount).ToList();

            SetPaddedPosition(stream, tableOffsetHeader.Table1ElementCount);
            var count = (int)(Table4Offsets[0] - stream.Position) / 4;
            Table5Offsets = ReadTableOffsets(stream, count).ToList();


            foreach (uint off in Table3Offsets)
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

        private static IEnumerable<uint> ReadTableOffsets(Stream stream, int count)
        {
            for (int i = 0; i < count; i++)
                yield return stream.ReadUInt32();
        }

        private static void SetPaddedPosition(Stream stream, int count)
        {
            var shift = (count * 4) % Alignment;
            stream.Position = shift > 0 ? stream.Position + Alignment - shift : stream.Position;
        }
    }
}
