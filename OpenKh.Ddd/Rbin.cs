using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Xe;
using Xe.IO;
using Xe.BinaryMapper;
using OpenKh.Common.Exceptions;

namespace OpenKh.Ddd
{
    public class Rbin
    {
        private const int MagicCode = 0x52415243;

        private class Header
        {
            [Data] public int MagicCode { get; set; }
            [Data] public short Version { get; set; }
            [Data] public short FileCount { get; set; }
            [Data] public uint DataOffset { get; set; }
            [Data] public uint Reserved { get; set; }
            [Data(Count = 16)] public string MountPath { get; set; }
        }

        public class TocEntry
        {
            [Data] public uint Hash { get; set; }
            [Data] public uint NameOffset { get; set; }
            [Data] public uint Info { get; set; }
            [Data] public uint Offset { get; set; }

            public uint Size { get => Info & 0x7FFFFFFF; }
            public bool IsCompressed { get => (Info & 0x80000000) != 0; }

            public string Name { get; set; }

            public static TocEntry Read(Stream stream)
            {
                var entry = BinaryMapping.ReadObject<TocEntry>(stream);
                var currOffset = stream.Position;
                // NameOffset is realtive to it's own location and we've read it and 2 more ints since then
                // so subtract 12 bytes
                stream.Seek(currOffset + (entry.NameOffset - 12), SeekOrigin.Begin);
                entry.Name = stream.ReadCString();

                stream.Seek(currOffset, SeekOrigin.Begin);
                return entry;
            }
        }

        public int Version { get; set; }
        public string MountPath { get; set; }
        public List<TocEntry> TOC { get; set; }

        protected Rbin(Stream stream)
        {
            var header = BinaryMapping.ReadObject<Header>(stream);
            if (header.MagicCode != MagicCode)
                throw new InvalidFileException<Rbin>();

            Version = header.Version;
            MountPath = header.MountPath;

            var entryList = Enumerable.Range(0, header.FileCount)
                .Select(x => TocEntry.Read(stream))
                .ToList();
            TOC = entryList;
        }

        public static Rbin Read(Stream stream) => new Rbin(stream);
    }
}
