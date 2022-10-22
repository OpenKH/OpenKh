using System;
using System.Collections.Generic;
using System.Text;
using Xe.BinaryMapper;

namespace OpenKh.Kh2.Events.EventModels
{
    public class SeqMovie : IEventObject
    {
        public static ushort Type => 68;

        [Data] public short start_frame { get; set; }
        public string name { get; set; }

    }
}