using OpenKh.Tools.Kh2ObjectEditor.Classes;
using System.Collections.ObjectModel;

namespace OpenKh.Tools.Kh2ObjectEditor.Views
{
    public class MotionSelector_ViewModel
    {
        public Main_ViewModel MainVM { get; set; }
        public ObservableCollection<MotionSelector_Wrapper> Motions { get; set; }
        public ObservableCollection<MotionSelector_Wrapper> MotionsView { get; set; }
        public MotionSelector_Wrapper SelectedMotion { get; set; }

        public MotionSelector_ViewModel()
        {
            MainVM = new Main_ViewModel();
            Motions = new ObservableCollection<MotionSelector_Wrapper>();
            MotionsView = new ObservableCollection<MotionSelector_Wrapper>();
            loadMotions();
            applyFilters(null, false);
        }

        public MotionSelector_ViewModel(Main_ViewModel mainVM)
        {
            MainVM = mainVM;
            Motions = new ObservableCollection<MotionSelector_Wrapper>();
            MotionsView = new ObservableCollection<MotionSelector_Wrapper>();
            loadMotions();
            applyFilters(null, false);
        }

        public void loadMotions()
        {
            Motions.Clear();
            if (MainVM.LoadedObject?.MsetEntries == null)
                return;

            foreach (MotionSelector_Wrapper msetEntry in MainVM.LoadedObject.MsetEntries)
            {
                Motions.Add(msetEntry);
            }
        }

        public void applyFilters(string name, bool hideDummies)
        {
            MotionsView.Clear();
            foreach (MotionSelector_Wrapper iMotion in Motions)
            {
                if (name != null && name != "" && !iMotion.Name.ToLower().Contains(name.ToLower()))
                {
                    continue;
                }
                if (hideDummies && iMotion.IsDummy)
                {
                    continue;
                }

                MotionsView.Add(iMotion);
            }
        }
    }
}
