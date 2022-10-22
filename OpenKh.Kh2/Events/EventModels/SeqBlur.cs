using System;
using System.Collections.Generic;
using System.Text;
using Xe.BinaryMapper;

namespace OpenKh.Kh2.Events.EventModels
{
    public class SeqBlur : IEventObject
    {
        public static ushort Type => 23;

        [Data] public short start_frame { get; set; }
        [Data] public byte sw { get; set; }
        [Data] public byte alpha { get; set; }
        [Data] public float rot { get; set; }
        [Data] public short x { get; set; }
        [Data] public short y { get; set; }
        [Data] public short rot_frame { get; set; }
        [Data] public short padding { get; set; }

    }
}
