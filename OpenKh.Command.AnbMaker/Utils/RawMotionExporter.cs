using OpenKh.Command.AnbMaker.Models;
using OpenKh.Kh2;

namespace OpenKh.Command.AnbMaker.Utils
{
    internal class RawMotionExporter
    {
        public RawMotionExporter(Motion.RawMotion raw)
        {
            var fps = raw.RawMotionHeader.FrameData.FramesPerSecond;
            var numBones = raw.RawMotionHeader.BoneCount;
            var numFrames = raw.RawMotionHeader.TotalFrameCount;

            Export = new RawMotionExport
            {
                BoneCount = numBones,
                FrameCount = numFrames,
                FramesPerSecond = raw.RawMotionHeader.FrameData.FramesPerSecond,

                Bones = Enumerable.Range(0, numBones)
                    .Select(
                        boneIdx =>
                        {
                            return new RawMotionExport.Bone
                            {
                                KeyFrames = Enumerable.Range(0, numFrames)
                                    .Select(
                                        frameIdx => new RawMotionExport.KeyFrame
                                        {
                                            KeyTime = frameIdx / fps,
                                            AbsoluteMatrix = raw.AnimationMatrices[numBones * frameIdx + boneIdx],
                                        }
                                    ),
                            };
                        }
                    ),
            };
        }

        public RawMotionExport Export { get; }
    }
}
