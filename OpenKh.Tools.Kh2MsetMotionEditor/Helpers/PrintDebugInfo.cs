using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenKh.Tools.Kh2MsetMotionEditor.Helpers
{
    public class PrintDebugInfo
    {
        public IDictionary<string, Action> Printers { get; set; } = new SortedDictionary<string, Action>();
    }
}
