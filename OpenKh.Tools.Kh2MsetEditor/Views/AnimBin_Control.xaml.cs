using OpenKh.Tools.Kh2MsetEditor.ViewModels;
using System.IO;
using System.Windows.Controls;

namespace OpenKh.Tools.Kh2MsetEditor.Views
{
    public partial class AnimBin_Control : UserControl
    {
        public AnimBin_VM animBin_VM { get; set; }
        public AnimBin_Control()
        {
            InitializeComponent();
        }
        public AnimBin_Control(Stream stream)
        {
            InitializeComponent();
            stream.Position = 0;
            animBin_VM = new AnimBin_VM(stream);
            DataContext = animBin_VM;

            if(animBin_VM.AnimationBinaryFile.MotionTriggerFile != null)
                contentFrame_Trigger.Content = new Trigger_Control(animBin_VM.AnimationBinaryFile.MotionTriggerFile);

            if (animBin_VM.AnimationBinaryFile.MotionTriggerFile != null)
                contentFrame_Motion.Content = new Motion_Control(animBin_VM.AnimationBinaryFile.MotionFile);
        }

        public Stream getAnimationBinaryAsStream()
        {
            return animBin_VM.AnimationBinaryFile.toStream();
        }
    }
}
