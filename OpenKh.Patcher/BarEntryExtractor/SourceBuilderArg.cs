using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenKh.Patcher.BarEntryExtractor
{
    public record SourceBuilderArg(
        string DestName, 
        string DestType, 
        string SourceName, 
        string OriginalRelativePath
    );
}
