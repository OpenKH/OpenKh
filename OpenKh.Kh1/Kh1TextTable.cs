using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace OpenKh.Kh1
{
    /// <summary>
    /// Reads the text-table format commonly used to describe the KH1 message encoding.
    /// </summary>
    public sealed class Kh1TextTable
    {
        private sealed class Entry
        {
            public byte[] Bytes { get; init; }
            public string Text { get; init; }
        }

        private readonly List<Entry> _decodeEntries;
        private readonly List<Entry> _encodeEntries;
        private readonly HashSet<string> _ambiguousText;

        /// <summary>
        /// Built-in international KH1 encoding used by the remastered BINL files.
        /// Update <see cref="CreateDefault"/> when the game's character table changes.
        /// </summary>
        public static Kh1TextTable Default { get; } = CreateDefault();

        private Kh1TextTable(IEnumerable<Entry> entries)
        {
            var entryList = entries.ToList();
            _decodeEntries = entryList
                .OrderByDescending(x => x.Bytes.Length)
                .ToList();

            _ambiguousText = entryList
                .GroupBy(x => x.Text, StringComparer.Ordinal)
                .Where(x => x.Count() > 1)
                .Select(x => x.Key)
                .ToHashSet(StringComparer.Ordinal);

            _encodeEntries = entryList
                .Where(x => !_ambiguousText.Contains(x.Text))
                .OrderByDescending(x => x.Text.Length)
                .ToList();
        }

        public static Kh1TextTable Read(Stream stream)
        {
            ArgumentNullException.ThrowIfNull(stream);

            var entries = new List<Entry>();
            var byteKeys = new HashSet<string>(StringComparer.Ordinal);
            using var reader = new StreamReader(stream, new UTF8Encoding(false, true), true, 1024, true);
            var lineNumber = 0;
            while (reader.ReadLine() is string line)
            {
                lineNumber++;
                if (string.IsNullOrWhiteSpace(line) || line.TrimStart().StartsWith('#'))
                    continue;

                var separator = line.IndexOf('=');
                if (separator <= 0)
                    throw new InvalidDataException($"Invalid TBL entry at line {lineNumber}. Expected HEX=text.");

                var hex = line.Substring(0, separator).Trim().Replace(" ", string.Empty);
                var text = line.Substring(separator + 1);
                if (hex.Length == 0 || (hex.Length & 1) != 0 || !hex.All(Uri.IsHexDigit))
                    throw new InvalidDataException($"Invalid hexadecimal key at TBL line {lineNumber}.");
                if (text.Length == 0)
                    throw new InvalidDataException($"Empty text value at TBL line {lineNumber} is not supported.");

                var bytes = Convert.FromHexString(hex);
                var byteKey = Convert.ToHexString(bytes);
                if (!byteKeys.Add(byteKey))
                    throw new InvalidDataException($"Duplicate byte key {byteKey} at TBL line {lineNumber}.");

                entries.Add(new Entry { Bytes = bytes, Text = text });
            }

            if (entries.Count == 0)
                throw new InvalidDataException("The TBL file contains no entries.");

            return new Kh1TextTable(entries);
        }

        public static Kh1TextTable Read(string fileName)
        {
            using var stream = File.OpenRead(fileName);
            return Read(stream);
        }

        private static Kh1TextTable CreateDefault()
        {
            var entries = new List<Entry>();
            void Add(byte value, string text) =>
                entries.Add(new Entry { Bytes = new[] { value }, Text = text });

            Add(0x00, "{eol}");
            Add(0x01, " ");
            Add(0x02, "{lf}");
            Add(0x0F, "{ctrl:0F}");
            Add(0x20, "—");

            for (var value = 0; value < 10; value++)
                Add((byte)(0x21 + value), value.ToString(CultureInfo.InvariantCulture));
            for (var value = 0; value < 26; value++)
            {
                Add((byte)(0x2B + value), ((char)('A' + value)).ToString());
                Add((byte)(0x45 + value), ((char)('a' + value)).ToString());
            }

            foreach (var (value, text) in new (byte, string)[]
            {
                (0x5F, "!"), (0x60, "?"), (0x61, "&"), (0x62, "%"),
                (0x63, "+"), (0x64, "{-}"), (0x65, "{mX}"), (0x66, "/"),
                (0x67, "*"), (0x68, "."), (0x69, ","), (0x6A, "・"),
                (0x6B, ":"), (0x6C, ";"), (0x6D, "…"), (0x6E, "-"),
                (0x6F, "ー"), (0x70, "~"), (0x71, "'"), (0x72, "\""),
                (0x73, "{゛b}"), (0x74, "("), (0x75, ")"), (0x76, "["),
                (0x77, "]"), (0x78, "<"), (0x79, ">"), (0x7A, "★"),
                (0x7B, "☆"), (0x7C, "↑"), (0x7D, "↓"), (0x7E, "→"),
                (0x7F, "←"), (0x80, "●"), (0x81, "■"),
                (0x82, "{iPotion}"), (0x83, "{iTent}"), (0x84, "{iGem}"),
                (0x85, "{iAbility}"), (0x86, "{iKey}"), (0x87, "{iStaff}"),
                (0x88, "{iShield}"), (0x89, "{iRing}"), (0x8A, "{iHat}"),
                (0x8B, "{iMickey}"), (0x8C, "○"), (0x8D, "×"),
                (0x8E, "△"), (0x8F, "□"), (0x90, "▲"), (0x91, "▼"),
                (0x92, "►"), (0x93, "◄"), (0xA9, "®"),
                (0xC4, "{III}"), (0xC5, "{VII}"), (0xC6, "{VIII}"),
                (0xC7, "{X}"), (0xC8, "Œ"), (0xC9, "œ"),
                (0xCA, "¡"), (0xCB, "¿"), (0xCC, "À"), (0xCD, "Á"),
                (0xCE, "Â"), (0xCF, "Ä"), (0xD0, "Ç"), (0xD1, "È"),
                (0xD2, "É"), (0xD3, "Ê"), (0xD4, "Ë"), (0xD5, "Ì"),
                (0xD6, "Í"), (0xD7, "Î"), (0xD8, "Ï"), (0xD9, "Ñ"),
                (0xDA, "Ò"), (0xDB, "Ó"), (0xDC, "Ô"), (0xDD, "Ö"),
                (0xDE, "Ù"), (0xDF, "Ú"), (0xE0, "Û"), (0xE1, "Ü"),
                (0xE2, "ß"), (0xE3, "à"), (0xE4, "á"), (0xE5, "â"),
                (0xE6, "ä"), (0xE7, "ç"), (0xE8, "è"), (0xE9, "é"),
                (0xEA, "ê"), (0xEB, "ë"), (0xEC, "ì"), (0xED, "í"),
                (0xEE, "î"), (0xEF, "ï"), (0xF0, "ñ"), (0xF1, "ò"),
                (0xF2, "ó"), (0xF3, "ô"), (0xF4, "ö"), (0xF5, "ù"),
                (0xF6, "ú"), (0xF7, "û"), (0xF8, "ü"), (0xF9, "°"),
                (0xFA, "{---}"), (0xFB, "》"), (0xFC, "《"),
            })
                Add(value, text);

            return new Kh1TextTable(entries);
        }

        public string Decode(ReadOnlySpan<byte> data)
        {
            var result = new StringBuilder();
            for (var offset = 0; offset < data.Length;)
            {
                var entry = FindDecodeEntry(data.Slice(offset));
                if (entry == null)
                {
                    result.AppendFormat(CultureInfo.InvariantCulture, "{{0x{0:X2}}}", data[offset]);
                    offset++;
                }
                else
                {
                    if (_ambiguousText.Contains(entry.Text))
                        result.Append("{0x").Append(Convert.ToHexString(entry.Bytes)).Append('}');
                    else
                        result.Append(entry.Text);
                    offset += entry.Bytes.Length;
                }
            }

            return result.ToString();
        }

        public byte[] Encode(string text)
        {
            ArgumentNullException.ThrowIfNull(text);

            using var output = new MemoryStream();
            for (var offset = 0; offset < text.Length;)
            {
                if (TryReadRawByteToken(text, offset, out var rawBytes, out var tokenLength))
                {
                    output.Write(rawBytes);
                    offset += tokenLength;
                    continue;
                }

                var entry = _encodeEntries.FirstOrDefault(x =>
                    text.AsSpan(offset).StartsWith(x.Text.AsSpan(), StringComparison.Ordinal));
                if (entry == null)
                {
                    var display = char.ConvertToUtf32(text, offset);
                    throw new InvalidDataException(
                        $"Text at position {offset + 1} cannot be encoded by the selected TBL (U+{display:X4}).");
                }

                output.Write(entry.Bytes);
                offset += entry.Text.Length;
            }

            return output.ToArray();
        }

        private Entry FindDecodeEntry(ReadOnlySpan<byte> data)
        {
            foreach (var entry in _decodeEntries)
            {
                if (data.StartsWith(entry.Bytes))
                    return entry;
            }
            return null;
        }

        private static bool TryReadRawByteToken(
            string text,
            int offset,
            out byte[] bytes,
            out int tokenLength)
        {
            bytes = null;
            tokenLength = 0;
            if (!text.AsSpan(offset).StartsWith("{0x".AsSpan(), StringComparison.OrdinalIgnoreCase))
                return false;

            var closeBrace = text.IndexOf('}', offset + 3);
            if (closeBrace < 0)
                return false;

            var hex = text.Substring(offset + 3, closeBrace - offset - 3);
            if (hex.Length == 0 || (hex.Length & 1) != 0 || !hex.All(Uri.IsHexDigit))
                return false;

            bytes = Convert.FromHexString(hex);
            tokenLength = closeBrace - offset + 1;
            return true;
        }
    }
}
