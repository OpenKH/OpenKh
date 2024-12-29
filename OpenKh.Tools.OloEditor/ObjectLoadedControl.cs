using System.Linq;
using System.Windows.Forms;
using OpenKh.Bbs.SystemData;

namespace OpenKh.Tools.OloEditor
{
    public partial class ObjectLoadedControl : UserControl
    {
        public ObjectLoadedControl()
        {
            InitializeComponent();
            ObjectLoadedComboBox.DataSource = GameObject.SpawnObjectList.Values.ToList();
        }
    }
}
