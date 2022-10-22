using System;
using System.Collections.Generic;
using System.Text;
using Xe.BinaryMapper;

namespace OpenKh.Kh2.Events.EventModels
{
    public class SeqMem : IEventObject
    {
        public static ushort Type => 26;

        [Data] public short start_frame { get; set; }
        [Data] public short put_id { get; set; }

    }
}