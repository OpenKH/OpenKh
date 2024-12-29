using System;
using System.Windows.Forms;
using OpenKh.Bbs.SystemData;

namespace OpenKh.Tools.IteEditor
{
    public partial class IteEntry : UserControl
    {
        public IteEntry()
        {
            InitializeComponent();
            ItemComboBox.DataSource = Enum.GetValues(typeof(Item.Type));
        }
    }
}
