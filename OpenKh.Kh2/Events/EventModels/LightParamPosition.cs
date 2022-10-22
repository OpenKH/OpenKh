using System;
using System.Collections.Generic;
using System.Text;
using Xe.BinaryMapper;

namespace OpenKh.Kh2.Events.EventModels
{
    public class LightParamPosition
    {
        [Data(Count = 9)] public float[] pos { get; set; }
        [Data(Count = 12)] public float[] color { get; set; }
    }
}
