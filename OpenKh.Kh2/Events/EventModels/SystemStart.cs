using System;
using System.Collections.Generic;
using System.Text;
using Xe.BinaryMapper;

namespace OpenKh.Kh2.Events.EventModels
{
    public class SystemStart : IEventObject
    {
        public static ushort Type => 16;

        [Data] public short fade_frame { get; set; }
        [Data] public short dummy { get; set; }

    }
}