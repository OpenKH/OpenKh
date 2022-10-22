using System;
using System.Collections.Generic;
using System.Text;
using Xe.BinaryMapper;

namespace OpenKh.Kh2.Events.EventModels
{
    public class PartSeq : IEventObject
    {
        public static ushort Type => 13;

        [Data] public short start_frame { get; set; }
        [Data] public short end_frame { get; set; }
        [Data] public short put_id { get; set; }
        [Data(Count = 33)] public short[] part { get; set; }

    }
}
