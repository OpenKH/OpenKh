using OpenKh.Kh2.Models;
using System.Windows.Controls;

namespace OpenKh.Tools.Kh2MdlxEditor.Views
{
    public partial class MeshProperties_Control : UserControl
    {
        public ModelSkeletal.SkeletalGroup Group { get; set; }

        public MeshProperties_Control()
        {
            InitializeComponent();
        }
        public MeshProperties_Control(ModelSkeletal.SkeletalGroup group)
        {
            InitializeComponent();
            this.Group = group;
            DataContext = Group;
        }
    }
}
