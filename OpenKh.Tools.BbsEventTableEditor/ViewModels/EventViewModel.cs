using OpenKh.Bbs;

namespace OpenKh.Tools.BbsEventTableEditor.ViewModels
{
    public class EventViewModel
    {
        public EventViewModel(Event @event)
        {
            Event = @event;
        }

        public Event Event { get; private set; }

        public short Id { get => Event.Id; set => Event.Id = value; }
        public short EventIndex { get => Event.EventIndex; set => Event.EventIndex = value; }
        public byte World { get => Event.World; set => Event.World = value; }
        public byte Room { get => Event.Room; set => Event.Room = value; }
        public short Unknown06 { get => Event.Unknown06; set => Event.Unknown06 = value; }


    }
}
