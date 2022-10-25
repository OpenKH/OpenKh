using ImGuiNET;
using OpenKh.Tools.Kh2MapStudio.Models;
using System.Numerics;
using System.Windows;
using static OpenKh.Tools.Common.CustomImGui.ImGuiEx;

namespace OpenKh.Tools.Kh2MapStudio.Windows
{
    static class SpawnScriptWindow
    {
        private static readonly Vector4 ErrorColor = new Vector4(1.0f, 0.0f, 0.0f, 1.0f);
        private static readonly Vector4 SuccessColor = new Vector4(0.0f, 1.0f, 0.0f, 1.0f);

        public static bool Run(string type, SpawnScriptModel model) => ForHeader($"Event Script compiler for {type}", () =>
        {
            if (model == null)
            {
                ImGui.Text($"Unable to find for '{type}'.");
                return;
            }

            if (ImGui.Button($"Compile##{type}"))
                model.Compile();

            ImGui.SameLine();

            if (ImGui.Button($"Copy all##copyall {type}"))
                model.CopyAll();

            ImGui.SameLine();

            if (ImGui.Button($"Paste and replace##pastereplace {type}"))
                model.PasteReplace();

            if (!string.IsNullOrEmpty(model.LastError))
                ImGui.TextColored(model.IsError ? ErrorColor : SuccessColor, model.LastError);

            var code = model.Decompiled;
            if (ImGui.InputTextMultiline($"code##{type}", ref code, 0x100000, new Vector2(0, 0), ImGuiInputTextFlags.AllowTabInput))
                model.Decompiled = code;
        });
    }
}
