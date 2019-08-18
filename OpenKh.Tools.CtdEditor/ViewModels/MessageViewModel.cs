using OpenKh.Bbs;
using System.Linq;
using Xe.Tools;

namespace OpenKh.Tools.CtdEditor.ViewModels
{
    public class MessageViewModel : BaseNotifyPropertyChanged
    {
        private readonly Ctd _ctd;

        public MessageViewModel(Ctd ctd, Ctd.FakeEntry message)
        {
            _ctd = ctd;
            Message = message;
        }

        public Ctd.FakeEntry Message { get; }

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

        public override string ToString() => Title;
    }
}
