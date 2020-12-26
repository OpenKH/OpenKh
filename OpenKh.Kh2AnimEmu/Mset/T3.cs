using System;
using System.Collections.Generic;
using System.Text;

namespace OpenKh.Kh2Anim.Mset
{
    public class T3
    {
        public int c00, c01, c02, c04, c06;
        public uint c08;

        public T3(int c00, int c01, int c02, int c04, int c06, uint c08)
        {
            this.c00 = c00;
            this.c01 = c01;
            this.c02 = c02;
            this.c04 = c04;
            this.c06 = c06;
            this.c08 = c08;
        }

        public override string ToString()
        {
            return string.Format("{0:X2} {1:X2} {2:X4} {3:X4} {4:X4} {5:X8}", c00, c01, c02, c04, c06, c08);
        }
    }
}
