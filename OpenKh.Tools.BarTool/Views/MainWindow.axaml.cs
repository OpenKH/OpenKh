using System.IO;
using System.Linq;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;

using OpenKh.Kh2;
using OpenKh.Tools.BarTool.Models;
using OpenKh.Tools.BarTool.ViewModels;


namespace OpenKh.Tools.BarTool.Views
{
    public class MainWindow : Window
    {
        public static MainWindow Instance;

        public Bar CurrentFile;
        public bool IsSaved = true;

        public MainWindow()
        {
            InitializeComponent();
            AddHandler(DragDrop.DropEvent, DropEvent);

            #if DEBUG
            this.AttachDevTools();
            #endif

            Instance = this;
        }

        void DropEvent(object sender, DragEventArgs e)
        {
            var _fileList = e.Data.GetFileNames();

            if (_fileList != null)
            {
                var _viewModel = DataContext as MainWindowViewModel;

                if (_viewModel != null)
                    _viewModel.OpenDrop(_fileList.ElementAt(0));
            }
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
