using System;
using System.Collections.Generic;
using System.Text;

namespace kh.kh2.Messages
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
            if (entry.Command == MessageCommand.PrintText)
                return entry.Text;
            if (entry.Command == MessageCommand.PrintComplex)
                return $"{{{entry.Text}}}";

            return null;
        }
    }
}
