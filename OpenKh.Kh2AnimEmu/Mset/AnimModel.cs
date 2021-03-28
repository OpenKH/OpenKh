using OpenKh.Kh2Anim.Legacy;
using System;
using System.Collections.Generic;
using System.Text;

namespace OpenKh.Kh2Anim.Mset
{
    public class AnimModel
    {
        public List<T1> t1List = new List<T1>();
        public List<T2> t2List = new List<T2>();
        public List<T2> t2xList = new List<T2>();
        public List<T3> t3List = new List<T3>();
        public List<T4> t4List = new List<T4>();
        public List<AxBone> t5List = new List<AxBone>();
        public List<T9> t9List = new List<T9>();
        public float[] t10List = null;
        public float[] t11List = null;
        public float[] t12List = null;

        public int off5 = 0, cnt5 = 0;
    }
}
