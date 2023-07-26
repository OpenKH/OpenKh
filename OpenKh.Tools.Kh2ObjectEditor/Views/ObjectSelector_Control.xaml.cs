using OpenKh.Tools.Kh2ObjectEditor.Classes;
using System.Diagnostics;
using System.IO;
using System.Windows.Controls;

namespace OpenKh.Tools.Kh2ObjectEditor.Views
{
    public partial class ObjectSelector_Control : UserControl
    {
        ObjectSelector_ViewModel ThisVM { get; set; }

        public ObjectSelector_Control()
        {
            InitializeComponent();
            ThisVM = new ObjectSelector_ViewModel();

            // TEST
            ObjectSelector_Wrapper object1 = new ObjectSelector_Wrapper();
            object1.FileName = "TEST1";
            ThisVM.Objects.Add(object1);
            ObjectSelector_Wrapper object2 = new ObjectSelector_Wrapper();
            object2.FileName = "TEST2";
            ThisVM.Objects.Add(object2);

            DataContext = ThisVM;
        }

        public ObjectSelector_Control(Main_ViewModel mainVM)
        {
            InitializeComponent();
            ThisVM = new ObjectSelector_ViewModel(mainVM);

            if(mainVM.FolderPath != null)
            {
                loadObjects();
            }

            DataContext = ThisVM;
        }

        public void loadObjects()
        {
            ThisVM.Objects.Clear();
            foreach (string file in Directory.GetFiles(ThisVM.MainVM.FolderPath))
            {
                if (file.ToLower().EndsWith(".mdlx"))
                {
                    ObjectSelector_Wrapper iObject = new ObjectSelector_Wrapper();
                    iObject.FilePath = file;
                    iObject.FileName = Path.GetFileNameWithoutExtension(file);
                    iObject.HasMset = File.Exists(file.ToLower().Replace(".mdlx",".mset"));
                    ThisVM.Objects.Add(iObject);
                }
            }
            applyFilters(null, false);
        }

        public void applyFilters(string name, bool hasMset)
        {
            ThisVM.applyFilters(name, hasMset);
        }

        private void list_doubleCLick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Trace.WriteLine("Clicked - Current count: " + ThisVM.Objects.Count);
            ObjectSelector_Wrapper iObject = new ObjectSelector_Wrapper();
            iObject.FileName = "TEST" + ThisVM.Objects.Count;
            //ThisVM.Objects.Add(iObject);

            ObjectSelector_Wrapper item = (ObjectSelector_Wrapper)(sender as ListView).SelectedItem;
            if (item != null)
            {
                ThisVM.MainVM.loadObject(new Object_Wrapper(item.FilePath.ToLower(), item.FilePath.ToLower().Replace(".mdlx", ".mset")));
                ThisVM.MainVM.LoadedMotion = null;
                //ThisVM.MainVM.LoadedObject = new Object_Wrapper(item.FilePath.ToLower(), item.FilePath.ToLower().Replace(".mdlx",".mset"));
                Trace.WriteLine("Selected: " + item.FileName);
            }
        }

        private void Button_ApplyFilters(object sender, System.Windows.RoutedEventArgs e)
        {
            applyFilters(FilterName.Text, (bool)FilterHasMset.IsChecked);
        }
    }
}
