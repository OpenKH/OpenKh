using System;
using System.Collections.Generic;
using System.Text;
using Xe.BinaryMapper;

namespace OpenKh.Kh2.Events.EventModels
{
    public class Bg : IEventObject
    {
        public static ushort Type => 3;

        [Data] public short area { get; set; }
        public string world { get; set; }

    }
}