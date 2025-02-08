using OpenKh.Tools.Kh2ObjectEditor.Classes;
using OpenKh.Tools.Kh2ObjectEditor.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;

namespace OpenKh.Tools.Kh2ObjectEditor.ViewModel
{
    public class ObjectSelector_ViewModel : NotifyPropertyChangedBase
    {
        private string _filterName { get; set; }
        public string FilterName
        {
            get { return _filterName; }
            set
            {
                _filterName = value;
                OnPropertyChanged("FilterName");
            }
        }
        private bool _filterHasMset { get; set; }
        public bool FilterHasMset
        {
            get { return _filterHasMset; }
            set
            {
                _filterHasMset = value;
                OnPropertyChanged("FilterHasMset");
            }
        }
        public List<ObjectSelector_Wrapper> Objects { get; set; }
        public ObservableCollection<ObjectSelector_Wrapper> ObjectsView { get; set; }

        public ObjectSelector_ViewModel()
        {
            FilterName = "";
            Objects = new List<ObjectSelector_Wrapper>();
            ObjectsView = new ObservableCollection<ObjectSelector_Wrapper>();
            applyFilters();

            subscribe_FolderLoaded();
            subscribe_FileLoaded();
        }

        public void loadObjects()
        {
            Objects.Clear();
            App_Context check = App_Context.Instance;

            if (App_Context.Instance.FolderPath == null)
            {
                applyFilters();
                return;
            }

            foreach (string file in Directory.GetFiles(App_Context.Instance.FolderPath))
            {
                if (file.ToLower().EndsWith(".mdlx"))
                {
                    ObjectSelector_Wrapper iObject = new ObjectSelector_Wrapper();
                    iObject.FilePath = file;
                    iObject.FileName = Path.GetFileNameWithoutExtension(file);
                    iObject.HasMset = File.Exists(file.ToLower().Replace(".mdlx", ".mset"));
                    Objects.Add(iObject);
                }
            }
            applyFilters();
        }

        public void applyFilters()
        {
            ObjectsView.Clear();

            bool applyNameFilter = FilterName != null && FilterName != "" & !FilterName.StartsWith(">");
            bool applyDescriptionFilter = FilterName != null && FilterName != "" & FilterName.StartsWith(">");
            bool applyMsetFilter = FilterHasMset;

            foreach(ObjectSelector_Wrapper iObject in Objects)
            {
                if (FilterHasMset && !iObject.HasMset)
                    continue;
            
                if(FilterName != "")
                {
                    string iObjectDescription = iObject.GetDescription();
                    if (!iObject.FileName.ToLower().Contains(FilterName.ToLower()) &&
                        !iObjectDescription.ToLower().Contains(FilterName.ToLower()))
                    {
                            continue;
                    }
                }
            
                ObjectsView.Add(iObject);
            }

            //for (int i = ObjectsView.Count - 1; i >= 0; i--)
            //{
            //    ObjectSelector_Wrapper iObject = ObjectsView[i];
            //    if (applyMsetFilter)
            //    {
            //        if (!iObject.HasMset)
            //        {
            //            ObjectsView.RemoveAt(i);
            //            continue;
            //        }
            //    }
            //    if (applyNameFilter)
            //    {
            //        if (!iObject.FileName.ToLower().Contains(FilterName.ToLower()))
            //        {
            //            ObjectsView.RemoveAt(i);
            //            continue;
            //        }
            //    }
            //    if (applyDescriptionFilter)
            //    {
            //        string searchCriteria = FilterName.ToLower().Substring(1);
            //        string iObjectDescription = iObject.GetDescription();
            //        if (!iObjectDescription.ToLower().Contains(searchCriteria.ToLower()))
            //        {
            //            ObjectsView.RemoveAt(i);
            //            continue;
            //        }
            //    }
            //}
        }

        private void subscribe_FolderLoaded()
        {
            App_Context.Instance.Event_FolderSelected += new App_Context.EventHandler(EH_FolderLoaded);
        }
        private void EH_FolderLoaded(App_Context m, EventArgs e)
        {
            loadObjects();
        }

        private void subscribe_FileLoaded()
        {
            App_Context.Instance.Event_ObjectSelected += new App_Context.EventHandler(EH_ObjectSelected);
        }
        private void EH_ObjectSelected(App_Context m, EventArgs e)
        {
            loadObjects();
        }
    }
}
