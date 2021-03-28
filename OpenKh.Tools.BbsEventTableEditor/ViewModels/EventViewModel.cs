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

        public static Dictionary<int, string> AsDictionary(string[] values) =>
            values.Select((value, index) => (value, index)).ToDictionary(x => x.index, x => x.value);

        private static readonly Dictionary<int, string>[] _rooms = new[]
        {
            AsDictionary(Constants.Rooms_DP),
            AsDictionary(Constants.Rooms_DP),
            AsDictionary(Constants.Rooms_SW),
            AsDictionary(Constants.Rooms_CD),
            AsDictionary(Constants.Rooms_SB),
            AsDictionary(Constants.Rooms_YT),
            AsDictionary(Constants.Rooms_RG),
            AsDictionary(Constants.Rooms_JB),
            AsDictionary(Constants.Rooms_HE),
            AsDictionary(Constants.Rooms_LS),
            AsDictionary(Constants.Rooms_DI),
            AsDictionary(Constants.Rooms_PP),
            AsDictionary(Constants.Rooms_DC),
            AsDictionary(Constants.Rooms_KG),
            AsDictionary(Constants.Rooms_DP),
            AsDictionary(Constants.Rooms_VS),
            AsDictionary(Constants.Rooms_BD)
        };

        public EventViewModel(Event @event)
        {
            Event = @event;
        }

        public Event Event { get; private set; }

        public IEnumerable<KeyValuePair<byte, string>> Worlds => _worlds;
        public Dictionary<int, string> Rooms => _rooms[World];

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
                OnPropertyChanged(nameof(Rooms));
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
                OnPropertyChanged(nameof(Rooms));
            }
        }

        public byte EventPtn
        {
            get => Event.EventPtn;
            set
            {
                Event.EventPtn = value;
                OnPropertyChanged(nameof(Name));
            }
        }

        public byte Opt
        {
            get => Event.Opt;
            set
            {
                Event.Opt = value;
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
