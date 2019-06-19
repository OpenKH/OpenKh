using OpenKh.Kh2;
using System.Collections.Generic;

namespace OpenKh.Tools.LayoutViewer.Models
{
    public class LayoutEntryPropertyModel<T>
    {
        public string Name { get; set; }
        public T Value { get; set; }
    }

    public class LayoutEntryModel
    {
        public LayoutEntryPropertyModel<Layout> Layout { get; set; }
        public LayoutEntryPropertyModel<List<Imgd>> Images { get; set; }
    }
}
