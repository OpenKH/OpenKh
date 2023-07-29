using ImGuiNET;
using OpenKh.Tools.Kh2MsetEditorCrazyEdition.Helpers;
using OpenKh.Tools.Kh2MsetEditorCrazyEdition.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using static OpenKh.Tools.Common.CustomImGui.ImGuiEx;

namespace OpenKh.Tools.Kh2MsetEditorCrazyEdition.Usecases.InsideTools.Old
{
    public class MotionLoaderToolUsecase : IToolRunnableProvider
    {
        private readonly LoadedModel _loadedModel;

        public MotionLoaderToolUsecase(
            LoadedModel loadedModel
        )
        {
            _loadedModel = loadedModel;
        }

        public Action CreateToolRunnable()
        {
            return () =>
            {
                ForControl(
                    () =>
                    {
                        var nextPos = ImGui.GetCursorPos();
                        var ret = ImGui.Begin("MotionList",
                            ImGuiWindowFlags.NoDecoration |
                            ImGuiWindowFlags.NoCollapse |
                            ImGuiWindowFlags.NoMove);
                        ImGui.SetWindowPos(nextPos);
                        ImGui.SetWindowSize(new Vector2(80, 0));
                        return ret;
                    },
                    () => { },
                    () =>
                    {
                        foreach (var (display, index) in _loadedModel.MotionList.SelectWithIndex())
                        {
                            if (ImGui.Selectable(display.Label, _loadedModel.SelectedMotionIndex == index))
                            {
                                _loadedModel.SelectedMotionIndex = index;
                            }
                        }

                        if (!_loadedModel.MotionList.Any())
                        {
                            ImGui.Text("This\nis\na\nmotion\nlist");
                        }
                    }
                );

                ImGui.SameLine();
            };
        }
    }
}
