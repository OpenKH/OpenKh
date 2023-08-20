using OpenKh.Tools.Kh2ObjectEditor.Classes;
using OpenKh.Tools.Kh2ObjectEditor.Services;
using OpenKh.Tools.Kh2ObjectEditor.Utils;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace OpenKh.Tools.Kh2ObjectEditor.Modules.Motions
{
    public class ModuleMotions_VM : NotifyPropertyChangedBase
    {
        // MOTIONS
        public List<MotionSelector_Wrapper> Motions { get; set; }
        public ObservableCollection<MotionSelector_Wrapper> MotionsView { get; set; }

        // FILTERS
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

        // CONSTRUCTOR
        public ModuleMotions_VM()
        {
            Motions = new List<MotionSelector_Wrapper>();
            MotionsView = new ObservableCollection<MotionSelector_Wrapper>();
            loadMotions();
            applyFilters();
        }

        // FUNCTIONS
        public void loadMotions()
        {
            Motions.Clear();

            if (Mset_Service.Instance.MsetBar == null)
                return;

            Motions = new List<MotionSelector_Wrapper>();
            for (int i = 0; i < Mset_Service.Instance.MsetBar.Count; i++)
            {
                Motions.Add(new MotionSelector_Wrapper(i, Mset_Service.Instance.MsetBar[i]));
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
    }
}
