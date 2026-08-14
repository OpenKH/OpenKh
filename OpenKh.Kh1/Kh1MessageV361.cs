using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace OpenKh.Kh1
{
    /// <summary>
    /// Preserving editor for the remastered KH1 "Message v361" BINL format.
    /// </summary>
    public sealed class Kh1MessageV361
    {
        private static readonly byte[] Magic = Encoding.ASCII.GetBytes("Message v361");

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
                    throw new InvalidDataException("Message v361 text cannot contain {eol}; byte 00 terminates the entry.");
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
        private readonly int _offsetTableOffset;
        private readonly int _textOffset;
        private readonly int _offsetCount;
        private readonly bool _hasTrailingSentinel;
        private readonly bool _usesCdPadding;

        private Kh1MessageV361(
            byte[] source,
            int offsetTableOffset,
            int textOffset,
            int offsetCount,
            bool hasTrailingSentinel,
            IReadOnlyList<TextEntry> entries)
        {
            _source = source;
            _offsetTableOffset = offsetTableOffset;
            _textOffset = textOffset;
            _offsetCount = offsetCount;
            _hasTrailingSentinel = hasTrailingSentinel;
            _usesCdPadding = source.Length > 0 && source[source.Length - 1] == 0xCD;
            Entries = entries;
        }

        public IReadOnlyList<TextEntry> Entries { get; }

        public static bool IsValid(Stream stream)
        {
            ArgumentNullException.ThrowIfNull(stream);
            if (!stream.CanSeek || stream.Length < 0x20)
                return false;
            var oldPosition = stream.Position;
            Span<byte> magic = stackalloc byte[12];
            var read = stream.Read(magic);
            stream.Position = oldPosition;
            return read == magic.Length && magic.SequenceEqual(Magic);
        }

        public static Kh1MessageV361 Read(Stream stream, Kh1TextTable table)
        {
            ArgumentNullException.ThrowIfNull(stream);
            ArgumentNullException.ThrowIfNull(table);
            if (!IsValid(stream))
                throw new InvalidDataException("The file is not a KH1 Message v361 BINL file.");

            stream.Position = 0;
            using var buffer = new MemoryStream();
            stream.CopyTo(buffer);
            var source = buffer.ToArray();
            var count = BitConverter.ToUInt32(source, 0x0C);
            var offsetTableOffset = checked((int)BitConverter.ToUInt32(source, 0x10));
            var textOffset = checked((int)BitConverter.ToUInt32(source, 0x14));
            var offsetTableLength = checked((int)BitConverter.ToUInt32(source, 0x18));
            var textLength = checked((int)BitConverter.ToUInt32(source, 0x1C));

            var offsetCount = offsetTableLength / sizeof(ushort);
            if (count == 0 || count > int.MaxValue ||
                offsetTableOffset < 0x20 ||
                (offsetTableLength & 1) != 0 ||
                (offsetCount != count && offsetCount != count + 1) ||
                textOffset != offsetTableOffset + offsetTableLength ||
                textLength < 1 || textOffset + textLength > source.Length ||
                source[textOffset + textLength - 1] != 0x00)
                throw new InvalidDataException("The Message v361 header is invalid.");

            var entries = new List<TextEntry>((int)count);
            var offsets = new ushort[offsetCount];
            for (var index = 0; index < offsetCount; index++)
            {
                offsets[index] = BitConverter.ToUInt16(source, offsetTableOffset + index * sizeof(ushort));
                if ((index == 0 && offsets[index] != 0) ||
                    (index > 0 && offsets[index] < offsets[index - 1]) ||
                    offsets[index] >= textLength)
                    throw new InvalidDataException("The Message v361 offset table is invalid.");
            }

            var hasTrailingSentinel = offsetCount > count ||
                (offsets[count - 1] < textLength - 1 &&
                    source[textOffset + textLength - 2] == 0x00);
            var entriesEnd = textLength - (hasTrailingSentinel ? 1 : 0);
            if (offsetCount > count && offsets[count] != entriesEnd)
                throw new InvalidDataException("The Message v361 final offset is invalid.");
            for (var index = 0; index < count; index++)
            {
                var start = offsets[index];
                var end = index + 1 < count ? offsets[index + 1] : entriesEnd;
                var length = end - start;
                if (length == 0 || source[textOffset + end - 1] != 0x00)
                    throw new InvalidDataException($"Message v361 entry #{index + 1} has no 00 terminator.");
                entries.Add(new TextEntry(
                    index,
                    textOffset + start,
                    source.AsSpan(textOffset + start, length - 1).ToArray(),
                    table));
            }

            return new Kh1MessageV361(
                source,
                offsetTableOffset,
                textOffset,
                offsetCount,
                hasTrailingSentinel,
                entries);
        }

        public static Kh1MessageV361 Read(Stream stream) => Read(stream, Kh1TextTable.Default);

        public static Kh1MessageV361 Read(string fileName, Kh1TextTable table)
        {
            using var stream = File.OpenRead(fileName);
            return Read(stream, table);
        }

        public static Kh1MessageV361 Read(string fileName) => Read(fileName, Kh1TextTable.Default);

        public void Write(Stream stream)
        {
            ArgumentNullException.ThrowIfNull(stream);
            if (!Entries.Any(x => x.IsModified))
            {
                stream.Write(_source);
                return;
            }

            var encoded = Entries.Select(x => x.Encode()).ToList();
            var entriesLength = encoded.Sum(x => x.Length + 1);
            var textLength = entriesLength + (_hasTrailingSentinel ? 1 : 0);
            if (textLength > ushort.MaxValue)
                throw new InvalidDataException("The edited Message v361 text block exceeds 65535 bytes.");

            var header = _source.AsSpan(0, _textOffset).ToArray();
            BitConverter.GetBytes(textLength).CopyTo(header, 0x1C);
            var currentOffset = 0;
            for (var index = 0; index < encoded.Count; index++)
            {
                BitConverter.GetBytes((ushort)currentOffset)
                    .CopyTo(header, _offsetTableOffset + index * sizeof(ushort));
                currentOffset += encoded[index].Length + 1;
            }
            if (_offsetCount > encoded.Count)
                BitConverter.GetBytes((ushort)currentOffset)
                    .CopyTo(header, _offsetTableOffset + encoded.Count * sizeof(ushort));

            stream.Write(header);
            foreach (var entry in encoded)
            {
                stream.Write(entry);
                stream.WriteByte(0x00);
            }
            if (_hasTrailingSentinel)
                stream.WriteByte(0x00);

            var targetLength = Math.Max(_source.Length, Align16(stream.Position));
            while (stream.Position < targetLength)
                stream.WriteByte(_usesCdPadding ? (byte)0xCD : (byte)0x00);
        }

        private static long Align16(long value) => (value + 0x0F) & ~0x0F;
    }
}
