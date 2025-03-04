using OpenKh.Kh2;
using System.Collections.Generic;
using System.Linq;

namespace OpenKh.Command.TexFooter.Utils
{
    internal static class DecisionHelper
    {
        internal static bool IsModelFile(List<Bar.Entry> barEntries)
        {
            var model = barEntries.First(it => it.Type == Bar.EntryType.Model);
            return !Mdlx.Read(model.Stream).IsMap;
        }
    }
}
