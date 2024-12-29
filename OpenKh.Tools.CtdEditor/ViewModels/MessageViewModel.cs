using OpenKh.Bbs;
using OpenKh.Bbs.Messages;
using OpenKh.Engine.Renders;
using OpenKh.Tools.CtdEditor.Helpers;
using OpenKh.Tools.CtdEditor.Interfaces;
using System.Linq;
using Xe.Tools;
using Xe.Tools.Wpf.Commands;

namespace OpenKh.Tools.CtdEditor.ViewModels
{
    public class MessageViewModel : BaseNotifyPropertyChanged
    {
        private readonly Ctd _ctd;

        public MessageViewModel(
            IDrawHandler drawHandler,
            Ctd ctd,
            Ctd.Message message,
            MessageConverter messageConverter)
        {
            _ctd = ctd;
            Message = message;
            MessageConverter = messageConverter;

            DrawHandler = new RelayCommand(_ =>
            {
                var layout = _ctd.Layouts[Message.LayoutIndex];
                drawHandler.DrawHandler(CtdEncoders.Unified, Message, layout);
                drawHandler.DrawingContext.Flush();
            });
        }

        public Ctd.Message Message { get; }

        public uint Id
        {
            get => Message.Id;
            set
            {
                Message.Id = value;
                OnPropertyChanged(nameof(Title));
            }
        }

        public string Text
        {
            get => MessageConverter.ToText(Message.Data);
            set
            {
                Message.Data = MessageConverter.FromText(value);
                OnPropertyChanged(nameof(Title));
                OnPropertyChanged(nameof(TextDump));
            }
        }

        #region MessageConverter
        private MessageConverter _messageConverter = null;
        public MessageConverter MessageConverter
        {
            get => _messageConverter;
            set
            {
                _messageConverter = value;
                OnPropertyChanged(nameof(Text));
                OnPropertyChanged(nameof(Title));
                OnPropertyChanged(nameof(MessageConverter));
            }
        }
        #endregion

        public ushort LayoutID
        {
            get => Message.LayoutIndex;
            set
            {
                Message.LayoutIndex = value;
            }
        }

        public string TextDump =>
            string.Join(" ", Message.Data.Select(x => $"{x:X02}"));

        public string Title => $"{Id}: {Text}";

        public RelayCommand DrawHandler { get; }

        public override string ToString() => Title;
    }
}
