using OpenKh.Kh2;
using OpenKh.Tools.Kh2ObjectEditor.Services;
using System.Windows;
using System.Windows.Controls;

namespace OpenKh.Tools.Kh2ObjectEditor.Modules.Motions
{
    public partial class MotionTriggers_Control : UserControl
    {
        MotionTriggers_VM ThisVM { get; set; }

        public MotionTriggers_Control(AnimationBinary animBinary)
        {
            InitializeComponent();
            ThisVM = new MotionTriggers_VM(animBinary);
            DataContext = ThisVM;
        }

        private void Button_SaveTriggers(object sender, RoutedEventArgs e)
        {
            ThisVM.saveMotion();
        }
        private void Button_CreateTriggers(object sender, RoutedEventArgs e)
        {
            if(MsetService.Instance.LoadedMotion.MotionTriggerFile != null)
            {
                return;
            }

            MsetService.Instance.LoadedMotion.MotionTriggerFile = new MotionTrigger();
            MsetService.Instance.SaveMotion();
            ThisVM.loadLists();
        }
    }
}
