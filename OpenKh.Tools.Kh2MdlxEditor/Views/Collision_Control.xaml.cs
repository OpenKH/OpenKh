using OpenKh.Kh2;
using OpenKh.Tools.Kh2MdlxEditor.ViewModels;
using System.Windows.Controls;

namespace OpenKh.Tools.Kh2MdlxEditor.Views
{
    public partial class Collision_Control : UserControl
    {
        Collision_VM collisionControlVM { get; set; }

        public Collision_Control()
        {
            InitializeComponent();
        }
        public Collision_Control(ModelCollision collisionFile)
        {
            InitializeComponent();
            collisionControlVM = new Collision_VM(collisionFile);
            DataContext = collisionControlVM;
        }
    }
}
