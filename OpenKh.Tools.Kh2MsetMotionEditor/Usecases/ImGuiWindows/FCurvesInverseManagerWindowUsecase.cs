using OpenKh.Tools.Kh2MsetMotionEditor.Interfaces;
using System;

namespace OpenKh.Tools.Kh2MsetMotionEditor.Usecases.ImGuiWindows
{
    public class FCurvesInverseManagerWindowUsecase : IWindowRunnableProvider
    {
        private readonly FCurvesManagerUsecase _fcurvesManagerUsecase;

        public FCurvesInverseManagerWindowUsecase(FCurvesManagerUsecase fcurvesManagerUsecase)
        {
            _fcurvesManagerUsecase = fcurvesManagerUsecase;
        }

        public Action CreateWindowRunnable()
        {
            return _fcurvesManagerUsecase.CreateWindowRunnable(false);
        }
    }
}
