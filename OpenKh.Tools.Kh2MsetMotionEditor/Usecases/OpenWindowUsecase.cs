using OpenKh.Tools.Kh2MsetMotionEditor.Helpers;

namespace OpenKh.Tools.Kh2MsetMotionEditor.Usecases
{
    public class OpenWindowUsecase
    {
        private readonly LoadedModel _loadedModel;

        public OpenWindowUsecase(LoadedModel loadedModel)
        {
            _loadedModel = loadedModel;
        }

        public void OpenFCurves(int index)
        {
            if (0 <= index)
            {
                var numFCurvesFK = _loadedModel.MotionData?.FCurvesForward.Count ?? 0;

                if (index < numFCurvesFK)
                {
                    _loadedModel.SelectFCurvesFoward = index;
                }
                else
                {
                    _loadedModel.SelectFCurvesInverse = index - numFCurvesFK;
                }
            }
        }
    }
}
