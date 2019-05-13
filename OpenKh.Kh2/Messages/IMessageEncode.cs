using System.Collections.Generic;

namespace OpenKh.Kh2.Messages
{
    public interface IMessageEncode
    {
        byte[] Encode(List<MessageCommandModel> messageCommands);
    }
}
