using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenKh.Command.AnbMaker.Utils.JsonAnimSource
{
    public class BBone
    {
        public string Name { get; set; }
        public int Parent { get; set; }
        public float[] MatrixLocal { get; set; }
        public float[] HeadLocal { get; set; }
    }
}
