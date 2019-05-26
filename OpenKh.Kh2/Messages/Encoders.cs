using OpenKh.Kh2.Messages.Internals;
using System;
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

        internal class JapaneseSystemEncoder : IMessageEncoder
        {
            private readonly IMessageDecode _decode = new JapaneseSystemDecode();
            private readonly IMessageEncode _encode = null;

            public List<MessageCommandModel> Decode(byte[] data) => _decode.Decode(data);
            public byte[] Encode(List<MessageCommandModel> messageCommands) => throw new NotImplementedException();
        }

        public static IMessageEncoder InternationalSystem { get; } =
            new InternationalSystemEncoder();
        public static IMessageEncoder JapaneseSystem { get; } =
            new JapaneseSystemEncoder();
    }
}
