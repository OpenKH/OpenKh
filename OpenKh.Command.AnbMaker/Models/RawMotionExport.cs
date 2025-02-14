using System.Numerics;

namespace OpenKh.Command.AnbMaker.Models
{
    internal class RawMotionExport
    {
        public int BoneCount { get; set; }
        public int FrameCount { get; set; }
        public float FramesPerSecond { get; set; }
        public IEnumerable<Bone> Bones { get; set; }

        public class Bone
        {
            public IEnumerable<KeyFrame> KeyFrames { get; set; }
        }

        public class KeyFrame
        {
            /// <summary>
            /// 0, 1, 2, 3, ...
            /// </summary>
            public float KeyTime { get; set; }

            public Matrix4x4 AbsoluteMatrix { get; set; }
        }
    }
}
