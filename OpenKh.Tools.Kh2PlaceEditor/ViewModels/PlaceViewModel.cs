using OpenKh.Engine;
using OpenKh.Kh2;
using Xe.Tools;

namespace OpenKh.Tools.Kh2PlaceEditor.ViewModels
{
    public class PlaceViewModel : BaseNotifyPropertyChanged
    {
        private readonly IMessageProvider _messageProvider;
        private readonly int _index;

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

        public short MessageId
        {
            get => (short)(Place.MessageId & 0x7fff);
            set
            {
                Place.MessageId = (ushort)(value | 0x8000);
                OnPropertyChanged(nameof(Name));
            }
        }

        public string Name =>
            $"{World}{_index:D02} | {MessageId:D5} | {_messageProvider.GetMessage(Place.MessageId)}";

        public void RefreshMessages() => OnPropertyChanged(nameof(Name));
    }
}
