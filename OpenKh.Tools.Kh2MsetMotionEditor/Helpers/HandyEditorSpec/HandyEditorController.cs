using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenKh.Tools.Kh2MsetMotionEditor.Helpers.HandyEditorSpec
{
    public record HandyEditorController(Action Load, Action Render, Action Save)
    {
    }
}
