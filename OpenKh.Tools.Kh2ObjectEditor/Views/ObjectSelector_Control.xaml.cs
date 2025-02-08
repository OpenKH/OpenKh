using OpenKh.Tools.Kh2ObjectEditor.Classes;
using OpenKh.Tools.Kh2ObjectEditor.ViewModel;
using System.Windows.Controls;
using System.Windows.Input;

namespace OpenKh.Tools.Kh2ObjectEditor.Views
{
    public partial class ObjectSelector_Control : UserControl
    {
        ObjectSelector_ViewModel ThisVM { get; set; }

        public ObjectSelector_Control()
        {
            InitializeComponent();
            ThisVM = new ObjectSelector_ViewModel();
            DataContext = ThisVM;
        }

        private void list_doubleCLick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            ObjectSelector_Wrapper item = (ObjectSelector_Wrapper)(sender as ListView).SelectedItem;
            if (item != null)
            {
                App_Context.Instance.MdlxPath = item.FilePath;
                App_Context.Instance.openObject();
            }
        }

        private void Button_ApplyFilters(object sender, System.Windows.RoutedEventArgs e)
        {
            ThisVM.applyFilters();
        }

        private void FilterName_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                ThisVM.FilterName = FilterName.Text;
                ThisVM.applyFilters();
            }
        }

        private void FilterName_TextChanged(object sender, TextChangedEventArgs e)
        {
            ThisVM.FilterName = FilterName.Text;
            ThisVM.applyFilters();
        }

        private void CheckBox_Checked(object sender, System.Windows.RoutedEventArgs e)
        {
            ThisVM.FilterHasMset = true;
            ThisVM.applyFilters();
        }

        private void CheckBox_Unchecked(object sender, System.Windows.RoutedEventArgs e)
        {
            ThisVM.FilterHasMset = false;
            ThisVM.applyFilters();
        }
    }
}
