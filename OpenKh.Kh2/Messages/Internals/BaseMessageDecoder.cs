using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OpenKh.Kh2.Messages.Internals
{
    internal interface IDecoder
    {
        bool IsEof(int offset = 0);
        byte Peek(int offset);
        byte Next();
        bool WrapTable(ref byte ch, ref byte parameter);
        void AppendComplex(string str);
    }

    internal partial class BaseMessageDecoder : IDecoder
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

        internal List<MessageCommandModel> Decode(Func<IDecoder, bool> handler = null)
        {
            while (!IsEof())
            {
                if (handler?.Invoke(this) ?? false)
                    continue;

                byte ch = Next();
                var cmdModel = GetCommandModel(ch);


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
                        AppendFromTable(cmdModel, ch, Next());
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

        private void AppendFromTable(BaseCmdModel cmdModel, byte ch, byte parameter)
        {
            if (WrapTable(ref ch, ref parameter))
                AppendFromTable(GetCommandModel(ch), ch, parameter);
            else
                Append((cmdModel as TableCmdModel).GetText(parameter));
        }

        private BaseCmdModel GetCommandModel(byte ch)
        {
            if (!_table.TryGetValue(ch, out var commandModel) || commandModel == null)
                throw new NotImplementedException($"Command {ch:X02} not implemented yet");

            return commandModel;
        }

        public bool IsEof(int offset = 0) => _index + offset >= _data.Length;
        public byte Peek(int offset) => _data[_index + offset];
        public byte Next() => _data[_index++];
        public bool WrapTable(ref byte ch, ref byte parameter)
        {
            if (ch >= 0x20)
                return false;

            var data = (ushort)((ch << 8) | parameter);
            if (data >= 0x1e40)
            {
                data -= 0x310;

                ch = (byte)(data >> 8);
                parameter = (byte)(data & 0xff);
                return true;
            }

            return false;
        }

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
        public void AppendComplex(string str)
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
