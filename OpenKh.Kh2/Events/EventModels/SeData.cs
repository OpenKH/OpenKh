using System;
using System.Collections.Generic;
using System.Text;
using Xe.BinaryMapper;

namespace OpenKh.Kh2.Events.EventModels
{
    public class SeData : IEventObject
    {
        public static ushort Type => 42;

        [Data] public short put_id { get; set; }
        [Data] public short seb_number { get; set; }
        [Data] public short wave_number { get; set; }

    }
}