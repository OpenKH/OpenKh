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
    public class FCurvesForwardManagerWindowUsecase : IWindowRunnableProvider
    {
        private readonly FCurvesManagerUsecase _fcurvesManagerUsecase;

        public FCurvesForwardManagerWindowUsecase(FCurvesManagerUsecase fcurvesManagerUsecase)
        {
            _fcurvesManagerUsecase = fcurvesManagerUsecase;
        }

        public Action CreateWindowRunnable()
        {
            return _fcurvesManagerUsecase.CreateWindowRunnable(true);
        }
    }
}
