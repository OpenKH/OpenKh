using System;
using System.Collections.Generic;
using System.Text;
using Xe.BinaryMapper;

namespace OpenKh.Kh2.Events.EventModels
{
    public class MusicalScene : IEventObject
    {
        public static ushort Type => 52;

        [Data] public short frame { get; set; }
        [Data] public short ngSceneFrame { get; set; }

    }
}