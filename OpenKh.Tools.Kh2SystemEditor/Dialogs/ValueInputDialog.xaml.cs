using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace OpenKh.Tools.Kh2SystemEditor.Dialogs
{
    /// <summary>
    /// ColorPickerDialog.xaml の相互作用ロジック
    /// </summary>
    public partial class ValueInputDialog : Window
    {
        public ValueInputDialog()
        {
            InitializeComponent();
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        public static bool Ask(object objectWithValueProperty)
        {
            var window = new ValueInputDialog();
            window.Owner = Application.Current.Windows.OfType<Window>().SingleOrDefault(x => x.IsActive);
            window.textBox.DataContext = objectWithValueProperty;
            if (window.ShowDialog() ?? false)
            {
                return true;
            }
            return false;
        }
    }
}
