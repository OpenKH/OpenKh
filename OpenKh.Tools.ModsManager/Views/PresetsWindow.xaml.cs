using OpenKh.Tools.ModsManager.ViewModels;
using System.Windows;
using System.Windows.Controls;
using Xe.Tools.Wpf.Commands;

namespace OpenKh.Tools.ModsManager.Views
{
    public partial class PresetsWindow : Window
    {
        public MainViewModel MainVm { get; set; }
        public RelayCommand CloseCommand { get; }
        public string PresetName { get; set; }

        public PresetsWindow()
        {
            InitializeComponent();
            DataContext = this;

            CloseCommand = new RelayCommand(_ => Close());
        }
        public PresetsWindow(MainViewModel mvm)
        {
            MainVm = mvm;
            InitializeComponent();
            DataContext = this;

            CloseCommand = new RelayCommand(_ => Close());
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            MainVm.SavePreset(txtSourceModUrl.Text);
        }

        private void txtSourceModUrl_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
                Save_Click(sender, e);

            e.Handled = true;
        }

        private void Button_ApplyPreset(object sender, RoutedEventArgs e)
        {
            if (List_Presets.SelectedItem == null)
                return;

            string presetName = (string)List_Presets.SelectedItem;
            MainVm.LoadPreset(presetName);
            Close();
        }

        private void Button_RemovePreset(object sender, RoutedEventArgs e)
        {
            if (List_Presets.SelectedItem == null)
                return;
            string presetName = (string)List_Presets.SelectedItem;
            MessageBoxResult messageBoxResult = MessageBox.Show($"Do you want to remove {presetName} preset.", "Delete Confirmation", MessageBoxButton.YesNo);
            if (messageBoxResult == MessageBoxResult.Yes)
            {                
                MainVm.RemovePreset(presetName);
            }
                
        }
    }
}
