using ImGuiNET;
using OpenKh.Kh2;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OpenKh.Tools.LayoutEditor.Dialogs
{
    public class TargetSelectionDialog : IDisposable
    {
        private readonly Bar.Entry[] _animations;
        private int _selectedAnimIndex;

        public bool HasTargetBeenSelected { get; private set; }
        public Bar.Entry SelectedAnimation { get; private set; }

        public TargetSelectionDialog(
            IEnumerable<Bar.Entry> entries,
            Bar.EntryType animationType)
        {
            _animations = entries
                .Where(x => x.Type == animationType && x.Index == 0)
                .ToArray();
        }

        public void Run()
        {
            if (_animations.Length == 1)
            {
                HasTargetBeenSelected = true;
                SelectedAnimation = _animations[0];
                ImGui.CloseCurrentPopup();
            }

            ImGui.Text("The selected file contains multiple elements.");
            ImGui.Text("Please choose the appropiate sub-file you want to target.");

            ImGui.Columns(1, "Select Sub-File:", true);
            ImGui.Text("Animation");
            for (var i = 0; i < _animations.Length; i++)
            {
                if (ImGui.Selectable($"{_animations[i].Name}##anm",
                    _selectedAnimIndex == i,
                    ImGuiSelectableFlags.DontClosePopups))
                    _selectedAnimIndex = i;
            }

            ImGui.Columns(1);
            ImGui.Separator();

            if (ImGui.Button("Open"))
            {
                HasTargetBeenSelected = true;
                SelectedAnimation = _animations[_selectedAnimIndex];
                ImGui.CloseCurrentPopup();
            }
        }

        public void Dispose()
        {

        }
    }
}
