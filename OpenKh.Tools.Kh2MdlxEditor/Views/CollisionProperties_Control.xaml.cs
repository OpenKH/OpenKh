using OpenKh.Kh2;
using System.Windows.Controls;

namespace OpenKh.Tools.Kh2MdlxEditor.Views
{
    public partial class CollisionProperties_Control : UserControl
    {
        public ObjectCollision Collision { get; set; }
        public CollisionProperties_Control()
        {
            InitializeComponent();
        }
        public CollisionProperties_Control(ObjectCollision collision)
        {
            InitializeComponent();
            this.Collision = collision;
            DataContext = Collision;
        }
    }
}
