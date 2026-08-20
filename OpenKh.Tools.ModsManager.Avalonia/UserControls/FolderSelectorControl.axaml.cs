using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using FileDialog = Xe.Tools.Wpf.Dialogs.FileDialog;

namespace OpenKh.Tools.ModsManager.UserControls
{
    public partial class FolderSelectorControl : UserControl
    {
        public FolderSelectorControl()
        {
            InitializeComponent();
        }

        public static readonly StyledProperty<string> FolderPathProperty =
            AvaloniaProperty.Register<FolderSelectorControl, string>(nameof(FolderPath), string.Empty);

        public string FolderPath
        {
            get => GetValue(FolderPathProperty);
            set => SetValue(FolderPathProperty, value);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            FileDialog.OnFolder(
                path => FolderPath = path,
                FolderPath,
                TopLevel.GetTopLevel(this) as Window
            );
        }
    }
}
