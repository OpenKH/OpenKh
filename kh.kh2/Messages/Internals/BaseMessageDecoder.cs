using System;
using System.Collections.Generic;
using System.Text;

namespace kh.kh2.Messages.Internals
{
    internal partial class BaseMessageDecoder
    {
        private readonly Dictionary<byte, Func<BaseMessageDecoder, MessageCommandModel>> _table;
        private readonly List<MessageCommandModel> _entries;
        private StringBuilder _stringBuilder;
        private byte[] _data;
        private int _index;

        internal BaseMessageDecoder(
            Dictionary<byte, Func<BaseMessageDecoder, MessageCommandModel>> table,
            byte[] data)
        {
            _table = table;
            _entries = new List<MessageCommandModel>();
            _data = data;
        }

        internal List<MessageCommandModel> Decode()
        {
            while (!IsEof())
            {
                byte ch = Next();
                if (!_table.TryGetValue(ch, out var getter) || getter == null)
                    throw new NotImplementedException($"Command {ch:X02} not implemented yet");

                var entry = getter(this);
                if (entry.Command == MessageCommand.PrintText)
                    AppendChar(entry.Text[0]);
                else
                    AppendEntry(entry);
            }

            FlushTextBuilder();
            return _entries;
        }

        private bool IsEof() => _index >= _data.Length;

        public byte Next() => _data[_index++];

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
                _entries.Add(new MessageCommandModel
                {
                    Command = MessageCommand.PrintText,
                    Text = _stringBuilder.ToString()
                });
                _stringBuilder = null;
            }
        }

        private void AppendEntry(MessageCommandModel entry)
        {
            FlushTextBuilder();
            _entries.Add(entry);
        }

        private void AppendChar(char ch) => RequestTextBuilder().Append(ch);
    }
}
