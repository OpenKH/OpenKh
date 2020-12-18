using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Media;

namespace OpenKh.Research.Kh2AnimIKC.Utils
{
    // From: https://stackoverflow.com/a/36126856
    public class VisualHost : UIElement
    {
        public Visual Visual { get; set; }

        protected override int VisualChildrenCount
        {
            get { return Visual != null ? 1 : 0; }
        }

        protected override Visual GetVisualChild(int index)
        {
            return Visual;
        }
    }
}
