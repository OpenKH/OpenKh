using OpenKh.Common;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xe.BinaryMapper;

namespace OpenKh.Bbs
{
    public static class Arc
    {
        private const int MagicCode = 0x435241;
        private const int Version = 1;
        private const int MetaEntrySize = 0x20;
        private const int Alignment = 0x10;

        private class Header
        {
            [Data] public int MagicCode { get; set; }
            [Data] public short Version { get; set; }
            [Data] public short EntryCount { get; set; }
            [Data] public int Unused08 { get; set; }
            [Data] public int Unused0c { get; set; }
        }

        private class MetaEntry
        {
            [Data] public int Unknown00 { get; set; }
            [Data] public int Offset { get; set; }
            [Data] public int Length { get; set; }
            [Data] public int Unknown0c { get; set; }
            [Data(Count = 16)] public string Name { get; set; }
        }

        public class Entry
        {
            public string Name { get; set; }
            public byte[] Data { get; set; }
        }

        public static IEnumerable<Entry> Read(Stream stream)
        {
            var header = BinaryMapping.ReadObject<Header>(stream);

            return Enumerable.Range(0, header.EntryCount)
                .Select(x => BinaryMapping.ReadObject<MetaEntry>(stream))
                .ToArray()
                .Select(x =>
                {
                    System.Diagnostics.Debug.Assert(x.Unknown00 == 0,
                        $"{nameof(x.Unknown00)} was expected to be 0 but was {x.Unknown00:X}");
                    System.Diagnostics.Debug.Assert(x.Unknown00 == 0,
                        $"{nameof(x.Unknown0c)} was expected to be 0 but was {x.Unknown0c:X}");

                    return new Entry
                    {
                        Name = x.Name,
                        Data = stream.SetPosition(x.Offset).ReadBytes(x.Length)
                    };
                })
                .ToArray();
        }

        public static void Write(this IEnumerable<Entry> entries, Stream stream)
        {
            var myEntries = entries.ToArray();

            stream.Position = 0;
            BinaryMapping.WriteObject(stream, new Header
            {
                MagicCode = MagicCode,
                Version = Version,
                EntryCount = (short)myEntries.Length,
                Unused08 = 0,
                Unused0c = 0
            });

            var dataStartOffset = (int)stream.Position + myEntries.Length * MetaEntrySize;
            foreach (var entry in myEntries)
            {
                BinaryMapping.WriteObject(stream, new MetaEntry
                {
                    Unknown00 = 0,
                    Offset = dataStartOffset,
                    Length = entry.Data.Length,
                    Unknown0c = 0,
                    Name = entry.Name
                });

                dataStartOffset += Helpers.Align(entry.Data.Length, Alignment);
            }

            foreach (var entry in myEntries)
            {
                stream.Write(entry.Data, 0, entry.Data.Length);
                stream.AlignPosition(Alignment);
            }
        }

        public static bool IsValid(Stream stream) =>
            new BinaryReader(stream.SetPosition(0)).ReadInt32() == MagicCode;
    }
}
