using OpenKh.Kh2;
using System.Collections.Generic;

namespace OpenKh.Tools.LayoutViewer.Interfaces
{
    public interface ISaveBar
    {
        IEnumerable<Bar.Entry> Save(IEnumerable<Bar.Entry> barEntries);
    }
}
