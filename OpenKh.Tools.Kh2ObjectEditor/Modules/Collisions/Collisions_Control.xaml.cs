using OpenKh.Kh2;
using OpenKh.Tools.Kh2ObjectEditor.Services;
using System.Windows.Controls;

namespace OpenKh.Tools.Kh2ObjectEditor.Modules.Collisions
{
    public partial class Collisions_Control : UserControl
    {
        public ModelCollision CollisionFile { get; set; }
        public Collisions_Control()
        {
            InitializeComponent();
            this.CollisionFile = Mdlx_Service.Instance.CollisionFile;
            DataContext = CollisionFile;
        }
    }
}
