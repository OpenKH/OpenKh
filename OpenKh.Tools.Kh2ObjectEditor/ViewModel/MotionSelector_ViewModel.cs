using OpenKh.Tools.Kh2ObjectEditor.Classes;
using OpenKh.Tools.Kh2ObjectEditor.Utils;
using OpenKh.Tools.Kh2ObjectEditor.ViewModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace OpenKh.Tools.Kh2ObjectEditor.Views
{
    public class MotionSelector_ViewModel : NotifyPropertyChangedBase
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
        private bool _filterHideDummies { get; set; }
        public bool FilterHideDummies
        {
            get { return _filterHideDummies; }
            set
            {
                _filterHideDummies = value;
                OnPropertyChanged("FilterHideDummies");
            }
        }

        public List<MotionSelector_Wrapper> Motions { get; set; }
        public ObservableCollection<MotionSelector_Wrapper> MotionsView { get; set; }

        public MotionSelector_ViewModel()
        {
            Motions = new List<MotionSelector_Wrapper>();
            MotionsView = new ObservableCollection<MotionSelector_Wrapper>();
            loadMotions();
            applyFilters();
            subscribe_ObjectSelected();
        }

        public void loadMotions()
        {
            Motions.Clear();

            if (App_Context.Instance.MsetEntries == null)
                return;

            foreach (MotionSelector_Wrapper msetEntry in App_Context.Instance.MsetEntries)
            {
                Motions.Add(msetEntry);
            }
        }

        public void applyFilters()
        {
            MotionsView.Clear();
            foreach (MotionSelector_Wrapper iMotion in Motions)
            {
                if (FilterName != null && FilterName != "" && !iMotion.Name.ToLower().Contains(FilterName.ToLower()))
                {
                    continue;
                }
                if (FilterHideDummies && iMotion.IsDummy)
                {
                    continue;
                }

                MotionsView.Add(iMotion);
            }
        }

        public void subscribe_ObjectSelected()
        {
            App_Context.Instance.Event_ObjectSelected += new App_Context.EventHandler(MyFunction);
        }
        private void MyFunction(App_Context m, EventArgs e)
        {
            loadMotions();
            applyFilters();
        }
    }
}
