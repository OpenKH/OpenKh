using OpenKh.Kh2;
using OpenKh.Tools.Kh2ObjectEditor.Classes;
using OpenKh.Tools.Kh2ObjectEditor.Modules.Motions;
using OpenKh.Tools.Kh2ObjectEditor.Services;
using OpenKh.Tools.Kh2ObjectEditor.ViewModel;
using System.Windows;
using System.Windows.Controls;

namespace OpenKh.Tools.Kh2ObjectEditor.Views
{
    public partial class ModuleMotions_Control : UserControl
    {
        public ModuleMotions_VM ThisVM { get; set; }
        public ModuleMotions_Control()
        {
            InitializeComponent();
            ThisVM = new ModuleMotions_VM();
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
                try
                {
                    App_Context.Instance.loadMotion(item.Index);
                    //Mset_Service.Instance.loadMotion(item.Index);
                    openMotionTabs(MsetService.Instance.LoadedMotion);
                }
                catch (System.Exception exc)
                {
                    System.Windows.Forms.MessageBox.Show("Maybe you are trying to open a Reaction Command motion", "Animation couldn't be loaded", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                    return;
                }
        }
        }

        private void openMotionTabs(AnimationBinary animBinary)
        {
            Frame_Metadata.Content = new MotionMetadata_Control(animBinary);
            Frame_Triggers.Content = new MotionTriggers_Control(animBinary);
        }
        public void Motion_Copy(object sender, RoutedEventArgs e)
        {
            if (MotionList.SelectedItem != null)
            {
                MotionSelector_Wrapper item = (MotionSelector_Wrapper)MotionList.SelectedItem;
                ThisVM.Motion_Copy(item.Index);
            }
        }
        public void Motion_Replace(object sender, RoutedEventArgs e)
        {
            if (MotionList.SelectedItem != null)
            {
                MotionSelector_Wrapper item = (MotionSelector_Wrapper)MotionList.SelectedItem;
                ThisVM.Motion_Replace(item.Index);
            }
        }
    }
}
