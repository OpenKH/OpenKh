using OpenKh.Common;
using OpenKh.Kh2;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace OpenKh.Tools.Kh2SystemEditor.Extensions
{
    public static class BarExtensions
    {
        public static Stream GetBinaryStream(this IEnumerable<Bar.Entry> entries, string name) =>
            // TODO throws exception if that entry is not found
            entries.First(x => x.Name == name && x.Index == 0 && x.Type == Bar.EntryType.List).Stream.SetPosition(0);
    }
}
