using System;
using System.Collections.Generic;
using System.Text;
using static kh.kh2.MsgParser;

namespace kh.kh2
{
    public static class MsgParser
    {
        public enum Command
        {
            End,
            PrintText,
            PrintIcon,

            Unknown = -1
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

    internal class MsgParserInternal
    {
        public class Character : Entry
        {
            public Character(byte chData) :
                this((char)chData)
            { }

            public Character(char ch)
            {
                Command = Command.PrintText;
                Text = $"{ch}";
            }
        }

        public class GenericCommand : Entry
        {
            public GenericCommand(Command command)
            {
                Command = Command;
            }
        }

        public class SingleData : Entry
        {
            public SingleData(Command command, MsgParserInternal msgParser)
            {
                Command = command;
                Data = new byte[] { msgParser.Next() };
            }
        }

        private readonly Dictionary<byte, Func<MsgParserInternal, Entry>> _table
            = new Dictionary<byte, Func<MsgParserInternal, Entry>>
            {
                [0x00] = x => new GenericCommand(Command.End),
                [0x01] = x => new Character(' '),
                [0x09] = x => new SingleData(Command.PrintIcon, x),
                [0x2e] = x => new Character('A'),
                [0x2f] = x => new Character('B'),
                [0x30] = x => new Character('C'),
                [0x31] = x => new Character('D'),
                [0x32] = x => new Character('E'),
                [0x33] = x => new Character('F'),
                [0x34] = x => new Character('G'),
                [0x35] = x => new Character('H'),
                [0x36] = x => new Character('I'),
                [0x37] = x => new Character('J'),
                [0x38] = x => new Character('K'),
                [0x39] = x => new Character('L'),
                [0x3a] = x => new Character('M'),
                [0x3b] = x => new Character('N'),
                [0x3c] = x => new Character('O'),
                [0x3d] = x => new Character('P'),
                [0x3e] = x => new Character('Q'),
                [0x3f] = x => new Character('R'),
                [0x40] = x => new Character('S'),
                [0x41] = x => new Character('T'),
                [0x42] = x => new Character('U'),
                [0x43] = x => new Character('V'),
                [0x44] = x => new Character('W'),
                [0x45] = x => new Character('X'),
                [0x46] = x => new Character('Y'),
                [0x47] = x => new Character('Z'),
                [0x48] = x => new Character('!'),
                [0x57] = x => new Character('\''),
                [0x90] = x => new Character('0'),
                [0x91] = x => new Character('1'),
                [0x92] = x => new Character('2'),
                [0x93] = x => new Character('3'),
                [0x94] = x => new Character('4'),
                [0x95] = x => new Character('5'),
                [0x96] = x => new Character('6'),
                [0x97] = x => new Character('7'),
                [0x98] = x => new Character('8'),
                [0x99] = x => new Character('9'),
                [0x9a] = x => new Character('a'),
                [0x9b] = x => new Character('b'),
                [0x9c] = x => new Character('c'),
                [0x9d] = x => new Character('d'),
                [0x9e] = x => new Character('e'),
                [0x9f] = x => new Character('f'),
                [0xa0] = x => new Character('g'),
                [0xa1] = x => new Character('h'),
                [0xa2] = x => new Character('i'),
                [0xa3] = x => new Character('j'),
                [0xa4] = x => new Character('k'),
                [0xa5] = x => new Character('l'),
                [0xa6] = x => new Character('m'),
                [0xa7] = x => new Character('n'),
                [0xa8] = x => new Character('o'),
                [0xa9] = x => new Character('p'),
                [0xaa] = x => new Character('q'),
                [0xab] = x => new Character('r'),
                [0xac] = x => new Character('s'),
                [0xad] = x => new Character('t'),
                [0xae] = x => new Character('u'),
                [0xaf] = x => new Character('v'),
                [0xb0] = x => new Character('w'),
                [0xb1] = x => new Character('x'),
                [0xb2] = x => new Character('y'),
                [0xb3] = x => new Character('z'),
            };

        private StringBuilder _stringBuilder;
        private List<Entry> _entries;
        private byte[] _data;
        private int _index;

        internal MsgParserInternal(byte[] data)
        {
            _entries = new List<Entry>();
            _data = data;
        }

        internal List<Entry> Parse()
        {
            while (!IsEof())
            {
                byte ch = Next();
                if (!_table.TryGetValue(ch, out var getter))
                    throw new NotImplementedException($"Command {ch:X02} not implemented yet");

                var entry = getter(this);
                if (entry.Command == Command.PrintText)
                    AppendChar(entry.Text[0]);
                else
                    AppendEntry(entry);
            }

            FlushTextBuilder();
            return _entries;
        }

        private bool IsEof() => _index >= _data.Length;

        private byte Next() => _data[_index++];

        private StringBuilder RequestTextBuilder()
        {
            if (_stringBuilder == null)
                _stringBuilder = new StringBuilder();

            return _stringBuilder;
        }

        private void FlushTextBuilder()
        {
            if (_stringBuilder != null)
            {
                _entries.Add(new Entry
                {
                    Command = Command.PrintText,
                    Text = _stringBuilder.ToString()
                });
                _stringBuilder = null;
            }
        }

        private void AppendEntry(Entry entry)
        {
            FlushTextBuilder();
            _entries.Add(entry);
        }

        private void AppendChar(char ch) => RequestTextBuilder().Append(ch);
    }
}
