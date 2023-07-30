using ImGuiNET;
using static OpenKh.Tools.Common.CustomImGui.ImGuiEx;
using xna = Microsoft.Xna.Framework;
using OpenKh.Tools.Kh2MsetEditorCrazyEdition.Helpers;
using OpenKh.Tools.Kh2MsetEditorCrazyEdition.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenKh.Kh2;
using System.Numerics;
using Assimp;

namespace OpenKh.Tools.Kh2MsetEditorCrazyEdition.Usecases.ImGuiWindows
{
    public class ConstraintManagerWindowUsecase : IWindowRunnableProvider
    {
        private readonly FormatExpressionNodesUsecase _formatExpressionNodesUsecase;
        private readonly EditCollectionNoErrorUsecase _editCollectionNoErrorUsecase;
        private readonly ErrorMessages _errorMessages;
        private readonly LoadedModel _loadedModel;
        private readonly Settings _settings;
        private readonly string[] _constraintTypes;
        private readonly string[] _targetChannels;
        private readonly string[] _limiterTypes;
        private Vector4 _highlightedTextColor = new Vector4(0, 1, 0, 1);

        public ConstraintManagerWindowUsecase(
            Settings settings,
            LoadedModel loadedModel,
            ErrorMessages errorMessages,
            EditCollectionNoErrorUsecase editCollectionNoErrorUsecase,
            FormatExpressionNodesUsecase formatExpressionNodesUsecase
        )
        {
            _formatExpressionNodesUsecase = formatExpressionNodesUsecase;
            _editCollectionNoErrorUsecase = editCollectionNoErrorUsecase;
            _errorMessages = errorMessages;
            _loadedModel = loadedModel;
            _settings = settings;
            _constraintTypes = Enumerable.Range(0, 14)
                .Select(index => ((Motion.ConstraintType)index).ToString())
                .ToArray();
            _targetChannels = "Sx,Sy,Sz,Rx,Ry,Rz,Tx,Ty,Tz".Split(',');
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
                                names.AddRange(
                                    sourceList!
                                        .Select(it => $"{(Motion.ConstraintType)it.Type}, {it.SourceJointId}, {it.ConstrainedJointId}")
                                );

                                activationNames.Clear();
                                activationNames.AddRange(
                                    activationList!
                                        .Select(it => $"{it.Time}, {it.Active}")
                                );

                                limiterNames.Clear();
                                limiterNames.AddRange(
                                    limiterList!
                                        .Select(it => $"{it}")
                                );
                            }
                            catch (Exception ex)
                            {
                                _errorMessages.Add(new Exception("Constraint has error of ToString().", ex));
                            }
                        }

                        ForHeader("Constraints --", () =>
                        {
                            if (names.Any())
                            {
                                if (ImGui.DragInt("index", ref selectedIndex, 0.1f, 0, names.Count - 1))
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


                        ForHeader("ConstraintActivations --", () =>
                        {
                            if (activationList?.Any() ?? false)
                            {
                                if (ImGui.DragInt("activationIndex", ref activationSelectedIndex, 0.05f, 0, activationNames.Count - 1))
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


                        ForHeader("Limiters --", () =>
                        {
                            if (limiterList?.Any() ?? false)
                            {
                                if (ImGui.DragInt("limiterIndex", ref limiterSelectedIndex, 0.05f, 0, limiterNames.Count - 1))
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
                        menuBar: true
                    );

                    if (windowClosed)
                    {
                        _settings.ViewConstraint = false;
                    }
                }
            };
        }

        private string FormatTargetChannel(short type)
        {
            if (0 <= type && type < 9)
            {
                return _targetChannels[type];
            }
            else
            {
                return type.ToString();
            }
        }
    }
}
