using OpenKh.Kh2.Messages.Internals;
using System.Collections.Generic;

namespace OpenKh.Kh2.Messages
{
    public static class Encoders
    {
        internal class InternationalSystemEncoder : IMessageEncoder
        {
            private readonly IMessageDecode _decode = new InternationalSystemDecode();
            private readonly IMessageEncode _encode = new InternationalSystemEncode();

            public List<MessageCommandModel> Decode(byte[] data) => _decode.Decode(data);
            public byte[] Encode(List<MessageCommandModel> messageCommands) => _encode.Encode(messageCommands);
        }

        public static IMessageEncoder InternationalSystem { get; } =
            new InternationalSystemEncoder();
    }
}
