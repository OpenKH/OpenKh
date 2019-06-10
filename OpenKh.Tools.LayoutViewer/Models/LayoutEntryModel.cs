using OpenKh.Kh2;
using System.Collections.Generic;

namespace OpenKh.Tools.LayoutViewer.Models
{
    public class LayoutEntryModel
    {
        public string Name { get; set; }
        public Layout Layout { get; set; }
        public List<Imgd> Images { get; set; }
    }
}
