using OpenKh.Common;
using OpenKh.Tools.Kh2SystemEditor.ViewModels;
using System.IO;
using System.Windows;

namespace OpenKh.Tools.Kh2SystemEditor.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class SystemEditorView : Window
    {
        public SystemEditorView()
        {
            InitializeComponent();
            DataContext = new SystemEditorViewModel();

#if DEBUG
            var vm = DataContext as SystemEditorViewModel;
            vm.OpenFile(@"D:\Hacking\KH2\reseach\03system.bin");
            File.OpenRead(@"D:\Hacking\KH2\reseach\msg\sys_ps4.bar").Using(x => vm.LoadMessage(x));
#endif
        }
    }
}
