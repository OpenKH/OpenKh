using System.Collections.Generic;

namespace kh.kh2.Messages.Internals
{
    internal class InternationalSystemEncoder : IMessageEncoder
    {
        private readonly IMessageDecode _decode = new InternationalSystemDecode();

        public List<MessageCommandModel> Decode(byte[] data) => _decode.Decode(data);
    }
}
