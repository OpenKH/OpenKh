using OpenKh.Tools.Kh2ObjectEditor.Services;
using OpenKh.Tools.Kh2ObjectEditor.Views;
using System.Windows;

namespace OpenKh.Tools.Kh2ObjectEditor.Modules.Motions
{
    public partial class MotionRenameWindow : Window
    {
        int MotionIndex { get; set; }
        ModuleMotions_Control ParentControl {  get; set; }
        public MotionRenameWindow(int motionIndex, ModuleMotions_Control parent)
        {
            MotionIndex = motionIndex;
            ParentControl = parent;
            InitializeComponent();
            NameTextBox.Text = MsetService.Instance.MsetBinarc.Entries[MotionIndex].Name;
        }

        private void Button_Rename(object sender, RoutedEventArgs e)
        {
            MsetService.Instance.MsetBinarc.Entries[MotionIndex].Name = NameTextBox.Text;
            ParentControl.ThisVM.Motions[MotionIndex].setName();
            ParentControl.ThisVM.applyFilters();
            this.Close();
        }
        private void Button_Cancel(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
