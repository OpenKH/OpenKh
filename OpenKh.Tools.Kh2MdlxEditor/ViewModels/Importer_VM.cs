using OpenKh.Kh2.Models.VIF;
using OpenKh.Tools.Kh2MdlxEditor.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenKh.Tools.Kh2MdlxEditor.ViewModels
{
    public class Importer_VM
    {
        public Main2_VM MainVM { get; set; }
        public bool KeepShadow { get; set; }
        public byte VertexLimitPerPacket { get; set; }
        public byte MemoryLimitPerPacket { get; set; }

        public Importer_VM(Main2_VM mainVM)
        {
            this.MainVM = mainVM;
            KeepShadow = false;
            VertexLimitPerPacket = (byte)VifProcessor.VERTEX_LIMIT;
            MemoryLimitPerPacket = (byte)VifProcessor.MEMORY_LIMIT;
        }
    }
}
