using OpenKh.Kh2;
using OpenKh.Tools.Kh2MsetEditor.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace OpenKh.Tools.Kh2MsetEditor.Views
{
    public partial class Motion_Control : UserControl
    {

        public Motion_VM MotionModel { get; set; }
        public Motion_Control()
        {
            InitializeComponent();
        }
        public Motion_Control(Motion.InterpolatedMotion motion)
        {
            InitializeComponent();
            MotionModel = new Motion_VM(motion);
            DataContext = MotionModel;
        }

        public void button_exportAsJson(object sender, RoutedEventArgs e)
        {
            MotionModel.exportAsJson();
        }
        public void button_exportAsMotion(object sender, RoutedEventArgs e)
        {
            MotionModel.exportAsMotion();
        }

        public void button_multiplyTimes(object sender, RoutedEventArgs e)
        {
            MotionModel.multiplyTimes();
            KeyTimes.Items.Refresh();
        }
        public void button_startFrom(object sender, RoutedEventArgs e)
        {
            MotionModel.startFrom();
            KeyTimes.Items.Refresh();
        }
    }
}
