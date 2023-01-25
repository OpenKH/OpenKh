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
    public partial class LayoutGroupControl : UserControl
    {
        public LayoutGroupControl()
        {
            InitializeComponent();
        }

        public Olo.GroupData data = new Olo.GroupData();

        public void GetGroupData()
        {
            if(data != null)
            {
                data.CenterX = float.Parse(CenterX.Text);
                data.CenterY = float.Parse(CenterY.Text);
                data.CenterZ = float.Parse(CenterZ.Text);
                data.Radius = float.Parse(ObjectRadius.Text);
                data.TriggerID = decimal.ToUInt32(NumericAssociatedTrigger.Value);
                data.Flag = Olo.MakeGroupFlag((Olo.AppearType)AppearTypeComboBox.SelectedIndex, GroupFlagCheckboxList.GetItemChecked(0), GroupFlagCheckboxList.GetItemChecked(1),
                    GroupFlagCheckboxList.GetItemChecked(2), decimal.ToByte(NumericStep.Value), GroupFlagCheckboxList.GetItemChecked(3), decimal.ToByte(NumericID.Value), GroupFlagCheckboxList.GetItemChecked(4),
                    GroupFlagCheckboxList.GetItemChecked(5), GroupFlagCheckboxList.GetItemChecked(6), GroupFlagCheckboxList.GetItemChecked(7), decimal.ToByte(NumericGroupID.Value));
                data.AppearParameter = float.Parse(AppearParameterBox.Text);
                data.NextGroupDataOffset = 0;
                data.DeadRate = float.Parse(DeadRateBox.Text);
                data.GameTrigger = decimal.ToUInt16(NumericGameTrigger.Value);
                data.MissionParameter = decimal.ToByte(NumericMissionParam.Value);
                data.UnkParameter = decimal.ToByte(NumericUnknownParam.Value);
            }
        }
    }
}
