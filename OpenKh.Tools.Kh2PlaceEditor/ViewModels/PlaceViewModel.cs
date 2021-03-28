using OpenKh.Engine;
using OpenKh.Kh2;
using System.Text;
using Xe.Tools;

namespace OpenKh.Tools.Kh2PlaceEditor.ViewModels
{
    public class PlaceViewModel : BaseNotifyPropertyChanged
    {
        private const int ShiftJisCodepage = 932;
        private readonly Encoding Encoding = Encoding.GetEncoding(ShiftJisCodepage);

        private readonly IMessageProvider _messageProvider;
        private readonly int _index;

        static PlaceViewModel()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        }

        public PlaceViewModel(IMessageProvider messageProvider,
            string world, int index, Place place)
        {
            _messageProvider = messageProvider;
            World = world;
            _index = index;
            Place = place;
        }

        public string World { get; }

        public Place Place { get; }

        public string Map => $"{World}{_index:D02}";

        public short MessageId
        {
            get => (short)(Place.MessageId & 0x7fff);
            set
            {
                Place.MessageId = (ushort)(value | 0x8000);
                OnPropertyChanged(nameof(Name));
                OnPropertyChanged(nameof(Message));
            }
        }

        public string Name
        {
            get => Encoding.GetString(Place.Name);
            set => Place.Name = Encoding.GetBytes(value);
        }

        public string Message => _messageProvider.GetString(Place.MessageId);

        public void RefreshMessages()
        {
            OnPropertyChanged(nameof(Name));
            OnPropertyChanged(nameof(Message));
        }
    }
}
