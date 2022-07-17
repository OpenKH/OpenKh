using OpenKh.Kh2;

namespace OpenKh.Tools.Kh2MsetEditor.ViewModels
{
    public class Trigger_VM
    {
        public MotionTrigger MotionTriggerFile { get; set; }

        public Trigger_VM() { }
        public Trigger_VM(MotionTrigger motionTrigger)
        {
            MotionTriggerFile = motionTrigger;
        }
    }
}
