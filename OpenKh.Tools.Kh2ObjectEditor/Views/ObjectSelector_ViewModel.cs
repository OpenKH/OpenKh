using OpenKh.Tools.Kh2ObjectEditor.Classes;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace OpenKh.Tools.Kh2ObjectEditor.Views
{
    public class ObjectSelector_ViewModel : INotifyPropertyChanged
    {
        public Main_ViewModel MainVM { get; set; }
        public ObservableCollection<ObjectSelector_Wrapper> Objects { get; set; }
        public ObservableCollection<ObjectSelector_Wrapper> ObjectsView { get; set; }
        public ObjectSelector_Wrapper SelectedObject { get; set; }

        public ObjectSelector_ViewModel()
        {
            MainVM = new Main_ViewModel();
            Objects = new ObservableCollection<ObjectSelector_Wrapper>();
            ObjectsView = new ObservableCollection<ObjectSelector_Wrapper>();
            applyFilters(null, false);
        }

        public ObjectSelector_ViewModel(Main_ViewModel mainVM)
        {
            MainVM = mainVM;
            Objects = new ObservableCollection<ObjectSelector_Wrapper>();
            ObjectsView = new ObservableCollection<ObjectSelector_Wrapper>();
            applyFilters(null, false);
        }

        public void applyFilters(string name, bool hasMset)
        {
            ObjectsView.Clear();
            foreach(ObjectSelector_Wrapper iObject in Objects)
            {
                if(name != null && name != "" && !iObject.FileName.ToLower().Contains(name.ToLower()))
                {
                    continue;
                }
                if(hasMset && !iObject.HasMset)
                {
                    continue;
                }

                ObjectsView.Add(iObject);
            }
        }

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
        #endregion
    }
}
