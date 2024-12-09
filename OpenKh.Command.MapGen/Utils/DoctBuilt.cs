using OpenKh.Command.MapGen.Models;
using OpenKh.Kh2;
using System.Collections.Generic;

namespace OpenKh.Command.MapGen.Utils
{
    public class DoctBuilt
    {
        public Doct Doct { get; set; }
        public List<SingleFace[]> VifPacketRenderingGroup { get; set; }
    }
}
