using System;
using System.Collections.Generic;
using System.Text;
using Xe.BinaryMapper;

namespace OpenKh.Kh2.Events.EventModels
{
    public class Scale : IEventObject
    {
        public static ushort Type => 40;

        [Data] public short start_frame { get; set; }
        [Data] public short put_id { get; set; }
        [Data] public float start { get; set; }
        [Data] public float end { get; set; }
        [Data] public short frame { get; set; }
        [Data] public short type { get; set; }

    }
}