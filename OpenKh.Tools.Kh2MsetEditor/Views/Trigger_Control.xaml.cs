using OpenKh.Kh2;
using OpenKh.Tools.Kh2MsetEditor.ViewModels;
using System.Windows.Controls;

namespace OpenKh.Tools.Kh2MsetEditor.Views
{
    public partial class Trigger_Control : UserControl
    {
        public Trigger_VM Trigger_VM { get; set; }
        public Trigger_Control()
        {
            InitializeComponent();
        }
        public Trigger_Control(MotionTrigger motionTrigger)
        {
            InitializeComponent();
            Trigger_VM = new Trigger_VM(motionTrigger);
            DataContext = Trigger_VM;
        }
    }
}
