using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Xe.Tools.Wpf.Dialogs;

namespace OpenKh.Tools.ModsManager.UserControls
{
    /// <summary>
    /// Interaction logic for FolderSelectorControl.xaml
    /// </summary>
    public partial class FolderSelectorControl : UserControl
    {
        public FolderSelectorControl()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty FolderPathProperty = DependencyProperty.Register(
            nameof(FolderPath),
            typeof(string),
            typeof(FolderSelectorControl),
            new PropertyMetadata(string.Empty)
        );

        public string FolderPath
        {
            get => (string)GetValue(FolderPathProperty);
            set => SetValue(FolderPathProperty, value);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            FileDialog.OnFolder(
                path => FolderPath = path,
                FolderPath,
                Window.GetWindow(this)
            );
        }
    }
}
