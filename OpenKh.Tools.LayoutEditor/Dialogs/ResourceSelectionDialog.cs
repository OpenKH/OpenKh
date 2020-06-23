using ImGuiNET;
using OpenKh.Kh2;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OpenKh.Tools.LayoutEditor.Dialogs
{
    public class ResourceSelectionDialog : IDisposable
    {
        private readonly Bar.Entry[] _animations;
        private readonly Bar.Entry[] _textures;
        private int _selectedAnimIndex;
        private int _selectedTextureIndex;

        public bool HasResourceBeenSelected { get; private set; }
        public Bar.Entry SelectedAnimation{ get; private set; }
        public Bar.Entry SelectedTexture { get; private set; }

        public ResourceSelectionDialog(
            IEnumerable<Bar.Entry> entries,
            Bar.EntryType animationType,
            Bar.EntryType textureType)
        {
            _animations = entries
                .Where(x => x.Type == animationType && x.Index == 0)
                .ToArray();
            _textures = entries
                .Where(x => x.Type == textureType && x.Index == 0)
                .ToArray();
        }

        public void Run()
        {
            ImGui.Text("The selected file contains multiple elements.");
            ImGui.Text("Please choose the appropiate sub-files you want to load.");

            ImGui.Columns(2, "resources", true);
            ImGui.Text("Animation");
            for (var i = 0; i < _animations.Length; i++)
            {
                if (ImGui.Selectable($"{_animations[i].Name}##anm",
                    _selectedAnimIndex == i,
                    ImGuiSelectableFlags.DontClosePopups))
                    _selectedAnimIndex = i;
            }

            ImGui.NextColumn();
            ImGui.Text("Texture");
            for (var i = 0; i < _textures.Length; i++)
            {
                if (ImGui.Selectable($"{_textures[i].Name}##tex",
                    _selectedTextureIndex == i,
                    ImGuiSelectableFlags.DontClosePopups))
                    _selectedTextureIndex = i;
            }

            ImGui.Columns(1);
            ImGui.Separator();
            if (ImGui.Button("Open"))
            {
                HasResourceBeenSelected = true;
                SelectedAnimation = _animations[_selectedAnimIndex];
                SelectedTexture = _textures[_selectedTextureIndex];
                ImGui.CloseCurrentPopup();
            }
        }

        public void Dispose()
        {

        }
    }
}
