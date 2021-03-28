using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace OpenKh.Tools.EpdEditor
{
    public partial class AddTechParam : UserControl
    {
        public AddTechParam()
        {
            InitializeComponent();
        }

        private void AddTechControlButton_Click(object sender, EventArgs e)
        {
            TechControl cont = new TechControl();
            cont.TechParamGBox.Text = "Parameter " + this.Parent.Controls.Count;
            this.Parent.Controls.Add(cont);
            this.Parent.Controls.Add(new AddTechParam());
            this.Parent.Controls.Remove(this);
        }
    }
}
