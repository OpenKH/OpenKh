using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenKh.Command.AnbMaker.Utils.Builder.Models
{
    public class AChannel
    {
        public AScalarKey[] PositionXKeys { get; set; }
        public AScalarKey[] PositionYKeys { get; set; }
        public AScalarKey[] PositionZKeys { get; set; }

        public AScalarKey[] RotationXKeys { get; set; }
        public AScalarKey[] RotationYKeys { get; set; }
        public AScalarKey[] RotationZKeys { get; set; }

        public AScalarKey[] ScaleXKeys { get; set; }
        public AScalarKey[] ScaleYKeys { get; set; }
        public AScalarKey[] ScaleZKeys { get; set; }
    }
}
