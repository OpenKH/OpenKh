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
using System.Windows.Shapes;

namespace OpenKh.Tools.ModsManager.Views
{
    /// <summary>
    /// Interaction logic for BuildConfirmWindow.xaml
    /// </summary>
    public partial class BuildConfirmWindow : Window
    {
        public BuildConfirmWindow()
        {
            InitializeComponent();
            DataContext = VM = new BuildConfirmWindowVM();
        }

        public BuildConfirmWindowVM VM { get; }
    }
}
