using System;
using System.Collections.Generic;
using System.Text;
using Xe.BinaryMapper;

namespace OpenKh.Kh2.Events.EventModels
{
    public class SplineDataEnc : IEventObject
    {
        public static ushort Type => 29;

        [Data] public short put_id { get; set; }
        [Data] public short trans_ofs { get; set; }
        [Data] public short trans_cnt { get; set; }
        [Data] public short dummy { get; set; }
        public CameraDataEncWk[] works { get; set; }

    }
}
