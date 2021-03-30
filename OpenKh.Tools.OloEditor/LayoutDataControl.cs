using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using OpenKh.Bbs;

namespace OpenKh.Tools.OloEditor
{
    public partial class LayoutDataControl : UserControl
    {
        public LayoutDataControl()
        {
            InitializeComponent();
        }

        uint MissionPos = 0;
        uint ScriptPos = 0;
        uint PathPos = 0;

        public Olo.LayoutData GetLayoutData()
        {
            Olo.LayoutData data = new Olo.LayoutData();

            data.ObjectNameOffset = (decimal.ToUInt32(NumericObjectRefer.Value) * 0x10) + 0x40;
            data.UniqueID = decimal.ToUInt32(NumericUniqueID.Value);
            data.Parameter1 = decimal.ToUInt16(NumericParam1.Value);
            data.Parameter2 = decimal.ToUInt16(NumericParam2.Value);
            data.Parameter3 = decimal.ToUInt16(NumericParam3.Value);
            data.Trigger = decimal.ToUInt16(NumericTrigger.Value);
            data.MessageID = decimal.ToInt32(NumericMessageID.Value);
            data.Parameter5 = float.Parse(Param5Box.Text);
            data.Parameter6 = float.Parse(Param6Box.Text);
            data.Parameter7 = float.Parse(Param7Box.Text);
            data.Parameter8 = float.Parse(Param8Box.Text);

            data.PositionX = float.Parse(PositionX.Text);
            data.PositionY = float.Parse(PositionY.Text);
            data.PositionZ = float.Parse(PositionZ.Text);
            data.RotationX = float.Parse(RotationX.Text);
            data.RotationY = float.Parse(RotationY.Text);
            data.RotationZ = float.Parse(RotationZ.Text);
            data.Height = float.Parse(HeightBox.Text);

            data.LayoutInfo = Olo.MakeLayoutInfo(AppearCheckbox.Checked, LoadOnlyCheckbox.Checked, DeadCheckbox.Checked, (byte)NumericID.Value, ModelDisplayOffCheckbox.Checked,
                (byte)NumericGroupID.Value, NoLoadCheckbox.Checked, (byte)NumericNetworkID.Value);

            data.MissionLabelOffset = MissionPos;
            data.ScriptNameOffset = ScriptPos;
            data.PathNameOffset = PathPos;

            return data;
        }

        public void SetLayoutData(Olo.LayoutData data, int index)
        {
            GBox.Text = "Layout " + (index+1);
            NumericObjectRefer.Value = (data.ObjectNameOffset - 0x40) / 0x10;
            NumericUniqueID.Value = data.UniqueID;
            NumericParam1.Value = data.Parameter1;
            NumericParam2.Value = data.Parameter2;
            NumericParam3.Value = data.Parameter3;
            NumericTrigger.Value = data.Trigger;
            NumericMessageID.Value = data.MessageID;
            Param5Box.Text = data.Parameter5.ToString();
            Param6Box.Text = data.Parameter6.ToString();
            Param7Box.Text = data.Parameter7.ToString();
            Param8Box.Text = data.Parameter8.ToString();

            PositionX.Text = data.PositionX.ToString();
            PositionY.Text = data.PositionY.ToString();
            PositionZ.Text = data.PositionZ.ToString();
            RotationX.Text = data.RotationX.ToString();
            RotationY.Text = data.RotationY.ToString();
            RotationZ.Text = data.RotationZ.ToString();
            HeightBox.Text = data.Height.ToString();

            Olo.LayoutInfo dt = Olo.GetLayoutInfo(data.LayoutInfo);

            AppearCheckbox.Checked = dt.Appear;
            LoadOnlyCheckbox.Checked = dt.LoadOnly;
            DeadCheckbox.Checked = dt.Dead;
            ModelDisplayOffCheckbox.Checked = dt.ModelDisplayOff;
            NoLoadCheckbox.Checked = dt.NoLoad;
            NumericID.Value = dt.ID;
            NumericGroupID.Value = dt.GroupID;
            NumericNetworkID.Value = dt.NetworkID;
            
            MissionPos = data.MissionLabelOffset;
            ScriptPos = data.ScriptNameOffset;
            PathPos = data.PathNameOffset;
        }
    }
}
