using OpenKh.Kh2;
using OpenKh.Tools.Kh2MsetEditor.ViewModels;
using System.Windows.Controls;

namespace OpenKh.Tools.Kh2MsetEditor.Views
{
    public partial class Motion_Control : UserControl
    {

        public Motion_VM MotionModel { get; set; }
        public Motion_Control()
        {
            InitializeComponent();
        }
        public Motion_Control(Motion motion)
        {
            InitializeComponent();
            MotionModel = new Motion_VM(motion);
            DataContext = MotionModel;
        }
    }
}
