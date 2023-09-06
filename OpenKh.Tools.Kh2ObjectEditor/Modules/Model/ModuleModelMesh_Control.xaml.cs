using OpenKh.Kh2.Models;
using System.Windows.Controls;

namespace OpenKh.Tools.Kh2ObjectEditor.Views
{
    public partial class ModuleModelMesh_Control : UserControl
    {
        public ModelSkeletal.SkeletalGroup Group { get; set; }
        public ModuleModelMesh_Control(ModelSkeletal.SkeletalGroup group)
        {
            InitializeComponent();
            Group = group;
            DataContext = Group;
        }
    }
}
