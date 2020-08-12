using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;

namespace OpenKh.Tools.ImageViewer.Models
{
    public class ActionDef
    {
        public string Text { get; set; }
        public ICommand Command { get; set; }
    }
}
