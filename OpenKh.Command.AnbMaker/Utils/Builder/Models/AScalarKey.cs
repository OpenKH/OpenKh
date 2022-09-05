using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace OpenKh.Command.AnbMaker.Utils.Builder.Models
{
    public class AScalarKey
    {
        public float Time { get; set; }
        public float Value { get; set; }

        public override string ToString() => $"{Time}, {Value}";
    }
}
