using ImGuiNET;
using OpenKh.Tools.Kh2MsetEditorCrazyEdition.Helpers;
using OpenKh.Tools.Kh2MsetEditorCrazyEdition.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using static OpenKh.Tools.Common.CustomImGui.ImGuiEx;

namespace OpenKh.Tools.Kh2MsetEditorCrazyEdition.Usecases.ImGuiWindows
{
    public class BonesManagerWindowUsecase : IWindowRunnableProvider
    {
        private readonly LoadedModel _loadedModel;
        private readonly Settings _settings;

        public BonesManagerWindowUsecase(
            Settings settings,
            LoadedModel loadedModel
        )
        {
            _loadedModel = loadedModel;
            _settings = settings;
        }

        private record JointDef(int AbsIndex, string Display)
        {

        }

        public Action CreateWindowRunnable()
        {
            var jointAge = _loadedModel.JointDescriptionsAge.Branch().MarkDirty();
            var jointDefs = new List<JointDef>();
            var configAge = _loadedModel.Kh2PresetsAge.Branch().MarkDirty();
            var scrollAge = _loadedModel.SelectedJointIndexAge.Branch(false);

            return () =>
            {
                if (_settings.ViewBones)
                {
                    var windowClosed = !ForWindow("Bones manager", () =>
                    {
                        {
                            var state = _settings.ViewFkBones;
                            if (ImGui.Checkbox("showFk", ref state))
                            {
                                _settings.ViewFkBones = state;
                                _settings.Save();
                            }
                        }

                        if (jointAge.NeedToCatchUpAnyOf(configAge))
                        {
                            var fkView = _loadedModel.GetActiveFkBoneViews?.Invoke();
                            var ikView = _loadedModel.GetActiveIkBoneViews?.Invoke();

                            string FindFkJointName(int index)
                            {
                                return fkView?
                                    .LastOrDefault(
                                        it => it.I == index
                                    )?
                                    .Name ?? "";
                            }

                            string FindIkJointName(int absIndex)
                            {
                                return ikView?
                                    .LastOrDefault(
                                        it => it.I == absIndex
                                    )?
                                    .Name ?? "";
                            }

                            jointDefs.Clear();
                            jointDefs.AddRange(
                                _loadedModel.FKJointDescriptions
                                    .Select(
                                        (joint, index) => new JointDef(
                                            index,
                                            $"FK<{joint.Index}> {new string('.', joint.Depth)} {FindFkJointName(index)}"
                                        )
                                    )
                            );
                            jointDefs.AddRange(
                                _loadedModel.IKJointDescriptions
                                    .Select(
                                        (joint, index) => new JointDef(
                                            joint.Index,
                                            $"IK<{joint.Index}> {new string('.', joint.Depth)} {FindIkJointName(joint.Index)}"
                                        )
                                    )
                            );
                        }

                        var needToScroll = scrollAge.NeedToCatchUp();

                        if (ImGui.BeginChild("bonesList"))
                        {
                            foreach (var jointDef in jointDefs)
                            {
                                var isSelected = _loadedModel.SelectedJointIndex == jointDef.AbsIndex;

                                if (ImGui.Selectable(jointDef.Display, isSelected))
                                {
                                    _loadedModel.SelectedJointIndex = jointDef.AbsIndex;
                                    _loadedModel.SelectedJointIndexAge.Bump();
                                    scrollAge.CatchUp();
                                }

                                if (isSelected && needToScroll)
                                {
                                    ImGui.SetScrollHereY();
                                }
                            }

                            ImGui.EndChild();
                        }
                    });

                    if (windowClosed)
                    {
                        _settings.ViewBones = false;
                        _settings.Save();
                    }
                }
            };
        }
    }
}
