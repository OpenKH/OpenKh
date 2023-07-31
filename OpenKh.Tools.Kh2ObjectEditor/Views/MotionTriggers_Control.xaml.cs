using OpenKh.Kh2;
using OpenKh.Tools.Kh2ObjectEditor.ViewModel;
using System.Windows.Controls;

namespace OpenKh.Tools.Kh2ObjectEditor.Views
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
    }
}
