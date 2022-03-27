using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace OpenKh.Tools.Kh2ToolHub
{
    internal class MainWindowHandler
    {
        public static void openPage(Frame loadFrame)
        {
            loadFrame.Navigate(new Uri("/OpenKh.Tools.BarEditor;component/Views/BarView.xaml", UriKind.Relative));
        }
    }
}
