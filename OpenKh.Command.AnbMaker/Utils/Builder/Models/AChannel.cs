using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenKh.Command.AnbMaker.Utils.Builder.Models
{
    public class AChannel
    {
        public int ScalingKeyCount { get; set; }
        public AVectorKey[] ScalingKeys { get; set; }
        public int RotationKeyCount { get; set; }
        public AQuaternionKey[] RotationKeys { get; set; }
        public int PositionKeyCount { get; internal set; }
        public AVectorKey[] PositionKeys { get; set; }
    }
}
