using System;
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
