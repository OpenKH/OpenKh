using System;
using System.Collections.Generic;
using System.Text;

namespace OpenKh.Kh2Anim.Legacy
{
    public class AxBone
    {
        public int cur, parent, v08, v0c;
        public float x1, y1, z1, w1; // S
        public float x2, y2, z2, w2; // R
        public float x3, y3, z3, w3; // T

        public AxBone Clone()
        {
            return (AxBone)MemberwiseClone();
        }
    }
}
