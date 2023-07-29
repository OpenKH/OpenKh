using OpenKh.Kh2;
using OpenKh.Common;
using OpenKh.Tools.Kh2MsetEditorCrazyEdition.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenKh.Tools.Kh2MsetEditorCrazyEdition.Usecases
{
    public class LoadMotionUsecase
    {
        private readonly LoadedModel _loadedModel;

        public LoadMotionUsecase(
            LoadedModel loadedModel
        )
        {
            _loadedModel = loadedModel;
        }

        public void OpenMotion(string msetFile)
        {
            Close();

            _loadedModel.MsetEntries = File.OpenRead(msetFile).Using(Bar.Read);
            _loadedModel.MsetFile = msetFile;

            var barEntries = _loadedModel.MsetEntries!;

            _loadedModel.MotionList.AddRange(
                barEntries
                    .Select(
                        (it, index) =>
                        {
                            return new MotionDisplay(
                                $"[{index}] {it.Name} [{(MotionSet.MotionName)(index / 4)}]",
                                it.Name != "DUMM",
                                new string[] { Path.GetFileNameWithoutExtension(msetFile), it.Name, index + "", }
                            );
                        }
                    )
            );
        }

        public void Close()
        {
            _loadedModel.MotionList.Clear();
            _loadedModel.SelectedMotionIndex = -1;
            _loadedModel.MotionData = null;
            _loadedModel.FrameTime = 0;
            _loadedModel.PoseProvider = null;
            _loadedModel.MsetFile = null;
        }
    }
}
