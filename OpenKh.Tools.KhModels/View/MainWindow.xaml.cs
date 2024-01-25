using OpenKh.AssimpUtils;
using OpenKh.Tools.Common.Wpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using Xe.Tools.Wpf.Commands;
using Xe.Tools.Wpf.Dialogs;

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

            {
                var loaders = new List<LoaderItem>();

                LoaderItem GetLoaderOf(string extensions, string name)
                {
                    var command = new RelayCommand(
                        _ =>
                        {
                            FileDialog.OnOpen(
                                filePath =>
                                {
                                    thisVM.LoadFilepath(filePath);
                                },
                                FileDialogFilterComposer.Compose()
                                    .AddExtensions(extensions, extensions.Split(','))
                            );
                        }
                    );

                    return new LoaderItem(extensions, name, command);
                }

                loaders.Add(GetLoaderOf("fbx,dae", "Assimp"));
                loaders.Add(GetLoaderOf("mdls", "KH1"));
                loaders.Add(GetLoaderOf("wpn", "KH1 - weapon"));
                loaders.Add(GetLoaderOf("mdlx", "KH2"));
                loaders.Add(GetLoaderOf("map", "KH 2 Map"));
                loaders.Add(GetLoaderOf("pmo", "KH BBS - DDD"));
                loaders.Add(GetLoaderOf("arc", "KH BBS Map"));
                loaders.Add(GetLoaderOf("pmp", "KH DDD Map"));

                _loader.ItemsSource = loaders;
            }
        }

        private record LoaderItem(
            string Extensions,
            string Name,
            ICommand Command
        )
        {
            public string Header => $"{Name} ({Extensions})";
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
        private void Menu_ExportAsFbx(object sender, RoutedEventArgs e)
        {
            thisVM.ExportModel();
        }
        private void Menu_ExportAsBasicDae(object sender, RoutedEventArgs e)
        {
            thisVM.ExportModelBasicDae();
        }
        // Assimp's DAE export doesn't work
        private void Menu_ExportAsDae(object sender, EventArgs e)
        {
            thisVM.ExportModel(AssimpGeneric.FileFormat.collada);
        }

        private void Button_ShowMesh(object sender, RoutedEventArgs e)
        {
            thisVM.ShowMesh = !thisVM.ShowMesh;
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
