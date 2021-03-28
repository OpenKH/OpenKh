using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace OpenKh.Tools.EpdEditor
{
    public partial class AddDropParam : UserControl
    {
        public AddDropParam()
        {
            InitializeComponent();
        }

        private void AddDropControl_Click(object sender, EventArgs e)
        {
            DropControl cont = new DropControl();
            cont.DropGBox.Text = "Prize " + this.Parent.Controls.Count;
            this.Parent.Controls.Add(cont);
            this.Parent.Controls.Add(new AddDropParam());
            this.Parent.Controls.Remove(this);
        }
    }
}
