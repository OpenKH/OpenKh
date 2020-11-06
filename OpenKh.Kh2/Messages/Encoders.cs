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

        internal class TurkishSystemEncoder : IMessageEncoder
        {
            private readonly IMessageDecode _decode = new TurkishSystemDecode();
            private readonly IMessageEncode _encode = new TurkishSystemEncode();

            public List<MessageCommandModel> Decode(byte[] data) => _decode.Decode(data);
            public byte[] Encode(List<MessageCommandModel> messageCommands) => _encode.Encode(messageCommands);
        }

        internal class JapaneseSystemEncoder : IMessageEncoder
        {
            private readonly IMessageDecode _decode = new JapaneseSystemDecode();
            private readonly IMessageEncode _encode = new JapaneseSystemEncode();

            public List<MessageCommandModel> Decode(byte[] data) => _decode.Decode(data);
            public byte[] Encode(List<MessageCommandModel> messageCommands) => _encode.Encode(messageCommands);
        }

        internal class JapaneseEventEncoder : IMessageEncoder
        {
            private readonly IMessageDecode _decode = new JapaneseEventDecode();
            private readonly IMessageEncode _encode = new JapaneseEventEncode();

            public List<MessageCommandModel> Decode(byte[] data) => _decode.Decode(data);
            public byte[] Encode(List<MessageCommandModel> messageCommands) => _encode.Encode(messageCommands);
        }

        public static IMessageEncoder InternationalSystem { get; } =
            new InternationalSystemEncoder();
        public static IMessageEncoder TurkishSystem { get; } =
            new TurkishSystemEncoder();
        public static IMessageEncoder JapaneseSystem { get; } =
            new JapaneseSystemEncoder();
        public static IMessageEncoder JapaneseEvent { get; } =
            new JapaneseEventEncoder();
    }
}
