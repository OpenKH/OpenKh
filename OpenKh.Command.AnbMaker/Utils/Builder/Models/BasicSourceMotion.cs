using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenKh.Command.AnbMaker.Utils.Builder.Models
{
    public class BasicSourceMotion
    {
        public int DurationInTicks { get; set; }
        public float TicksPerSecond { get; set; }
        public int BoneCount { get; set; }
        public float NodeScaling { get; set; }
        public float PositionScaling { get; set; }
        public Func<int, AChannel> GetAChannel { get; set; }
        public ABone[] Bones { get; set; }
        public GetInitialMatrixDelegate GetInitialMatrix { get; set; }
    }
}
