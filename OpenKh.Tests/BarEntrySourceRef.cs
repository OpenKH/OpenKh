using OpenKh.Common;
using OpenKh.Kh2;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace OpenKh.Tests
{
    internal record BarEntrySourceRef(
        string BarFile,
        Bar.Entry Entry,
        int Index
    )
    {
        public BarEntrySource GetSource() => new BarEntrySource($"{BarFile}\t#{Index}");

        public static IEnumerable<BarEntrySourceRef> FromFile(string barFile, string rootDir = null)
        {
            var barSeparatableFile = (rootDir != null && barFile.StartsWith(rootDir))
                ? rootDir.TrimEnd('\\') + "\\.\\" + barFile.Substring(rootDir.Length).TrimStart('\\')
                : barFile;
            return File.OpenRead(barFile)
                .Using(fs => Bar.Read(fs))
                .Select((barEntry, index) => new BarEntrySourceRef(barSeparatableFile, barEntry, index));
        }
    }
}
