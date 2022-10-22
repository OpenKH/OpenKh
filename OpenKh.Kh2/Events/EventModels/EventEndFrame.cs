using System;
using System.Collections.Generic;
using System.Text;
using Xe.BinaryMapper;

namespace OpenKh.Kh2.Events.EventModels
{
    public class EventEndFrame : IEventObject
    {
        public static ushort Type => 8;

        [Data] public short event_end_frame { get; set; }
        [Data] public short dummy { get; set; }

    }
}