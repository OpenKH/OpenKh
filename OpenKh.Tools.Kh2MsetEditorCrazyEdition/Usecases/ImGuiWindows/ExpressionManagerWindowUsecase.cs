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

namespace OpenKh.Tools.Kh2MsetEditorCrazyEdition.Usecases.ImGuiWindows
{
    public class ExpressionManagerWindowUsecase : IWindowRunnableProvider
    {
        private readonly FormatExpressionNodesUsecase _formatExpressionNodesUsecase;
        private readonly EditCollectionNoErrorUsecase _editCollectionNoErrorUsecase;
        private readonly ErrorMessages _errorMessages;
        private readonly LoadedModel _loadedModel;
        private readonly Settings _settings;
        private readonly string[] _expressionNodeTypes;
        private Vector4 _highlightedTextColor = new Vector4(0, 1, 0, 1);

        public ExpressionManagerWindowUsecase(
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
            _expressionNodeTypes = Enumerable.Range(0, 44)
                .Select(index => ((Motion.ExpressionType)index).ToString())
                .ToArray();
        }

        public Action CreateWindowRunnable()
        {
            var age = _loadedModel.JointDescriptionsAge.Branch(false);
            var list = new List<string>();
            var expressionNodeList = new List<string>();
            var selectedIndex = -1;
            var nextTimeRefresh = new OneTimeOn(false);
            var expressionNodeSelectedIndex = -1;

            return () =>
            {
                if (_settings.ViewExpression)
                {
                    ForWindow("Expression manager", () =>
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
                                list.Clear();
                                list.AddRange(
                                    sourceList!
                                        .Select(it => $"T# {it.TargetId} TC {it.TargetChannel} N# {it.NodeId}")
                                );

                                expressionNodeList.Clear();
                                expressionNodeList.AddRange(
                                    nodeList!
                                        .Select(it => $"{(Motion.ExpressionType)it.Type}, {it.Value}, {it.CAR}, {it.CDR}, {it.Element}, {(it.IsGlobal ? "Global" : "Local")}")
                                );
                            }
                            catch (Exception ex)
                            {
                                _errorMessages.Add(new Exception("Expression has error of ToString().", ex));
                            }
                        }

                        if (list.Any())
                        {
                            ForHeader("Expressions --", () =>
                            {
                                if (ImGui.DragInt("index", ref selectedIndex, 0.05f, 0, list.Count - 1))
                                {

                                }

                                if (ImGui.BeginCombo($"Expression", list.GetAtOrNull(selectedIndex) ?? "..."))
                                {
                                    foreach (var (item, index) in list.SelectWithIndex())
                                    {
                                        if (ImGui.Selectable(list[index], index == selectedIndex))
                                        {
                                            selectedIndex = index;
                                        }
                                    }
                                    ImGui.EndCombo();
                                }

                                if (sourceList?.GetAtOrNull(selectedIndex) is Motion.Expression expression)
                                {
                                    ForEdit("TargetId", () => expression.TargetId, it => { expression.TargetId = it; saved = true; });
                                    ForEdit("TargetChannel", () => expression.TargetChannel, it => { expression.TargetChannel = it; saved = true; });
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

                                    if (ImGui.Button("Explore"))
                                    {
                                        expressionNodeSelectedIndex = expression.NodeId;
                                    }
                                }
                                else
                                {
                                    ImGui.Text("(Editor will appear after selection)");
                                }
                            },
                                openByDefault: true
                            );
                        }
                        else
                        {
                            ImGui.Text("(Collection is empty)");
                        }


                        if (expressionNodeList.Any())
                        {
                            ForHeader("ExpressionNodes --", () =>
                            {
                                if (ImGui.DragInt("nodeIndex", ref expressionNodeSelectedIndex, 0.05f, 0, expressionNodeList.Count - 1))
                                {

                                }

                                if (ImGui.BeginCombo($"expressionNode", expressionNodeList.GetAtOrNull(expressionNodeSelectedIndex) ?? "..."))
                                {
                                    foreach (var (item, index) in expressionNodeList.SelectWithIndex())
                                    {
                                        if (ImGui.Selectable(expressionNodeList[index], index == expressionNodeSelectedIndex))
                                        {
                                            expressionNodeSelectedIndex = index;
                                        }
                                    }
                                    ImGui.EndCombo();
                                }

                                if (nodeList?.GetAtOrNull(expressionNodeSelectedIndex) is Motion.ExpressionNode node)
                                {
                                    if (ImGui.Button("Enter CAR"))
                                    {
                                        expressionNodeSelectedIndex = node.CAR;
                                    }
                                    ImGui.SameLine();
                                    if (ImGui.Button("Enter CDR"))
                                    {
                                        expressionNodeSelectedIndex = node.CDR;
                                    }

                                    ForCombo("Type", _expressionNodeTypes, () => node.Type, it => { node.Type = (byte)it; saved = true; });
                                    ForEdit("IsGlobal", () => node.IsGlobal, it => { node.IsGlobal = it; saved = true; });
                                    ForEdit("Element", () => node.Element, it => { node.Element = it; saved = true; });
                                    ForEdit("Type", () => node.Type, it => { node.Type = it; saved = true; });
                                    ForEdit("Value", () => node.Value, it => { node.Value = it; saved = true; });
                                    ForEdit("CAR", () => node.CAR, it => { node.CAR = it; saved = true; });
                                    ForEdit("CDR", () => node.CDR, it => { node.CDR = it; saved = true; });
                                }
                                else
                                {
                                    ImGui.Text("(Editor will appear after selection)");
                                }
                            },
                                openByDefault: true
                            );
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
                }
            };
        }
    }
}
