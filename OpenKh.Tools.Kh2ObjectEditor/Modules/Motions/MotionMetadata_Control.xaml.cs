using OpenKh.Kh2;
using OpenKh.Tools.Kh2ObjectEditor.Services;
using System.Windows;
using System.Windows.Controls;

namespace OpenKh.Tools.Kh2ObjectEditor.Modules.Motions
{
    public partial class MotionMetadata_Control : UserControl
    {
        public AnimationBinary AnimBinary { get; set; }
        public MotionMetadata_Control(AnimationBinary animationBinary)
        {
            InitializeComponent();
            AnimBinary = animationBinary;
            DataContext = AnimBinary;
        }

        private void Button_SaveTriggers(object sender, RoutedEventArgs e)
        {
            MsetService.Instance.SaveMotion();
        }
    }
}
