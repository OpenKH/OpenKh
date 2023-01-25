using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
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
