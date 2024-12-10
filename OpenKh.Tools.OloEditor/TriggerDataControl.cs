using System.Windows.Forms;
using OpenKh.Bbs;

namespace OpenKh.Tools.OloEditor
{
    public partial class TriggerDataControl : UserControl
    {
        public TriggerDataControl()
        {
            InitializeComponent();
        }

        public Olo.TriggerData GetTriggerData()
        {
            Olo.TriggerData trgData = new Olo.TriggerData();
            trgData.PositionX = float.Parse(TriggerLocX.Text);
            trgData.PositionY = float.Parse(TriggerLocY.Text);
            trgData.PositionZ = float.Parse(TriggerLocZ.Text);
            trgData.ScaleX = float.Parse(TriggerScaleX.Text);
            trgData.ScaleY = float.Parse(TriggerScaleY.Text);
            trgData.ScaleZ = float.Parse(TriggerScaleZ.Text);
            trgData.Yaw = float.Parse(TriggerYaw.Text);
            trgData.ID = decimal.ToUInt32(NumericTriggerID.Value);
            trgData.Behavior = Olo.MakeTriggerBehavior((Olo.TriggerType)TriggerTypeComboBox.SelectedIndex, (Olo.TriggerShape)TriggerShapeComboBox.SelectedIndex, FireCheckbox.Checked, StopCheckbox.Checked);
            trgData.CTDid = decimal.ToUInt32(NumericCTDID.Value);
            trgData.TypeRef = decimal.ToUInt32(NumericTriggerTID.Value);
            trgData.Param1 = decimal.ToUInt16(NumericUnkParam1.Value);
            trgData.Param2 = decimal.ToUInt16(NumericUnkParam2.Value);

            return trgData;
        }
    }
}
