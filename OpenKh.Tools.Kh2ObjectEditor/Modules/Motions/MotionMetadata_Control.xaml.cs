using OpenKh.Kh2;
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
    }
}
