using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenKh.Tools.Kh2SoraikoTools
{
    public class MDLX:Model
    {
        public MDLX(string filename):base (filename)
        {
            if (this.RootCopy != null)
                return;
            
        }
    }
}
