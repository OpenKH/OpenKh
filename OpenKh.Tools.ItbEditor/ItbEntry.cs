using System;
using System.Windows.Forms;
using OpenKh.Bbs.SystemData;

namespace OpenKh.Tools.ItbEditor
{
    public partial class ItbEntry : UserControl
    {
        public ItbEntry()
        {
            InitializeComponent();
        }

        private void ItemIDComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void ItemKindComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch (ItemKindComboBox.SelectedIndex)
            {
                case 0:
                    ItemIDComboBox.DataSource = Enum.GetValues(typeof(Item.Type));
                    break;
                case 1:
                    ItemIDComboBox.DataSource = Enum.GetValues(typeof(Command.Kind));
                    break;
            }
        }
    }
}
