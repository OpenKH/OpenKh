using System;
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
            Subscribe();
        }

        public MotionSelector_Control(Main_ViewModel mainVM)
        {
            InitializeComponent();
            ThisVM = new MotionSelector_ViewModel(mainVM);

            if (mainVM.FolderPath != null)
            {
                //loadObjects();
            }

            DataContext = ThisVM;
            Subscribe();
        }

        public void applyFilters(string name, bool hasMset)
        {
            ThisVM.applyFilters(name, hasMset);
        }

        private void Button_ApplyFilters(object sender, System.Windows.RoutedEventArgs e)
        {
            applyFilters(FilterName.Text, (bool)FilterHideDummies.IsChecked);
        }

        private void list_doubleCLick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            /*Trace.WriteLine("Clicked - Current count: " + ThisVM.Objects.Count);
            ObjectSelector_Wrapper iObject = new ObjectSelector_Wrapper();
            iObject.FileName = "TEST" + ThisVM.Objects.Count;
            //ThisVM.Objects.Add(iObject);

            ObjectSelector_Wrapper item = (ObjectSelector_Wrapper)(sender as ListView).SelectedItem;
            if (item != null)
            {
                ThisVM.MainVM.loadObject(new Object_Wrapper(item.FilePath.ToLower(), item.FilePath.ToLower().Replace(".mdlx", ".mset")));
                //ThisVM.MainVM.LoadedObject = new Object_Wrapper(item.FilePath.ToLower(), item.FilePath.ToLower().Replace(".mdlx",".mset"));
                Trace.WriteLine("Selected: " + item.FileName);
            }*/
        }
        // Event load file
        public void Subscribe()
        {
            ThisVM.MainVM.Load += new Main_ViewModel.EventHandler(MyFunction);
        }
        private void MyFunction(Main_ViewModel m, EventArgs e)
        {
            ThisVM.loadMotions();
            applyFilters(FilterName.Text, (bool)FilterHideDummies.IsChecked);
        }
    }
}
