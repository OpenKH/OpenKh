using OpenKh.Kh2;
using OpenKh.Kh2.Messages;
using System.Collections.Generic;
using System.Linq;

namespace OpenKh.Engine
{
    public class Kh2MessageProvider : IMessageProvider
    {
        private List<Msg.Entry> _messages;

        public IMessageEncoder Encoder { get; set; } = Encoders.InternationalSystem;

        public byte[] GetMessage(ushort id)
        {
            var message = _messages?.FirstOrDefault(x => x.Id == (id & 0x7fff));
            if (message == null)
            {
                if (id == Msg.FallbackMessage)
                    return new byte[0];

                return GetMessage(Msg.FallbackMessage);
            }

            return message.Data;
        }

        public string GetString(ushort id) =>
            MsgSerializer.SerializeText(Encoder.Decode(GetMessage(id)));

        public void SetString(ushort id, string text)
        {
            var message = _messages?.FirstOrDefault(x => x.Id == (id & 0x7fff));
            if (message == null)
                return;

            message.Data = Encoder.Encode(MsgSerializer.DeserializeText(text).ToList());
        }

        public void Load(List<Msg.Entry> entries) =>
            _messages = entries;
    }
}
