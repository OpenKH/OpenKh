using System;

namespace OpenKh.Tools.Kh2MsetMotionEditor.Helpers.HandyEditorSpec
{
    public record HandyEditorController(Action Load, Action Render, Action Save)
    {
    }
}
