using OpenKh.Common.Exceptions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace OpenKh.Kh2.Messages
{
    public partial class MsgSerializer
    {
        public static string SerializeText(IEnumerable<MessageCommandModel> entries)
        {
            var sb = new StringBuilder();
            foreach (var entry in entries)
                sb.Append(SerializeToText(entry));
            return sb.ToString();
        }

        public static string SerializeToText(MessageCommandModel entry)
        {
            if (entry.Command == MessageCommand.End)
                return string.Empty;
            if (entry.Command == MessageCommand.PrintText)
                return entry.Text;
            if (entry.Command == MessageCommand.PrintComplex)
                return $"{{{entry.Text}}}";
            if (entry.Command == MessageCommand.NewLine)
                return "\n";
            if (entry.Command == MessageCommand.Tabulation)
                return "\t";

            if (!_serializer.TryGetValue(entry.Command, out var serializeModel))
                throw new NotImplementedException($"The command {entry.Command} serialization is not implemented yet.");

            Debug.Assert(serializeModel != null, $"BUG: {nameof(serializeModel)} should never be null");

            if (serializeModel.ValueGetter != null)
                return $"{{:{serializeModel.Name} {serializeModel.ValueGetter(entry)}}}";
            return $"{{:{serializeModel.Name}}}";
        }


        public static IEnumerable<MessageCommandModel> DeserializeText(string value)
        {
            var entries = new List<MessageCommandModel>();
            var strBuilder = new StringBuilder();

            var i = 0;
            while (i < value.Length)
            {
                var ch = value[i++];
                if (ch == '{')
                {
                    if (strBuilder.Length > 0)
                    {
                        entries.Add(new MessageCommandModel
                        {
                            Command = MessageCommand.PrintText,
                            Text = strBuilder.ToString()
                        });
                        strBuilder.Clear();
                    }

                    var closeBracketIndex = value.Substring(i).IndexOf('}') + i;
                    if (closeBracketIndex < i)
                        throw new ParseException(value, i, "Expected '}'");

                    entries.Add(new MessageCommandModel
                    {
                        Command = MessageCommand.Unsupported,
                    });

                    i = closeBracketIndex + 1;
                }
                else
                {
                    strBuilder.Append(ch);
                }
            }

            if (strBuilder.Length > 0)
            {
                entries.Add(new MessageCommandModel
                {
                    Command = MessageCommand.PrintText,
                    Text = strBuilder.ToString()
                });
            }

            entries.Add(new MessageCommandModel
            {
                Command = MessageCommand.End,
            });

            return entries;
        }
    }
}
