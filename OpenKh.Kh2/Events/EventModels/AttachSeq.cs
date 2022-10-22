using System;
using System.Collections.Generic;
using System.Text;
using Xe.BinaryMapper;

namespace OpenKh.Kh2.Events.EventModels
{
    public class AttachSeq : IEventObject
    {
        public static ushort Type => 10;

        [Data] public short start_frame { get; set; }
        [Data] public short end_frame { get; set; }
        [Data] public short my_put_id { get; set; }
        [Data] public short attach_put_id { get; set; }
        [Data] public short bone_no { get; set; }
        [Data] public short pax_no { get; set; }
        [Data] public short type { get; set; }

    }
}