using System;
using System.Collections.Generic;

namespace OpenKh.Tools.Kh2MsetMotionEditor.Helpers
{
    public class ImportResult
    {
        public List<Exception> Errors { get; set; } = new List<Exception>();
        public List<ImportSheetResult> Results { get; set; } = new List<ImportSheetResult>();
    }
}
