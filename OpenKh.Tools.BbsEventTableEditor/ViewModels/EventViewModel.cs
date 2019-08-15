using OpenKh.Bbs;
using System.Collections.Generic;
using System.Linq;
using Xe.Tools;

namespace OpenKh.Tools.BbsEventTableEditor.ViewModels
{
    public class EventViewModel : BaseNotifyPropertyChanged
    {
        private static readonly Dictionary<byte, string> _worlds = Constants.WorldNames
            .Select((x, i) => new { Name = x, Id = i })
            .ToDictionary(x => (byte)x.Id, x => x.Name);

        public EventViewModel(Event @event)
        {
            Event = @event;
        }

        public Event Event { get; private set; }

        public IEnumerable<KeyValuePair<byte, string>> Worlds => _worlds;

        public short Id
        {
            get => Event.Id;
            set
            {
                Event.Id = value;
                OnPropertyChanged(nameof(Name));
            }
        }

        public short EventIndex
        {
            get => Event.EventIndex;
            set
            {
                Event.EventIndex = value;
                OnPropertyChanged(nameof(Name));
            }
        }

        public byte World
        {
            get => Event.World;
            set
            {
                Event.World = value;
                OnPropertyChanged(nameof(Name));
            }
        }

        public byte Room
        {
            get => Event.Room;
            set
            {
                Event.Room = value;
                OnPropertyChanged(nameof(Name));
            }
        }

        public short Unknown06
        {
            get => Event.Unknown06;
            set
            {
                Event.Unknown06 = value;
                OnPropertyChanged(nameof(Name));
            }
        }

        private string WorldName => World >= 0 && World < Constants.WorldNames.Length ?
            Constants.WorldNames[World] : $"{World:X02}";

        public string Name => $"{Id} {WorldName} {Room} {EventIndex}";

        public override string ToString() => Name;
    }
}
