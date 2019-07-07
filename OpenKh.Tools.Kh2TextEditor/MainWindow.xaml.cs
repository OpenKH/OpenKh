using System;
using System.Windows;
using OpenKh.Tools.Kh2TextEditor.ViewModels;

namespace OpenKh.Tools.Kh2TextEditor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            var vm = new TextEditorViewModel();

            DataContext = vm;
        }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
        }
    }
}
