using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace kh.kh2.Messages.Internals
{
    internal partial class BaseMessageDecoder
    {
        private readonly Dictionary<byte, BaseCmdModel> _table;
        private readonly List<MessageCommandModel> _entries;
        private StringBuilder _stringBuilder;
        private byte[] _data;
        private int _index;

        internal BaseMessageDecoder(
            Dictionary<byte, BaseCmdModel> table,
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
                if (!_table.TryGetValue(ch, out var cmdModel) || cmdModel == null)
                    throw new NotImplementedException($"Command {ch:X02} not implemented yet");

                if (cmdModel.Command == MessageCommand.PrintText)
                    AppendChar(cmdModel.Text[0]);
                else if (cmdModel.Command == MessageCommand.PrintComplex)
                    AppendText(cmdModel.Text);
                else if (cmdModel.Command == MessageCommand.Unsupported)
                    AppendEntry(cmdModel.Command, new byte[] { cmdModel.RawData });
                else
                    AppendEntry(cmdModel);
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

        private void FlushTextBuilder(MessageCommand command = MessageCommand.PrintText)
        {
            if (_stringBuilder != null)
            {
                _entries.Add(new MessageCommandModel
                {
                    Command = command,
                    Text = _stringBuilder.ToString()
                });
                _stringBuilder = null;
            }
        }

        private void AppendEntry(BaseCmdModel cmdModel) => AppendEntry(cmdModel.Command, ReadBytes(cmdModel.Length));

        private void AppendEntry(MessageCommand command, byte[] data)
        {
            FlushTextBuilder();
            _entries.Add(new MessageCommandModel
            {
                Command = command,
                Data = data
            });
        }

        private void AppendChar(char ch) => RequestTextBuilder().Append(ch);
        private void AppendText(string str)
        {
            FlushTextBuilder();
            RequestTextBuilder().Append(str);
            FlushTextBuilder(MessageCommand.PrintComplex);
        }

        private byte[] ReadBytes(int length) =>
            Enumerable.Range(0, length)
            .Select(x => Next())
            .ToArray();
    }
}
