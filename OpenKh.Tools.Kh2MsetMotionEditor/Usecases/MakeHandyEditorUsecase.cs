using ImGuiNET;
using OpenKh.Tools.Kh2MsetMotionEditor.Helpers.HandyEditorSpec;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace OpenKh.Tools.Kh2MsetMotionEditor.Usecases
{
    public class MakeHandyEditorUsecase
    {
        public HandyEditorController InputInt(string label, Func<int> onLoad, Action<int> onSave)
        {
            int value = default;

            return new HandyEditorController(
                () => value = onLoad(),
                () =>
                {
                    ImGui.InputInt(label, ref value);
                },
                () => onSave(value)
            );
        }

        public HandyEditorController InputFloat(string label, Func<float> onLoad, Action<float> onSave)
        {
            float value = default;

            return new HandyEditorController(
                () => value = onLoad(),
                () =>
                {
                    ImGui.InputFloat(label, ref value);
                },
                () => onSave(value)
            );
        }

        public HandyEditorController ForEdit3(string label, Func<Vector3> onLoad, Action<Vector3> onSave, float speed = 1f)
        {
            Vector3 value = default;

            return new HandyEditorController(
                () => value = onLoad(),
                () =>
                {
                    ImGui.DragFloat3(label, ref value, speed);
                },
                () => onSave(value)
            );
        }

        public HandyEditorController Checkbox(string label, Func<bool> onLoad, Action<bool> onSave)
        {
            bool value = default;

            return new HandyEditorController(
                () => value = onLoad(),
                () =>
                {
                    ImGui.Checkbox(label, ref value);
                },
                () => onSave(value)
            );
        }
    }
}
