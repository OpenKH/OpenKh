using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace OpenKh.Kh1
{
    /// <summary>
    /// Preserving editor for the EvMsg bytecode section embedded in remastered
    /// KH1 *.ev and *.evdl files.
    /// </summary>
    public sealed class Kh1EventMessage
    {
        private readonly byte[] _source;
        private readonly int _sectionStart;
        private readonly int _sectionEnd;

        private Kh1EventMessage(
            byte[] source,
            int sectionStart,
            int sectionEnd,
            IReadOnlyList<Kh1Binl.TextEntry> entries)
        {
            _source = source;
            _sectionStart = sectionStart;
            _sectionEnd = sectionEnd;
            Entries = entries;
        }

        public IReadOnlyList<Kh1Binl.TextEntry> Entries { get; }

        public static bool IsValid(Stream stream)
        {
            ArgumentNullException.ThrowIfNull(stream);
            if (!stream.CanSeek || stream.Length < 0x20)
                return false;

            var oldPosition = stream.Position;
            try
            {
                Span<byte> firstHeader = stackalloc byte[0x20];
                if (stream.Read(firstHeader) != firstHeader.Length)
                    return false;

                var sectionStart = checked((int)BitConverter.ToUInt32(firstHeader.Slice(0x0C, 4)));
                var sectionEnd = checked((int)BitConverter.ToUInt32(firstHeader.Slice(0x10, 4)));
                if (sectionStart < 0x14 ||
                    sectionStart > 0x1000 ||
                    (sectionStart & 3) != 0 ||
                    sectionEnd <= sectionStart ||
                    sectionEnd > stream.Length ||
                    ((sectionEnd - sectionStart) & 0x0F) != 0)
                    return false;

                var header = new byte[sectionStart];
                stream.Position = 0;
                stream.ReadExactly(header);
                var previous = sectionStart;
                for (var offset = 0x10; offset < sectionStart; offset += sizeof(uint))
                {
                    var value = checked((int)BitConverter.ToUInt32(header, offset));
                    if (value <= previous || value > stream.Length)
                        return false;
                    previous = value;
                }
                return true;
            }
            catch (Exception ex) when (ex is IOException or OverflowException or ArgumentException)
            {
                return false;
            }
            finally
            {
                stream.Position = oldPosition;
            }
        }

        public static Kh1EventMessage Read(Stream stream, Kh1TextTable table)
        {
            ArgumentNullException.ThrowIfNull(stream);
            ArgumentNullException.ThrowIfNull(table);
            if (!IsValid(stream))
                throw new InvalidDataException("The file does not contain a valid KH1 event-message section.");

            stream.Position = 0;
            using var buffer = new MemoryStream();
            stream.CopyTo(buffer);
            var source = buffer.ToArray();
            var sectionStart = checked((int)BitConverter.ToUInt32(source, 0x0C));
            var sectionEnd = checked((int)BitConverter.ToUInt32(source, 0x10));
            ValidateOffsets(source, sectionStart, sectionEnd);

            var recordOffsets = FindRecordOffsets(source, sectionStart + sizeof(uint), sectionEnd);
            var entries = new List<Kh1Binl.TextEntry>();
            for (var recordIndex = 0; recordIndex < recordOffsets.Count; recordIndex++)
            {
                var recordStart = recordOffsets[recordIndex];
                var recordEnd = recordIndex + 1 < recordOffsets.Count
                    ? recordOffsets[recordIndex + 1]
                    : sectionEnd;
                var bodySearchStart = source[recordStart] == 0x0A
                    ? recordStart + GetInstructionLength(0x0A)
                    : recordStart;

                if (!TryFindBody(source, bodySearchStart, recordEnd, out var bodyStart, out var bodyEnd))
                    continue;

                var entry = new Kh1Binl.TextEntry(
                    entries.Count,
                    bodyStart,
                    bodyEnd - bodyStart,
                    source.AsSpan(bodyStart, bodyEnd - bodyStart).ToArray(),
                    table);
                if (!entry.ContainsStructuralCommands && IsReadableText(entry.Text))
                    entries.Add(entry);
            }

            return new Kh1EventMessage(source, sectionStart, sectionEnd, entries);
        }

        public static Kh1EventMessage Read(Stream stream) => Read(stream, Kh1TextTable.Default);

        public static Kh1EventMessage Read(string fileName, Kh1TextTable table)
        {
            using var stream = File.OpenRead(fileName);
            return Read(stream, table);
        }

        public static Kh1EventMessage Read(string fileName) => Read(fileName, Kh1TextTable.Default);

        public void Write(Stream stream)
        {
            ArgumentNullException.ThrowIfNull(stream);
            if (!Entries.Any(x => x.IsModified))
            {
                stream.Write(_source);
                return;
            }

            using var section = new MemoryStream();
            var cursor = _sectionStart;
            foreach (var entry in Entries)
            {
                section.Write(_source, cursor, entry.Offset - cursor);
                section.Write(entry.EncodeBody());
                cursor = entry.Offset + entry.OriginalLength;
            }
            section.Write(_source, cursor, _sectionEnd - cursor);
            while ((section.Length & 0x0F) != 0)
                section.WriteByte(0x00);

            var sectionBytes = section.ToArray();
            var oldSectionLength = _sectionEnd - _sectionStart;
            var delta = checked(sectionBytes.Length - oldSectionLength);
            var header = _source.AsSpan(0, _sectionStart).ToArray();
            if (delta != 0)
            {
                for (var offset = 0x10; offset < _sectionStart; offset += sizeof(uint))
                {
                    var oldValue = BitConverter.ToUInt32(header, offset);
                    BitConverter.GetBytes(checked((uint)(oldValue + delta))).CopyTo(header, offset);
                }
            }

            stream.Write(header);
            stream.Write(sectionBytes);
            stream.Write(_source, _sectionEnd, _source.Length - _sectionEnd);
        }

        private static void ValidateOffsets(byte[] source, int sectionStart, int sectionEnd)
        {
            var previous = sectionStart;
            for (var offset = 0x10; offset < sectionStart; offset += sizeof(uint))
            {
                var value = checked((int)BitConverter.ToUInt32(source, offset));
                if (value <= previous || value > source.Length)
                    throw new InvalidDataException("The KH1 event-message offset table is invalid.");
                previous = value;
            }
            if (BitConverter.ToUInt32(source, 0x10) != sectionEnd)
                throw new InvalidDataException("The KH1 event-message section boundary is invalid.");
        }

        private static List<int> FindRecordOffsets(byte[] source, int start, int end)
        {
            var result = new List<int> { start };
            for (var offset = start; offset < end;)
            {
                var opcode = source[offset];
                var length = opcode <= 0x0E ? GetInstructionLength(opcode) : 1;
                if (offset + length > end)
                    break;
                if (opcode == 0x0A && offset != start)
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
                else if (opcode == 0x04 || opcode == 0x05 || opcode == 0x06)
                {
                    bodyEnd = offset;
                    return bodyEnd > bodyStart;
                }

                offset += length;
            }
            return false;
        }

        private static bool IsReadableText(string text)
        {
            var letters = 0;
            var visible = 0;
            var squares = 0;
            var inToken = false;
            foreach (var character in text)
            {
                if (character == '{')
                {
                    inToken = true;
                    continue;
                }
                if (inToken)
                {
                    if (character == '}')
                        inToken = false;
                    continue;
                }
                if (char.IsWhiteSpace(character))
                    continue;
                visible++;
                if (char.IsLetterOrDigit(character))
                    letters++;
                if (character == '■')
                    squares++;
            }

            return letters >= 2 &&
                visible > 0 &&
                letters * 100 / visible >= 35 &&
                squares <= Math.Max(2, letters / 4);
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
