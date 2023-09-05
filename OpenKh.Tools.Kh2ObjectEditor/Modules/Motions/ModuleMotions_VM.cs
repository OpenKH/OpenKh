using OpenKh.Kh2;
using OpenKh.Tools.Kh2ObjectEditor.Classes;
using OpenKh.Tools.Kh2ObjectEditor.Services;
using OpenKh.Tools.Kh2ObjectEditor.Utils;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;

namespace OpenKh.Tools.Kh2ObjectEditor.Modules.Motions
{
    public class ModuleMotions_VM : NotifyPropertyChangedBase
    {
        // MOTIONS
        public List<MotionSelector_Wrapper> Motions { get; set; }
        public ObservableCollection<MotionSelector_Wrapper> MotionsView { get; set; }
        public Bar.Entry copiedMotion { get; set; }

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

            if (MsetService.Instance.MsetBar == null)
                return;

            Motions = new List<MotionSelector_Wrapper>();
            for (int i = 0; i < MsetService.Instance.MsetBar.Count; i++)
            {
                Motions.Add(new MotionSelector_Wrapper(i, MsetService.Instance.MsetBar[i]));
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

        public void Motion_Copy(int index)
        {
            Bar.Entry item = MsetService.Instance.MsetBar[index];
            copiedMotion = new Bar.Entry();
            copiedMotion.Index = item.Index;
            copiedMotion.Name = item.Name;
            copiedMotion.Type = item.Type;
            item.Stream.Position = 0;
            copiedMotion.Stream = new MemoryStream();
            item.Stream.CopyTo(copiedMotion.Stream);
            item.Stream.Position = 0;
            copiedMotion.Stream.Position = 0;
        }

        public void Motion_Replace(int index)
        {
            if (copiedMotion == null)
                return;

            MsetService.Instance.MsetBar[index] = copiedMotion;
            loadMotions();
            applyFilters();
        }
        public void Motion_Rename(int index)
        {
            Bar.Entry item = MsetService.Instance.MsetBar[index];
        }
    }
}
