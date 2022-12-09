using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenKh.Command.AnbMaker.Utils.JsonAnimSource
{
    public class BActionGroup
    {
        public string Name { get; set; }
        public BFCurve[] Channels { get; set; }
    }
}
