using NLog;
using OpenKh.Command.AnbMaker.Utils.Builder.Models;
using System.Numerics;
using static OpenKh.Kh2.Motion;

namespace OpenKh.Command.AnbMaker.Utils.Builder
{
    public class RawMotionBuilder
    {
        public RawMotion Raw { get; }

        public class Parameter
        {
            public int DurationInTicks { get; set; }
            public float TicksPerSecond { get; set; }
            public int BoneCount { get; set; }
            public float NodeScaling { get; set; }
            public ABone[] Bones { get; set; }
            public GetRelativeMatrixDelegate GetRelativeMatrix { get; set; }
        }

        public delegate Matrix4x4 GetRelativeMatrixDelegate(int frameIdx, int boneIdx);

        public RawMotionBuilder(
            Parameter parm
        )
        {
            var logger = LogManager.GetLogger("RawMotionBuilder");

            var nodeFix = System.Numerics.Matrix4x4.CreateScale(
                parm.NodeScaling
            );

            Raw = RawMotion.CreateEmpty();

            var frameCount = (int)parm.DurationInTicks;

            Raw.RawMotionHeader.BoneCount = parm.BoneCount;
            Raw.RawMotionHeader.FrameCount = (int)(frameCount * 60 / parm.TicksPerSecond); // in 1/60 seconds
            Raw.RawMotionHeader.TotalFrameCount = frameCount;
            Raw.RawMotionHeader.FrameData.FrameStart = 0;
            Raw.RawMotionHeader.FrameData.FrameEnd = frameCount - 1;
            Raw.RawMotionHeader.FrameData.FramesPerSecond = (float)parm.TicksPerSecond;

            var fbxArmatureNodes = parm.Bones;

            for (int frameIdx = 0; frameIdx < frameCount; frameIdx++)
            {
                var matrices = new List<System.Numerics.Matrix4x4>();

                for (int boneIdx = 0; boneIdx < parm.BoneCount; boneIdx++)
                {
                    var parentIdx = fbxArmatureNodes[boneIdx].ParentIndex;

                    var parentMatrix = (parentIdx == -1)
                        ? Matrix4x4.Identity
                        : matrices[parentIdx];

                    var absoluteMatrix = System.Numerics.Matrix4x4.Identity
                        * parm.GetRelativeMatrix(frameIdx, boneIdx)
                        * parentMatrix;

                    Raw.AnimationMatrices.Add(nodeFix * absoluteMatrix);
                    matrices.Add(absoluteMatrix);
                }
            }
        }
    }
}
