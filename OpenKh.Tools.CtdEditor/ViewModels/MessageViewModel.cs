using OpenKh.Bbs;
using OpenKh.Bbs.Messages;
using OpenKh.Engine.Renders;
using OpenKh.Tools.CtdEditor.Interfaces;
using System.Linq;
using Xe.Tools;
using Xe.Tools.Wpf.Commands;

namespace OpenKh.Tools.CtdEditor.ViewModels
{
    public class MessageViewModel : BaseNotifyPropertyChanged
    {
        private readonly IDrawHandler _drawHandler;
        private readonly Ctd _ctd;

        public MessageViewModel(
            IDrawHandler drawHandler,
            Ctd ctd,
            Ctd.Message message)
        {
            _drawHandler = drawHandler;
            _ctd = ctd;
            Message = message;

            DrawHandler = new RelayCommand(_ =>
            {
                var layout = _ctd.Layouts[Message.LayoutIndex];
                _drawHandler.DrawHandler(CtdEncoders.International, Message, layout);
                _drawHandler.DrawingContext.Flush();
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
            get => Message.Text;
            set
            {
                Message.Text = value;
                OnPropertyChanged(nameof(Title));
                OnPropertyChanged(nameof(TextDump));
            }
        }

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

        public ISpriteDrawing DrawingContext => _drawHandler.DrawingContext;
        public RelayCommand DrawHandler { get; }

        public override string ToString() => Title;
    }
}
