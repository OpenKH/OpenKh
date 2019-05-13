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

        public class Entry
        {
            public int Id { get; set; }
            public byte[] Data { get; set; }
        }

        public static List<Entry> Open(Stream stream)
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
            foreach (var entry in entries)
            {
                writer.Write(entry.Id);
                writer.Write(offset);
                offset += entry.Data.Length;
            }
            foreach (var entry in entries)
            {
                writer.Write(entry.Data);
            }
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
            stream.Length >= 4 && new BinaryReader(stream).PeekUInt32() == MagicCode;

    }
}
