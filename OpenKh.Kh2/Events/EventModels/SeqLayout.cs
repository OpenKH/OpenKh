using System;
using System.Collections.Generic;
using System.Text;
using Xe.BinaryMapper;

namespace OpenKh.Kh2.Events.EventModels
{
    public class SeqLayout : IEventObject
    {
        public static ushort Type => 61;

        [Data] public short start_frame { get; set; }
        [Data] public short number { get; set; }
        public string name { get; set; }

    }
}