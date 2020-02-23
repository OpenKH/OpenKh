using OpenKh.Bbs;
using OpenKh.Bbs.Messages;
using OpenKh.Tools.Common;
using OpenKh.Tools.CtdEditor.Interfaces;
using System.Linq;
using Xe.Drawing;
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
                if (FontContext == null)
                    return;
                _drawHandler.DrawHandler(CtdEncoders.International, FontContext, Message);
            });
        }

        public Ctd.Message Message { get; }

        public short Id
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

        public string TextDump =>
            string.Join(" ", Message.Data.Select(x => $"{x:X02}"));

        public string Title => $"{Id}: {Text}";

        public IDrawing DrawingContext => _drawHandler.DrawingContext;
        public RelayCommand DrawHandler { get; }
        public FontsArc.Font FontContext { get; set; }

        public override string ToString() => Title;
    }
}
