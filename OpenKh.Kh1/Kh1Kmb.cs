using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace OpenKh.Kh1
{
    /// <summary>
    /// Preserving editor for remastered KH1 menu-message *.kmb files.
    /// </summary>
    public sealed class Kh1Kmb
    {
        public sealed class TextEntry
        {
            private readonly Kh1TextTable _table;
            private readonly byte[] _originalBytes;

            internal TextEntry(int index, int offset, byte[] bytes, Kh1TextTable table)
            {
                Index = index;
                Offset = offset;
                OriginalLength = bytes.Length;
                _originalBytes = bytes;
                _table = table;
                Text = Decode(bytes, table);
                OriginalText = Text;
            }

            public int Index { get; }
            public int Offset { get; }
            public int OriginalLength { get; }
            public string OriginalText { get; }
            public string Text { get; set; }
            public bool IsModified => !string.Equals(OriginalText, Text, StringComparison.Ordinal);

            internal byte[] Encode()
            {
                if (!IsModified)
                    return _originalBytes;

                using var output = new MemoryStream();
                var plainText = new StringBuilder();

                void FlushPlainText()
                {
                    if (plainText.Length == 0)
                        return;
                    output.Write(_table.Encode(plainText.ToString()));
                    plainText.Clear();
                }

                for (var position = 0; position < Text.Length; position++)
                {
                    if (Text[position] != '\r' && Text[position] != '\n')
                    {
                        plainText.Append(Text[position]);
                        continue;
                    }

                    FlushPlainText();
                    if (Text[position] == '\r' && position + 1 < Text.Length && Text[position + 1] == '\n')
                        position++;
                    output.WriteByte(0x02);
                }

                FlushPlainText();
                var bytes = output.ToArray();
                if (bytes.Contains((byte)0x00))
                    throw new InvalidDataException("KMB text cannot contain {eol}; byte 00 terminates the entry.");
                return bytes;
            }

            private static string Decode(byte[] bytes, Kh1TextTable table)
            {
                var output = new StringBuilder();
                var textStart = 0;
                for (var offset = 0; offset < bytes.Length; offset++)
                {
                    if (bytes[offset] != 0x02)
                        continue;

                    if (offset > textStart)
                        output.Append(table.Decode(bytes.AsSpan(textStart, offset - textStart)));
                    output.AppendLine();
                    textStart = offset + 1;
                }

                if (textStart < bytes.Length)
                    output.Append(table.Decode(bytes.AsSpan(textStart)));
                return output.ToString();
            }
        }

        private readonly byte[] _source;
        private readonly bool _usesCdPadding;

        private Kh1Kmb(byte[] source, IReadOnlyList<TextEntry> entries)
        {
            _source = source;
            _usesCdPadding = source.Length > 0 && source[source.Length - 1] == 0xCD;
            Entries = entries;
        }

        public IReadOnlyList<TextEntry> Entries { get; }

        public static Kh1Kmb Read(Stream stream, Kh1TextTable table)
        {
            ArgumentNullException.ThrowIfNull(stream);
            ArgumentNullException.ThrowIfNull(table);

            using var buffer = new MemoryStream();
            stream.CopyTo(buffer);
            var source = buffer.ToArray();
            if (source.Length < sizeof(uint))
                throw new InvalidDataException("The file is too small to be a KH1 KMB file.");

            var count = BitConverter.ToUInt32(source, 0);
            if (count > int.MaxValue || count > source.Length - sizeof(uint))
                throw new InvalidDataException("The KMB entry count is invalid.");

            var entries = new List<TextEntry>((int)count);
            var offset = sizeof(uint);
            for (var index = 0; index < count; index++)
            {
                var end = Array.IndexOf(source, (byte)0x00, offset);
                if (end < 0)
                    throw new InvalidDataException($"KMB entry #{index + 1} has no 00 terminator.");

                entries.Add(new TextEntry(
                    index,
                    offset,
                    source.AsSpan(offset, end - offset).ToArray(),
                    table));
                offset = end + 1;
            }

            return new Kh1Kmb(source, entries);
        }

        public static Kh1Kmb Read(Stream stream) => Read(stream, Kh1TextTable.Default);

        public static Kh1Kmb Read(string fileName, Kh1TextTable table)
        {
            using var stream = File.OpenRead(fileName);
            return Read(stream, table);
        }

        public static Kh1Kmb Read(string fileName) => Read(fileName, Kh1TextTable.Default);

        public void Write(Stream stream)
        {
            ArgumentNullException.ThrowIfNull(stream);

            if (!Entries.Any(x => x.IsModified))
            {
                stream.Write(_source);
                return;
            }

            stream.Write(_source, 0, sizeof(uint));
            foreach (var entry in Entries)
            {
                stream.Write(entry.Encode());
                stream.WriteByte(0x00);
            }

            var minimumLength = stream.Position + (_usesCdPadding ? 1 : 0);
            var targetLength = Math.Max(_source.Length, Align16(minimumLength));
            if (_usesCdPadding)
                stream.WriteByte(0x00);
            while (stream.Position < targetLength)
                stream.WriteByte(_usesCdPadding ? (byte)0xCD : (byte)0x00);
        }

        private static long Align16(long value) => (value + 0x0F) & ~0x0F;
    }
}
