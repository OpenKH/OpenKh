using System.Windows;
using Xe.Tools.Wpf.Commands;

namespace OpenKh.Tools.ModsManager.Views
{
    /// <summary>
    /// SavePreset
    /// </summary>
    public partial class SavePreset : Window
    {

        public RelayCommand CloseCommand { get; }
        public string PresetName { get; set; }

        public SavePreset()
        {
            InitializeComponent();
            DataContext = this;

            CloseCommand = new RelayCommand(_ => Close());
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {    
            DialogResult = true;
            Close();
        }        

        private void txtSourceModUrl_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
                Save_Click(sender, e);

            e.Handled = true;
        }
    }
}
