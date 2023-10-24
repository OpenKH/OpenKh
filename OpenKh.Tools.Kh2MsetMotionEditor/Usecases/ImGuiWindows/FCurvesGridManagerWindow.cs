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
    public class FCurvesGridManagerWindow : IWindowRunnableProvider
    {
        private readonly EditMotionDataUsecase _editMotionDataUsecase;
        private readonly LoadedModel _loadedModel;
        private readonly Settings _settings;

        public FCurvesGridManagerWindow(
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
                    var windowClosed = !ForWindow("FCurves grid manager", () =>
                    {
                        if (true
                            && _loadedModel.MotionData is InterpolatedMotion motionData
                            )
                        {
                            var fcurvesBoth = motionData.FCurvesForward.Concat(motionData.FCurvesInverse).ToArray();

                            if (ImGui.DragInt("index (slider)", ref selectedIndex, 0.2f, 0, fcurvesBoth.Count() - 1))
                            {

                            }

                            if (fcurvesBoth.GetAtOrNull(selectedIndex) is FCurve fcurve)
                            {
                                ImGui.Text("Insert Delete | Time | Value | LeftTangent | RightTangent");

                                var saved = false;

                                for (int y = 0; y < fcurve.KeyCount; y++)
                                {
                                    if (motionData.FCurveKeys.GetAtOrNull(fcurve.KeyStartId + y) is Key key)
                                    {
                                        ImGui.Button($"I##I{y}");

                                        ImGui.SameLine();
                                        ImGui.Button($"x##D{y}");

                                        ImGui.SameLine();
                                        ForEdit4($"#{y}",
                                            () => new Vector4(
                                                motionData.KeyTimes.GetAtOrNull(key.Time),
                                                motionData.KeyValues.GetAtOrNull(key.ValueId),
                                                motionData.KeyTangents.GetAtOrNull(key.LeftTangentId),
                                                motionData.KeyTangents.GetAtOrNull(key.RightTangentId)
                                            ),
                                            it =>
                                            {
                                                key.Time = _editMotionDataUsecase.AssignTimeId(key.Time, it.X);
                                                key.ValueId = _editMotionDataUsecase.AssignValueId(key.ValueId, it.Y);
                                                key.LeftTangentId = _editMotionDataUsecase.AssignTangentId(key.LeftTangentId, it.Z);
                                                key.RightTangentId = _editMotionDataUsecase.AssignTangentId(key.RightTangentId, it.W);
                                                saved = true;
                                            }
                                        );
                                    }
                                }

                                ImGui.Button($"Append");

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
