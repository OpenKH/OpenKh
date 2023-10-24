using OpenKh.Kh2;
using OpenKh.Common;
using OpenKh.Tools.Kh2MsetMotionEditor.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenKh.Tools.Kh2MsetMotionEditor.Usecases
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

            var anbOrMset = File.OpenRead(msetFile).Using(Bar.Read);
            var isMset = anbOrMset.Any(it => it.Type == Bar.EntryType.Anb);

            if (isMset)
            {
                _loadedModel.MsetEntries = anbOrMset;
                _loadedModel.MsetFile = msetFile;

                var barEntries = _loadedModel.MsetEntries!;

                _loadedModel.MotionList.Clear();
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
            else
            {
                _loadedModel.AnbEntries = anbOrMset;
                _loadedModel.AnbFile = msetFile;

                _loadedModel.MotionList.Clear();
                _loadedModel.MotionList.Add(
                    new MotionDisplay(
                        Path.GetFileNameWithoutExtension(msetFile),
                        true,
                        new string[] { Path.GetFileNameWithoutExtension(msetFile), "0", }
                    )
                );
            }
        }

        public void Close()
        {
            _loadedModel.MotionList.Clear();
            _loadedModel.GetActiveFkBoneViews = null;
            _loadedModel.GetActiveIkBoneViews = null;
            _loadedModel.SelectedMotionIndex = -1;
            _loadedModel.MotionData = null;
            _loadedModel.FrameTime = 0;
            _loadedModel.PoseProvider = null;
            _loadedModel.MsetFile = null;
            _loadedModel.AnbEntries = null;
            _loadedModel.AnbFile = null;
        }
    }
}
