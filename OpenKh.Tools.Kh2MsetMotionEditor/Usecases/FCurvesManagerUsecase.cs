using ImGuiNET;
using OpenKh.Kh2;
using OpenKh.Tools.Kh2MsetMotionEditor.Helpers;
using OpenKh.Tools.Kh2MsetMotionEditor.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using static OpenKh.Tools.Common.CustomImGui.ImGuiEx;

namespace OpenKh.Tools.Kh2MsetMotionEditor.Usecases
{
    public class FCurvesManagerUsecase
    {
        private readonly FormatListItemUsecase _formatListItemUsecase;
        private readonly EditCollectionNoErrorUsecase _editCollectionNoErrorUsecase;
        private readonly ErrorMessages _errorMessages;
        private readonly LoadedModel _loadedModel;
        private readonly Settings _settings;
        private readonly string[] _channelValues;
        private readonly string[] _cycleTypes;

        public FCurvesManagerUsecase(
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
            _channelValues = Enumerable.Range(0, 9)
                .Select(it => ((Motion.Channel)it).ToString())
                .ToArray();
            _cycleTypes = Enumerable.Range(0, 4)
                .Select(it => ((Motion.CycleType)it).ToString())
                .ToArray();
        }

        public Action CreateWindowRunnable(bool forward)
        {
            var age = _loadedModel.JointDescriptionsAge.Branch(false);
            var names = new List<string>();
            var selectedIndex = -1;
            var nextTimeRefresh = new OneTimeOn(false);

            return () =>
            {
                if (forward)
                {
                    if (0 <= _loadedModel.SelectFCurvesFoward)
                    {
                        selectedIndex = _loadedModel.SelectFCurvesFoward;

                        _settings.ViewFCurvesForward = true;

                        _loadedModel.SelectFCurvesFoward = -1;
                    }
                }
                else
                {
                    if (0 <= _loadedModel.SelectFCurvesInverse)
                    {
                        selectedIndex = _loadedModel.SelectFCurvesInverse;

                        _settings.ViewFCurvesInverse = true;

                        _loadedModel.SelectFCurvesInverse = -1;
                    }
                }

                if (forward ? _settings.ViewFCurvesForward : _settings.ViewFCurvesInverse)
                {
                    var windowClosed = !ForWindow(forward ? "FCurvesForward manager" : "FCurvesInverse manager", () =>
                    {
                        var sourceList = forward
                            ? _loadedModel.MotionData?.FCurvesForward
                            : _loadedModel.MotionData?.FCurvesInverse;

                        var saved = false;

                        ForMenuBar(() =>
                        {
                            ForMenuItem("Insert", () =>
                            {
                                saved = _editCollectionNoErrorUsecase.InsertAt(sourceList, selectedIndex, new Motion.FCurve());
                            });
                            ForMenuItem("Append", () =>
                            {
                                if (saved = _editCollectionNoErrorUsecase.Append(sourceList, new Motion.FCurve()))
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
                                        .Select(_formatListItemUsecase.FormatFCurve)
                                );
                            }
                            catch (Exception ex)
                            {
                                _errorMessages.Add(new Exception("FCurve has error of ToString().", ex));
                            }
                        }

                        if (names.Any())
                        {
                            if (ImGui.DragInt("index (slider)", ref selectedIndex, 0.2f, 0, names.Count - 1))
                            {

                            }

                            if (ImGui.BeginCombo($"FCurve", names.GetAtOrNull(selectedIndex) ?? "..."))
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

                            if (sourceList?.GetAtOrNull(selectedIndex) is Motion.FCurve joint)
                            {
                                ForEdit("JointId", () => joint.JointId, it => { joint.JointId = it; saved = true; });

                                ForCombo("ChannelValue", _channelValues, () => (int)joint.ChannelValue, it => { joint.ChannelValue = (Motion.Channel)(it); saved = true; });
                                ForCombo("Pre", _cycleTypes, () => (int)joint.Pre, it => { joint.Pre = (Motion.CycleType)it; saved = true; });
                                ForCombo("Post", _cycleTypes, () => (int)joint.Post, it => { joint.Post = (Motion.CycleType)it; saved = true; });

                                ForEdit("KeyStartId", () => joint.KeyStartId, it => { joint.KeyStartId = it; saved = true; });
                                ForEdit("KeyCount", () => joint.KeyCount, it => { joint.KeyCount = it; saved = true; });

                                ImGui.Text("Goto:");
                                ImGui.SameLine();
                                if (ImGui.Button("Joint##gotoJoint"))
                                {
                                    _loadedModel.SelectedJointIndex = forward
                                        ? joint.JointId
                                        : _loadedModel.FKJointDescriptions.Count + joint.JointId;
                                    _loadedModel.SelectedJointIndexAge.Bump();
                                }
                                ImGui.SameLine();
                                if (ImGui.Button("FCurveKey##goto"))
                                {
                                    _loadedModel.SelectFCurveKey = joint.KeyStartId;
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
                        if (forward)
                        {
                            _settings.ViewFCurvesForward = false;
                        }
                        else
                        {
                            _settings.ViewFCurvesInverse = false;
                        }

                        _settings.Save();
                    }
                }
            };
        }
    }
}
