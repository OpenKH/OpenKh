using System.Collections.Generic;

namespace kh.kh2
{
    public static class MsgParser
    {
        public enum Command
        {
            End,
            PrintText,
            NewLine,
            Reset,
            Unknown04,
            Unknown05,
            Unknown06,
            Color,
            Unknown08,
            PrintIcon,
            TextScale, // Default: 0x10
            TextWidth, // Default: 0x48
            Unknown0c,
            Unknown0d,
            Unknown0e,
            Unknown0f,
            Unknown10,
            Unknown11,
            Unknown12,
            Unknown13,
            Unknown14,
            Unknown15,
            Unknown16,
            Unknown17,
            Unknown18,
            Unknown19,
            Unknown1a,
            Unknown1b,
            Unknown1c,
            Unknown1d,
            Unknown1e,
            Unknown1f,
            Number,
        }

        public class Entry
        {
            public Command Command { get; set; }
            public byte[] Data { get; set; }
            public string Text { get; set; }
        }

        public static List<Entry> Map(this Msg.Entry entry) =>
            new MsgParserInternal(entry.Data).Parse();
    }
}
