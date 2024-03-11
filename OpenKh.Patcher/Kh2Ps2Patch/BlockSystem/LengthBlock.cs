using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenKh.Patcher.Kh2Ps2Patch.BlockSystem
{
    public record LengthBlock(int Length, string Tag = "") : IBlock
    {
        public List<IBlock> Children { get; set; } = new List<IBlock>();

        public int EnsuredOffset { get; set; }
    }
}
