using System.Collections.Generic;
using System.IO;
using System.Linq;
using OpenKh.Kh2;

namespace OpenKh.Tools.Kh2BattleEditor.Extensions
{
    public static class BarExtensions
    {
        public static Stream GetBattleStream(this IEnumerable<Bar.Entry> entries, string name) =>
            // TODO throws exception if that entry is not found
            entries.First(x => x.Name == name && x.Index == 0 && x.Type == Bar.EntryType.List).Stream;
    }
}
