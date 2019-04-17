using System.Collections.Generic;

namespace kh.kh2.Messages
{
    public interface IMessageDecode
    {
        List<MessageCommandModel> Decode(byte[] data);
    }
}
