using OpenKh.Kh2;
using OpenKh.Tools.Kh2ObjectEditor.Services;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using static OpenKh.Kh2.MotionTrigger;

namespace OpenKh.Tools.Kh2ObjectEditor.Modules.Motions
{
    public class MotionTriggers_VM
    {
        public AnimationBinary AnimBinary { get; set; }

        public ObservableCollection<RangeTrigger> RangeTriggerList { get; set; }
        public ObservableCollection<FrameTrigger> FrameTriggerList { get; set; }

        public static Dictionary<byte,string> RangeTriggerOptions { get; set; }
        public static Dictionary<byte, string> FrameTriggerOptions { get; set; }

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
            loadOptions();
            AnimBinary = animBinary;

            RangeTriggerList = new ObservableCollection<RangeTrigger>();
            FrameTriggerList = new ObservableCollection<FrameTrigger>();
            loadLists();
        }

        public void loadLists()
        {
            if (AnimBinary.MotionTriggerFile != null)
            {
                RangeTriggerList.Clear();
                foreach (RangeTrigger trigger in AnimBinary.MotionTriggerFile.RangeTriggerList)
                {
                    RangeTriggerList.Add(trigger);
                }
                FrameTriggerList.Clear();
                foreach (FrameTrigger trigger in AnimBinary.MotionTriggerFile.FrameTriggerList)
                {
                    FrameTriggerList.Add(trigger);
                }
            }
        }
        public void saveMotion()
        {
            AnimBinary.MotionTriggerFile.RangeTriggerList.Clear();
            foreach (RangeTrigger trigger in RangeTriggerList)
            {
                AnimBinary.MotionTriggerFile.RangeTriggerList.Add(trigger);
            }
            AnimBinary.MotionTriggerFile.FrameTriggerList.Clear();
            foreach (FrameTrigger trigger in FrameTriggerList)
            {
                AnimBinary.MotionTriggerFile.FrameTriggerList.Add(trigger);
            }
            MsetService.Instance.SaveMotion();
        }

        private void loadOptions()
        {
            RangeTriggerOptions = TriggerDictionary.Range;
            FrameTriggerOptions = TriggerDictionary.Frame;
        }
    }
}
