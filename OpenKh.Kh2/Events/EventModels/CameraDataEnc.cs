using System;
using System.Collections.Generic;
using System.Text;
using Xe.BinaryMapper;

namespace OpenKh.Kh2.Events.EventModels
{
    public class CameraDataEnc : IEventObject
    {
        public static ushort Type => 19;

        [Data] public short put_id { get; set; }
        [Data] public short cmetx_ofs { get; set; }
        [Data] public short cmetx_cnt { get; set; }
        [Data] public short cmety_ofs { get; set; }
        [Data] public short cmety_cnt { get; set; }
        [Data] public short cmetz_ofs { get; set; }
        [Data] public short cmetz_cnt { get; set; }
        [Data] public short cmietx_ofs { get; set; }
        [Data] public short cmietx_cnt { get; set; }
        [Data] public short cmiety_ofs { get; set; }
        [Data] public short cmiety_cnt { get; set; }
        [Data] public short cmietz_ofs { get; set; }
        [Data] public short cmietz_cnt { get; set; }
        [Data] public short cmroll_ofs { get; set; }
        [Data] public short cmroll_cnt { get; set; }
        [Data] public short cmfov_ofs { get; set; }
        [Data] public short cmfov_cnt { get; set; }
        [Data] public short dummy { get; set; }
        public CameraDataEncWk[] works { get; set; }

    }
}
