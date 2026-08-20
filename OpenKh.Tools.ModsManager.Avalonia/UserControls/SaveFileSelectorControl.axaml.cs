using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using System.Collections.Generic;
using System.Linq;
using FileDialog = Xe.Tools.Wpf.Dialogs.FileDialog;
using FileDialogFilter = Xe.Tools.Wpf.Dialogs.FileDialogFilter;

namespace OpenKh.Tools.ModsManager.UserControls
{
    public partial class SaveFileSelectorControl : UserControl
    {
        public SaveFileSelectorControl()
        {
            InitializeComponent();
        }

        public static readonly StyledProperty<string> FilePathProperty =
            AvaloniaProperty.Register<SaveFileSelectorControl, string>(nameof(FilePath), string.Empty);

        public string FilePath
        {
            get => GetValue(FilePathProperty);
            set => SetValue(FilePathProperty, value);
        }

        public static readonly StyledProperty<string> FilterProperty =
            AvaloniaProperty.Register<SaveFileSelectorControl, string>(nameof(Filter), string.Empty);

        public string Filter
        {
            get => GetValue(FilterProperty);
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
                TopLevel.GetTopLevel(this) as Window
            );
        }
    }
}
