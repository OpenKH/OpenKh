using System;
using System.Collections.Generic;
using System.Text;
using Xe.BinaryMapper;

namespace OpenKh.Kh2.Events.EventModels
{
    public class SeqObjcamera : IEventObject
    {
        public static ushort Type => 49;

        [Data] public short start_frame { get; set; }
        [Data] public short type { get; set; }

    }
}