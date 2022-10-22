using System;
using System.Collections.Generic;
using System.Text;
using Xe.BinaryMapper;

namespace OpenKh.Kh2.Events.EventModels
{
    public class SeqTexanim : IEventObject
    {
        public static ushort Type => 25;

        [Data] public short start_frame { get; set; }
        [Data] public short put_id { get; set; }
        [Data] public short no { get; set; }
        [Data] public byte flag { get; set; }
        [Data] public byte dummy { get; set; }

    }
}