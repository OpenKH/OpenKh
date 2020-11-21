using System;
using System.Collections.Generic;
using System.Text;

namespace OpenKh.Kh2Anim.Mset
{
    public class T2
    {
        public int c00, c01, c02, c03, c04;
        public List<T9f> al9f = new List<T9f>();

        public T2(int c00, int c01, int c02, int c03, int c04)
        {
            this.c00 = c00;
            this.c01 = c01;
            this.c02 = c02;
            this.c03 = c03;
            this.c04 = c04;
        }

        public override string ToString()
        {
            return string.Format("{0:X2} {1:X2} {2:X2} {3:X2} {4:X4}", c00, c01, c02, c03, c04);
        }
    }
}
