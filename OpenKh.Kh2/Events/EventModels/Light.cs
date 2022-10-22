using System;
using System.Collections.Generic;
using System.Text;
using Xe.BinaryMapper;

namespace OpenKh.Kh2.Events.EventModels
{
    public class Light : IEventObject
    {
        public static ushort Type => 78;

        [Data] public short cnt { get; set; }
        [Data] public short work_num { get; set; }
        public LightData[] lightData { get; set; }

    }
}
