using OpenKh.Common.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OpenKh.Kh2.Messages.Internals
{
    internal class TurkishSystemEncode : IMessageEncode
    {
        private static readonly Dictionary<MessageCommand, KeyValuePair<byte, BaseCmdModel>> _tableCommands =
            TurkishSystemDecode._table
            .Where(x => x.Value != null && x.Value.Command != MessageCommand.PrintText)
            .GroupBy(x => x.Value.Command)
            .ToDictionary(x => x.Key, x => x.First());

        private static readonly Dictionary<char, byte> _tableCharacters =
            TurkishSystemDecode._table
            .Where(x => x.Value?.Command == MessageCommand.PrintText)
            .ToDictionary(x => x.Value.Text[0], x => x.Key);

        private static readonly Dictionary<string, byte> _tableComplex =
            TurkishSystemDecode._table
            .Where(x => x.Value?.Command == MessageCommand.PrintComplex)
            .ToDictionary(x => x.Value.Text, x => x.Key);

        private void AppendEncodedMessageCommand(List<byte> list, MessageCommandModel messageCommand)
        {
            if (messageCommand.Command == MessageCommand.PrintText)
                AppendEncodedText(list, messageCommand.Text);
            else if (messageCommand.Command == MessageCommand.PrintComplex)
                AppendEncodedComplex(list, messageCommand.Text);
            else if (messageCommand.Command == MessageCommand.Unsupported)
                list.AddRange(messageCommand.Data);
            else
                AppendEncodedCommand(list, messageCommand.Command, messageCommand.Data);
        }

        private void AppendEncodedCommand(List<byte> list, MessageCommand command, byte[] data)
        {
            if (!_tableCommands.TryGetValue(command, out var pair))
                throw new ArgumentException($"The command {command} it is not supported by the specified encoding.");

            list.Add(pair.Key);
            for (var i = 0; i < pair.Value.Length; i++)
                list.Add(data[i]);
        }

        private void AppendEncodedText(List<byte> list, string text)
        {
            foreach (var ch in text)
                AppendEncodedChar(list, ch);
        }

        private void AppendEncodedComplex(List<byte> list, string text)
        {
            if (!_tableComplex.TryGetValue(text, out var data))
                throw new ParseException(text, 0, "Complex text does not exists");

            list.Add(data);
        }

        private void AppendEncodedChar(List<byte> list, char ch)
        {
            if (!_tableCharacters.TryGetValue(ch, out var data))
                throw new ArgumentException($"The character {ch} it is not supported by the specified encoding.");

            list.Add(data);
        }

        public byte[] Encode(List<MessageCommandModel> messageCommands)
        {
            var list = new List<byte>(100);
            foreach (var model in messageCommands)
                AppendEncodedMessageCommand(list, model);

            return list.ToArray();
        }
    }
}
