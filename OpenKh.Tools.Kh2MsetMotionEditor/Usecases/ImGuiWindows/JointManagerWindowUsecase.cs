using ImGuiNET;
using OpenKh.Kh2;
using OpenKh.Tools.Kh2MsetMotionEditor.Helpers;
using OpenKh.Tools.Kh2MsetMotionEditor.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using static OpenKh.Tools.Common.CustomImGui.ImGuiEx;

namespace OpenKh.Tools.Kh2MsetMotionEditor.Usecases.ImGuiWindows
{
    public class JointManagerWindowUsecase : IWindowRunnableProvider
    {
        private readonly FormatListItemUsecase _formatListItemUsecase;
        private readonly EditCollectionNoErrorUsecase _editCollectionNoErrorUsecase;
        private readonly ErrorMessages _errorMessages;
        private readonly LoadedModel _loadedModel;
        private readonly Settings _settings;

        public JointManagerWindowUsecase(
            Settings settings,
            LoadedModel loadedModel,
            ErrorMessages errorMessages,
            EditCollectionNoErrorUsecase editCollectionNoErrorUsecase,
            FormatListItemUsecase formatListItemUsecase
        )
        {
            _formatListItemUsecase = formatListItemUsecase;
            _editCollectionNoErrorUsecase = editCollectionNoErrorUsecase;
            _errorMessages = errorMessages;
            _loadedModel = loadedModel;
            _settings = settings;
        }

        public Action CreateWindowRunnable()
        {
            var age = _loadedModel.JointDescriptionsAge.Branch(false);
            var names = new List<string>();
            var selectedIndex = -1;
            var nextTimeRefresh = new OneTimeOn(false);

            return () =>
            {
                if (_settings.ViewJoint)
                {
                    var windowClosed = !ForWindow("Joint manager", () =>
                    {
                        var sourceList = _loadedModel.MotionData?.Joints;

                        var saved = false;

                        ForMenuBar(() =>
                        {
                            ForMenuItem("Insert", () =>
                            {
                                saved = _editCollectionNoErrorUsecase.InsertAt(sourceList, selectedIndex, new Motion.Joint());
                            });
                            ForMenuItem("Append", () =>
                            {
                                if (saved = _editCollectionNoErrorUsecase.Append(sourceList, new Motion.Joint()))
                                {
                                    selectedIndex = sourceList!.Count - 1;
                                }
                            });
                            ForMenuItem("Delete", () =>
                            {
                                saved = _editCollectionNoErrorUsecase.DeleteAt(sourceList, selectedIndex);
                            });
                        });

                        if (saved | nextTimeRefresh.Consume() | age.NeedToCatchUp())
                        {
                            try
                            {
                                names.Clear();
                                names.AddRangeIfNotNull(
                                    sourceList?
                                        .Select(_formatListItemUsecase.FormatJoint)
                                );
                            }
                            catch (Exception ex)
                            {
                                _errorMessages.Add(new Exception("Joint has error of ToString().", ex));
                            }
                        }

                        if (names.Any())
                        {
                            if (ImGui.DragInt("index (slider)", ref selectedIndex, 0.2f, 0, names.Count - 1))
                            {

                            }

                            if (ImGui.BeginCombo($"Joint", names.GetAtOrNull(selectedIndex) ?? "..."))
                            {
                                foreach (var (item, index) in names.SelectWithIndex())
                                {
                                    if (ImGui.Selectable(names[index], index == selectedIndex))
                                    {
                                        selectedIndex = index;
                                    }
                                }
                                ImGui.EndCombo();
                            }

                            if (sourceList?.GetAtOrNull(selectedIndex) is Motion.Joint joint)
                            {
                                ForEdit("JointId", () => joint.JointId, it => { joint.JointId = it; saved = true; });

                                ForEdit("IK", () => joint.IK, it => { joint.IK = it; saved = true; });
                                ForEdit("ExtEffector", () => joint.ExtEffector, it => { joint.ExtEffector = it; saved = true; });
                                ForEdit("CalcMatrix2Rot", () => joint.CalcMatrix2Rot, it => { joint.CalcMatrix2Rot = it; saved = true; });
                                ForEdit("Calculated", () => joint.Calculated, it => { joint.Calculated = it; saved = true; });
                                ForEdit("Fixed", () => joint.Fixed, it => { joint.Fixed = it; saved = true; });
                                ForEdit("Rotation", () => joint.Rotation, it => { joint.Rotation = it; saved = true; });
                                ForEdit("Trans", () => joint.Trans, it => { joint.Trans = it; saved = true; });

                                ForEdit("Reserved", () => joint.Reserved, it => { joint.Reserved = it; saved = true; });

                                ImGui.Text("Goto:");
                                ImGui.SameLine();
                                if (ImGui.Button("Joint##gotoJoint"))
                                {
                                    _loadedModel.SelectedJointIndex = joint.JointId;
                                    _loadedModel.SelectedJointIndexAge.Bump();
                                }
                            }
                            else
                            {
                                ImGui.Text("(Editor will appear after selection)");
                            }
                        }
                        else
                        {
                            ImGui.Text("(Collection is empty)");
                        }

                        if (saved)
                        {
                            nextTimeRefresh.TurnOn();
                            _loadedModel.SendBackMotionData.TurnOn();
                        }
                    },
                        menuBar: true
                    );

                    if (windowClosed)
                    {
                        _settings.ViewJoint = false;
                        _settings.Save();
                    }
                }
            };
        }
    }
}
