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
        Stream epdFile;

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
            foreach (Control con in AnimationLayoutPanel.Controls)
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

                techCon.TechParamGBox.Text = "Parameter " + (t + 1);
                techCon.TechniquePower.Text = epd.techniqueParameters[t].TechniquePowerCorrection.ToString();
                techCon.NumericTechniqueNumber.Value = epd.techniqueParameters[t].TechniqueNumber;
                techCon.AttackKind.SelectedIndex = (epd.techniqueParameters[t].TechniqueKind > 0x38 ? 0 : epd.techniqueParameters[t].TechniqueKind);
                techCon.AttackAttribute.SelectedIndex = epd.techniqueParameters[t].TechniqueAttribute % 8;
                techCon.NumericSuccessRate.Value = epd.techniqueParameters[t].SuccessRate;
                TechniqueLayout.Controls.Add(techCon);
            }

            AddTechParam techParamPlus = new AddTechParam();
            TechniqueLayout.Controls.Add(techParamPlus);

            // Drop Parameters
            for (int d = 0; d < epd.dropParameters.Count; d++)
            {
                DropControl dropCon = new DropControl();

                dropCon.DropGBox.Text = "Drop Item " + (d + 1);
                dropCon.ItemComboBox.SelectedIndex = (int)epd.dropParameters[d].ItemIndex;
                dropCon.NumericItemCount.Value = epd.dropParameters[d].ItemCount;
                dropCon.NumericItemProbability.Value = epd.dropParameters[d].Probability;
                DroppedLayout.Controls.Add(dropCon);
            }

            AddDropParam dropParamPlus = new AddDropParam();
            DroppedLayout.Controls.Add(dropParamPlus);

            // Extra Parameters
            for (int e = 0; e < epd.extraParameters.Count; e++)
            {
                ExtraControl extraCon = new ExtraControl();

                extraCon.ExtraParamGBox.Text = "Extra Param " + (e + 1);
                extraCon.ParameterName.Text = epd.extraParameters[e].ParameterName;
                extraCon.ParameterValue.Text = epd.extraParameters[e].ParameterValue.ToString();
                ExtraLayout.Controls.Add(extraCon);
            }

            AddExtraParam extraParamPlus = new AddExtraParam();
            ExtraLayout.Controls.Add(extraParamPlus);
        }

        private void UpdateWriteInfo()
        {
            Epd.Header head = epd.header;
            epd = new Epd();

            epd.header = head;

            // General Parameters
            epd.generalParameters = new Epd.GeneralParameters();
            epd.generalParameters.StatusAilmentsFlag =
                Epd.GetStatusAilmentFromStates(
                    StatusAilment_checkbox_01.Checked, StatusAilment_checkbox_02.Checked, StatusAilment_checkbox_03.Checked, StatusAilment_checkbox_04.Checked, StatusAilment_checkbox_05.Checked,
                    StatusAilment_checkbox_06.Checked, StatusAilment_checkbox_07.Checked, StatusAilment_checkbox_08.Checked, StatusAilment_checkbox_09.Checked, StatusAilment_checkbox_10.Checked,
                    StatusAilment_checkbox_11.Checked, StatusAilment_checkbox_12.Checked, StatusAilment_checkbox_13.Checked, StatusAilment_checkbox_14.Checked, StatusAilment_checkbox_15.Checked,
                    StatusAilment_checkbox_16.Checked, StatusAilment_checkbox_17.Checked, StatusAilment_checkbox_18.Checked, StatusAilment_checkbox_19.Checked);
            epd.generalParameters.Health = float.Parse(MaxHealthBox.Text);
            epd.generalParameters.ExperienceMultiplier = float.Parse(EXPMultiplierBox.Text);
            epd.generalParameters.Size = uint.Parse(SizeBox.Text, System.Globalization.NumberStyles.Integer);
            epd.generalParameters.PhysicalDamageMultiplier = float.Parse(PhysicalDamageBox.Text);
            epd.generalParameters.FireDamageMultiplier = float.Parse(FireDamageBox.Text);
            epd.generalParameters.IceDamageMultiplier = float.Parse(IceDamageBox.Text);
            epd.generalParameters.ThunderDamageMultiplier = float.Parse(ThunderDamageBox.Text);
            epd.generalParameters.DarknessDamageMultiplier = float.Parse(DarknessDamageBox.Text);
            epd.generalParameters.NonElementalDamageMultiplier = float.Parse(SpecialDamageBox.Text);

            // Anim list
            epd.AnimationList = new List<char[]>();
            foreach (TextBox txt in AnimationLayoutPanel.Controls)
            {
                char[] arr = new char[4];
                if (txt.Text != "")
                {
                    arr[0] = txt.Text.ToCharArray()[0];
                    arr[1] = txt.Text.ToCharArray()[1];
                    arr[2] = txt.Text.ToCharArray()[2];
                    arr[3] = (char)0;
                }
                epd.AnimationList.Add(arr);
            }

            // Other Parameters
            epd.otherParameters = new Epd.OtherParameters();
            epd.otherParameters.DamageCeiling = Decimal.ToUInt16(NumericDamageCeiling.Value);
            epd.otherParameters.DamageFloor = Decimal.ToUInt16(NumericDamageFloor.Value);
            epd.otherParameters.fWeight = float.Parse(NumericWeight.Text);
            epd.otherParameters.EffectivenessFlag =
                Epd.GetEffectivenessFlagFromStates(decimal.ToUInt32(NumericPoison.Value), decimal.ToUInt32(NumericStop.Value), decimal.ToUInt32(NumericBind.Value),
                                                   decimal.ToUInt32(NumericFaint.Value), decimal.ToUInt32(NumericBlind.Value), decimal.ToUInt32(NumericMini.Value));
            epd.otherParameters.PrizeBoxProbability = decimal.ToSByte(NumericPrizeboxProbability.Value);
            epd.otherParameters.padding = new byte[3];
            epd.otherParameters.TechniqueParameterCount = (uint)(TechniqueLayout.Controls.Count - 1 < 0 ? 0 : TechniqueLayout.Controls.Count - 1);
            epd.otherParameters.TechniqueParameterOffset = 0xA8;
            epd.otherParameters.DropItemsCount = (uint)(DroppedLayout.Controls.Count - 1 < 0 ? 0 : DroppedLayout.Controls.Count - 1);
            epd.otherParameters.DropItemsOffset = 0xA8 + (epd.otherParameters.TechniqueParameterCount * 8);
            epd.otherParameters.ExtraParametersCount = (uint)(ExtraLayout.Controls.Count - 1 < 0 ? 0 : ExtraLayout.Controls.Count - 1);
            epd.otherParameters.ExtraParametersOffset = 0xA8 + ((epd.otherParameters.TechniqueParameterCount + epd.otherParameters.DropItemsCount) * 8);

            // Technique Parameters
            epd.techniqueParameters = new List<Epd.TechniqueParameters>();
            try
            {
                foreach (TechControl tech in TechniqueLayout.Controls)
                {
                    Epd.TechniqueParameters param = new Epd.TechniqueParameters();
                    param.TechniquePowerCorrection = float.Parse(tech.TechniquePower.Text);
                    param.TechniqueNumber = decimal.ToByte(tech.NumericTechniqueNumber.Value);
                    param.TechniqueKind = (byte)tech.AttackKind.SelectedIndex;
                    param.TechniqueAttribute = (byte)tech.AttackAttribute.SelectedIndex;
                    param.SuccessRate = decimal.ToByte(tech.NumericSuccessRate.Value);
                    epd.techniqueParameters.Add(param);
                }
            }
            catch (InvalidCastException)
            {
                Console.WriteLine("Cannot convert to this type.");
            }

            // Drop Parameters
            epd.dropParameters = new List<Epd.DropParameters>();
            try
            {
                foreach (DropControl drop in DroppedLayout.Controls)
                {
                    Epd.DropParameters param = new Epd.DropParameters();
                    param.ItemIndex = (uint)drop.ItemComboBox.SelectedIndex;
                    param.ItemCount = decimal.ToUInt16(drop.NumericItemCount.Value);
                    param.Probability = decimal.ToUInt16(drop.NumericItemProbability.Value);
                    epd.dropParameters.Add(param);
                }
            }
            catch (InvalidCastException)
            {
                Console.WriteLine("Cannot convert to this type.");
            }

            // Extra Parameters
            epd.extraParameters = new List<Epd.ExtraParameters>();
            try
            {
                foreach (ExtraControl extra in ExtraLayout.Controls)
                {
                    Epd.ExtraParameters param = new Epd.ExtraParameters();
                    param.ParameterName = extra.ParameterName.Text;
                    param.ParameterValue = float.Parse(extra.ParameterValue.Text);
                    epd.extraParameters.Add(param);
                }
            }
            catch (InvalidCastException)
            {
                Console.WriteLine("Cannot convert to this type.");
            }
        }

        private void LoadEPDButton_Click(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "Enemy Parameter Data files (*.epd)|*.epd|All files (*.*)|*.*";
            DialogResult result = dialog.ShowDialog();

            if (result == DialogResult.OK)
            {
                TechniqueLayout.Controls.Clear();
                DroppedLayout.Controls.Clear();
                ExtraLayout.Controls.Clear();
                if (epdFile != null)
                    epdFile.Close();
                epdFile = File.OpenRead(dialog.FileName);
                FileLoadedLabel.Text = "File currently loaded: " + dialog.FileName;
                epd = Epd.Read(epdFile);
                UpdateEPDData();
                SaveEPDButton.Enabled = true;
            }
        }

        private void SaveEPDButton_Click(object sender, EventArgs e)
        {
            SaveFileDialog dialog = new SaveFileDialog();
            dialog.Filter = "Enemy Parameter Data files (*.epd)|*.epd|All files (*.*)|*.*";
            DialogResult result = dialog.ShowDialog();

            if (result == DialogResult.OK)
            {
                Stream epdOut = File.OpenWrite(dialog.FileName);
                UpdateWriteInfo();
                Epd.Write(epdOut, epd);
                epdOut.Close();
            }

            MessageBox.Show("File saved successfully!", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}
