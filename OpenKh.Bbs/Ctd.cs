using OpenKh.Bbs.Messages;
using OpenKh.Common;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xe.BinaryMapper;

namespace OpenKh.Bbs
{
    public class Ctd
    {
        private const int MagicCode = 0x44544340;
        private const int Version = 1;
        private const int HeaderLength = 0x20;
        private const int Entry1Length = 0xC;
        private const int Entry2Length = 0x20;

        private class Header
        {
            [Data] public int MagicCode { get; set; }
            [Data] public int Version { get; set; }
            [Data] public short Unknown08 { get; set; }
            [Data] public short Unknown0a { get; set; }
            [Data] public short LayoutCount { get; set; }
            [Data] public short MessageCount { get; set; }
            [Data] public int MessageOffset { get; set; }
            [Data] public int LayoutOffset { get; set; }
            [Data] public int TextOffset { get; set; }
            [Data] public int Unknown1c { get; set; }
        }

        private class _Message
        {
            [Data] public short Id { get; set; }
            [Data] public short Unknown02 { get; set; }
            [Data] public int Offset { get; set; }
            [Data] public int Entry2Index { get; set; }
        }

        public class Message
        {
            public short Id { get; set; }
            public short Unknown02 { get; set; }
            public int LayoutIndex { get; set; }

            public byte[] Data { get; set; }

            public string Text
            {
                get => CtdEncoders.International.ToText(Data);
                set => Data = CtdEncoders.International.FromText(value);
            }

            public override string ToString() =>
                $"{Id:X04} {Unknown02:X04} {LayoutIndex:X08}: {Text}";
        }

        public class Layout
        {
            [Data] public ushort DialogX { get; set; }
            [Data] public ushort DialogY { get; set; }
            [Data] public ushort DialogWidth { get; set; }
            [Data] public ushort DialogHeight { get; set; }
            [Data] public byte DialogAlignment { get; set; }
            [Data] public byte DialogBorders { get; set; }
            [Data] public byte TextAlignment { get; set; }
            [Data] public byte Unknown0b { get; set; }
            [Data] public ushort FontSize { get; set; }
            [Data] public ushort HorizontalSpace { get; set; }
            [Data] public ushort VerticalSpace { get; set; }
            [Data] public ushort TextX { get; set; }
            [Data] public ushort TextY { get; set; }
            [Data] public ushort DialogHook { get; set; }
            [Data] public ushort DialogHookX { get; set; }
            [Data] public ushort Unknown1a { get; set; }
            [Data] public ushort Unknown1c { get; set; }
            [Data] public ushort Unknown1e { get; set; }
        }

        public short Unknown { get; set; }
        public List<Message> Messages { get; set; }
        public List<Layout> Layouts { get; set; }

        public string GetString(int id)
        {
            var entry = Messages.FirstOrDefault(x => x.Id == id);
            if (entry == null)
                return null;

            return entry.Text;
        }

        public void Write(Stream stream)
        {
            var messageOffset = HeaderLength;
            var layoutOffset = Helpers.Align(messageOffset + Messages.Count * Entry1Length, 16);
            var textOffset = layoutOffset + Layouts.Count * Entry2Length;

            BinaryMapping.WriteObject(stream, new Header
            {
                MagicCode = MagicCode,
                Version = Version,
                Unknown08 = 0,
                Unknown0a = Unknown,
                LayoutCount = (short)Layouts.Count,
                MessageCount = (short)Messages.Count,
                MessageOffset = messageOffset,
                LayoutOffset = layoutOffset,
                TextOffset = textOffset,
                Unknown1c = 0,
            });

            stream.Position = messageOffset;
            var textStream = new MemoryStream(4096);
            var nextTextOffset = textOffset;
            foreach (var item in Messages)
            {
                textStream.Write(item.Data, 0, item.Data.Length);
                textStream.WriteByte(0);

                BinaryMapping.WriteObject(stream, new _Message
                {
                    Id = item.Id,
                    Unknown02 = item.Unknown02,
                    Offset = nextTextOffset,
                    Entry2Index = item.LayoutIndex
                });

                nextTextOffset += item.Data.Length + 1;
            }

            stream.Position = layoutOffset;
            foreach (var item in Layouts)
                BinaryMapping.WriteObject(stream, item);

            stream.Position = textOffset;
            foreach (var entry in Messages)
            {
                stream.Write(entry.Data, 0, entry.Data.Length);
                stream.WriteByte(0);
            }
        }

        public Ctd()
        {
            Unknown = 0;
            Messages = new List<Message>();
            Layouts = new List<Layout>();
        }

        private Ctd(Stream stream)
        {
            var header = BinaryMapping.ReadObject<Header>(stream);
            Unknown = header.Unknown0a;

            stream.Position = header.MessageOffset;
            var textEntries = Enumerable.Range(0, header.MessageCount)
                .Select(x => BinaryMapping.ReadObject<_Message>(stream))
                .ToList();

            stream.Position = header.LayoutOffset;
            Layouts = Enumerable.Range(0, header.LayoutCount)
                .Select(x => BinaryMapping.ReadObject<Layout>(stream))
                .ToList();

            Messages = textEntries
                .Select(x =>
                {
                    stream.SetPosition(x.Offset);
                    return new Message
                    {
                        Id = x.Id,
                        Unknown02 = x.Unknown02,
                        LayoutIndex = x.Entry2Index,
                        Data = ReadUntilTerminator(stream)
                    };
                }).ToList();
        }

        private byte[] ReadUntilTerminator(Stream stream)
        {
            var byteList = new List<byte>(100);

            while (stream.Position < stream.Length)
            {
                var ch = stream.ReadByte();
                if (ch <= 0)
                    break;

                byteList.Add((byte)ch);
            }

            return byteList.ToArray();
        }

        public static Ctd Read(Stream stream) => new Ctd(stream.SetPosition(0));

        public static bool IsValid(Stream stream) =>
            new BinaryReader(stream.SetPosition(0)).ReadInt32() == MagicCode;
    }
}
