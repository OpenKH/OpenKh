using OpenKh.Common;
using OpenKh.Kh2;
using OpenKh.Kh2Anim.Mset;
using OpenKh.Tools.Kh2MsetEditorCrazyEdition.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace OpenKh.Tools.Kh2MsetEditorCrazyEdition.Usecases
{
    public class LoadMotionDataUsecase
    {
        private readonly LoadedModel _loadedModel;

        public LoadMotionDataUsecase(
            LoadedModel loadedModel
        )
        {
            _loadedModel = loadedModel;
        }

        internal void LoadAt(int index)
        {
            var barEntries = _loadedModel.AnbEntries = Bar.Read(
                _loadedModel.MsetEntries![index].Stream
                    .FromBegin()
            );

            var motionEntry = barEntries.Single(it => it.Type == Bar.EntryType.Motion);
            _loadedModel.MotionData = Motion.Read(motionEntry.Stream);

            {
                _loadedModel.PoseProvider = null;

                var anbIndir = new AnbIndir(barEntries);
                if (anbIndir.HasAnimationData)
                {
                    var provider = anbIndir.GetAnimProvider(
                        new MemoryStream(_loadedModel.MdlxBytes!, false)
                    );

                    _loadedModel.FramePerSecond = provider.FramePerSecond;
                    _loadedModel.FrameEnd = provider.FrameEnd;

                    var lastFrameTime = float.MinValue;
                    Matrix4x4[]? lastPose = null;

                    _loadedModel.PoseProvider = frameTime =>
                    {
                        if (lastFrameTime == frameTime)
                        {
                            return lastPose!;
                        }
                        else
                        {
                            provider.ResetGameTimeDelta();
                            var pose = provider.ProvideMatrices(frameTime);
                            lastFrameTime = frameTime;
                            lastPose = pose;
                            return pose;
                        }
                    };
                }
            }
        }
    }
}
