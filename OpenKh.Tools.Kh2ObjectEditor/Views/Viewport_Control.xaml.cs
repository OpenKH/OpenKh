using System.Windows.Controls;

namespace OpenKh.Tools.Kh2ObjectEditor.Views
{
    public partial class Viewport_Control : UserControl
    {
        public Viewport_ViewModel ThisVM { get; set; }

        public Viewport_Control()
        {
            InitializeComponent();
            ThisVM = new Viewport_ViewModel();
            DataContext = ThisVM;
        }

        public Viewport_Control(Main_ViewModel mainVM)
        {
            InitializeComponent();
            ThisVM = new Viewport_ViewModel(mainVM, Viewport);
            DataContext = ThisVM;
        }
    }
}
