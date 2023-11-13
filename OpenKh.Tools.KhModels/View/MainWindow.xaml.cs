using OpenKh.AssimpUtils;
using System;
using System.Linq;
using System.Windows;

namespace OpenKh.Tools.KhModels.View
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindowVM thisVM { get; set; }

        public MainWindow()
        {
            InitializeComponent();
            thisVM = new MainWindowVM(viewport);
        }

        private void Grid_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                string firstFile = files?.FirstOrDefault();

                thisVM.LoadFilepath(firstFile);
            }
        }

        private void Menu_DebugReload(object sender, EventArgs e)
        {
            thisVM.LoadFile();
        }
        private void Menu_DebugExportMdls(object sender, EventArgs e)
        {
            thisVM.ExportMdls();
        }
        private void Menu_ExportAsFbx(object sender, EventArgs e)
        {
            thisVM.ExportModel();
        }
        // Assimp's DAE export doesn't work
        private void Menu_ExportAsDae(object sender, EventArgs e)
        {
            thisVM.ExportModel(AssimpGeneric.FileFormat.collada);
        }

        private void Button_ShowMesh(object sender, RoutedEventArgs e)
        {
            thisVM.ShowMesh = ! thisVM.ShowMesh;
            thisVM.SetOptions();
            thisVM.VpService.Render();
        }
        private void Button_ShowWireframe(object sender, RoutedEventArgs e)
        {
            thisVM.ShowWireframe = !thisVM.ShowWireframe;
            thisVM.SetOptions();
            thisVM.VpService.Render();
        }
        private void Button_ShowSkeleton(object sender, RoutedEventArgs e)
        {
            thisVM.ShowSkeleton = !thisVM.ShowSkeleton;
            thisVM.ShowJoints = !thisVM.ShowJoints;
            thisVM.SetOptions();
            thisVM.VpService.Render();
        }
    }
}
