using OpenKh.Kh2;

namespace OpenKh.Tools.Kh2ObjectEditor.Modules.Motions
{
    public class MotionTriggers_VM
    {
        public AnimationBinary AnimBinary { get; set; }

        public bool HasNoTriggers
        {
            get
            {
                return AnimBinary?.MotionTriggerFile == null;
            }
        }

        public MotionTriggers_VM() { }
        public MotionTriggers_VM(AnimationBinary animBinary)
        {
            AnimBinary = animBinary;
        }
    }
}
