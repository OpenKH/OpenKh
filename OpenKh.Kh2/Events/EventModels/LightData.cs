using System;
using System.Collections.Generic;
using System.Text;
using Xe.BinaryMapper;

namespace OpenKh.Kh2.Events.EventModels
{
    public class LightData
    {
        [Data] public short put_id { get; set; }
        [Data] public short start_frame { get; set; }
        [Data] public short end_frame { get; set; }
        [Data] public byte cam_num { get; set; }
        [Data] public byte sub_num { get; set; }
        [Data] public LightParamPosition light_param_pos { get; set; }

    }
}
