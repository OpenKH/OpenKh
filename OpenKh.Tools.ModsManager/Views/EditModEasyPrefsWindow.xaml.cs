using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using System.Windows.Shapes;

namespace OpenKh.Tools.ModsManager.Views
{
    /// <summary>
    /// Interaction logic for EditModPrefWindow.xaml
    /// </summary>
    public partial class EditModEasyPrefsWindow : Window
    {
        public EditModEasyPrefsWindow()
        {
            InitializeComponent();
        }

        public TViewModel ViewModel
        {
            get => (TViewModel)DataContext;
            set => DataContext = value;
        }

        public record TViewModel(
            Action OnSave,
            string PropertyGridCaption,
            object PropertyGridSelectedObject
        );

        protected override void OnClosing(CancelEventArgs e)
        {
            switch (MessageBox.Show(this, "Save changes?", "ModsManager", MessageBoxButton.YesNoCancel))
            {
                case MessageBoxResult.Yes:
                    ViewModel?.OnSave?.Invoke();
                    DialogResult = true;
                    break;
                case MessageBoxResult.No:
                    DialogResult = false;
                    break;
                default:
                    e.Cancel = true;
                    break;
            }
        }
    }
}
