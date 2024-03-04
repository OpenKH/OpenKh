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

namespace OpenKh.Tools.Kh2FontImageEditor.Views
{
    /// <summary>
    /// Interaction logic for SpacingWindow.xaml
    /// </summary>
    public partial class SpacingWindow : Window
    {
        public SpacingWindow(
            SpacingWindowVM vm
        )
        {
            InitializeComponent();
            DataContext = vm;

            _image.MouseDown += (object sender, MouseButtonEventArgs e) =>
            {
                var spacingDelta = (e.LeftButton == MouseButtonState.Pressed) ? -1
                    : (e.RightButton == MouseButtonState.Pressed) ? +1
                    : 0;
                var point = e.GetPosition(_image);

                vm.State?.AdjustSpacing((int)point.X, (int)point.Y, spacingDelta);
            };
        }
    }
}
