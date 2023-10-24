using OpenKh.Common;
using OpenKh.Kh2;
using OpenKh.Kh2Anim.Mset;
using OpenKh.Tools.Kh2MsetMotionEditor.Helpers;
using OpenKh.Tools.Kh2MsetMotionEditor.Models.BoneDictSpec;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using static OpenKh.Kh2.Motion;

namespace OpenKh.Tools.Kh2MsetMotionEditor.Usecases
{
    public class LoadMotionDataUsecase
    {
        private readonly FilterBoneViewUsecase _filterBoneViewUsecase;
        private readonly LoadedModel _loadedModel;

        public LoadMotionDataUsecase(
            LoadedModel loadedModel,
            FilterBoneViewUsecase filterBoneViewUsecase
        )
        {
            _filterBoneViewUsecase = filterBoneViewUsecase;
            _loadedModel = loadedModel;
        }

        internal UpdateMotionData LoadAt(int index)
        {
            Bar barEntries;
            Action? saveAndReload = null;

            void Reload()
            {
                if (_loadedModel.AnbEntries != null)
                {
                    // source is anb file
                    barEntries = _loadedModel.AnbEntries;

                    saveAndReload = () =>
                    {
                        _loadedModel.AnbEntries = AddOrReplaceBarEntry(
                            _loadedModel.AnbEntries,
                            Bar.EntryType.Motion,
                            _loadedModel.MotionData!.toStream().FromBegin()
                        );
                    };
                }
                else if (_loadedModel.MsetEntries != null)
                {
                    // source is mset file
                    barEntries = Bar.Read(
                        _loadedModel.MsetEntries![index].Stream
                            .FromBegin()
                    );

                    saveAndReload = () =>
                    {
                        var subBarStream = _loadedModel.MsetEntries![index].Stream;
                        subBarStream.SetLength(0);

                        Bar.Write(
                            subBarStream,
                            AddOrReplaceBarEntry(
                                barEntries,
                                Bar.EntryType.Motion,
                                _loadedModel.MotionData!.toStream()
                            )
                        );

                        subBarStream.FromBegin();
                    };
                }
                else
                {
                    return;
                }

                var motionEntry = barEntries.Single(it => it.Type == Bar.EntryType.Motion);
                _loadedModel.MotionData = GetCacheOrLoad(motionEntry);

                _loadedModel.PreferredMotionExportXlsx = (_loadedModel.MsetFile != null)
                    ? $"{Path.GetFileNameWithoutExtension(_loadedModel.MsetFile)}_{motionEntry.Name}.xlsx"
                    : $"{Path.GetFileNameWithoutExtension(_loadedModel.MdlxFile!)}_{Path.GetFileNameWithoutExtension(_loadedModel.AnbFile)}.xlsx";

                _loadedModel.IKJointDescriptions.Clear();
                _loadedModel.IKJointDescriptions.AddRange(
                    ConvertBones(
                        _loadedModel.MotionData!.IKHelpers,
                        _loadedModel.FKJointDescriptions.Count
                    )
                );
                _loadedModel.JointDescriptionsAge.Bump();

                _loadedModel.GetActiveIkBoneViews = () => _filterBoneViewUsecase
                    .Filter(
                        _loadedModel.MotionList.Skip(index).FirstOrDefault()?.BoneViewMatcher ?? new string[0]
                    );

                {
                    _loadedModel.PoseProvider = null;

                    var anbIndir = new AnbIndir(barEntries);
                    if (anbIndir.HasAnimationData)
                    {
                        var provider = anbIndir.GetAnimProvider(
                            new MemoryStream(_loadedModel.MdlxBytes!, false)
                        );

                        _loadedModel.FrameLoop = provider.FrameLoop;
                        _loadedModel.FramePerSecond = provider.FramePerSecond;
                        _loadedModel.FrameEnd = provider.FrameEnd;

                        var lastFrameTime = float.MinValue;
                        FkIkMatrices? lastPose = null;

                        _loadedModel.PoseProvider = frameTime =>
                        {
                            if (lastFrameTime == frameTime)
                            {
                                return lastPose!;
                            }
                            else
                            {
                                provider.ResetGameTimeDelta();
                                var pair = provider.ProvideMatrices2(frameTime);
                                lastFrameTime = frameTime;
                                lastPose = new FkIkMatrices(
                                    pair.Fk,
                                    pair.Ik
                                );
                                return lastPose;
                            }
                        };
                    }
                }
            }

            Reload();

            return new UpdateMotionData(
                () =>
                {
                    saveAndReload!();
                    Reload();
                }
            );
        }

        private Bar AddOrReplaceBarEntry(Bar entries, Bar.EntryType type, Stream stream)
        {
            if (entries.SingleOrDefault(it => it.Type == type) is Bar.Entry found)
            {
                found.Stream = stream;
            }
            else
            {
                entries.Add(
                    new Bar.Entry
                    {
                        Name = "xxxx",
                        Stream = stream,
                        Type = type,
                    }
                );
            }
            return entries;
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
            _loadedModel.PreferredMotionExportXlsx = null;
        }
    }
}
