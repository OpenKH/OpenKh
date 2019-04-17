using System.Collections.Generic;

namespace kh.kh2.Messages
{
    public interface IMessageEncode
    {
        byte[] Encode(List<MessageCommandModel> messageCommands);
    }
}
