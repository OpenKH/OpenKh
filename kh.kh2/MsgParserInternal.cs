using System;
using System.Collections.Generic;
using System.Text;
using static kh.kh2.MsgParser;

namespace kh.kh2
{
    internal class MsgParserInternal
    {
        public class Text : Entry
        {
            public Text(byte chData) :
                this((char)chData)
            { }

            public Text(char ch) :
                this($"{ch}")
            { }

            public Text(string str)
            {
                Command = Command.PrintText;
                Text = $"{str}";
            }
        }

        public class GenericCommand : Entry
        {
            public GenericCommand(Command command)
            {
                Command = command;
            }
        }

        public class SingleData : Entry
        {
            public SingleData(Command command, MsgParserInternal msgParser) :
                this(command, msgParser.Next())
            { }

            public SingleData(Command command, byte data)
            {
                Command = command;
                Data = new byte[] { data };
            }
        }

        private readonly Dictionary<byte, Func<MsgParserInternal, Entry>> _table
            = new Dictionary<byte, Func<MsgParserInternal, Entry>>
            {
                [0x00] = x => new GenericCommand(Command.End),
                [0x01] = x => new Text(' '),
                [0x02] = x => new GenericCommand(Command.NewLine),
                [0x03] = x => new GenericCommand(Command.Reset),
                [0x04] = x => new SingleData(Command.Unknown04, x),
                [0x05] = x => new Entry { Command = Command.Unknown05, Data = new byte[] { x.Next(), x.Next(), x.Next(), x.Next(), x.Next(), x.Next() } },
                [0x06] = x => new SingleData(Command.Unknown06, x),
                [0x07] = x => new Entry { Command = Command.Color, Data = new byte[] { x.Next(), x.Next(), x.Next(), x.Next() } },
                [0x08] = x => new Entry { Command = Command.Unknown08, Data = new byte[] { x.Next(), x.Next(), x.Next() } },
                [0x09] = x => new SingleData(Command.PrintIcon, x),
                [0x0a] = x => new SingleData(Command.TextScale, x),
                [0x0b] = x => new SingleData(Command.TextWidth, x),
                [0x0c] = x => new SingleData(Command.Unknown0c, x),
                [0x0d] = x => new GenericCommand(Command.Unknown0d),
                [0x0e] = x => new SingleData(Command.Unknown0e, x),
                [0x0f] = x => new Entry { Command = Command.Unknown0f, Data = new byte[] { x.Next(), x.Next(), x.Next(), x.Next(), x.Next() } },
                [0x10] = null,
                [0x11] = null,
                [0x12] = x => new Entry { Command = Command.Unknown12, Data = new byte[] { x.Next(), x.Next() } },
                [0x13] = x => new Entry { Command = Command.Unknown13, Data = new byte[] { x.Next(), x.Next(), x.Next(), x.Next() } },
                [0x14] = x => new Entry { Command = Command.Unknown14, Data = new byte[] { x.Next(), x.Next() } },
                [0x15] = x => new Entry { Command = Command.Unknown15, Data = new byte[] { x.Next(), x.Next() } },
                [0x16] = x => new SingleData(Command.Unknown16, x),
                [0x17] = null,
                [0x18] = x => new Entry { Command = Command.Unknown18, Data = new byte[] { x.Next(), x.Next() } },
                [0x19] = x => new SingleData(Command.Unknown19, x),
                [0x1a] = x => new SingleData(Command.Unknown1a, x),
                [0x1b] = x => new SingleData(Command.Unknown1b, x),
                [0x1c] = x => new SingleData(Command.Unknown1c, x),
                [0x1d] = x => new SingleData(Command.Unknown1d, x),
                [0x1e] = x => new SingleData(Command.Unknown1e, x),
                [0x1f] = x => new SingleData(Command.Unknown1f, x),
                [0x20] = null,
                [0x21] = x => new SingleData(Command.Number, 0),
                [0x22] = x => new SingleData(Command.Number, 1),
                [0x23] = x => new SingleData(Command.Number, 2),
                [0x24] = x => new SingleData(Command.Number, 3),
                [0x25] = x => new SingleData(Command.Number, 4),
                [0x26] = x => new SingleData(Command.Number, 5),
                [0x27] = x => new SingleData(Command.Number, 6),
                [0x28] = x => new SingleData(Command.Number, 7),
                [0x29] = x => new SingleData(Command.Number, 8),
                [0x2a] = x => new SingleData(Command.Number, 9),
                [0x2b] = x => new Text('+'),
                [0x2c] = x => new Text('−'),
                [0x2d] = x => new Text('ₓ'),
                [0x2e] = x => new Text('A'),
                [0x2f] = x => new Text('B'),
                [0x30] = x => new Text('C'),
                [0x31] = x => new Text('D'),
                [0x32] = x => new Text('E'),
                [0x33] = x => new Text('F'),
                [0x34] = x => new Text('G'),
                [0x35] = x => new Text('H'),
                [0x36] = x => new Text('I'),
                [0x37] = x => new Text('J'),
                [0x38] = x => new Text('K'),
                [0x39] = x => new Text('L'),
                [0x3a] = x => new Text('M'),
                [0x3b] = x => new Text('N'),
                [0x3c] = x => new Text('O'),
                [0x3d] = x => new Text('P'),
                [0x3e] = x => new Text('Q'),
                [0x3f] = x => new Text('R'),
                [0x40] = x => new Text('S'),
                [0x41] = x => new Text('T'),
                [0x42] = x => new Text('U'),
                [0x43] = x => new Text('V'),
                [0x44] = x => new Text('W'),
                [0x45] = x => new Text('X'),
                [0x46] = x => new Text('Y'),
                [0x47] = x => new Text('Z'),
                [0x48] = x => new Text('!'),
                [0x49] = x => new Text('?'),
                [0x4a] = x => new Text('%'),
                [0x4b] = x => new Text('/'),
                [0x4c] = x => new Text('※'),
                [0x4d] = x => new Text('、'),
                [0x4e] = x => new Text('。'),
                [0x4f] = x => new Text('.'),
                [0x50] = x => new Text(','),
                [0x51] = x => new Text(';'),
                [0x52] = x => new Text(':'),
                [0x53] = x => new Text('-'),
                [0x56] = x => new Text('〜'),
                [0x57] = x => new Text('\''),
                [0x5a] = x => new Text('('),
                [0x5b] = x => new Text(')'),
                [0x5c] = x => new Text('「'),
                [0x5d] = x => new Text('」'),
                [0x5e] = x => new Text('『'),
                [0x5f] = x => new Text('』'),
                [0x60] = x => new Text('“'),
                [0x61] = x => new Text('”'),
                [0x62] = x => new Text('['),
                [0x63] = x => new Text(']'),
                [0x64] = x => new Text('<'),
                [0x65] = x => new Text('>'),
                [0x66] = x => new Text('-'),
                [0x67] = x => new Text('–'),
                [0x69] = x => new Text('♪'),
                [0x6c] = x => new Text('◯'),
                [0x6d] = x => new Text('✕'),
                [0x73] = x => new Text('\t'),
                [0x74] = x => new Text('Ⅰ'),
                [0x75] = x => new Text('Ⅱ'),
                [0x76] = x => new Text('Ⅲ'),
                [0x77] = x => new Text('Ⅳ'),
                [0x78] = x => new Text('Ⅴ'),
                [0x79] = x => new Text('Ⅵ'),
                [0x7a] = x => new Text('Ⅶ'),
                [0x7b] = x => new Text('Ⅷ'),
                [0x7c] = x => new Text('Ⅸ'),
                [0x7d] = x => new Text('Ⅹ'),
                [0x7e] = x => new Text("ⅩⅢ"),
                [0x7f] = x => new Text('α'),
                [0x80] = x => new Text('β'),
                [0x81] = x => new Text('β'),
                [0x82] = x => new Text('★'),
                [0x83] = x => new Text('☆'),
                [0x84] = x => new Text('Ⅺ'),
                [0x85] = x => new Text('Ⅻ'),
                [0x86] = x => new Text('&'),
                [0x87] = x => new Text('#'),
                [0x88] = x => new Text('®'),
                [0x90] = x => new Text('0'),
                [0x91] = x => new Text('1'),
                [0x92] = x => new Text('2'),
                [0x93] = x => new Text('3'),
                [0x94] = x => new Text('4'),
                [0x95] = x => new Text('5'),
                [0x96] = x => new Text('6'),
                [0x97] = x => new Text('7'),
                [0x98] = x => new Text('8'),
                [0x99] = x => new Text('9'),
                [0x9a] = x => new Text('a'),
                [0x9b] = x => new Text('b'),
                [0x9c] = x => new Text('c'),
                [0x9d] = x => new Text('d'),
                [0x9e] = x => new Text('e'),
                [0x9f] = x => new Text('f'),
                [0xa0] = x => new Text('g'),
                [0xa1] = x => new Text('h'),
                [0xa2] = x => new Text('i'),
                [0xa3] = x => new Text('j'),
                [0xa4] = x => new Text('k'),
                [0xa5] = x => new Text('l'),
                [0xa6] = x => new Text('m'),
                [0xa7] = x => new Text('n'),
                [0xa8] = x => new Text('o'),
                [0xa9] = x => new Text('p'),
                [0xaa] = x => new Text('q'),
                [0xab] = x => new Text('r'),
                [0xac] = x => new Text('s'),
                [0xad] = x => new Text('t'),
                [0xae] = x => new Text('u'),
                [0xaf] = x => new Text('v'),
                [0xb0] = x => new Text('w'),
                [0xb1] = x => new Text('x'),
                [0xb2] = x => new Text('y'),
                [0xb3] = x => new Text('z'),
                [0xeb] = x => new Text('‘'),
                [0xec] = x => new Text('’'),
                [0xee] = x => new Text('\''),
                [0xf0] = x => new Text('☆'),
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
                if (!_table.TryGetValue(ch, out var getter) || getter == null)
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
