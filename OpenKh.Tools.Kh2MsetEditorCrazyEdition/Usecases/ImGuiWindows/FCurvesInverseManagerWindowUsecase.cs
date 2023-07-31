using OpenKh.Tools.Kh2MsetEditorCrazyEdition.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenKh.Tools.Kh2MsetEditorCrazyEdition.Usecases.ImGuiWindows
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
