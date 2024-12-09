using System;
using System.Collections.Generic;

namespace OpenKh.Tools.Kh2MsetMotionEditor.Helpers
{
    public class PrintDebugInfo
    {
        public IDictionary<string, Action> Printers { get; set; } = new SortedDictionary<string, Action>();
    }
}
