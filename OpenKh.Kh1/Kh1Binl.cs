using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace OpenKh.Kh1
{
    /// <summary>
    /// Preserving editor for remastered KH1 EvMsg *.binl files.
    /// Unknown bytecode and command parameters are kept byte-for-byte.
    /// </summary>
    public sealed class Kh1Binl
    {
        public sealed class TextEntry
        {
            private readonly Kh1TextTable _table;

            internal TextEntry(int index, int offset, int length, byte[] bytes, Kh1TextTable table)
            {
                Index = index;
                Offset = offset;
                OriginalLength = length;
                _table = table;
                Text = DecodeBody(bytes);
            }

            public int Index { get; }
            public int Offset { get; }
            public int OriginalLength { get; }
            public string Text { get; set; }

            internal byte[] EncodeBody()
            {
                var output = new List<byte>();
                var plainText = new StringBuilder();

                void FlushPlainText()
                {
                    if (plainText.Length == 0)
                        return;
                    output.AddRange(_table.Encode(plainText.ToString()));
                    plainText.Clear();
                }

                for (var position = 0; position < Text.Length;)
                {
                    if (Text[position] == '\r' || Text[position] == '\n')
                    {
                        FlushPlainText();
                        if (Text[position] == '\r' && position + 1 < Text.Length && Text[position + 1] == '\n')
                            position++;
                        output.Add(0x02);
                        position++;
                        continue;
                    }

                    if (TryReadCommandToken(Text, position, out var command, out var tokenLength))
                    {
                        FlushPlainText();
                        ValidateCommand(command);
                        output.AddRange(command);
                        position += tokenLength;
                        continue;
                    }

                    plainText.Append(Text[position]);
                    position++;
                }

                FlushPlainText();
                ValidateBody(output);
                return output.ToArray();
            }

            private string DecodeBody(byte[] bytes)
            {
                var output = new StringBuilder();
                for (var offset = 0; offset < bytes.Length;)
                {
                    var opcode = bytes[offset];
                    if (opcode == 0x01)
                    {
                        output.Append(' ');
                        offset++;
                    }
                    else if (opcode == 0x02)
                    {
                        output.AppendLine();
                        offset++;
                    }
                    else if (opcode <= 0x0E)
                    {
                        var length = GetInstructionLength(opcode);
                        if (offset + length > bytes.Length)
                            length = bytes.Length - offset;
                        output.Append("{cmd:")
                            .Append(string.Join(" ", bytes.AsSpan(offset, length).ToArray().Select(x => x.ToString("X2"))))
                            .Append('}');
                        offset += length;
                    }
                    else
                    {
                        var nextControl = offset + 1;
                        while (nextControl < bytes.Length && bytes[nextControl] > 0x0E)
                            nextControl++;
                        output.Append(_table.Decode(bytes.AsSpan(offset, nextControl - offset)));
                        offset = nextControl;
                    }
                }

                return output.ToString();
            }

            private static bool TryReadCommandToken(
                string text,
                int offset,
                out byte[] command,
                out int tokenLength)
            {
                command = null;
                tokenLength = 0;
                if (!text.AsSpan(offset).StartsWith("{cmd:".AsSpan(), StringComparison.OrdinalIgnoreCase))
                    return false;

                var closeBrace = text.IndexOf('}', offset + 5);
                if (closeBrace < 0)
                    throw new InvalidDataException($"Command token at position {offset + 1} has no closing brace.");

                var value = text.Substring(offset + 5, closeBrace - offset - 5)
                    .Replace(" ", string.Empty);
                if (value.Length == 0 || (value.Length & 1) != 0 || !value.All(Uri.IsHexDigit))
                    throw new InvalidDataException($"Invalid command token at position {offset + 1}.");

                command = Convert.FromHexString(value);
                tokenLength = closeBrace - offset + 1;
                return true;
            }

            private static void ValidateCommand(byte[] command)
            {
                if (command.Length == 0 || command[0] > 0x0E)
                    throw new InvalidDataException("A {cmd:...} token must begin with a BINL opcode from 00 to 0E.");
                if (command.Length != GetInstructionLength(command[0]))
                    throw new InvalidDataException(
                        $"BINL command {command[0]:X2} must contain {GetInstructionLength(command[0])} byte(s).");
            }

            private static void ValidateBody(IReadOnlyList<byte> bytes)
            {
                for (var offset = 0; offset < bytes.Count;)
                {
                    var opcode = bytes[offset];
                    var length = opcode <= 0x0E ? GetInstructionLength(opcode) : 1;
                    if (offset + length > bytes.Count)
                        throw new InvalidDataException($"Truncated BINL command {opcode:X2} in edited text.");
                    if (opcode == 0x05 || opcode == 0x06 || opcode == 0x0A)
                        throw new InvalidDataException(
                            $"Structural BINL command {opcode:X2} cannot be inserted into editable text.");
                    offset += length;
                }
            }
        }

        private readonly byte[] _source;
        private readonly int _contentLength;

        private Kh1Binl(byte[] source, int contentLength, IReadOnlyList<TextEntry> entries)
        {
            _source = source;
            _contentLength = contentLength;
            Entries = entries;
            Language = Encoding.ASCII.GetString(source, 5, 2);
        }

        public string Language { get; }
        public IReadOnlyList<TextEntry> Entries { get; }

        public static bool IsValid(Stream stream)
        {
            ArgumentNullException.ThrowIfNull(stream);
            if (!stream.CanSeek || stream.Length < 12)
                return false;

            var oldPosition = stream.Position;
            Span<byte> magic = stackalloc byte[5];
            var read = stream.Read(magic);
            stream.Position = oldPosition;
            return read == magic.Length && magic.SequenceEqual("EvMsg"u8);
        }

        public static Kh1Binl Read(Stream stream, Kh1TextTable table)
        {
            ArgumentNullException.ThrowIfNull(stream);
            ArgumentNullException.ThrowIfNull(table);
            if (!IsValid(stream))
                throw new InvalidDataException("The file is not a KH1 EvMsg BINL file.");

            stream.Position = 0;
            using var buffer = new MemoryStream();
            stream.CopyTo(buffer);
            var source = buffer.ToArray();
            var contentLength = source.Length;
            while (contentLength > 11 && source[contentLength - 1] == 0xCD)
                contentLength--;

            var recordOffsets = FindRecordOffsets(source, contentLength);
            var entries = new List<TextEntry>();
            for (var recordIndex = 0; recordIndex < recordOffsets.Count; recordIndex++)
            {
                var recordStart = recordOffsets[recordIndex];
                var recordEnd = recordIndex + 1 < recordOffsets.Count
                    ? recordOffsets[recordIndex + 1]
                    : contentLength;
                if (TryFindBody(source, recordStart + 4, recordEnd, out var bodyStart, out var bodyEnd))
                {
                    entries.Add(new TextEntry(
                        entries.Count,
                        bodyStart,
                        bodyEnd - bodyStart,
                        source.AsSpan(bodyStart, bodyEnd - bodyStart).ToArray(),
                        table));
                }
            }

            if (entries.Count == 0)
                throw new InvalidDataException("No editable text entries were found in the BINL file.");

            return new Kh1Binl(source, contentLength, entries);
        }

        public static Kh1Binl Read(Stream stream) => Read(stream, Kh1TextTable.Default);

        public static Kh1Binl Read(string fileName, Kh1TextTable table)
        {
            using var stream = File.OpenRead(fileName);
            return Read(stream, table);
        }

        public static Kh1Binl Read(string fileName) => Read(fileName, Kh1TextTable.Default);

        public void Write(Stream stream)
        {
            ArgumentNullException.ThrowIfNull(stream);

            var replacements = Entries
                .Select(x => new { Entry = x, Bytes = x.EncodeBody() })
                .ToList();

            var cursor = 0;
            foreach (var replacement in replacements)
            {
                stream.Write(_source, cursor, replacement.Entry.Offset - cursor);
                stream.Write(replacement.Bytes);
                cursor = replacement.Entry.Offset + replacement.Entry.OriginalLength;
            }

            stream.Write(_source, cursor, _contentLength - cursor);
            while ((stream.Position & 0x0F) != 0)
                stream.WriteByte(0xCD);
        }

        private static List<int> FindRecordOffsets(byte[] source, int contentLength)
        {
            var result = new List<int>();
            for (var offset = 11; offset < contentLength;)
            {
                var opcode = source[offset];
                var length = opcode <= 0x0E ? GetInstructionLength(opcode) : 1;
                if (offset + length > contentLength)
                    throw new InvalidDataException($"Truncated BINL instruction at 0x{offset:X}.");
                if (opcode == 0x0A)
                    result.Add(offset);
                offset += length;
            }

            return result;
        }

        private static bool TryFindBody(
            byte[] source,
            int start,
            int end,
            out int bodyStart,
            out int bodyEnd)
        {
            bodyStart = -1;
            bodyEnd = -1;
            var pendingWhitespace = -1;
            for (var offset = start; offset < end;)
            {
                var opcode = source[offset];
                var length = opcode <= 0x0E ? GetInstructionLength(opcode) : 1;
                if (offset + length > end)
                    return false;

                if (bodyStart < 0)
                {
                    if ((opcode == 0x01 || opcode == 0x02) && pendingWhitespace < 0)
                        pendingWhitespace = offset;
                    else if (opcode > 0x0E)
                        bodyStart = pendingWhitespace >= 0 ? pendingWhitespace : offset;
                }
                else if (opcode == 0x05 || opcode == 0x06)
                {
                    bodyEnd = offset;
                    return bodyEnd > bodyStart;
                }

                offset += length;
            }

            if (bodyStart >= 0)
            {
                bodyEnd = end;
                return true;
            }

            return false;
        }

        private static int GetInstructionLength(byte opcode) => opcode switch
        {
            0x05 or 0x06 or 0x07 => 3,
            0x0A or 0x0B or 0x0D => 4,
            0x0C or 0x0E => 2,
            _ => 1,
        };
    }
}
