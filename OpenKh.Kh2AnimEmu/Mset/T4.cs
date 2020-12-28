using System;
using System.Collections.Generic;
using System.Text;

namespace OpenKh.Kh2Anim.Mset
{
    public class T4
    {
        public int c00;
        public int c02;

        public T4(int c00, int c02)
        {
            this.c00 = c00;
            this.c02 = c02;
        }

        public override string ToString()
        {
            return string.Format("{0:X4} {1:X4}", c00, c02);
        }
    }
}
