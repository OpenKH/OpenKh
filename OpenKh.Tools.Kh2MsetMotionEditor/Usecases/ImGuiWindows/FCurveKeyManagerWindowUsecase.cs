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
    public class FCurveKeyManagerWindowUsecase : IWindowRunnableProvider
    {
        private readonly FormatListItemUsecase _formatListItemUsecase;
        private readonly EditCollectionNoErrorUsecase _editCollectionNoErrorUsecase;
        private readonly ErrorMessages _errorMessages;
        private readonly LoadedModel _loadedModel;
        private readonly Settings _settings;
        private readonly string[] _interpolations;

        public FCurveKeyManagerWindowUsecase(
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
            _interpolations = Enumerable.Range(0, 5)
                .Select(it => ((Motion.Interpolation)it).ToString())
                .ToArray();
        }

        public Action CreateWindowRunnable()
        {
            var age = _loadedModel.JointDescriptionsAge.Branch(false);
            var names = new List<string>();
            var selectedIndex = -1;
            var nextTimeRefresh = new OneTimeOn(false);
            var timeNames = new List<string>();
            var valueNames = new List<string>();
            var tangentNames = new List<string>();
            var timeSelectedIndex = -1;
            var valueSelectedIndex = -1;
            int tangentSelectedIndex = -1;

            return () =>
            {
                if (_loadedModel.SelectFCurveKey != -1)
                {
                    _settings.ViewFCurveKey = true;
                    selectedIndex = _loadedModel.SelectFCurveKey;
                    _loadedModel.SelectFCurveKey = -1;
                }

                if (_settings.ViewFCurveKey)
                {
                    var windowClosed = !ForWindow("FCurveKey manager", () =>
                    {
                        var sourceList = _loadedModel.MotionData?.FCurveKeys;
                        var timeList = _loadedModel.MotionData?.KeyTimes;
                        var valueList = _loadedModel.MotionData?.KeyValues;
                        var tangentList = _loadedModel.MotionData?.KeyTangents;

                        var saved = false;

                        ForMenuBar(() =>
                        {
                            ForMenuItem("Insert", () =>
                            {
                                saved = _editCollectionNoErrorUsecase.InsertAt(sourceList, selectedIndex, new Motion.Key());
                            });
                            ForMenuItem("Append", () =>
                            {
                                if (saved = _editCollectionNoErrorUsecase.Append(sourceList, new Motion.Key()))
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
                                        .Select(_formatListItemUsecase.FormatKey)
                                );

                                timeNames.Clear();
                                timeNames.AddRangeIfNotNull(
                                    timeList?
                                        .Select(_formatListItemUsecase.FormatTime)
                                );

                                valueNames.Clear();
                                valueNames.AddRangeIfNotNull(
                                    valueList?
                                        .Select(_formatListItemUsecase.FormatValue)
                                );

                                tangentNames.Clear();
                                tangentNames.AddRangeIfNotNull(
                                    tangentList?
                                        .Select(_formatListItemUsecase.FormatTangent)
                                );
                            }
                            catch (Exception ex)
                            {
                                _errorMessages.Add(new Exception("FCurveKey has error of ToString().", ex));
                            }
                        }

                        ForHeader("FCurveKey", () =>
                        {
                            if (names.Any())
                            {
                                if (ImGui.DragInt("index (slider)##keyIndex", ref selectedIndex, 0.2f, 0, names.Count - 1))
                                {

                                }

                                if (ImGui.BeginCombo($"FCurveKey##names", names.GetAtOrNull(selectedIndex) ?? "..."))
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

                                if (sourceList?.GetAtOrNull(selectedIndex) is Motion.Key key)
                                {
                                    ForEdit("TimeId", () => key.Time, it => { key.Time = it; saved = true; });
                                    ForCombo("Type", _interpolations, () => (int)(key.Type), it => { key.Type = (Motion.Interpolation)(it); saved = true; });

                                    ForEdit("ValueId", () => key.ValueId, it => { key.ValueId = it; saved = true; });
                                    ForEdit("LeftTangentId", () => key.LeftTangentId, it => { key.LeftTangentId = it; saved = true; });
                                    ForEdit("RightTangentId", () => key.RightTangentId, it => { key.RightTangentId = it; saved = true; });

                                    ImGui.Text("Goto:");
                                    ImGui.SameLine();
                                    if (ImGui.Button("Time##gotoTime"))
                                    {
                                        timeSelectedIndex = key.Time;
                                    }
                                    ImGui.SameLine();
                                    if (ImGui.Button("Value##gotoValue"))
                                    {
                                        valueSelectedIndex = key.Time;
                                    }
                                    ImGui.SameLine();
                                    if (ImGui.Button("LeftTangent##gotoLeftTangent"))
                                    {
                                        tangentSelectedIndex = key.LeftTangentId;
                                    }
                                    ImGui.SameLine();
                                    if (ImGui.Button("RightTangent##gotoRightTangent"))
                                    {
                                        tangentSelectedIndex = key.RightTangentId;
                                    }

                                    void AllocTime(Action<int> onAdd)
                                    {
                                        if (timeList != null)
                                        {
                                            var index = timeList.Count;
                                            timeList.Add(default);
                                            onAdd(index);
                                            saved = true;
                                        }
                                    }

                                    void AllocValue(Action<int> onAdd)
                                    {
                                        if (valueList != null)
                                        {
                                            var index = valueList.Count;
                                            valueList.Add(default);
                                            onAdd(index);
                                            saved = true;
                                        }
                                    }

                                    void AllocTangent(Action<int> onAdd)
                                    {
                                        if (tangentList != null)
                                        {
                                            var index = tangentList.Count;
                                            tangentList.Add(default);
                                            onAdd(index);
                                            saved = true;
                                        }
                                    }

                                    ImGui.Separator();

                                    ImGui.Text("Manipulator alloc:");

                                    ImGui.SameLine();
                                    if (ImGui.Button("Time"))
                                    {
                                        AllocTime(it => timeSelectedIndex = key.Time = (short)it);
                                    }
                                    ImGui.SameLine();
                                    if (ImGui.Button("Value"))
                                    {
                                        AllocValue(it => valueSelectedIndex = key.ValueId = (short)it);
                                    }
                                    ImGui.SameLine();
                                    if (ImGui.Button("LeftTangent"))
                                    {
                                        AllocTangent(it => tangentSelectedIndex = key.LeftTangentId = (short)it);
                                    }
                                    ImGui.SameLine();
                                    if (ImGui.Button("RightTangent"))
                                    {
                                        AllocTangent(it => tangentSelectedIndex = key.RightTangentId = (short)it);
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


                        ForHeader("Time##timeTab", () =>
                        {
                            if (timeNames.Any())
                            {
                                if (ImGui.DragInt("index (slider)##timeIndex", ref timeSelectedIndex, 0.2f, 0, timeNames.Count - 1))
                                {

                                }

                                if (ImGui.BeginCombo($"Time", timeNames.GetAtOrNull(timeSelectedIndex) ?? "..."))
                                {
                                    foreach (var (item, index) in timeNames.SelectWithIndex())
                                    {
                                        if (ImGui.Selectable(timeNames[index], index == timeSelectedIndex))
                                        {
                                            timeSelectedIndex = index;
                                        }
                                    }
                                    ImGui.EndCombo();
                                }

                                if (timeList?.GetAtOrNull(timeSelectedIndex) is float value)
                                {
                                    ForEdit("Value##timeValue", () => value, it => { timeList[timeSelectedIndex] = it; saved = true; });
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

                        ForHeader("Value##valueTab", () =>
                        {
                            if (valueNames.Any())
                            {
                                if (ImGui.DragInt("index (slider)##valueIndex", ref valueSelectedIndex, 0.2f, 0, valueNames.Count - 1))
                                {

                                }

                                if (ImGui.BeginCombo($"Value", valueNames.GetAtOrNull(valueSelectedIndex) ?? "..."))
                                {
                                    foreach (var (item, index) in valueNames.SelectWithIndex())
                                    {
                                        if (ImGui.Selectable(valueNames[index], index == valueSelectedIndex))
                                        {
                                            valueSelectedIndex = index;
                                        }
                                    }
                                    ImGui.EndCombo();
                                }

                                if (valueList?.GetAtOrNull(valueSelectedIndex) is float value)
                                {
                                    ForEdit("Value##valueValue", () => value, it => { valueList[valueSelectedIndex] = it; saved = true; });
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

                        ForHeader("Tangent##tangentTab", () =>
                        {
                            if (tangentNames.Any())
                            {
                                if (ImGui.DragInt("index (slider)##tangentIndex", ref tangentSelectedIndex, 0.2f, 0, tangentNames.Count - 1))
                                {

                                }

                                if (ImGui.BeginCombo($"Tangent", tangentNames.GetAtOrNull(tangentSelectedIndex) ?? "..."))
                                {
                                    foreach (var (item, index) in tangentNames.SelectWithIndex())
                                    {
                                        if (ImGui.Selectable(tangentNames[index], index == tangentSelectedIndex))
                                        {
                                            tangentSelectedIndex = index;
                                        }
                                    }
                                    ImGui.EndCombo();
                                }

                                if (tangentList?.GetAtOrNull(tangentSelectedIndex) is float value)
                                {
                                    ForEdit("Value##tangentValue", () => value, it => { tangentList[tangentSelectedIndex] = it; saved = true; }, speed: 0.01f);
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
                        _settings.ViewFCurveKey = false;
                        _settings.Save();
                    }
                }
            };
        }
    }
}
