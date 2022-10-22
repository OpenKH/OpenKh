using System;
using System.Collections.Generic;
using System.Text;
using Xe.BinaryMapper;

namespace OpenKh.Kh2.Events.EventModels
{
    public class CameraDataEncWk
    {
        [Data] public int flags { get; set; }
        [Data] public float value { get; set; }
        [Data] public float left { get; set; }
        [Data] public float right { get; set; }
    }
}
