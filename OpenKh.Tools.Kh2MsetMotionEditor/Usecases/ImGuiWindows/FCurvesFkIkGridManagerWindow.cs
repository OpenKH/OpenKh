using ImGuiNET;
using OpenKh.Tools.Kh2MsetMotionEditor.Helpers;
using OpenKh.Tools.Kh2MsetMotionEditor.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static OpenKh.Tools.Common.CustomImGui.ImGuiEx;
using static OpenKh.Kh2.Motion;
using System.Numerics;

namespace OpenKh.Tools.Kh2MsetMotionEditor.Usecases.ImGuiWindows
{
    public class FCurvesFkIkGridManagerWindow : IWindowRunnableProvider
    {
        private readonly EditMotionDataUsecase _editMotionDataUsecase;
        private readonly LoadedModel _loadedModel;
        private readonly Settings _settings;

        public FCurvesFkIkGridManagerWindow(
            Settings settings,
            LoadedModel loadedModel,
            EditMotionDataUsecase editMotionDataUsecase
        )
        {
            _editMotionDataUsecase = editMotionDataUsecase;
            _loadedModel = loadedModel;
            _settings = settings;
        }

        public Action CreateWindowRunnable()
        {
            var selectedIndex = -1;

            return () =>
            {
                if (_settings.ViewFCurvesGrid)
                {
                    var windowClosed = !ForWindow("FCurvesFkIk grid manager", () =>
                    {
                        if (true
                            && _loadedModel.MotionData is InterpolatedMotion motionData
                            )
                        {
                            var numFk = motionData.FCurvesForward.Count;
                            var numIk = motionData.FCurvesInverse.Count;
                            var numFkIk = numFk + numIk;


                            if (ImGui.DragInt("index (slider)", ref selectedIndex, 0.2f, 0, numFkIk - 1))
                            {

                            }

                            FCurve? fcurve;
                            string which = "";

                            if (selectedIndex < 0)
                            {
                                fcurve = null;
                            }
                            else if (selectedIndex < numFk)
                            {
                                fcurve = motionData.FCurvesForward[selectedIndex];
                                which = $"Current: FCurvesForward #{selectedIndex}";
                            }
                            else if (selectedIndex < numFk + numIk)
                            {
                                fcurve = motionData.FCurvesInverse[selectedIndex - numFk];
                                which = $"Current: FCurvesInverse #{selectedIndex - numFk}";
                            }
                            else
                            {
                                fcurve = null;
                            }

                            if (fcurve != null)
                            {
                                ImGui.Text(which);

                                if (ImGui.BeginChild("FCurvesGridArea"))
                                {
                                    if (ImGui.BeginTable($"Grid", 5, ImGuiTableFlags.Resizable | ImGuiTableFlags.ScrollY))
                                    {
                                        ImGui.TableSetupColumn("Type", ImGuiTableColumnFlags.IsVisible);
                                        ImGui.TableSetupColumn("Time", ImGuiTableColumnFlags.IsVisible);
                                        ImGui.TableSetupColumn("Value", ImGuiTableColumnFlags.IsVisible);
                                        ImGui.TableSetupColumn("LeftTangent", ImGuiTableColumnFlags.IsVisible);
                                        ImGui.TableSetupColumn("RightTangent", ImGuiTableColumnFlags.IsVisible);
                                        ImGui.TableSetupScrollFreeze(0, 1);
                                        ImGui.TableHeadersRow();

                                        for (int y = 0; y < fcurve.KeyCount; y++)
                                        {
                                            if (motionData.FCurveKeys.GetAtOrNull(fcurve.KeyStartId + y) is Key key)
                                            {
                                                ImGui.TableNextRow();

                                                if (ImGui.TableNextColumn())
                                                {
                                                    ImGui.Text(key.Type.ToString());
                                                }

                                                if (ImGui.TableNextColumn())
                                                {
                                                    ImGui.Text(motionData.KeyTimes.GetAtOrNull(key.Time).ToString());
                                                }

                                                if (ImGui.TableNextColumn())
                                                {
                                                    ImGui.Text(motionData.KeyValues.GetAtOrNull(key.ValueId).ToString());
                                                }

                                                if (ImGui.TableNextColumn())
                                                {
                                                    ImGui.Text(motionData.KeyTangents.GetAtOrNull(key.LeftTangentId).ToString());
                                                }

                                                if (ImGui.TableNextColumn())
                                                {
                                                    ImGui.Text(motionData.KeyTangents.GetAtOrNull(key.RightTangentId).ToString());
                                                }
                                            }
                                        }
                                        ImGui.EndTable();
                                    }

                                    ImGui.EndChild();
                                }

                                var saved = false;

                                if (saved)
                                {
                                    _loadedModel.SendBackMotionData.TurnOn();
                                }
                            }
                            else
                            {
                                ImGui.Text("(Editor will appear after selection)");
                            }
                        }
                        else
                        {
                            ImGui.Text("(Editor will appear after selection)");
                        }
                    });

                    if (windowClosed)
                    {
                        _settings.ViewFCurvesGrid = false;
                        _settings.Save();
                    }
                }
            };
        }
    }
}
