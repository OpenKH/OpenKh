using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace OpenKh.Kh1
{
    /// <summary>
    /// Preserving editor for the null-terminated KH1 text tables stored in
    /// remastered *.bin files.
    /// </summary>
    public sealed class Kh1TextBin
    {
        private readonly byte[] _source;

        private Kh1TextBin(byte[] source, IReadOnlyList<Kh1Kmb.TextEntry> entries)
        {
            _source = source;
            Entries = entries;
        }

        public IReadOnlyList<Kh1Kmb.TextEntry> Entries { get; }

        public static Kh1TextBin Read(Stream stream, Kh1TextTable table)
        {
            ArgumentNullException.ThrowIfNull(stream);
            ArgumentNullException.ThrowIfNull(table);

            using var buffer = new MemoryStream();
            stream.CopyTo(buffer);
            var source = buffer.ToArray();
            var entries = new List<Kh1Kmb.TextEntry>();
            var contentLength = source.Length;
            while (contentLength > 0 && source[contentLength - 1] == 0xCD)
                contentLength--;

            for (var offset = 0; offset < contentLength;)
            {
                var end = Array.IndexOf(source, (byte)0x00, offset, contentLength - offset);
                if (end < 0)
                    throw new InvalidDataException("The KH1 BIN text table has an unterminated entry.");

                if (end > offset)
                {
                    entries.Add(new Kh1Kmb.TextEntry(
                        entries.Count,
                        offset,
                        source.AsSpan(offset, end - offset).ToArray(),
                        table));
                }
                offset = end + 1;
            }

            if (entries.Count == 0)
                throw new InvalidDataException("The KH1 BIN file contains no text entries.");

            return new Kh1TextBin(source, entries);
        }

        public static Kh1TextBin Read(Stream stream) => Read(stream, Kh1TextTable.Default);

        public static Kh1TextBin Read(string fileName, Kh1TextTable table)
        {
            using var stream = File.OpenRead(fileName);
            return Read(stream, table);
        }

        public static Kh1TextBin Read(string fileName) => Read(fileName, Kh1TextTable.Default);

        public void Write(Stream stream)
        {
            ArgumentNullException.ThrowIfNull(stream);
            if (!Entries.Any(x => x.IsModified))
            {
                stream.Write(_source);
                return;
            }

            var cursor = 0;
            foreach (var entry in Entries)
            {
                stream.Write(_source, cursor, entry.Offset - cursor);
                stream.Write(entry.Encode());
                cursor = entry.Offset + entry.OriginalLength;
            }
            stream.Write(_source, cursor, _source.Length - cursor);

            while ((stream.Position & 0x0F) != 0)
                stream.WriteByte(0x00);
        }
    }
}
