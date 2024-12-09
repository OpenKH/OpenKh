using System.Collections.Generic;

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
