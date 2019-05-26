using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xe.BinaryMapper;

namespace OpenKh.Common.Archives
{
    public class HdAsset
    {
        private class Header
        {
            [Data] public int OriginalLength { get; set; }
            [Data] public int AssetCount { get; set; }
            [Data] public int Unused08 { get; set; }
            [Data] public int Unused0c { get; set; }
            [Data] public List<HeaderEntry> Entries { get; set; }
        }

        private class HeaderEntry
        {
            [Data(Count = 0x20)] public string Name { get; set; }
            [Data] public int Offset { get; set; }
            [Data] public int Flags { get; set; }
            [Data] public int Length { get; set; }
            [Data] public int Unused { get; set; }
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

        static HdAsset()
        {
            BinaryMapping.SetMemberLengthMapping<Header>(nameof(Header.Entries), (o, memberName) => o.AssetCount);
        }

        private readonly Header _header;
        private Stream stream;
        private List<Entry> entries;

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

        private HdAsset()
        {
            Stream = new MemoryStream();
            Entries = new List<Entry>();
        }

        private HdAsset(Stream stream)
        {
            var binaryReader = new BinaryReader(stream);
            _header = BinaryMapping.ReadObject<Header>(binaryReader);

            Stream = new MemoryStream(_header.OriginalLength);
            stream.Copy(Stream, _header.OriginalLength);
            Entries = _header.Entries.Select(x => new Entry
            {
                Name = x.Name,
                Stream = GetSubStreamCopy(stream, x.Offset, x.Length),
                Flags = x.Flags,
            }).ToList();
        }

        public void Write(Stream stream)
        {
            _header.OriginalLength = (int)Stream.Length;
            _header.AssetCount = Entries.Count;
            _header.Entries = Entries.Select(x => new HeaderEntry
            {
                Name = x.Name,
                Offset = -1,
                Length = (int)x.Stream.Length,
                Flags = x.Flags,
                Unused = 0
            }).ToList();

            BinaryMapping.WriteObject(stream, _header);

            Stream.Position = 0;
            Stream.CopyTo(stream);
            for (var i = 0; i < Entries.Count; i++)
            {
                _header.Entries[i].Offset = (int)stream.Position;

                var srcStream = Entries[i].Stream;
                srcStream.Position = 0;
                srcStream.CopyTo(stream);
            }

            stream.Position = 0;
            BinaryMapping.WriteObject(stream, _header);
        }

        private Stream GetSubStreamCopy(Stream stream, int offset, int length)
        {
            var outStream = new MemoryStream(length);
            stream.Position = offset;
            stream.Copy(outStream, length);
            return outStream;
        }

        public static HdAsset New() => new HdAsset();
        public static HdAsset Read(Stream stream) => new HdAsset(stream);
    }
}
