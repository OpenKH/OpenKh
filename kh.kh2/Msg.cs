using kh.common;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace kh.kh2
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
                    case 0x07:
                        data.Add(stream.ReadByte());
                        data.Add(stream.ReadByte());
                        data.Add(stream.ReadByte());
                        data.Add(stream.ReadByte());
                        break;
                    case 0x09:
                    case 0x0B:
                        data.Add(stream.ReadByte());
                        break;
                }
            } while (r != Terminator);

            data.Add(0);

            return data.ToArray();
        }

        public static bool IsValid(Stream stream) =>
            new BinaryReader(stream).PeekUInt32() == MagicCode;

    }
}
