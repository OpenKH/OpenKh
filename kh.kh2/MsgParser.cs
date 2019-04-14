using System.Collections.Generic;
using static kh.kh2.MsgParser;

namespace kh.kh2
{
    public static class MsgParser
    {
        public enum Command
        {
            End,
            PrintText,

            Unknown = -1
        }

        public class Entry
        {
            public Command Command { get; set; }
            public byte[] Data { get; set; }
            public string Text { get; set; }
        }

        public static List<Entry> Map(this Msg.Entry entry) =>
            new MsgParserInternal().Parse(entry);
    }

    internal class MsgParserInternal
    {
        private Entry _textEntry;
        private List<Entry> _entries;

        internal MsgParserInternal()
        {
            _entries = new List<Entry>();
        }

        internal List<Entry> Parse(Msg.Entry entry)
        {
            foreach (byte c in entry.Data)
            {
                if (c >= 0x2e && c <= 0x47)
                {
                    AppendChar((c - 0x2e) + 'A');
                }
                else if (c >= 0x90 && c <= 0x99)
                {
                    AppendChar((c - 0x90) + '0');
                }
                else if (c >= 0x9a && c <= 0xb3)
                {
                    AppendChar((c - 0x9a) + 'a');
                }
                else
                {
                    if (c == 0x00)
                    {
                        FlushTextEntry();

                        _entries.Add(new Entry
                        {
                            Command = Command.End
                        });

                        break;
                    }
                    else if (c == 0x01)
                    {
                        AppendChar(' ');
                    }
                    else if (c == 0x48)
                    {
                        AppendChar('!');
                    }
                    else if (c == 0x57)
                    {
                        AppendChar(''');
                    }
                    else
                    {
                        AppendDebug(c);
                    }
                }
            }

            FlushTextEntry();
            return _entries;
        }

        private Entry RequestTextEntry()
        {
            if (_textEntry == null)
            {
                _textEntry = new Entry();
                _textEntry.Command = Command.PrintText;
                _textEntry.Text = "";
            }

            return _textEntry;
        }

        private void FlushTextEntry()
        {
            if (_textEntry != null)
            {
                _entries.Add(_textEntry);
                _textEntry = null;
            }
        }

        private void AppendDebug(byte ch)
        {
            RequestTextEntry();
            _textEntry.Text += $"{{0x{ch:X02}}}";
        }

        private void AppendChar(char ch)
        {
            RequestTextEntry();
            _textEntry.Text += ch;
        }

        private void AppendChar(int ch)
        {
            AppendChar((char)ch);
        }
    }
}
