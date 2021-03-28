using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace OpenKh.Tools.EpdEditor
{
    public partial class AddExtraParam : UserControl
    {
        public AddExtraParam()
        {
            InitializeComponent();
        }

        private void AddExtraControl_Click(object sender, EventArgs e)
        {
            ExtraControl cont = new ExtraControl();
            cont.ExtraParamGBox.Text = "Extra Param " + this.Parent.Controls.Count;
            cont.ParameterName.Text = "Parameter" + this.Parent.Controls.Count;
            this.Parent.Controls.Add(cont);
            this.Parent.Controls.Add(new AddExtraParam());
            this.Parent.Controls.Remove(this);
        }
    }
}
