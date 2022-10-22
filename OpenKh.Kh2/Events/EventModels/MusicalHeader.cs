using System;
using System.Collections.Generic;
using System.Text;
using Xe.BinaryMapper;

namespace OpenKh.Kh2.Events.EventModels
{
    public class MusicalHeader : IEventObject
    {
        public static ushort Type => 50;

        [Data] public short rhythm { get; set; }
        [Data] public short clearScore { get; set; }

    }
}