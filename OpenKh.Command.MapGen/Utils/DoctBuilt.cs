using OpenKh.Command.MapGen.Models;
using OpenKh.Kh2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenKh.Command.MapGen.Utils
{
    public class DoctBuilt
    {
        public Doct Doct { get; set; }
        public List<SingleFace[]> VifPacketRenderingGroup { get; set; }
    }
}
