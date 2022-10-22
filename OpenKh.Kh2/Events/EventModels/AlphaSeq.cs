using System;
using System.Collections.Generic;
using System.Text;
using Xe.BinaryMapper;

namespace OpenKh.Kh2.Events.EventModels
{
    public class AlphaSeq : IEventObject
    {
        public static ushort Type => 14;

        [Data] public short start_frame { get; set; }
        [Data] public short end_frame { get; set; }
        [Data] public byte start_alpha { get; set; }
        [Data] public byte end_alpha { get; set; }
        [Data] public short time { get; set; }
        [Data] public short put_id { get; set; }

    }
}