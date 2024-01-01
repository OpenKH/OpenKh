using OpenKh.Tools.ModsManager.Models.ViewHelper;
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
    /// Interaction logic for CopySourceFilesWindow.xaml
    /// </summary>
    public partial class CopySourceFilesWindow : Window
    {
        public CopySourceFilesWindow()
        {
            InitializeComponent();
            DataContext = VM = new CopySourceFilesVM();
        }

        public CopySourceFilesVM VM { get; }
    }
}
