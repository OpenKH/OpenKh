using System;
using System.Collections.Generic;
using System.Text;
using Xe.BinaryMapper;

namespace OpenKh.Kh2.Events.EventModels
{
    public class EventJump : IEventObject
    {
        public static ushort Type => 17;

        [Data] public short start_frame { get; set; }
        [Data] public short type { get; set; }
        [Data] public short set_no { get; set; }
        [Data] public short area { get; set; }
        [Data] public short entrance { get; set; }
        public string world { get; set; }

    }
}