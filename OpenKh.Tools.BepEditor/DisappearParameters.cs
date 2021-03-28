using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using OpenKh.Bbs;

namespace OpenKh.Tools.BepEditor
{
    public partial class DisappearParameters : UserControl
    {
        public DisappearParameters()
        {
            InitializeComponent();
            WorldIDComboBox.DataSource = Constants.WorldNames;
        }
    }
}
