using kh.kh2.Messages;
using System.Collections.Generic;

namespace kh.kh2.Messages
{
    public static class MsgParser
    {
        public static List<MessageCommandModel> Map(this Msg.Entry entry) =>
            new MsgParserInternal(entry.Data).Parse();
    }
}
