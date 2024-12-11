using System;
using System.Windows.Forms;
using OpenKh.Bbs.SystemData;

namespace OpenKh.Tools.ItcEditor
{
    public partial class ItcEntry : UserControl
    {
        public ItcEntry()
        {
            InitializeComponent();
            ItemIDComboBox.DataSource = Enum.GetValues(typeof(Item.Type));
        }

        private void ItemIDComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

    }
}
