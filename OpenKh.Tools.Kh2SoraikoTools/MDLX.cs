using SrkAlternatives;
using System;
using System.Collections.Generic;
using System.IO;
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
            FileStream fs = new FileStream(filename, FileMode.Open);
            Mdlx m = new Mdlx(fs);

            for (int i=0;i<m.models[0].Meshes.Count;i++)
            {

            }

            fs.Close();
        }
    }
}
