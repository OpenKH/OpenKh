using System.Collections.Generic;

namespace OpenKh.Patcher.Kh2Ps2Patch.BlockSystem
{
    public record LengthBlock(int Length, string Tag = "") : IBlock
    {
        public List<IBlock> Children { get; set; } = new List<IBlock>();

        public int EnsuredOffset { get; set; }
    }
}
