using ImGuiNET;
using static OpenKh.Tools.Common.CustomImGui.ImGuiEx;
using xna = Microsoft.Xna.Framework;
using OpenKh.Tools.Kh2MsetEditorCrazyEdition.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenKh.Tools.Kh2MsetEditorCrazyEdition.Helpers;

namespace OpenKh.Tools.Kh2MsetEditorCrazyEdition.Usecases.ImGuiWindows
{
    public class InitialPoseManagerWindowUsecase : IWindowRunnableProvider
    {
        private readonly LoadedModel _loadedModel;
        private readonly Settings _settings;

        public InitialPoseManagerWindowUsecase(
            Settings settings,
            LoadedModel loadedModel
        )
        {
            _loadedModel = loadedModel;
            _settings = settings;
        }

        public Action CreateWindowRunnable()
        {
            var age = _loadedModel.JointDescriptionsAge.Branch(false);
            var list = new List<string>();
            var selectedIndex = -1;

            return () =>
            {
                if (_settings.ViewInitialPose)
                {
                    ForWindow("InitialPose manager", () =>
                    {
                        if (age.NeedToCatchUp())
                        {
                            list.Clear();
                            list.AddRange(
                                _loadedModel.MotionData!.InitialPoses
                                    .Select(it => $"{it}")
                            );
                            selectedIndex = -1;
                        }

                        if (ImGui.BeginCombo("item", ""))
                        {
                            foreach (var (one, index) in list.SelectWithIndex())
                            {
                                if (ImGui.Selectable(one, selectedIndex == index))
                                {
                                    selectedIndex = index;
                                }
                            }
                            ImGui.EndCombo();
                        }
                    });
                }
            };
        }
    }
}
