using System;
using System.Windows;
using OpenKh.Tools.Kh2TextEditor.ViewModels;

namespace OpenKh.Tools.Kh2TextEditor.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            DataContext = new MainViewModel();

            InitializeComponent();
        }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
        }
    }
}
