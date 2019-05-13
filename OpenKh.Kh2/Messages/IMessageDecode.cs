using System.Collections.Generic;

namespace OpenKh.Kh2.Messages
{
    public interface IMessageDecode
    {
        List<MessageCommandModel> Decode(byte[] data);
    }
}
