using System.Collections.Generic;

namespace OpenKh.Tools.Kh2MsetMotionEditor.Helpers
{
    public record MotionDisplay(string Label, bool Valid, IEnumerable<string> BoneViewMatcher)
    {
    }
}
