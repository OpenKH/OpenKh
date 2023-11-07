using ImGuiNET;
using OpenKh.Kh2;
using OpenKh.Tools.Kh2MsetMotionEditor.Helpers;
using OpenKh.Tools.Kh2MsetMotionEditor.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using static OpenKh.Tools.Common.CustomImGui.ImGuiEx;

namespace OpenKh.Tools.Kh2MsetMotionEditor.Usecases.ImGuiWindows
{
    public class ConstraintManagerWindowUsecase : IWindowRunnableProvider
    {
        private readonly FormatListItemUsecase _formatListItemUsecase;
        private readonly EditCollectionNoErrorUsecase _editCollectionNoErrorUsecase;
        private readonly ErrorMessages _errorMessages;
        private readonly LoadedModel _loadedModel;
        private readonly Settings _settings;
        private readonly string[] _constraintTypes;
        private readonly string[] _limiterTypes;

        public ConstraintManagerWindowUsecase(
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
            _constraintTypes = Enumerable.Range(0, 14)
                .Select(index => ((Motion.ConstraintType)index).ToString())
                .ToArray();
            _limiterTypes = "None,Rot,Sphere,Box".Split(',');
        }

        public Action CreateWindowRunnable()
        {
            var age = _loadedModel.JointDescriptionsAge.Branch(false);
            var names = new List<string>();
            var activationNames = new List<string>();
            var limiterNames = new List<string>();
            var selectedIndex = -1;
            var nextTimeRefresh = new OneTimeOn(false);
            var activationSelectedIndex = -1;
            var limiterSelectedIndex = -1;

            return () =>
            {
                if (_settings.ViewConstraint)
                {
                    var windowClosed = !ForWindow("Constraint manager", () =>
                    {
                        var sourceList = _loadedModel.MotionData?.Constraints;
                        var activationList = _loadedModel.MotionData?.ConstraintActivations;
                        var limiterList = _loadedModel.MotionData?.Limiters;

                        var saved = false;

                        ForMenuBar(() =>
                        {
                            ForMenuItem("Insert", () =>
                            {
                                saved = _editCollectionNoErrorUsecase.InsertAt(sourceList, selectedIndex, new Motion.Constraint());
                            });
                            ForMenuItem("Append", () =>
                            {
                                if (saved = _editCollectionNoErrorUsecase.Append(sourceList, new Motion.Constraint()))
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
                                        .Select(it => _formatListItemUsecase.FormatConstraint(it))
                                );

                                activationNames.Clear();
                                activationNames.AddRangeIfNotNull(
                                    activationList?
                                        .Select(it => _formatListItemUsecase.FormatConstraintActivation(it))
                                );

                                limiterNames.Clear();
                                limiterNames.AddRangeIfNotNull(
                                    limiterList?
                                        .Select(it => _formatListItemUsecase.FormatLimiter(it))
                                );
                            }
                            catch (Exception ex)
                            {
                                _errorMessages.Add(new Exception("Constraint has error of ToString().", ex));
                            }
                        }

                        ForHeader("Constraints", () =>
                        {
                            if (names.Any())
                            {
                                if (ImGui.DragInt("index (slider)##constraintsIndex", ref selectedIndex, 0.1f, 0, names.Count - 1))
                                {

                                }

                                if (ImGui.BeginCombo($"Constraint", names.GetAtOrNull(selectedIndex) ?? "..."))
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

                                if (sourceList?.GetAtOrNull(selectedIndex) is Motion.Constraint constraint)
                                {
                                    ForCombo("Type##constraintType", _constraintTypes, () => constraint.Type, it => { constraint.Type = (byte)it; saved = true; });
                                    ForEdit("TemporaryActiveFlag", () => constraint.TemporaryActiveFlag, it => { constraint.TemporaryActiveFlag = it; saved = true; });
                                    ForEdit("ConstrainedJointId", () => constraint.ConstrainedJointId, it => { constraint.ConstrainedJointId = it; saved = true; });
                                    ForEdit("SourceJointId", () => constraint.SourceJointId, it => { constraint.SourceJointId = it; saved = true; });
                                    ForEdit("LimiterId", () => constraint.LimiterId, it => { constraint.LimiterId = it; saved = true; });
                                    ForEdit("ActivationCount", () => constraint.ActivationCount, it => { constraint.ActivationCount = it; saved = true; });
                                    ForEdit("ActivationStartId", () => constraint.ActivationStartId, it => { constraint.ActivationStartId = it; saved = true; });

                                    ImGui.Text("Goto:");
                                    ImGui.SameLine();
                                    if (ImGui.Button("ConstrainedJoint##gotoConstrainedJoint"))
                                    {
                                        _loadedModel.SelectedJointIndex = constraint.ConstrainedJointId;
                                        _loadedModel.SelectedJointIndexAge.Bump();
                                    }
                                    ImGui.SameLine();
                                    if (ImGui.Button("SourceJoint##gotoSourceJoint"))
                                    {
                                        _loadedModel.SelectedJointIndex = constraint.SourceJointId;
                                        _loadedModel.SelectedJointIndexAge.Bump();
                                    }
                                    ImGui.SameLine();
                                    if (ImGui.Button("Activation##gotoActivation"))
                                    {
                                        activationSelectedIndex = constraint.ActivationStartId;
                                    }
                                    ImGui.SameLine();
                                    if (ImGui.Button("Limiter##gotoLimiter"))
                                    {
                                        limiterSelectedIndex = constraint.LimiterId;
                                    }

                                    void AllocActivation(Action<int> onAdd)
                                    {
                                        if (activationList != null)
                                        {
                                            activationList.Add(new Motion.ConstraintActivation { });
                                            onAdd(activationList.Count - 1);
                                            saved = true;
                                        }
                                    }
                                    void AllocLimiter(Action<int> onAdd)
                                    {
                                        if (limiterList != null)
                                        {
                                            limiterList.Add(new Motion.Limiter { });
                                            onAdd(limiterList.Count - 1);
                                            saved = true;
                                        }
                                    }

                                    ImGui.Separator();
                                    ImGui.Text("Manipulator:");

                                    ImGui.SameLine();
                                    if (ImGui.Button("Alloc activation"))
                                    {
                                        AllocActivation(index =>
                                        {
                                            activationSelectedIndex = constraint.ActivationStartId = (short)index;
                                            constraint.ActivationCount = 1;
                                        });
                                    }

                                    ImGui.SameLine();
                                    if (ImGui.Button("Inject one more"))
                                    {
                                        if (constraint.ActivationCount == 0)
                                        {
                                            AllocActivation(index =>
                                            {
                                                activationSelectedIndex = constraint.ActivationStartId = (short)index;
                                                constraint.ActivationCount = 1;
                                            });
                                        }
                                        else if (activationList != null)
                                        {
                                            var at = constraint.ActivationStartId + constraint.ActivationCount;
                                            activationList.Insert(at, new Motion.ConstraintActivation { });
                                            ++constraint.ActivationCount;

                                            activationSelectedIndex = at;

                                            sourceList
                                                .Where(it => !ReferenceEquals(it, constraint))
                                                .ToList()
                                                .ForEach(
                                                    one =>
                                                    {
                                                        one.ActivationStartId = (at <= one.ActivationStartId)
                                                            ? (short)(one.ActivationStartId + 1)
                                                            : one.ActivationStartId;
                                                    }
                                                );

                                            saved = true;
                                        }
                                    }

                                    ImGui.SameLine();
                                    if (ImGui.Button("Alloc limiter"))
                                    {
                                        AllocLimiter(index => limiterSelectedIndex = constraint.LimiterId = (short)index);
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
                        },
                            openByDefault: true
                        );


                        ForHeader("ConstraintActivations", () =>
                        {
                            if (activationList?.Any() ?? false)
                            {
                                if (ImGui.DragInt("index (slider)##activationIndex", ref activationSelectedIndex, 0.05f, 0, activationNames.Count - 1))
                                {

                                }

                                if (ImGui.BeginCombo($"constraintActivation", activationNames.GetAtOrNull(activationSelectedIndex) ?? "..."))
                                {
                                    foreach (var (item, index) in activationNames.SelectWithIndex())
                                    {
                                        if (ImGui.Selectable(activationNames[index], index == activationSelectedIndex))
                                        {
                                            activationSelectedIndex = index;
                                        }
                                    }
                                    ImGui.EndCombo();
                                }

                                if (activationList?.GetAtOrNull(activationSelectedIndex) is Motion.ConstraintActivation activation)
                                {
                                    ForEdit("Time", () => activation.Time, it => { activation.Time = it; saved = true; });
                                    ForEdit("Active", () => activation.Active, it => { activation.Active = it; saved = true; });
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
                        },
                            openByDefault: true
                        );


                        ForHeader("Limiters", () =>
                        {
                            if (limiterList?.Any() ?? false)
                            {
                                if (ImGui.DragInt("index (slider)##limiterIndex", ref limiterSelectedIndex, 0.05f, 0, limiterNames.Count - 1))
                                {

                                }

                                if (ImGui.BeginCombo($"limiters", limiterNames.GetAtOrNull(limiterSelectedIndex) ?? "..."))
                                {
                                    foreach (var (item, index) in limiterNames.SelectWithIndex())
                                    {
                                        if (ImGui.Selectable(limiterNames[index], index == limiterSelectedIndex))
                                        {
                                            limiterSelectedIndex = index;
                                        }
                                    }
                                    ImGui.EndCombo();
                                }

                                if (limiterList?.GetAtOrNull(limiterSelectedIndex) is Motion.Limiter limiter)
                                {
                                    ForCombo("Type##limiterType", _limiterTypes, () => (int)limiter.Type, it => { limiter.Type = (Motion.LimiterType)it; saved = true; });

                                    ForEdit("HasXMin", () => limiter.HasXMin, it => { limiter.HasXMin = it; saved = true; });
                                    ImGui.SameLine();
                                    ForEdit("HasYMin", () => limiter.HasYMin, it => { limiter.HasYMin = it; saved = true; });
                                    ImGui.SameLine();
                                    ForEdit("HasZMin", () => limiter.HasZMin, it => { limiter.HasZMin = it; saved = true; });

                                    ForEdit("HasXMax", () => limiter.HasXMax, it => { limiter.HasXMax = it; saved = true; });
                                    ImGui.SameLine();
                                    ForEdit("HasYMax", () => limiter.HasYMax, it => { limiter.HasYMax = it; saved = true; });
                                    ImGui.SameLine();
                                    ForEdit("HasZMax", () => limiter.HasZMax, it => { limiter.HasZMax = it; saved = true; });

                                    ForEdit("Global", () => limiter.Global, it => { limiter.Global = it; saved = true; });
                                    ForEdit("DampingWidth", () => limiter.DampingWidth, it => { limiter.DampingWidth = it; saved = true; });
                                    ForEdit("DampingStrength", () => limiter.DampingStrength, it => { limiter.DampingStrength = it; saved = true; });

                                    ForEdit3("Min",
                                        () => new Vector3(limiter.MinX, limiter.MinY, limiter.MinZ),
                                        it =>
                                        {
                                            limiter.MinX = it.X;
                                            limiter.MinY = it.Y;
                                            limiter.MinZ = it.Z;
                                            saved = true;
                                        }
                                    );
                                    ForEdit("MinW", () => limiter.MinW, it => { limiter.MinW = it; saved = true; });

                                    ForEdit3("Max",
                                        () => new Vector3(limiter.MaxX, limiter.MaxY, limiter.MaxZ),
                                        it =>
                                        {
                                            limiter.MaxX = it.X;
                                            limiter.MaxY = it.Y;
                                            limiter.MaxZ = it.Z;
                                            saved = true;
                                        }
                                    );
                                    ForEdit("MaxW", () => limiter.MaxW, it => { limiter.MaxW = it; saved = true; });
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
                        },
                            openByDefault: true
                        );


                        if (saved)
                        {
                            nextTimeRefresh.TurnOn();
                            _loadedModel.SendBackMotionData.TurnOn();
                        }
                    },
                        menuBar:true
                    );

                    if (windowClosed)
                    {
                        _settings.ViewConstraint = false;
                        _settings.Save();
                    }
                }
            };
        }

    }
}
