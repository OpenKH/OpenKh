using OpenKh.Bbs;
using System;
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

        public ushort Id
        {
            get => Event.Id;
            set
            {
                Event.Id = value;
                OnPropertyChanged(nameof(Name));
            }
        }

        public ushort EventIndex
        {
            get => Event.EventIndex;
            set
            {
                Event.EventIndex = (byte)Math.Min(999, (int)value);
                OnPropertyChanged(nameof(Name));
                OnPropertyChanged(nameof(EventPath));
            }
        }

        public byte World
        {
            get => Event.World;
            set
            {
                Event.World = (byte)Math.Min(Constants.Worlds.Length, value);
                OnPropertyChanged(nameof(Name));
                OnPropertyChanged(nameof(EventPath));
            }
        }

        public byte Room
        {
            get => Event.Room;
            set
            {
                Event.Room = (byte)Math.Min(99, (int)value);
                OnPropertyChanged(nameof(Name));
                OnPropertyChanged(nameof(MapPath));
            }
        }

        public ushort Unknown06
        {
            get => Event.Unknown06;
            set
            {
                Event.Unknown06 = value;
                OnPropertyChanged(nameof(Name));
            }
        }

        private string WorldId => World >= 0 && World < Constants.Worlds.Length ?
            Constants.Worlds[World] : "{invalid}";

        public string Name => $"{Id} {WorldId.ToUpper()} {Room:D02} {EventIndex:D03}";

        public string MapPath => $"arc/map/{WorldId}{Room:D02}.arc";

        public string EventPath => $"event/{WorldId}/{WorldId}_{EventIndex:D03}.exa";

        public override string ToString() => Name;
    }
}
