using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenKh.Command.AnbMaker.Utils.JsonAnimSource
{
    public class BAnimationAction
    {
        public string Name { get; set; }
        public float FrameStart { get; set; }
        public float FrameEnd { get; set; }
        public BActionGroup[] Groups { get; set; }
    }
}
