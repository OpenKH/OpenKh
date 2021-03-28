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
            [Data] public uint DirectoryPointer { get; set; }
            [Data] public int Offset { get; set; }
            [Data] public int Length { get; set; }
            [Data] public int Unused { get; set; }
            [Data(Count = 16)] public string Name { get; set; }

            public bool IsPointer => DirectoryPointer != 0;
        }

        public class Entry
        {
            public bool IsLink => DirectoryPointer != 0;
            public uint DirectoryPointer { get; set; }
            public string Name { get; set; }
            public byte[] Data { get; set; }

            public string Path
            {
                get
                {
                    if (DirectoryPointer == 0)
                        return Name;

                    var directory = Bbsa.GetDirectoryName(DirectoryPointer);
                    if (string.IsNullOrEmpty(directory))
                        return $"{DirectoryPointer:X08}/{Name}";

                    return $"{directory}/{Name}";

                }
            }
        }

        public static IEnumerable<Entry> Read(Stream stream)
        {
            var header = BinaryMapping.ReadObject<Header>(stream.SetPosition(0));

            return Enumerable.Range(0, header.EntryCount)
                .Select(x => BinaryMapping.ReadObject<MetaEntry>(stream))
                .ToArray()
                .Select(x => new Entry
                {
                    DirectoryPointer = x.DirectoryPointer,
                    Name = x.Name,
                    Data = x.IsPointer ? null : stream.SetPosition(x.Offset).ReadBytes(x.Length)
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
                    DirectoryPointer = entry.DirectoryPointer,
                    Offset = entry.IsLink ? 0 : dataStartOffset,
                    Length = entry.IsLink ? 0 : entry.Data.Length,
                    Unused = 0,
                    Name = entry.Name
                });

                dataStartOffset += Helpers.Align(entry.Data?.Length ?? 0, Alignment);
            }

            foreach (var entry in myEntries.Where(x => !x.IsLink))
            {
                stream.Write(entry.Data, 0, entry.Data.Length);
                stream.AlignPosition(Alignment);
            }
        }

        public static bool IsValid(Stream stream) =>
            stream.Length >= 4 &&
            new BinaryReader(stream.SetPosition(0)).ReadInt32() == MagicCode;
    }
}
