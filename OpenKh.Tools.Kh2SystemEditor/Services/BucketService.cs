using OpenKh.Kh2;
using OpenKh.Kh2.Messages;
using OpenKh.Tools.Kh2SystemEditor.Interfaces;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace OpenKh.Tools.Kh2SystemEditor.Services
{
    public class BucketService : IMessageProvider
    {
        private List<Msg.Entry> _messages;

        public string GetMessage(ushort id)
        {
            var message = _messages.FirstOrDefault(x => x.Id == (id & 0x7fff));
            if (message == null)
            {
                if (id == Msg.FallbackMessage)
                    return null;

                return GetMessage(Msg.FallbackMessage);
            }

            return MsgSerializer.SerializeText(Encoders.InternationalSystem.Decode(message.Data));
        }

        public void SetMessage(ushort id, string text)
        {
            var message = _messages.FirstOrDefault(x => x.Id == (id & 0x7fff));
            if (message == null)
                return;

            message.Data = Encoders.InternationalSystem.Encode(MsgSerializer.DeserializeText(text).ToList());
        }

        public bool LoadMessages(Stream stream) =>
            TryReadMessagesAsRaw(stream) || TryReadMessagesAsBar(stream);

        private bool TryReadMessagesAsBar(Stream stream)
        {
            if (!Bar.IsValid(stream))
                return false;

            var entries = Bar.Read(stream);
            var entry = entries.FirstOrDefault(x => x.Type == Bar.EntryType.Binary && x.Name == "sys");
            if (entry == null)
                return false;

            return TryReadMessagesAsRaw(entry.Stream);
        }

        private bool TryReadMessagesAsRaw(Stream stream)
        {
            if (!Msg.IsValid(stream))
                return false;

            _messages = Msg.Read(stream);
            return true;
        }
    }
}
