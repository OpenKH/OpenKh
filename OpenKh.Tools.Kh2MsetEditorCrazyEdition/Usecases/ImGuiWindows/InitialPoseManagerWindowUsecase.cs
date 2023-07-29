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
using OpenKh.Tools.Kh2MsetEditorCrazyEdition.Helpers.HandyEditorSpec;
using OpenKh.Kh2;

namespace OpenKh.Tools.Kh2MsetEditorCrazyEdition.Usecases.ImGuiWindows
{
    public class InitialPoseManagerWindowUsecase : IWindowRunnableProvider
    {
        private readonly MakeHandyEditorUsecase _makeHandyEditorUsecase;
        private readonly LoadedModel _loadedModel;
        private readonly Settings _settings;

        public InitialPoseManagerWindowUsecase(
            Settings settings,
            LoadedModel loadedModel,
            MakeHandyEditorUsecase makeHandyEditorUsecase
        )
        {
            _makeHandyEditorUsecase = makeHandyEditorUsecase;
            _loadedModel = loadedModel;
            _settings = settings;
        }

        public Action CreateWindowRunnable()
        {
            var age = _loadedModel.JointDescriptionsAge.Branch(false);
            var list = new List<string>();
            var selectedIndex = -1;

            var editors = new List<HandyEditorController>();
            Motion.InitialPose? pose = null;
            editors.Add(_makeHandyEditorUsecase.InputInt("BoneId", () => pose!.BoneId, it => pose!.BoneId = (short)it));
            editors.Add(_makeHandyEditorUsecase.InputInt("Channel", () => pose!.Channel, it => pose!.Channel = (short)it));
            editors.Add(_makeHandyEditorUsecase.InputFloat("Value", () => pose!.Value, it => pose!.Value = it));

            return () =>
            {
                if (_settings.ViewInitialPose)
                {
                    ForWindow("InitialPose manager", () =>
                    {
                        var refresh = false;

                        ForMenuBar(() =>
                        {
                            ForMenuItem("Apply", () =>
                            {
                                if (selectedIndex != -1)
                                {
                                    pose = _loadedModel.MotionData!.InitialPoses[selectedIndex];
                                    editors.SaveAll();
                                    refresh = true;

                                    _loadedModel.SendBackMotionData.TurnOn();
                                }
                            });
                        });

                        if (refresh || age.NeedToCatchUp())
                        {
                            list.Clear();
                            list.AddRange(
                                _loadedModel.MotionData!.InitialPoses
                                    .Select(it => $"{it}")
                            );

                            if (!refresh)
                            {
                                selectedIndex = -1;
                            }
                        }

                        if (ImGui.BeginCombo("", (selectedIndex == -1) ? "..." : list[selectedIndex]))
                        {
                            foreach (var (one, index) in list.SelectWithIndex())
                            {
                                if (ImGui.Selectable(one, selectedIndex == index))
                                {
                                    selectedIndex = index;

                                    pose = _loadedModel.MotionData!.InitialPoses[selectedIndex];
                                    editors.LoadAll();
                                }
                            }
                            ImGui.EndCombo();
                        }

                        editors.RenderAll();
                    },
                        menuBar: true
                    );
                }
            };
        }
    }
}
