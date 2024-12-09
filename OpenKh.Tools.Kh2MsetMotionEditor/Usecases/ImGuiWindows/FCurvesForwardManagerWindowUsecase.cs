using OpenKh.Tools.Kh2MsetMotionEditor.Interfaces;
using System;

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
