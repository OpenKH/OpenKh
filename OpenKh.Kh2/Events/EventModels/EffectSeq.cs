using System;
using System.Collections.Generic;
using System.Text;
using Xe.BinaryMapper;

namespace OpenKh.Kh2.Events.EventModels
{
    public class EffectSeq : IEventObject
    {
        public static ushort Type => 9;

        [Data] public short start_frame { get; set; }
        [Data] public short loop { get; set; }
        [Data] public short put_id { get; set; }
        [Data] public short pax_no { get; set; }
        [Data] public short no { get; set; }
        [Data] public short end_type { get; set; }
        [Data] public short fade_frame { get; set; }

    }
}