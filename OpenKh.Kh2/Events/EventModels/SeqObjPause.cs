using System;
using System.Collections.Generic;
using System.Text;
using Xe.BinaryMapper;

namespace OpenKh.Kh2.Events.EventModels
{
    public class SeqObjPause : IEventObject
    {
        public static ushort Type => 65;

        [Data] public short start_frame { get; set; }
        [Data] public byte sw { get; set; }
        [Data] public byte dummy { get; set; }

    }
}