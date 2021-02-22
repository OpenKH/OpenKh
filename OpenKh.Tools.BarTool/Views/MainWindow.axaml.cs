using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;

using OpenKh.Kh2;
using OpenKh.Tools.BarTool.Models;
using OpenKh.Tools.BarTool.ViewModels;
using System.IO;
using System.Linq;

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
                {
                    using (FileStream _stream = new FileStream(_fileList.ElementAt(0), FileMode.Open))
                    {
                        CurrentFile = Bar.Read(_stream);

                        foreach (var _item in CurrentFile)
                        {
                            var _barItem = new EntryModel(_item);
                            _viewModel.Items.Add(_barItem);
                        }

                        _viewModel.FileName = Path.GetFileName(_fileList.ElementAt(0));
                        _viewModel.Title = string.Format("{0} | BAR - OpenKH", _viewModel.FileName);
                    }
                }
            }
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
