using OpenKh.Common;
using OpenKh.Kh2;
using OpenKh.Kh2Anim.Mset;
using OpenKh.Tools.Kh2MsetEditorCrazyEdition.Helpers;
using OpenKh.Tools.Kh2MsetEditorCrazyEdition.Models.BoneDictSpec;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using static OpenKh.Kh2.Motion;

namespace OpenKh.Tools.Kh2MsetEditorCrazyEdition.Usecases
{
    public class LoadMotionDataUsecase
    {
        private readonly BoneDictElement _boneDictElement;
        private readonly FilterBoneViewUsecase _filterBoneViewUsecase;
        private readonly LoadedModel _loadedModel;

        public LoadMotionDataUsecase(
            LoadedModel loadedModel,
            FilterBoneViewUsecase filterBoneViewUsecase,
            BoneDictElement boneDictElement
        )
        {
            _boneDictElement = boneDictElement;
            _filterBoneViewUsecase = filterBoneViewUsecase;
            _loadedModel = loadedModel;
        }

        internal void LoadAt(int index)
        {
            var barEntries = _loadedModel.AnbEntries = Bar.Read(
                _loadedModel.MsetEntries![index].Stream
                    .FromBegin()
            );

            var motionEntry = barEntries.Single(it => it.Type == Bar.EntryType.Motion);
            _loadedModel.MotionData = GetCacheOrLoad(motionEntry);

            _loadedModel.IKJointDescriptions.Clear();
            _loadedModel.IKJointDescriptions.AddRange(
                ConvertBones(
                    _loadedModel.MotionData.IKHelpers,
                    _loadedModel.FKJointDescriptions.Count
                )
            );
            _loadedModel.JointDescriptionsAge.Bump();

            _loadedModel.ActiveIkBoneViews = _filterBoneViewUsecase
                .Filter(
                    _boneDictElement.BoneView,
                    _loadedModel.MotionList[index].BoneViewMatcher
                );

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

        private InterpolatedMotion GetCacheOrLoad(Bar.Entry motionEntry)
        {
            _loadedModel.InterpolatedMotionCache.TryGetValue(motionEntry, out InterpolatedMotion? motion);

            if (motion == null)
            {
                motion = new InterpolatedMotion(motionEntry.Stream);

                _loadedModel.InterpolatedMotionCache[motionEntry] = motion;
            }

            return motion;
        }

        private IEnumerable<JointDescription> ConvertBones(List<IKHelper> bones, int numFkBones)
        {
            int GetDepthOf(int relativeIndex)
            {
                var depth = 0;
                while (true)
                {
                    var parent = bones[relativeIndex].ParentId;
                    if (parent < numFkBones)
                    {
                        break;
                    }
                    else
                    {
                        relativeIndex = parent - numFkBones;
                        ++depth;
                    }
                }
                return depth;
            }

            return bones
                .Select(
                    (bone, index) => new JointDescription(bone.Index, GetDepthOf(index))
                );
        }

        public void Close()
        {
            _loadedModel.InterpolatedMotionCache.Clear();
            _loadedModel.SelectedJointIndex = -1;
        }
    }
}
