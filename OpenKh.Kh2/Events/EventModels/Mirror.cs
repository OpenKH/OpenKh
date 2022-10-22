using System;
using System.Collections.Generic;
using System.Text;
using Xe.BinaryMapper;

namespace OpenKh.Kh2.Events.EventModels
{
    public class Mirror : IEventObject
    {
        public static ushort Type => 58;

        [Data] public short start_frame { get; set; }
        [Data] public short put_id { get; set; }
        [Data] public short x { get; set; }
        [Data] public short y { get; set; }
        [Data] public short z { get; set; }

    }
}