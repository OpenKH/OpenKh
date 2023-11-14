using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenKh.Tools.Kh2MsetMotionEditor.Helpers
{
    public class ImportResult
    {
        public List<Exception> Errors { get; set; } = new List<Exception>();
        public List<ImportSheetResult> Results { get; set; } = new List<ImportSheetResult>();
    }
}
