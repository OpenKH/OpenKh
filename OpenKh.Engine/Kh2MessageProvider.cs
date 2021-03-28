using OpenKh.Kh2;
using OpenKh.Kh2.Messages;
using System.Collections.Generic;
using System.Linq;

namespace OpenKh.Engine
{
    public class Kh2MessageProvider : IMessageProvider
    {
        private Dictionary<int, byte[]> _messages = new Dictionary<int, byte[]>();

        public IMessageEncoder Encoder { get; set; } = Encoders.InternationalSystem;

        public byte[] GetMessage(ushort id)
        {
            if (_messages.TryGetValue(id & 0x7fff, out var data))
                return data;

            if (_messages.TryGetValue(Msg.FallbackMessage, out data))
                return data;

            return new byte[0];
        }

        public string GetString(ushort id) =>
            MsgSerializer.SerializeText(Encoder.Decode(GetMessage(id)));

        public void SetString(ushort id, string text) =>
            _messages[id] = Encoder.Encode(MsgSerializer.DeserializeText(text).ToList());

        public void Load(List<Msg.Entry> entries) =>
            _messages = entries.ToDictionary(x => x.Id, x => x.Data);
    }
}
