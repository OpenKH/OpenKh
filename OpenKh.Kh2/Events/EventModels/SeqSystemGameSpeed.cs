using System;
using System.Collections.Generic;
using System.Text;
using Xe.BinaryMapper;

namespace OpenKh.Kh2.Events.EventModels
{
    public class SeqSystemGameSpeed : IEventObject
    {
        public static ushort Type => 32;

        [Data] public short start_frame { get; set; }
        [Data] public short game_speed_frame { get; set; }
        [Data] public float game_speed { get; set; }

    }
}