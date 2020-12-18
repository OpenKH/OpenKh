using System;
using System.Collections.Generic;
using System.Text;

namespace OpenKh.Research.Kh2AnimIKC.Models
{
    public class BoneDef
    {
        public string Display { get; set; }

        public int Idx { get; set; }
        public int Parent { get; set; } = -1;
        public int Unk2 { get; set; }
        public int Unk3 { get; set; }

        public float Sx { get; set; } = 1;
        public float Sy { get; set; } = 1;
        public float Sz { get; set; } = 1;
        public float Sw { get; set; }

        public float Rx { get; set; }
        public float Ry { get; set; }
        public float Rz { get; set; }
        public float Rw { get; set; }

        public float Tx { get; set; }
        public float Ty { get; set; }
        public float Tz { get; set; }
        public float Tw { get; set; }
    }
}
