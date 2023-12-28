using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenKh.Patcher.Kh2Ps2Patch
{
    public class PatchHeader
    {
        public uint Revision { get; set; }
        public string Author { get; set; } = "";
        public string[] ChangeLogs { get; set; } = new string[0];
        public string[] Credits { get; set; } = new string[0];
        public string OtherInformation { get; set; } = "";

        public List<PatchEntry> PatchEntries { get; set; } = new List<PatchEntry>();
    }
}
