using System;
using System.Collections.Generic;
using System.Text;

namespace OpenKh.Kh2Anim.Mset
{
    public class T1
    {
        public int c00, c02;
        public float c04;

        public T1(int c00, int c02, float c04)
        {
            this.c00 = c00;
            this.c02 = c02;
            this.c04 = c04;
        }

        public override string ToString()
        {
            return string.Format("{0:X4} {1:X4} {2}", c00, c02, c04);
        }
    }
}
