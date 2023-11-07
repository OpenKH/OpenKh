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
    public class ExpressionManagerWindowUsecase : IWindowRunnableProvider
    {
        private readonly FormatListItemUsecase _formatListItemUsecase;
        private readonly FormatExpressionNodesUsecase _formatExpressionNodesUsecase;
        private readonly EditCollectionNoErrorUsecase _editCollectionNoErrorUsecase;
        private readonly ErrorMessages _errorMessages;
        private readonly LoadedModel _loadedModel;
        private readonly Settings _settings;
        private readonly string[] _expressionNodeTypes;
        private readonly string[] _targetChannels;
        private Vector4 _highlightedTextColor = new Vector4(0, 1, 0, 1);

        public ExpressionManagerWindowUsecase(
            Settings settings,
            LoadedModel loadedModel,
            ErrorMessages errorMessages,
            EditCollectionNoErrorUsecase editCollectionNoErrorUsecase,
            FormatExpressionNodesUsecase formatExpressionNodesUsecase,
            FormatListItemUsecase formatListItemUsecase
        )
        {
            _formatListItemUsecase = formatListItemUsecase;
            _formatExpressionNodesUsecase = formatExpressionNodesUsecase;
            _editCollectionNoErrorUsecase = editCollectionNoErrorUsecase;
            _errorMessages = errorMessages;
            _loadedModel = loadedModel;
            _settings = settings;
            _expressionNodeTypes = Enumerable.Range(0, 44)
                .Select(index => ((Motion.ExpressionType)index).ToString())
                .ToArray();
            _targetChannels = "Sx,Sy,Sz,Rx,Ry,Rz,Tx,Ty,Tz".Split(',');
        }

        public Action CreateWindowRunnable()
        {
            var age = _loadedModel.JointDescriptionsAge.Branch(false);
            var names = new List<string>();
            var expressionNodeNames = new List<string>();
            var selectedIndex = -1;
            var nextTimeRefresh = new OneTimeOn(false);
            var expressionNodeSelectedIndex = -1;

            return () =>
            {
                if (_settings.ViewExpression)
                {
                    var windowClosed = !ForWindow("Expression manager", () =>
                    {
                        var sourceList = _loadedModel.MotionData?.Expressions;
                        var nodeList = _loadedModel.MotionData?.ExpressionNodes;

                        var saved = false;

                        ForMenuBar(() =>
                        {
                            ForMenuItem("Insert", () =>
                            {
                                saved = _editCollectionNoErrorUsecase.InsertAt(sourceList, selectedIndex, new Motion.Expression());
                            });
                            ForMenuItem("Append", () =>
                            {
                                if (saved = _editCollectionNoErrorUsecase.Append(sourceList, new Motion.Expression()))
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
                                        .Select(it => _formatListItemUsecase.FormatExpression(it, _targetChannels))
                                );

                                expressionNodeNames.Clear();
                                expressionNodeNames.AddRangeIfNotNull(
                                    nodeList?
                                        .Select(_formatListItemUsecase.FormatExpressionNode)
                                );
                            }
                            catch (Exception ex)
                            {
                                _errorMessages.Add(new Exception("Expression has error of ToString().", ex));
                            }
                        }

                        void AllocNode(Action<short> onNodeId)
                        {
                            if (nodeList != null)
                            {
                                onNodeId((short)(expressionNodeSelectedIndex = nodeList.Count));
                                nodeList.Add(new Motion.ExpressionNode { CAR = -1, CDR = -1, Type = (byte)Motion.ExpressionType.CONSTANT_NUM, });
                                saved = true;
                            }
                        }

                        var allocCar = false;
                        var allocCdr = false;
                        var enterCar = false;
                        var enterCdr = false;

                        ForHeader("Expressions", () =>
                        {
                            if (names.Any())
                            {
                                if (ImGui.DragInt("index (slider)##expressionIndex", ref selectedIndex, 0.05f, 0, names.Count - 1))
                                {

                                }

                                if (ImGui.BeginCombo($"Expression", names.GetAtOrNull(selectedIndex) ?? "..."))
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

                                if (sourceList?.GetAtOrNull(selectedIndex) is Motion.Expression expression)
                                {
                                    ForEdit("TargetId", () => expression.TargetId, it => { expression.TargetId = it; saved = true; });
                                    ForCombo("TargetChannel", _targetChannels, () => expression.TargetChannel, it => { expression.TargetChannel = (short)it; saved = true; });
                                    ForEdit("NodeId", () => expression.NodeId, it => { expression.NodeId = it; saved = true; });

                                    ImGui.Text("ExpressionNodes --");

                                    var tokens = _formatExpressionNodesUsecase.ToTree(
                                            expression.NodeId,
                                            index => nodeList?.GetAtOrNull(index)
                                        );

                                    ImGui.Text("");

                                    foreach (var token in tokens)
                                    {
                                        ImGui.SameLine(0, 0);
                                        if (token.NodeId == expressionNodeSelectedIndex)
                                        {
                                            ImGui.TextColored(_highlightedTextColor, token.Text);
                                        }
                                        else
                                        {
                                            ImGui.Text(token.Text);
                                        }
                                    }

                                    ImGui.Text("Goto:");

                                    ImGui.SameLine();
                                    if (ImGui.Button("TargetId##gotoTargetId"))
                                    {
                                        _loadedModel.SelectedJointIndex = expression.TargetId;
                                        _loadedModel.SelectedJointIndexAge.Bump();
                                    }

                                    ImGui.SameLine();
                                    if (ImGui.Button("RootNode##gotoRootNode"))
                                    {
                                        expressionNodeSelectedIndex = expression.NodeId;
                                    }

                                    ImGui.SameLine();
                                    if (ImGui.Button("CAR##gotoCAR"))
                                    {
                                        enterCar = true;
                                    }

                                    ImGui.SameLine();
                                    if (ImGui.Button("CDR##gotoCDR"))
                                    {
                                        enterCdr = true;
                                    }

                                    ImGui.SameLine();
                                    ImGui.Text("Other:");

                                    ImGui.SameLine();
                                    if (ImGui.Button("Copy expr"))
                                    {
                                        ClipboardService.SetText(
                                            string.Concat(
                                                tokens.Select(token => token.Text)
                                            )
                                        );
                                    }

                                    ImGui.Separator();
                                    ImGui.Text("Manipulator:");

                                    ImGui.SameLine();
                                    if (ImGui.Button("Alloc RootNode"))
                                    {
                                        AllocNode(nodeId => expression.NodeId = nodeId);
                                    }

                                    ImGui.SameLine();
                                    if (ImGui.Button("Alloc CAR"))
                                    {
                                        allocCar = true;
                                    }

                                    ImGui.SameLine();
                                    if (ImGui.Button("Alloc CDR"))
                                    {
                                        allocCdr = true;
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


                        ForHeader("ExpressionNodes", () =>
                        {
                            if (expressionNodeNames.Any())
                            {
                                if (ImGui.DragInt("index (slider)##nodeIndex", ref expressionNodeSelectedIndex, 0.05f, 0, expressionNodeNames.Count - 1))
                                {

                                }

                                if (ImGui.BeginCombo($"expressionNode", expressionNodeNames.GetAtOrNull(expressionNodeSelectedIndex) ?? "..."))
                                {
                                    foreach (var (item, index) in expressionNodeNames.SelectWithIndex())
                                    {
                                        if (ImGui.Selectable(expressionNodeNames[index], index == expressionNodeSelectedIndex))
                                        {
                                            expressionNodeSelectedIndex = index;
                                        }
                                    }
                                    ImGui.EndCombo();
                                }

                                if (nodeList?.GetAtOrNull(expressionNodeSelectedIndex) is Motion.ExpressionNode node)
                                {
                                    if (enterCar)
                                    {
                                        expressionNodeSelectedIndex = node.CAR;
                                    }
                                    if (enterCdr)
                                    {
                                        expressionNodeSelectedIndex = node.CDR;
                                    }
                                    if (allocCar)
                                    {
                                        AllocNode(nodeId => node.CAR = nodeId);
                                    }
                                    if (allocCdr)
                                    {
                                        AllocNode(nodeId => node.CDR = nodeId);
                                    }

                                    ForCombo("Type", _expressionNodeTypes, () => node.Type, it => { node.Type = (byte)it; saved = true; });
                                    ForEdit("IsGlobal", () => node.IsGlobal, it => { node.IsGlobal = it; saved = true; });
                                    ForEdit("Element", () => node.Element, it => { node.Element = it; saved = true; });
                                    ForEdit("Value", () => node.Value, it => { node.Value = it; saved = true; });
                                    ForEdit("CAR", () => node.CAR, it => { node.CAR = it; saved = true; });
                                    ForEdit("CDR", () => node.CDR, it => { node.CDR = it; saved = true; });
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
                        _settings.ViewExpression = false;
                        _settings.Save();
                    }
                }
            };
        }
    }
}
