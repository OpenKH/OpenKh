using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OpenKh.Research.Kh2Anim.Models
{
    public class BuildMatricesRequest
    {
        public byte[] Mdlx { get; set; }
        public byte[] Motion { get; set; }
        public float[] KeyFrames { get; set; }
    }
}
