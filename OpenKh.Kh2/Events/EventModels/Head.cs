using System;
using System.Collections.Generic;
using System.Text;
using Xe.BinaryMapper;

namespace OpenKh.Kh2.Events.EventModels
{
    public class Head : IEventObject
    {
        public static ushort Type => 0;

        [Data] public int mem_size { get; set; }
        [Data] public byte ver { get; set; }
        [Data] public byte obj_camera_type { get; set; }
        public string name { get; set; }
    }
}
