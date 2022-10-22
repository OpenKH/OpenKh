using System;
using System.Collections.Generic;
using System.Text;
using Xe.BinaryMapper;

namespace OpenKh.Kh2.Events.EventModels
{
    public class Countdown : IEventObject
    {
        public static ushort Type => 80;

        [Data] public short start_frame { get; set; }

    }
}