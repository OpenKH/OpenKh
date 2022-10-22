using System;
using System.Collections.Generic;
using System.Text;
using Xe.BinaryMapper;

namespace OpenKh.Kh2.Events.EventModels
{
    public class Shake : IEventObject
    {
        public static ushort Type => 39;

        [Data] public short start_frame { get; set; }
        [Data] public short width { get; set; }
        [Data] public short height { get; set; }
        [Data] public short depth { get; set; }
        [Data] public short frame { get; set; }
        [Data] public short type { get; set; }

    }
}