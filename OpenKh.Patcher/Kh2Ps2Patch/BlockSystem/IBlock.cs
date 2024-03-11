using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenKh.Patcher.Kh2Ps2Patch.BlockSystem
{
    public interface IBlock
    {
        int EnsuredOffset { get; set; }
        int Length { get; }
        List<IBlock> Children { get; }
        string Tag { get; }
    }
}
