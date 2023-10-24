using ImGuiNET;
using OpenKh.Tools.Kh2MsetMotionEditor.Helpers;
using OpenKh.Tools.Kh2MsetMotionEditor.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using YamlDotNet.Serialization;
using static OpenKh.Kh2.Motion;
using static OpenKh.Tools.Common.CustomImGui.ImGuiEx;

namespace OpenKh.Tools.Kh2MsetMotionEditor.Usecases.ImGuiWindows
{
    public class RootPositionWindowUsecase : IWindowRunnableProvider
    {
        private readonly OpenWindowUsecase _openWindowUsecase;
        private readonly LoadedModel _loadedModel;
        private readonly Settings _settings;

        public RootPositionWindowUsecase(
            Settings settings,
            LoadedModel loadedModel,
            OpenWindowUsecase openWindowUsecase
        )
        {
            _openWindowUsecase = openWindowUsecase;
            _loadedModel = loadedModel;
            _settings = settings;
        }

        public Action CreateWindowRunnable()
        {
            var names = "Sx,Sy,Sz,Rx,Ry,Rz,Tx,Ty,Tz".Split(',');
            var selectedIndex = 0;

            return () =>
            {
                if (_settings.ViewRootPosition)
                {
                    var windowClosed = !ForWindow("RootPosition", () =>
                    {
                        var saved = false;

                        if (true
                            && _loadedModel.MotionData is InterpolatedMotion motionData
                            && motionData.RootPosition is RootPosition rootPosition)
                        {
                            ForEdit3("Scaling",
                                () => new Vector3(rootPosition.ScaleX, rootPosition.ScaleY, rootPosition.ScaleZ),
                                it =>
                                {
                                    rootPosition.ScaleX = it.X;
                                    rootPosition.ScaleY = it.Y;
                                    rootPosition.ScaleZ = it.Z;
                                    saved = true;
                                }
                            );
                            ForEdit3("Rotation",
                                () => new Vector3(rootPosition.RotateX, rootPosition.RotateY, rootPosition.RotateZ),
                                it =>
                                {
                                    rootPosition.RotateX = it.X;
                                    rootPosition.RotateY = it.Y;
                                    rootPosition.RotateZ = it.Z;
                                    saved = true;
                                }
                            );
                            ForEdit3("Translation",
                                () => new Vector3(rootPosition.TranslateX, rootPosition.TranslateY, rootPosition.TranslateZ),
                                it =>
                                {
                                    rootPosition.TranslateX = it.X;
                                    rootPosition.TranslateY = it.Y;
                                    rootPosition.TranslateZ = it.Z;
                                    saved = true;
                                }
                            );

                            ImGui.Text("FCurveId --");
                            {
                                var array = rootPosition.FCurveId;

                                for (int y = 0; y < 3; y++)
                                {
                                    ImGui.Text(array[3 * y + 0].ToString());
                                    ImGui.SameLine();
                                    ImGui.Text(array[3 * y + 1].ToString());
                                    ImGui.SameLine();
                                    ImGui.Text(array[3 * y + 2].ToString());
                                }
                            }

                            if (ImGui.DragInt("index (slider)##FCurveIdIndex", ref selectedIndex, 0.1f, 0, 8))
                            {

                            }

                            if (0 <= selectedIndex && selectedIndex <= 8)
                            {
                                var maxFCurveId = motionData.FCurvesForward.Count + motionData.FCurvesInverse.Count - 1;

                                ForEdit("FCurves##FCurvesIdFkOrIk",
                                    () => rootPosition.FCurveId[selectedIndex],
                                    it =>
                                    {
                                        rootPosition.FCurveId[selectedIndex] = Math.Max(-1, Math.Min(maxFCurveId, it));
                                        saved = true;
                                    }
                                );

                                ImGui.Text("Goto");
                                ImGui.SameLine();
                                if (ImGui.Button("FCurves##openFCurves"))
                                {
                                    _openWindowUsecase.OpenFCurves(rootPosition.FCurveId[selectedIndex]);
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

                        if (saved)
                        {
                            _loadedModel.SendBackMotionData.TurnOn();
                        }
                    },
                        menuBar: false
                    );

                    if (windowClosed)
                    {
                        _settings.ViewRootPosition = false;
                        _settings.Save();
                    }
                }
            };
        }
    }
}
