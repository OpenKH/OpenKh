using OpenKh.Kh2;
using OpenKh.Kh2.Messages;
using System.Collections.Generic;
using System.Linq;
using Xe.Tools;

namespace OpenKh.Tools.Kh2TextEditor.Models
{
    public class MessageModel : BaseNotifyPropertyChanged
    {
        private readonly IMessageEncoder _encoder;
        private readonly Msg.Entry _entry;

        public MessageModel(IMessageEncoder encoder, Msg.Entry entry)
        {
            _encoder = encoder;
            _entry = entry;
        }

        public int Id => _entry.Id;

        public string Text
        {
            get => MsgSerializer.SerializeText(MessageCommands);
            set
            {
                MessageCommands = MsgSerializer.DeserializeText(value);
                OnPropertyChanged();
                OnPropertyChanged(nameof(Title));
            }
        }

        public IEnumerable<MessageCommandModel> MessageCommands
        {
            get => _encoder.Decode(_entry.Data);
            set
            {
                _entry.Data = _encoder.Encode(value.ToList());
                OnPropertyChanged();
            }
        }

        public string Title
        {
            get
            {
                const int MaxTitleLength = 100;
                var title = $"{Id}: {Text}";
                return title.Length > MaxTitleLength ? $"{title.Substring(0, MaxTitleLength)}..." : title;
            }
        }
    }
}
