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

namespace OpenKh.Tools.EpdEditor
{
    public partial class EPDForm : Form
    {
        public EPDForm()
        {
            InitializeComponent();
        }

        Epd epd = new Epd();

        private void UpdateEPDData()
        {
            // General Parameters
            Epd.StatusAilment ailments = Epd.GetStatusAilment(epd);
            StatusAilment_checkbox_01.Checked = ailments.bFly;
            StatusAilment_checkbox_02.Checked = ailments.bSmallDamageReaction;
            StatusAilment_checkbox_03.Checked = ailments.bSmallDamageReactionOnly;
            StatusAilment_checkbox_04.Checked = ailments.bHitback;
            StatusAilment_checkbox_05.Checked = ailments.bPoison;
            StatusAilment_checkbox_06.Checked = ailments.bSlow;
            StatusAilment_checkbox_07.Checked = ailments.bStop;
            StatusAilment_checkbox_08.Checked = ailments.bBind;
            StatusAilment_checkbox_09.Checked = ailments.bFaint;
            StatusAilment_checkbox_10.Checked = ailments.bFreeze;
            StatusAilment_checkbox_11.Checked = ailments.bBurn;
            StatusAilment_checkbox_12.Checked = ailments.bConfuse;
            StatusAilment_checkbox_13.Checked = ailments.bBlind;
            StatusAilment_checkbox_14.Checked = ailments.bDeath;
            StatusAilment_checkbox_15.Checked = ailments.bZeroGravity;
            StatusAilment_checkbox_16.Checked = ailments.bMini;
            StatusAilment_checkbox_17.Checked = ailments.bMagnet;
            StatusAilment_checkbox_18.Checked = ailments.bDegen;
            StatusAilment_checkbox_19.Checked = ailments.bSleep;

            MaxHealthBox.Text = epd.generalParameters.Health.ToString();
            SizeBox.Text = epd.generalParameters.Size.ToString();
            EXPMultiplierBox.Text = epd.generalParameters.ExperienceMultiplier.ToString();
            PhysicalDamageBox.Text = epd.generalParameters.PhysicalDamageMultiplier.ToString();
            FireDamageBox.Text = epd.generalParameters.FireDamageMultiplier.ToString();
            IceDamageBox.Text = epd.generalParameters.IceDamageMultiplier.ToString();
            ThunderDamageBox.Text = epd.generalParameters.ThunderDamageMultiplier.ToString();
            DarknessDamageBox.Text = epd.generalParameters.DarknessDamageMultiplier.ToString();
            SpecialDamageBox.Text = epd.generalParameters.NonElementalDamageMultiplier.ToString();

            // Animations
            int i = 0;
            foreach(Control con in AnimationLayoutPanel.Controls)
            {
                con.Text = new String(epd.AnimationList[i]);
                i++;
            }

            // Other Parameters
            NumericDamageCeiling.Value = epd.otherParameters.DamageCeiling;
            NumericDamageFloor.Value = epd.otherParameters.DamageFloor;
            NumericWeight.Text = epd.otherParameters.fWeight.ToString();

            Epd.EffectivenessFlag flag = Epd.GetEffectivenessFlag(epd);
            NumericPoison.Value = flag.Poison;
            NumericStop.Value = flag.Stop;
            NumericBind.Value = flag.Bind;
            NumericFaint.Value = flag.Faint;
            NumericBlind.Value = flag.Blind;
            NumericMini.Value = flag.Mini;

            NumericPrizeboxProbability.Value = epd.otherParameters.PrizeBoxProbability;


            // Technique Parameters
            for (int t = 0; t < epd.techniqueParameters.Count; t++)
            {
                TechControl techCon = new TechControl();

                techCon.TechParamGBox.Text = "Parameter " + (t+1);
                techCon.TechniquePower.Text = epd.techniqueParameters[t].TechniquePowerCorrection.ToString();
                techCon.TechniqueNumber.Text = epd.techniqueParameters[t].TechniqueNumber.ToString();
                techCon.AttackKind.SelectedIndex = epd.techniqueParameters[t].TechniqueKind;
                techCon.AttackAttribute.SelectedIndex = epd.techniqueParameters[t].TechniqueAttribute % 8;
                techCon.NumericSuccessRate.Value = epd.techniqueParameters[t].SuccessRate;
                TechniqueLayout.Controls.Add(techCon);
            }

            // Drop Parameters
            for (int d = 0; d < epd.dropParameters.Count; d++)
            {
                DropControl dropCon = new DropControl();

                dropCon.DropGBox.Text = "Drop Item " + (d+1);
                dropCon.ItemComboBox.SelectedIndex = (int)epd.dropParameters[d].ItemIndex;
                dropCon.NumericItemCount.Value = epd.dropParameters[d].ItemCount;
                dropCon.NumericItemProbability.Value = epd.dropParameters[d].Probability;
                DroppedLayout.Controls.Add(dropCon);
            }

            // Extra Parameters
            for (int e = 0; e < epd.extraParameters.Count; e++)
            {
                ExtraControl extraCon = new ExtraControl();

                extraCon.ExtraParamGBox.Text = "Extra Param " + (e+1);
                string ParamName = new string(epd.extraParameters[e].ParameterName);
                extraCon.ParameterName.Text = new string(ParamName);
                extraCon.ParameterValue.Text = epd.extraParameters[e].ParameterValue.ToString();
                ExtraLayout.Controls.Add(extraCon);
            }
        }

        private void LoadEPDButton_Click(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "Enemy Parameter Data files (*.epd)|*.epd|All files (*.*)|*.*";
            DialogResult result = dialog.ShowDialog();

            if(result == DialogResult.OK)
            {
                Stream epdFile = File.OpenRead(dialog.FileName);
                FileLoadedLabel.Text = "File currently loaded: " + dialog.FileName;
                epd = Epd.Read(epdFile);
                UpdateEPDData();
                SaveEPDButton.Enabled = true;
            }
        }

        private void SaveEPDButton_Click(object sender, EventArgs e)
        {
            MessageBox.Show("File saved successfully!", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}
