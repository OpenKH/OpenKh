using OpenKh.Kh2.Messages;
using System.Collections.Generic;
using Xe.Tools;

namespace OpenKh.Tools.Kh2TextEditor.Models
{
    public class TextAreaModel : BaseNotifyPropertyChanged
    {
        private IEnumerable<MessageCommandModel> textCommands;

        public IEnumerable<MessageCommandModel> TextCommands
        {
            get => textCommands;
            set
            {
                textCommands = value;
                OnPropertyChanged();
            }
        }

        public string Text
        {
            get => MsgSerializer.SerializeText(TextCommands);
            set => TextCommands = MsgSerializer.DeserializeText(value);
        }
    }
}
