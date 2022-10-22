using System;
using System.Collections.Generic;
using System.Text;
using Xe.BinaryMapper;

namespace OpenKh.Kh2.Events.EventModels
{
    public class ReadCtrlCharaTbl : IEventObject
    {
        public static ushort Type => 56;

        [Data] public short entry_id { get; set; }
        [Data] public short put_id { get; set; }
        public string name { get; set; }

    }
}