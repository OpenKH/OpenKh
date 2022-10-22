using System;
using System.Collections.Generic;
using System.Text;
using Xe.BinaryMapper;

namespace OpenKh.Kh2.Events.EventModels
{
    public class Turn : IEventObject
    {
        public static ushort Type => 41;

        [Data] public short start_frame { get; set; }
        [Data] public short put_id { get; set; }
        [Data] public short type { get; set; }
        [Data] public short frame { get; set; }
        [Data] public float end { get; set; }

    }
}