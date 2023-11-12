using OpenKh.Tools.Kh2ObjectEditor.Modules.AI;
using System.Windows;
using System.Windows.Controls;

namespace OpenKh.Tools.Kh2ObjectEditor.Views
{
    public partial class ModuleAI_Control : UserControl
    {
        public ModuleAI_VM ThisVM { get; set; }
        public ModuleAI_Control()
        {
            InitializeComponent();
            ThisVM = new ModuleAI_VM();
            DataContext = ThisVM;
        }

        private void Button_DebugRead(object sender, RoutedEventArgs e)
        {
            ThisVM.read();
        }

        private void Button_Save(object sender, RoutedEventArgs e)
        {
            ThisVM.write();
        }

        private void Button_ClipboardCopy(object sender, RoutedEventArgs e)
        {
            ThisVM.clipboardCopy();
        }
        private void Button_ClipboardPaste(object sender, RoutedEventArgs e)
        {
            ThisVM.clipboardPaste();
        }
    }
}
