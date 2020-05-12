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

        public string GetMessage(ushort id)
        {
            var message = _messages?.FirstOrDefault(x => x.Id == (id & 0x7fff));
            if (message == null)
            {
                if (id == Msg.FallbackMessage)
                    return null;

                return GetMessage(Msg.FallbackMessage);
            }

            return MsgSerializer.SerializeText(Encoder.Decode(message.Data));
        }

        public void SetMessage(ushort id, string text)
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
