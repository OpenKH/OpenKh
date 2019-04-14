using System.Collections.Generic;

namespace kh.kh2
{
    public static class MsgParser
    {
        public enum Command
        {
            End,
            PrintText,
            PrintIcon,
            Parameter,
            NewLine,
            TextSize,
            Color
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
