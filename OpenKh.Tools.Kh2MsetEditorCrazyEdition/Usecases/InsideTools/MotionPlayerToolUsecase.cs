using ImGuiNET;
using OpenKh.Tools.Kh2MsetEditorCrazyEdition.Helpers;
using OpenKh.Tools.Kh2MsetEditorCrazyEdition.Interfaces;
using System;
using System.Collections.Generic;
using static OpenKh.Tools.Common.CustomImGui.ImGuiEx;

namespace OpenKh.Tools.Kh2MsetEditorCrazyEdition.Usecases.InsideTools
{
    public class MotionPlayerToolUsecase : IToolRunnableProvider
    {
        private readonly LoadedModel _loadedModel;
        private readonly IEnumerable<(string Label, float Speed)> _displayAndSpeed = new (string, float)[]
        {
            ("x0", 0f),
            ("x0.1", 0.1f),
            ("x0.25", 0.25f),
            ("x0.5", 0.50f),
            ("x1", 1f),
            ("x2", 2f),
        };

        public MotionPlayerToolUsecase(
            LoadedModel loadedModel
        )
        {
            _loadedModel = loadedModel;
        }

        public Action CreateToolRunnable()
        {
            float frame = 0;
            float motionSpeed = 0;

            return () =>
            {
                frame += motionSpeed * (1 / Math.Max(1, _loadedModel.FramePerSecond) / 2.0f);

                var lastKeyTime = 60f / _loadedModel.FramePerSecond * _loadedModel.FrameEnd;

                if (lastKeyTime < frame)
                {
                    frame -= lastKeyTime - (60f / _loadedModel.FramePerSecond * _loadedModel.FrameLoop);
                }

                if (_loadedModel.OpenMotionPlayerOnce.Consume())
                {
                    ImGui.SetNextItemOpen(true);
                }

                ForHeader("MotionPlayer", () =>
                {
                    ImGui.Text("Player --");
                    ForEdit("frame", () => frame, it => frame = it);

                    ForEdit("motionSpeed", () => motionSpeed, it => motionSpeed = it, speed: 0.1f);

                    if (ImGui.Button("zero"))
                    {
                        frame = 0;
                    }

                    foreach (var (tuple, index) in _displayAndSpeed.SelectWithIndex())
                    {
                        if (index != 0)
                        {
                            ImGui.SameLine();
                        }

                        if (ImGui.Button(tuple.Label))
                        {
                            motionSpeed = 60.0f * tuple.Speed;
                        }
                    }

                    ImGui.Text("Anb --");
                    ForEdit("FrameLoop", () => _loadedModel.FrameLoop, it => { });
                    ForEdit("FramePerSecond", () => _loadedModel.FramePerSecond, it => { });
                    ForEdit("FrameEnd", () => _loadedModel.FrameEnd, it => { });

                });

                _loadedModel.FrameTime = frame;
            };
        }
    }
}
