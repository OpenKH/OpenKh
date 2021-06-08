using System;
using System.Collections.Generic;
using Xe.BinaryMapper;
using System.Linq;
using System.IO;
using OpenKh.Common;
using YamlDotNet.Serialization;

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

        public List<uint> Table1Offsets { get; set; } //Info entries (if SSCF version 4 sound entries have names)
        public List<uint> Table2Offsets { get; set; }
        public List<uint> Table3Offsets { get; set; } //Sound entries
        public List<uint> Table4Offsets { get; set; }
        public List<uint> Table5Offsets { get; set; }

        public class Table4
        {
            [Data(Count = 0x80)] public byte[] Unknown { get; set; }
        }
        public List<Table4> Table4Entries { get; set; }

        public class Table1
        {
            public byte[] Data { get; set; }
        }
        public List<Table1> Table1Entries { get; set; }

        public class Table2
        {
            [Data(Count = 0x58)] public byte[] Data { get; set; }
        }
        public List<Table2> Table2Entries { get; set; }

        public class Table5
        {
            [Data(Count = 0x7C)] public byte[] Data { get; set; }
        }
        public List<Table5> Table5Entries { get; set; }

        public class StreamFile
        {
            [Data] public uint StreamSize { get; set; }
            [Data] public uint ChannelCount { get; set; }
            [Data] public uint SampleRate { get; set; }
            [Data] public Codec Codec { get; set; }
            [Data] public uint LoopStart { get; set; }
            [Data] public uint LoopEnd { get; set; }
            [Data] public uint ExtraDataSize { get; set; }
            [Data] public uint AuxChunkCount { get; set; }

            [YamlIgnore] public byte[] ExtraData { get; set; }
            [YamlIgnore] public byte[] Data { get; set; }
            public string Name { get; set; }
        }
        public List<StreamFile> StreamFileEntries { get; set; }

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

        public Header header = new Header();
        public TableOffsetHeader tableOffsetHeader = new TableOffsetHeader();

        private Scd(Stream stream)
        {
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
            Table5Offsets.RemoveAll(x => x == 0);

            Table4Entries = new List<Table4>();
            foreach (var offset in Table4Offsets)
            {
                stream.Position = offset;
                Table4Entries.Add(BinaryMapping.ReadObject<Table4>(stream));
            }

            Table1Entries = new List<Table1>();
            for (int i = 0; i < Table1Offsets.Count; i++)
            {
                stream.Position = Table1Offsets[i];
                uint len;
                if (i == Table1Offsets.Count - 1)
                    len = Table2Offsets[0] - Table1Offsets[i];
                else
                    len = Table1Offsets[i + 1] - Table1Offsets[i];
                Table1 table = new();
                table.Data = stream.ReadBytes((int)len);
                Table1Entries.Add(table);
            }

            Table2Entries = new List<Table2>();
            foreach (var offset in Table2Offsets)
            {
                stream.Position = offset;
                Table2Entries.Add(BinaryMapping.ReadObject<Table2>(stream));
            }

            Table5Entries = new List<Table5>();
            foreach (var offset in Table5Offsets)
            {
                stream.Position = offset;
                Table5Entries.Add(BinaryMapping.ReadObject<Table5>(stream));
            }

            StreamFileEntries = new List<StreamFile>();
            foreach (var offset in Table3Offsets)
            {
                stream.Position = offset;
                var streamInfo = BinaryMapping.ReadObject<StreamFile>(stream);

                streamInfo.ExtraData = stream.ReadBytes((int)streamInfo.ExtraDataSize);
                streamInfo.Data = stream.ReadBytes((int)streamInfo.StreamSize);

                //extract stream names (if v4) or set number as name
                // ...

                StreamFileEntries.Add(streamInfo);
            }
        }

        public static Scd Read(Stream stream) => new(stream);

        public void Write(Stream stream)
        {
            BinaryMapping.WriteObject(stream, header);
            BinaryMapping.WriteObject(stream, tableOffsetHeader);

            WriteTableOffsets(stream, Table1Offsets);
            WriteTableOffsets(stream, Table2Offsets);
            WriteTableOffsets(stream, Table3Offsets);
            WriteTableOffsets(stream, Table4Offsets);
            WriteTableOffsets(stream, Table5Offsets);

            foreach (var ent in Table4Entries)
                BinaryMapping.WriteObject(stream, ent);

            foreach (var ent in Table1Entries)
            {
                stream.Write(ent.Data);
                //padding?
            }

            foreach (var ent in Table2Entries)
                BinaryMapping.WriteObject(stream, ent);

            stream.Position = Helpers.Align((int)stream.Position, Alignment);

            foreach (var ent in Table5Entries)
                BinaryMapping.WriteObject(stream, ent);

            stream.Position = Helpers.Align((int)stream.Position, Alignment);

            foreach (var ent in StreamFileEntries)
            {
                BinaryMapping.WriteObject(stream, ent);
                stream.Write(ent.ExtraData);
                stream.Write(ent.Data);

                var misalignment = Helpers.Align((int)stream.Position, Alignment) - stream.Position;
                while (misalignment-- > 0)
                    stream.WriteByte(0);
            }
        }

        private static void WriteTableOffsets(Stream stream, List<uint> table)
        {
            foreach (var offset in table)
            {
                stream.Write(offset);
            }
            SetPaddedPosition(stream, table.Count);
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
