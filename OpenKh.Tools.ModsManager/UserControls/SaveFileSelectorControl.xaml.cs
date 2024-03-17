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
    /// Interaction logic for SaveFileSelectorControl.xaml
    /// </summary>
    public partial class SaveFileSelectorControl : UserControl
    {
        public SaveFileSelectorControl()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty FilePathProperty = DependencyProperty.Register(
            nameof(FilePath),
            typeof(string),
            typeof(SaveFileSelectorControl),
            new PropertyMetadata(string.Empty)
        );

        public string FilePath
        {
            get => (string)GetValue(FilePathProperty);
            set => SetValue(FilePathProperty, value);
        }

        public static readonly DependencyProperty FilterProperty = DependencyProperty.Register(
            nameof(Filter),
            typeof(string),
            typeof(SaveFileSelectorControl),
            new PropertyMetadata(string.Empty)
        );

        public string Filter
        {
            get => (string)GetValue(FilterProperty);
            set => SetValue(FilterProperty, value);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            IEnumerable<FileDialogFilter> filters =
                (Filter.Length != 0)
                    ? Filter.Split('|')
                        .Chunk(2)
                        .Select(pair => FileDialogFilter.ByPatterns(pair[0], pair[1].Split(';').AsEnumerable()))
                        .ToArray()
                    : null;

            FileDialog.OnSave(
                path => FilePath = path,
                filters,
                FilePath,
                Window.GetWindow(this)
            );
        }
    }
}
