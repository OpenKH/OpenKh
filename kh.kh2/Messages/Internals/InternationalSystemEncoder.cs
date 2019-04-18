using System.Collections.Generic;

namespace kh.kh2.Messages.Internals
{
    internal class InternationalSystemEncoder : IMessageEncoder
    {
        private readonly IMessageDecode _decode = new InternationalSystemDecode();
        private readonly IMessageEncode _encode = new InternationalSystemEncode();

        public List<MessageCommandModel> Decode(byte[] data) => _decode.Decode(data);
        public byte[] Encode(List<MessageCommandModel> messageCommands) => _encode.Encode(messageCommands);
    }
}
