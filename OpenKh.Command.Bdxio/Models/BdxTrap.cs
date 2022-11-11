using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenKh.Command.Bdxio.Models
{
    public class BdxTrap
    {
        public int TableIndex { get; set; }
        public int TrapIndex { get; set; }
        public string Name { get; set; }
        public int Flags { get; set; }
    }
}
