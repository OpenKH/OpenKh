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

namespace OpenKh.Tools.OloEditor
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }

        Stream oloFile;
        Olo olo = new Olo();

        private void UpdateParameters()
        {
            FlowObjects.Controls.Clear();
            FlowFiles.Controls.Clear();
            FlowScripts.Controls.Clear();
            FlowMissions.Controls.Clear();
            FlowTriggers.Controls.Clear();
            FlowLayout.Controls.Clear();

            TabObjects.Text = "Objects (" + olo.ObjectList.Count + ")";
            TabFilePath.Text = "Files (" + olo.FileList.Count + ")";
            TabScripts.Text = "Scripts (" + olo.ScriptList.Count + ")";
            TabMissions.Text = "Missions (" + olo.MissionNameList.Count + ")";
            TabTriggers.Text = "Triggers (" + olo.TriggerList.Count + ")";
            TabLayout.Text = "Layout (" + olo.header.GroupDataCount + ")";

            int i = 1;
            foreach (Olo.ObjectName obj in olo.ObjectList)
            {
                ObjectLoadedControl cObjectCon = new ObjectLoadedControl();
                cObjectCon.GBox.Text = "Object Loaded " + i;
                cObjectCon.ObjectLoadedComboBox.SelectedItem = Olo.SpawnObjectList[obj.Name];
                FlowObjects.Controls.Add(cObjectCon);
                i++;
            }

            i = 1;
            foreach (Olo.PathName obj in olo.FileList)
            {
                PathNameControl cFileCon = new PathNameControl();
                cFileCon.GBox.Text = "File Loaded " + i;
                cFileCon.PathNameText.Text = obj.Name;
                FlowFiles.Controls.Add(cFileCon);
                i++;
            }

            i = 1;
            foreach (Olo.PathName obj in olo.ScriptList)
            {
                PathNameControl cScriptCon = new PathNameControl();
                cScriptCon.GBox.Text = "Script Loaded " + i;
                cScriptCon.PathNameText.Text = obj.Name;
                FlowScripts.Controls.Add(cScriptCon);
                i++;
            }

            i = 1;
            foreach (Olo.ObjectName obj in olo.MissionNameList)
            {
                ObjectNameControl cMissionCon = new ObjectNameControl();
                cMissionCon.GBox.Text = "Mission Loaded " + i;
                cMissionCon.ObjectNameText.Text = obj.Name;
                FlowMissions.Controls.Add(cMissionCon);
                i++;
            }

            i = 1;
            foreach (Olo.TriggerData obj in olo.TriggerList)
            {
                TriggerDataControl cTriggerCon = new TriggerDataControl();
                cTriggerCon.GBox.Text = "Trigger " + i;
                cTriggerCon.TriggerLocX.Text = obj.PositionX.ToString();
                cTriggerCon.TriggerLocY.Text = obj.PositionY.ToString();
                cTriggerCon.TriggerLocZ.Text = obj.PositionZ.ToString();
                cTriggerCon.TriggerScaleX.Text = obj.ScaleX.ToString();
                cTriggerCon.TriggerScaleY.Text = obj.ScaleY.ToString();
                cTriggerCon.TriggerScaleZ.Text = obj.ScaleZ.ToString();
                cTriggerCon.TriggerYaw.Text = obj.Yaw.ToString();
                cTriggerCon.NumericTriggerID.Value = obj.ID;
                cTriggerCon.NumericUnkParam1.Value = obj.Param1;
                cTriggerCon.NumericUnkParam2.Value = obj.Param2;
                cTriggerCon.NumericCTDID.Value = obj.CTDid;
                cTriggerCon.NumericTriggerTID.Value = obj.TypeRef;

                Olo.TriggerBehavior behavior = Olo.GetTriggerBehavior(obj.Behavior);

                cTriggerCon.TriggerTypeComboBox.SelectedIndex = (int)behavior.Type;
                cTriggerCon.TriggerShapeComboBox.SelectedIndex = (int)behavior.Shape;
                cTriggerCon.FireCheckbox.Checked = behavior.Fire;
                cTriggerCon.StopCheckbox.Checked = behavior.Stop;

                FlowTriggers.Controls.Add(cTriggerCon);
                i++;
            }

            i = 1;
            foreach (Olo.GroupData obj in olo.GroupList)
            {
                LayoutGroupControl cLayoutCon = new LayoutGroupControl();
                cLayoutCon.GBox.Text = "Group Layout " + i;
                cLayoutCon.CenterX.Text = obj.CenterX.ToString();
                cLayoutCon.CenterY.Text = obj.CenterY.ToString();
                cLayoutCon.CenterZ.Text = obj.CenterZ.ToString();
                cLayoutCon.ObjectRadius.Text = obj.Radius.ToString();
                cLayoutCon.AppearParameterBox.Text = obj.AppearParameter.ToString();
                cLayoutCon.DeadRateBox.Text = obj.DeadRate.ToString();
                cLayoutCon.NumericGameTrigger.Value = obj.GameTrigger;
                cLayoutCon.NumericMissionParam.Value = obj.MissionParameter;
                cLayoutCon.NumericUnknownParam.Value = obj.UnkParameter;

                var gFlag = Olo.GetGroupFlag(obj.Flag);
                cLayoutCon.AppearTypeComboBox.SelectedIndex = (int)gFlag.Type;
                cLayoutCon.NumericStep.Value = gFlag.Step;
                cLayoutCon.NumericID.Value = gFlag.ID;
                cLayoutCon.NumericGroupID.Value = gFlag.GroupID;

                cLayoutCon.GroupFlagCheckboxList.SetItemChecked(0, gFlag.Linked);
                cLayoutCon.GroupFlagCheckboxList.SetItemChecked(1, gFlag.AppearOK);
                cLayoutCon.GroupFlagCheckboxList.SetItemChecked(2, gFlag.LinkInvoke);
                cLayoutCon.GroupFlagCheckboxList.SetItemChecked(3, gFlag.Fire);
                cLayoutCon.GroupFlagCheckboxList.SetItemChecked(4, gFlag.Specified);
                cLayoutCon.GroupFlagCheckboxList.SetItemChecked(5, gFlag.GameTriggerFire);
                cLayoutCon.GroupFlagCheckboxList.SetItemChecked(6, gFlag.MissionFire);
                cLayoutCon.GroupFlagCheckboxList.SetItemChecked(7, gFlag.AllDeadNoAppear);

                FlowLayout.Controls.Add(cLayoutCon);
                i++;
            }
        }

        private void UpdateWriteInfo()
        {

        }

        private void LoadOLOButton_Click(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "Object Layout files (*.olo)|*.olo|All files (*.*)|*.*";
            DialogResult result = dialog.ShowDialog();

            if (result == DialogResult.OK)
            {
                if (oloFile != null)
                    oloFile.Close();
                oloFile = File.OpenRead(dialog.FileName);
                olo = Olo.Read(oloFile);
                UpdateParameters();
                SaveOLOButton.Enabled = true;
            }
        }

        private void SaveOLOButton_Click(object sender, EventArgs e)
        {
            SaveFileDialog dialog = new SaveFileDialog();
            dialog.Filter = "Object Layout files (*.olo)|*.olo|All files (*.*)|*.*";
            DialogResult result = dialog.ShowDialog();

            if (result == DialogResult.OK)
            {
                Stream oloOut = File.OpenWrite(dialog.FileName);
                UpdateWriteInfo();
                //Olo.Write(oloOut, olo);
                oloOut.Close();
            }

            MessageBox.Show("File saved successfully!", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}
