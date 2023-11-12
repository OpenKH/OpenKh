using System.Windows.Controls;
using OpenKh.Tools.Kh2ObjectEditor.ViewModel;

namespace OpenKh.Tools.Kh2ObjectEditor.Views
{
    public partial class Viewport_Control : UserControl
    {
        public Viewport_ViewModel ThisVM { get; set; }

        public Viewport_Control()
        {
            InitializeComponent();
            ThisVM = new Viewport_ViewModel();
            ThisVM.ViewportControl = Viewport;
            DataContext = ThisVM;
        }

        private void Button_PreviousFrame(object sender, System.Windows.RoutedEventArgs e)
        {
            if (!ThisVM.enable_frameCommands)
                return;
            ThisVM.previousFrame();
        }

        private void Button_NextFrame(object sender, System.Windows.RoutedEventArgs e)
        {
            if (!ThisVM.enable_frameCommands)
                return;
            ThisVM.nextFrame();
        }

        private void Button_Reload(object sender, System.Windows.RoutedEventArgs e)
        {
            if (!ThisVM.enable_reload)
                return;
            ThisVM.reload();
        }
    }
}
