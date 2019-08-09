using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OpenKh.Kh2.Messages.Internals
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

                switch (cmdModel.Command)
                {
                    case MessageCommand.PrintText:
                        Append(cmdModel.Text[0]);
                        break;
                    case MessageCommand.PrintComplex:
                        AppendComplex(cmdModel.Text);
                        break;
                    case MessageCommand.Table2:
                    case MessageCommand.Table3:
                    case MessageCommand.Table4:
                    case MessageCommand.Table5:
                    case MessageCommand.Table6:
                    case MessageCommand.Table7:
                    case MessageCommand.Table8:
                        Append((cmdModel as TableCmdModel).GetText(Next()));
                        break;
                    case MessageCommand.Unsupported:
                        AppendEntry(cmdModel.Command, new byte[] { cmdModel.RawData });
                        break;
                    default:
                        AppendEntry(cmdModel);
                        break;
                }
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

        private void Append(char ch) => RequestTextBuilder().Append(ch);
        private void Append(string str) => RequestTextBuilder().Append(str);
        private void AppendComplex(string str)
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
