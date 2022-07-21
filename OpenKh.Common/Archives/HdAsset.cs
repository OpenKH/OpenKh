using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Xe.BinaryMapper;

namespace OpenKh.Common.Archives
{
    public class HdAsset
    {
        private class Header
        {
            public int OriginalLength { get; set; }
            public int AssetCount { get; set; }
            public int Unused08 { get; set; }
            public int Unused0c { get; set; }
        }

        private class HeaderEntry
        {
            public string Name { get; set; }
            public int Offset { get; set; }
            public int Flags { get; set; }
            public int Length { get; set; }
            public int Unused { get; set; }

            public HeaderEntry() { }
            public HeaderEntry(Stream input)
            {
                using (var _br = new BinaryReader(input))
                {
                    Name = Encoding.Default.GetString(_br.ReadBytes(0x20));
                    Offset = _br.ReadInt32();
                    Flags = _br.ReadInt32();
                    Length = _br.ReadInt32();
                    Unused = _br.ReadInt32();
                }
            }
        }

        public class Entry
        {
            private Stream stream;

            public string Name { get; set; }
            public Stream Stream
            {
                get => stream;
                set => stream = value ?? throw new ArgumentNullException(nameof(Stream));
            }
            public int Flags { get; set; }

            public Entry()
            {
                Stream = new MemoryStream();
            }
        }

        private readonly Header _header;
        private Stream stream;
        private List<Entry> entries;

        /// <summary>
        /// Original file stream, without the ReMIX assets
        /// </summary>
        public Stream Stream
        {
            get => stream;
            set => stream = value ?? throw new ArgumentNullException(nameof(Stream));
        }

        public List<Entry> Entries
        {
            get => entries;
            set => entries = value ?? throw new ArgumentNullException(nameof(Entries));
        }

        public HdAsset()
        {
            _header = new Header();
            Stream = new MemoryStream();
            Entries = new List<Entry>();
        }

        private HdAsset(Stream stream)
        {
            _header = Helpers.FromRawData<Header>(stream.ReadBytes(0x10));

            var entries = Enumerable.Range(0, _header.AssetCount)
                .Select(_ => new HeaderEntry(stream))
                .ToList();

            Stream = new MemoryStream(_header.OriginalLength);
            stream.Copy(Stream, _header.OriginalLength);
            Stream.Position = 0;

            Entries = entries.Select(x => new Entry
            {
                Name = x.Name,
                Stream = GetSubStreamCopy(stream, x.Offset, x.Length).SetPosition(0),
                Flags = x.Flags,
            }).ToList();
        }

        public void Write(Stream stream)
        {
            _header.OriginalLength = (int)Stream.Length;
            _header.AssetCount = Entries.Count;
            var entries = Entries.Select(x => new HeaderEntry
            {
                Name = x.Name,
                Offset = -1,
                Length = (int)x.Stream.Length,
                Flags = x.Flags,
                Unused = 0
            }).ToList();

            Stream.SetPosition(0).CopyTo(stream.SetPosition(0x10 + Entries.Count * 0x30));
            for (var i = 0; i < Entries.Count; i++)
            {
                entries[i].Offset = (int)stream.Position;

                var srcStream = Entries[i].Stream;
                srcStream.Position = 0;
                srcStream.CopyTo(stream);
            }

            stream.Position = 0;
            stream.Write(Helpers.ToRawData(_header));

            foreach (var entry in entries)
                stream.Write(Helpers.ToRawData(entry));
        }

        private Stream GetSubStreamCopy(Stream stream, int offset, int length)
        {
            var outStream = new MemoryStream(length);
            stream.Position = offset;
            stream.Copy(outStream, length);
            return outStream;
        }

        public static HdAsset Read(Stream stream) => new HdAsset(stream);

        public static bool IsValid(Stream stream)
        {
            const int MinimumPossibleSizeForHeader = 0x10;
            const int EstimatedMaximumPossibleSizeForOriginalAsset = 32 * 1024 * 1024;
            const int EstimatedMaximumPossibleRemasteredAssetCount = 1024;

            if (stream.Length < MinimumPossibleSizeForHeader)
                return false;

            var originalAssetLength = stream.ReadInt32();
            if (originalAssetLength > EstimatedMaximumPossibleSizeForOriginalAsset)
                return false;

            var assetCount = stream.ReadInt32();
            if (assetCount >= EstimatedMaximumPossibleRemasteredAssetCount)
                return false;

            if (stream.ReadInt32() != 0)
                return false;
            if (stream.ReadInt32() != 0)
                return false;

            if (originalAssetLength + MinimumPossibleSizeForHeader > stream.Length)
                return false;

            return true;
        }
    }
}
