using OpenKh.Kh2;

namespace OpenKh.Tools.Kh2ObjectEditor.ViewModel
{
    public class MotionTriggers_VM
    {
        public AnimationBinary AnimBinary { get; set; }

        public MotionTriggers_VM() { }
        public MotionTriggers_VM(AnimationBinary animBinary)
        {
            AnimBinary = animBinary;
        }
    }
}
