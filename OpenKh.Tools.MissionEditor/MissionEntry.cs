using System;
using System.Windows.Forms;
using OpenKh.Bbs;

namespace OpenKh.Tools.MissionEditor
{
    public partial class MissionEntry : UserControl
    {
        public MissionEntry()
        {
            InitializeComponent();
            MissionKindComboBox.DataSource = Enum.GetValues(typeof(Mission.MissionKind));
        }

        public Mission.MissionData GetMissionData()
        {
            Mission.MissionData data = new Mission.MissionData();

            data.MissionName = MissionName.Text;
            data.StartEXA = StartEXA.Text;
            data.Script = Script.Text;
            data.SuccessEXA = SuccessEXA.Text;
            data.FailureEXA = FailureEXA.Text;
            data.InformationCTD = decimal.ToUInt32(NumericInformationCTD.Value);
            data.PauseInformationCTD = decimal.ToUInt32(NumericPauseInformationCTD.Value);
            data.Kind = (Mission.MissionKind)MissionKindComboBox.SelectedItem;
            data.Flag = decimal.ToByte(NumericMissionFlag.Value);
            data.Bonus1 = decimal.ToByte(NumericBonus1.Value);
            data.Bonus2 = decimal.ToByte(NumericBonus2.Value);
            data.BonusParam1 = decimal.ToUInt16(NumericBonus1Param.Value);
            data.BonusParam2 = decimal.ToUInt16(NumericBonus2Param.Value);
            data.Navigation = decimal.ToByte(NumericNavigation.Value);
            data.GeneralPath = (Mission.GeneralPathBit)NumericGeneralPath.Value;
            data.Present1 = decimal.ToByte(NumericPresent1.Value);
            data.Present2 = decimal.ToByte(NumericPresent2.Value);
            data.PresentParam1 = decimal.ToUInt16(NumericPresent1Param.Value);
            data.PresentParam2 = decimal.ToUInt16(NumericPresent2Param.Value);
            data.HPRecovery = decimal.ToByte(NumericHPRecovery.Value);
            data.ExtraRecovery = decimal.ToByte(NumericExtraRecovery.Value);

            return data;
        }

        public void SetMissionData(Mission.MissionData data)
        {
            MissionName.Text = data.MissionName;
            StartEXA.Text = data.StartEXA;
            Script.Text = data.Script;
            SuccessEXA.Text = data.SuccessEXA;
            FailureEXA.Text = data.FailureEXA;
            NumericInformationCTD.Value = data.InformationCTD;
            NumericPauseInformationCTD.Value = data.PauseInformationCTD;
            MissionKindComboBox.SelectedItem = data.Kind;
            NumericMissionFlag.Value = data.Flag;
            NumericBonus1.Value = data.Bonus1;
            NumericBonus2.Value = data.Bonus2;
            NumericBonus1Param.Value = data.BonusParam1;
            NumericBonus2Param.Value = data.BonusParam2;
            NumericNavigation.Value = data.Navigation;
            NumericGeneralPath.Value = (decimal)data.GeneralPath;
            NumericPresent1.Value = data.Present1;
            NumericPresent2.Value = data.Present2;
            NumericPresent1Param.Value = data.PresentParam1;
            NumericPresent2Param.Value = data.PresentParam2;
            NumericHPRecovery.Value = data.HPRecovery;
            NumericExtraRecovery.Value = data.ExtraRecovery;
        }
    }
}
