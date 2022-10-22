using System;
using System.Collections.Generic;
using System.Text;
using Xe.BinaryMapper;

namespace OpenKh.Kh2.Events.EventModels
{
    public class MusicalTarget : IEventObject
    {
        public static ushort Type => 51;

        [Data] public short appearFrame { get; set; }
        [Data] public short button { get; set; }
        [Data] public short countDownNumber { get; set; }
        [Data] public short countDownStartFrame { get; set; }
        [Data] public short possible { get; set; }
        [Data] public short point { get; set; }
        [Data] public short okSceneFrame { get; set; }
        [Data] public short put_id { get; set; }
        [Data] public short bone { get; set; }
        [Data] public short dummy { get; set; }

    }
}