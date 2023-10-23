// #define PLACE_CRASH_BUTTON

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
    public class PrintDebugInfoManagerWindowUsecase : IWindowRunnableProvider
    {
        private readonly Settings _settings;
        private readonly PrintDebugInfo _printDebugInfo;

        public PrintDebugInfoManagerWindowUsecase(
            PrintDebugInfo printDebugInfo,
            Settings settings
        )
        {
            _settings = settings;
            _printDebugInfo = printDebugInfo;
        }

        public Action CreateWindowRunnable()
        {
            return () =>
            {
                if (_settings.ViewDebugInfo)
                {
                    var windowClosed = !ForWindow("PrintDebugInfo manager", () =>
                    {
#if PLACE_CRASH_BUTTON
                        if (ImGui.Button("Crash"))
                        {
                            throw new Exception("Crash");
                        }
#endif

                        foreach (var pair in _printDebugInfo.Printers)
                        {
                            ForHeader(pair.Key, pair.Value, openByDefault: true);
                        }
                    });

                    if (windowClosed)
                    {
                        _settings.ViewDebugInfo = false;
                        _settings.Save();
                    }
                }
            };
        }
    }
}
