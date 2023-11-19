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
        public enum Arrange : byte
        {
            Position = 0,
            Left = 1,
            Center = 2,
            Right = 3,
        }

        public enum FontArrange : ushort
        { 
            WD_FNT_LEFT = 0,
            WD_FNT_RIGHT = 1,
            WD_FNT_CENTER = 2,
            WD_FNT_BOTTOM = 3,
            WD_FNT_RIGHT_L = 5,
            WD_FNT_CENTER_L = 6,
            WD_FNT_BOTTOM_L = 7,
            WD_FNT_LEFT_C = 8,
            WD_FNT_RIGHT_C = 9,
            WD_FNT_CENTER_C = 10,
            WD_FNT_BOTTOM_C = 11,
            WD_FNT_RIGHT_LC = 13,
            WD_FNT_CENTER_LC = 14,
            WD_FNT_BOTTOM_LC = 15,
        }
        
        public enum Style : byte
        {
            Normal = 0,
            Square = 1,
            Angry = 2,
            System = 3,
            Square2 = 4,
            Subtitle = 5,
            Dice = 6,
            Dice2 = 7,
            Dice3 = 8,
        }

        public enum HookStyle : ushort
        {
            Hook_Bottom_Left,
            Hook_Bottom_Right,
            Hook_Top_Left,
            Hook_Top_Right,
            Bubble_Bottom_Left,
            Bubble_Bottom_Right,
            Bubble_Top_Left,
            Bubble_Top_Right,
            Spike_Bottom_Left,
            Spike_Bottom_Right,
            Spike_Top_Left,
            Spike_Top_Right,
        }

        private const int MagicCode = 0x44544340;
        private const int Version = 1;
        private const uint HeaderLength = 0x20;
        private const uint Entry1Length = 0xC;
        private const uint Entry2Length = 0x20;

        private class Header
        {
            [Data] public int MagicCode { get; set; }
            [Data] public uint Version { get; set; }
            [Data] public uint FileID { get; set; }
            [Data] public ushort LayoutCount { get; set; }
            [Data] public ushort MessageCount { get; set; }
            [Data] public uint MessageOffset { get; set; }
            [Data] public uint LayoutOffset { get; set; }
            [Data] public uint TextOffset { get; set; }
            [Data] public int Unknown1c { get; set; }
        }

        private class _Message
        {
            [Data] public uint Id { get; set; }
            [Data] public uint Offset { get; set; }
            [Data] public ushort LayoutIndex { get; set; }
            [Data] public ushort WaitFrames { get; set; }
        }

        public class Message
        {
            public uint Id { get; set; }
            public ushort LayoutIndex { get; set; }
            public ushort WaitFrames { get; set; }

            public byte[] Data { get; set; }

            public string Text
            {
                get => CtdEncoders.International.ToText(Data);
                set => Data = CtdEncoders.International.FromText(value);
            }

            public override string ToString() =>
                $"{Id:X08} {LayoutIndex:X08}: {Text}";
        }

        public class Layout
        {
            [Data] public ushort DialogX { get; set; }
            [Data] public ushort DialogY { get; set; }
            [Data] public ushort DialogWidth { get; set; }
            [Data] public ushort DialogHeight { get; set; }
            [Data] public Arrange DialogAlignment { get; set; }
            [Data] public Style DialogBorders { get; set; }
            [Data] public FontArrange TextAlignment { get; set; }
            [Data] public ushort FontSize { get; set; }
            [Data] public ushort HorizontalSpace { get; set; }
            [Data] public ushort VerticalSpace { get; set; }
            [Data] public ushort TextX { get; set; }
            [Data] public ushort TextY { get; set; }
            [Data] public HookStyle DialogHook { get; set; }
            [Data] public ushort DialogHookX { get; set; }
            [Data] public ushort TextColorIdx { get; set; }
            [Data] public ushort Unknown1c { get; set; }
            [Data] public ushort Unknown1e { get; set; }
        }

        public uint FileID { get; set; }
        public List<Message> Messages { get; set; }
        public List<Layout> Layouts { get; set; }

        public string GetString(uint id)
        {
            var entry = Messages.FirstOrDefault(x => x.Id == id);
            if (entry == null)
                return null;

            return entry.Text;
        }

        public void Write(Stream stream)
        {
            uint messageOffset = HeaderLength;
            uint layoutOffset = (uint)Helpers.Align(messageOffset + Messages.Count * Entry1Length, 16);
            uint textOffset = (uint)(layoutOffset + Layouts.Count * Entry2Length);

            BinaryMapping.WriteObject(stream, new Header
            {
                MagicCode = MagicCode,
                Version = Version,
                FileID = FileID,
                LayoutCount = (ushort)Layouts.Count,
                MessageCount = (ushort)Messages.Count,
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
                    Offset = nextTextOffset,
                    LayoutIndex = item.LayoutIndex,
                    WaitFrames = item.WaitFrames
                });

                nextTextOffset += (uint)(item.Data.Length + 1);
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
            FileID = 0;
            Messages = new List<Message>();
            Layouts = new List<Layout>();
        }

        private Ctd(Stream stream)
        {
            var header = BinaryMapping.ReadObject<Header>(stream);
            FileID = header.FileID;

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
                        LayoutIndex = x.LayoutIndex,
                        WaitFrames = x.WaitFrames,
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
