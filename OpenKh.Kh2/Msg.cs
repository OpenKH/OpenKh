using OpenKh.Common;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace OpenKh.Kh2
{
    public class Msg
    {
        private static uint MagicCode = 1;
        private static byte Terminator = 0;
        public static ushort FallbackMessage = 2780;

        public class Entry
        {
            public int Id { get; set; }
            public byte[] Data { get; set; }
        }

        internal class OptimizedEntry
        {
            public int Id { get; }
            public byte[] Data { get; }
            public int Offset { get; set; }
            public int LinkId { get; private set; }
            public int LinkOffset { get; private set; }

            public bool HasbeenLinked { get; private set; }
            public bool IsLinked => LinkId >= 0;

            public OptimizedEntry(Entry entry)
            {
                Id = entry.Id;
                Data = entry.Data;
                Offset = -1;
                LinkId = -1;
                LinkOffset = -1;
                HasbeenLinked = false;
            }

            public void LinkTo(OptimizedEntry entry, int offset)
            {
                LinkId = entry.Id;
                LinkOffset = offset;
                entry.HasbeenLinked = true;
            }
        }

        public static List<Entry> Read(Stream stream)
        {
            if (!stream.CanRead || !stream.CanSeek)
                throw new InvalidDataException($"Read or seek must be supported.");

            var reader = new BinaryReader(stream);
            if (stream.Length < 8L || reader.ReadUInt32() != MagicCode)
                throw new InvalidDataException("Invalid header");

            int entriesCount = reader.ReadInt32();
            return Enumerable.Range(0, entriesCount)
                .Select(x => new
                {
                    Id = reader.ReadInt32(),
                    Offset = reader.ReadInt32(),
                })
                .ToList()
                .Select(x =>
                {
                    reader.BaseStream.Position = x.Offset;

                    return new Entry
                    {
                        Id = x.Id,
                        Data = GetMsgData(reader)
                    };
                })
                .ToList();
        }

        public static void Write(Stream stream, List<Entry> entries)
        {
            if (!stream.CanWrite || !stream.CanSeek)
                throw new InvalidDataException($"Write or seek must be supported.");

            var writer = new BinaryWriter(stream);
            writer.Write(MagicCode);
            writer.Write(entries.Count);

            var offset = 8 + entries.Count * 8;
            var orderedEntries = entries.OrderBy(x => x.Id).ToList();
            foreach (var entry in orderedEntries)
            {
                writer.Write(entry.Id);
                writer.Write(offset);
                offset += entry.Data.Length;
            }
            foreach (var entry in orderedEntries)
            {
                writer.Write(entry.Data);
            }
        }

        public static void WriteOptimized(Stream stream, List<Entry> entries)
        {
            if (!stream.CanWrite || !stream.CanSeek)
                throw new InvalidDataException($"Write or seek must be supported.");

            var writer = new BinaryWriter(stream);
            writer.Write(MagicCode);
            writer.Write(entries.Count);

            var optimizedEntries = entries.OrderBy(x => x.Id).Select(x => new OptimizedEntry(x)).ToList();
            foreach (var entry in optimizedEntries)
            {
                if (entry.HasbeenLinked)
                    continue;

                foreach (var x in optimizedEntries)
                {
                    if (entry == x || x.IsLinked)
                        continue;

                    var indexFound = IndexOf(x.Data, entry.Data);
                    if (indexFound >= 0)
                    {
                        entry.LinkTo(x, indexFound);
                        break;
                    }
                }
            }

            var offset = 8 + entries.Count * 8;
            foreach (var entry in optimizedEntries)
            {
                if (entry.LinkId < 0)
                {
                    entry.Offset = offset;
                    offset += entry.Data.Length;
                }
            }

            foreach (var entry in optimizedEntries)
            {
                writer.Write(entry.Id);
                if (entry.IsLinked)
                {
                    var msgLink = optimizedEntries.Find(x => x.Id == entry.LinkId);
                    writer.Write(msgLink.Offset + entry.LinkOffset);
                }
                else
                    writer.Write(entry.Offset);
            }

            foreach (var entry in optimizedEntries)
            {
                if (!entry.IsLinked)
                    writer.Write(entry.Data);
            }
        }

        private static int IndexOf(byte[] data, byte[] pattern)
        {
            var c = data.Length - pattern.Length + 1;
            for (var i = 0; i < c; i++)
            {
                if (data[i] != pattern[0])
                    continue;

                int j;
                for (j = pattern.Length - 1; j >= 1 && data[i + j] == pattern[j]; j--)
                    ;
                if (j == 0)
                    return i;
            }

            return -1;
        }

        private static byte[] GetMsgData(BinaryReader stream)
        {
            byte r;
            var data = new List<byte>();
            do
            {
                r = stream.ReadByte();
                data.Add(r);

                switch (r)
                {
                    case 0x04:
                    case 0x06:
                    case 0x09:
                    case 0x0a:
                    case 0x0b:
                    case 0x0c:
                    case 0x0e:
                    case 0x16:
                    case 0x19:
                    case 0x1a:
                    case 0x1b:
                    case 0x1c:
                    case 0x1d:
                    case 0x1e:
                    case 0x1f:
                        data.Add(stream.ReadByte());
                        break;
                    case 0x12:
                    case 0x14:
                    case 0x15:
                    case 0x18:
                        data.Add(stream.ReadByte());
                        data.Add(stream.ReadByte());
                        break;
                    case 0x08:
                        data.Add(stream.ReadByte());
                        data.Add(stream.ReadByte());
                        data.Add(stream.ReadByte());
                        break;
                    case 0x07:
                    case 0x11:
                    case 0x13:
                        data.Add(stream.ReadByte());
                        data.Add(stream.ReadByte());
                        data.Add(stream.ReadByte());
                        data.Add(stream.ReadByte());
                        break;
                    case 0x0f:
                        data.Add(stream.ReadByte());
                        data.Add(stream.ReadByte());
                        data.Add(stream.ReadByte());
                        data.Add(stream.ReadByte());
                        data.Add(stream.ReadByte());
                        break;
                    case 0x05:
                        data.Add(stream.ReadByte());
                        data.Add(stream.ReadByte());
                        data.Add(stream.ReadByte());
                        data.Add(stream.ReadByte());
                        data.Add(stream.ReadByte());
                        data.Add(stream.ReadByte());
                        break;
                }
            } while (r != Terminator);

            return data.ToArray();
        }

        public static bool IsValid(Stream stream) =>
            stream.Length >= 4 && stream.PeekUInt32() == MagicCode;

    }
}
