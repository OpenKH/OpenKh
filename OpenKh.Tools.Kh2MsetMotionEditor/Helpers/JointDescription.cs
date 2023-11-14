using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenKh.Tools.Kh2MsetMotionEditor.Helpers
{
    /// <param name="Index">Absolute index of bone. FK is always zero based, IK starts after FK. for example, IK of P_EX100 starts from 228</param>
    /// <param name="Depth">Number of parent bones. 0 for root</param>
    public record JointDescription(int Index, int Depth)
    {
    }
}
