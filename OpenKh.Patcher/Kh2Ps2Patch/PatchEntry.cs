using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenKh.Patcher.Kh2Ps2Patch
{
    public class PatchEntry
    {
        public uint Hash { get; set; }
        public uint CompressedSize { get; set; }
        public uint UncompressedSize { get; set; }
        public uint Parent { get; set; }
        public uint Relink { get; set; }
        public bool IsCompressed { get; set; }
        public bool IsCustomFile { get; set; }
        public Memory<byte> RawContent { get; set; }
    }
}
