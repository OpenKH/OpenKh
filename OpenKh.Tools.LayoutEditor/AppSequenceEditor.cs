using ImGuiNET;
using OpenKh.Kh2;
using OpenKh.Tools.LayoutEditor.Interfaces;
using System.Numerics;
using static OpenKh.Tools.Common.CustomImGui.ImGuiEx;

namespace OpenKh.Tools.LayoutEditor
{
    public class AppSequenceEditor : IApp
    {
        private readonly Sequence _sequence;
        private readonly Imgd _image;

        private int _selectedSprite = 0;

        public AppSequenceEditor(Sequence sequence, Imgd image)
        {
            _sequence = sequence;
            _image = image;
        }

        public bool Run()
        {
            ForGrid(
                new GridElement("SpriteList", 1, 256, true, DrawSpriteList),
                new GridElement("Animation", 1, 0, false, DrawAnimation),
                new GridElement("Right", 1, 256, true, DrawRight));

            return true;
        }

        private void DrawSpriteList()
        {
            for (int i = 0; i < _sequence.Frames.Count; i++)
            {
                if (ImGui.Selectable($"##sprite{i}", _selectedSprite == i,
                    ImGuiSelectableFlags.AllowItemOverlap,
                    new Vector2(0, 40)))
                    _selectedSprite = i;

                ImGui.SameLine();
                ImGui.Text("Some sprite");
            }
        }

        private void DrawAnimation()
        {
            ImGui.Text("Animation");
        }

        private void DrawRight()
        {
            ImGui.Text("Right");
            ImGui.Text("Right");
            ImGui.Text("Right");
            ImGui.Text("Right");
            ImGui.Text("Right");
            ImGui.Text("Right");
            ImGui.Text("Right");
        }
    }
}
