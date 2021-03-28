using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
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
