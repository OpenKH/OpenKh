using OpenKh.Kh2;

namespace OpenKh.Tools.Kh2MsetEditor.ViewModels
{
    public class Motion_VM
    {
        public Motion MotionFile { get; set; }

        public Motion_VM() { }
        public Motion_VM(Motion motion)
        {
            MotionFile = motion;
        }
    }
}
