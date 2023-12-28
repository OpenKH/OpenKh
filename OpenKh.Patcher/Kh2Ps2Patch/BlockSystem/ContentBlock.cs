using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenKh.Patcher.Kh2Ps2Patch.BlockSystem
{
    public record ContentBlock(ReadOnlyMemory<byte> Content, string Tag = "") : IBlock
    {
        public int Length => Content.Length;

        public List<IBlock> Children { get; set; } = new List<IBlock>();

        public int EnsuredOffset { get; set; }

        public void TransferTo(Memory<byte> destination)
        {
            Content.CopyTo(this.SliceBuffer(destination));
        }
    }
}
