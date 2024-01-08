using OpenKh.Kh2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenKh.Patcher.BarEntryExtractor
{
    public record Extractor(
        IEnumerable<string> Tags,
        IfApplyToBarEntryDelegate IfApply,
        Func<string, bool> SourceFileTest,
        string FileExtension,
        Func<Bar.Entry, Task<byte[]>> ExtractAsync,
        Func<SourceBuilderArg, IEnumerable<AssetFile>> SourceBuilder = null
    );
}
