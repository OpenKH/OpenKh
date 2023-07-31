using OpenKh.Tools.Kh2ObjectEditor.Classes;
using OpenKh.Tools.Kh2ObjectEditor.ViewModel;
using System.Windows.Controls;

namespace OpenKh.Tools.Kh2ObjectEditor.Views
{
    public partial class MotionSelector_Control : UserControl
    {
        MotionSelector_ViewModel ThisVM { get; set; }

        public MotionSelector_Control()
        {
            InitializeComponent();
            ThisVM = new MotionSelector_ViewModel();

            DataContext = ThisVM;
        }

        private void Button_ApplyFilters(object sender, System.Windows.RoutedEventArgs e)
        {
            ThisVM.applyFilters();
        }

        private void list_doubleCLick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            MotionSelector_Wrapper item = (MotionSelector_Wrapper)(sender as ListView).SelectedItem;
            if (item != null && !item.Name.Contains("DUMM"))
            {
                App_Context.Instance.loadMotion(item);
            }
        }
    }
}
