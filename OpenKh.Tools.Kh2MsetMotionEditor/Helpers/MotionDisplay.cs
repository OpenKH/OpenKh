using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenKh.Tools.Kh2MsetMotionEditor.Helpers
{
    public record MotionDisplay(string Label, bool Valid, IEnumerable<string> BoneViewMatcher)
    {
    }
}
