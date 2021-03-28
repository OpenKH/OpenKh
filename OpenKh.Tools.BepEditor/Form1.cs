using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using OpenKh.Bbs;

namespace OpenKh.Tools.BepEditor
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        Bep bep = new Bep();
        Stream bepFile;

        private void UpdateParameters(Bep bep)
        {
            for (int i = 0; i < bep.baseParameters.Count; i++)
            {
                BaseParameters baseParam = new BaseParameters();
                baseParam.BaseParamGBox.Text = "Base Param " + (i + 1);
                baseParam.NumericBattleLevel.Value = bep.baseParameters[i].BattleLevel;
                baseParam.NumericBaseAttack.Value = bep.baseParameters[i].BaseAttack;
                baseParam.NumericDefense.Value = bep.baseParameters[i].Defense;
                baseParam.NumericDamageCeiling.Value = bep.baseParameters[i].DamageCeiling;
                baseParam.NumericDamageFloor.Value = bep.baseParameters[i].DamageFloor;
                baseParam.NumericBaseHP.Value = bep.baseParameters[i].BaseHP;
                baseParam.numericBaseEXP.Value = bep.baseParameters[i].BaseEXP;
                BaseStatsLayout.Controls.Add(baseParam);
            }

            for (int j = 0; j < bep.disappearParameters.Count; j++)
            {
                DisappearParameters baseParam = new DisappearParameters();
                baseParam.DisappearGBox.Text = "Disappear Param " + (j + 1);
                baseParam.WorldIDComboBox.SelectedIndex = bep.disappearParameters[j].WorldID;
                baseParam.NumericRoomID.Value = bep.disappearParameters[j].RoomID;
                baseParam.DistanceTextBox.Text = bep.disappearParameters[j].Distance.ToString();
                DisappearLayout.Controls.Add(baseParam);
            }
        }

        private void UpdateWriteInfo()
        {
            Bep.Header head = bep.header;
            bep = new Bep();
            bep.header = head;

            foreach (BaseParameters baseParam in BaseStatsLayout.Controls)
            {
                Bep.BaseParameter param = new Bep.BaseParameter();
                param.BattleLevel = decimal.ToUInt16(baseParam.NumericBattleLevel.Value);
                param.BaseAttack = decimal.ToUInt16(baseParam.NumericBaseAttack.Value);
                param.Defense = decimal.ToUInt16(baseParam.NumericDefense.Value);
                param.DamageCeiling = decimal.ToByte(baseParam.NumericDamageCeiling.Value);
                param.DamageFloor = decimal.ToByte(baseParam.NumericDamageFloor.Value);
                param.BaseHP = decimal.ToUInt32(baseParam.NumericBaseHP.Value);
                param.BaseEXP = decimal.ToUInt32(baseParam.numericBaseEXP.Value);
                bep.baseParameters.Add(param);
            }

            foreach (DisappearParameters disappearParam in DisappearLayout.Controls)
            {
                Bep.DisappearParameter param = new Bep.DisappearParameter();
                param.WorldID = (ushort)disappearParam.WorldIDComboBox.SelectedIndex;
                param.RoomID = decimal.ToUInt16(disappearParam.NumericRoomID.Value);
                param.Distance = float.Parse(disappearParam.DistanceTextBox.Text);
                bep.disappearParameters.Add(param);
            }
        }

        private void LoadBEP_Click(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "Base Enemy Parameters files (*.bep)|*.bep|All files (*.*)|*.*";
            DialogResult result = dialog.ShowDialog();

            if (result == DialogResult.OK)
            {
                BaseStatsLayout.Controls.Clear();
                DisappearLayout.Controls.Clear();
                if (bepFile != null)
                    bepFile.Close();
                bepFile = File.OpenRead(dialog.FileName);
                bep = Bep.Read(bepFile);
                UpdateParameters(bep);
                SaveBEP.Enabled = true;
            }
        }

        private void SaveBEP_Click(object sender, EventArgs e)
        {
            SaveFileDialog dialog = new SaveFileDialog();
            dialog.Filter = "Base Enemy Parameters files (*.bep)|*.bep|All files (*.*)|*.*";
            DialogResult result = dialog.ShowDialog();

            if (result == DialogResult.OK)
            {
                Stream bepOut = File.OpenWrite(dialog.FileName);
                UpdateWriteInfo();
                Bep.Write(bepOut, bep);
                bepOut.Close();
            }

            MessageBox.Show("File saved successfully!", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}
