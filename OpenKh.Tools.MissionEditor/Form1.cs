using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using OpenKh.Bbs;
using System.IO;

namespace OpenKh.Tools.MissionEditor
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        Stream missionFile;
        List<Mission.MissionData> mission = new List<Mission.MissionData>();

        private void UpdateParameters()
        {
            FlowMission.Controls.Clear();
            int i = 1;
            foreach(Mission.MissionData miss in mission)
            {
                MissionEntry ent = new MissionEntry();
                ent.groupBox1.Text = "Mission " + i++;
                ent.SetMissionData(miss);
                FlowMission.Controls.Add(ent);
            }
        }

        private void UpdateWriteInfo()
        {
            mission.Clear();
            foreach (MissionEntry miss in FlowMission.Controls)
            {
                mission.Add(miss.GetMissionData());
            }
        }

        private void LoadMissionButton_Click(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "Mission files (*.bin)|*.bin|All files (*.*)|*.*";
            DialogResult result = dialog.ShowDialog();

            if (result == DialogResult.OK)
            {
                if (missionFile != null)
                    missionFile.Close();
                missionFile = File.OpenRead(dialog.FileName);
                mission = Mission.Read(missionFile).ToList();
                UpdateParameters();
                SaveMissionButton.Enabled = true;
                AddMissionButton.Enabled = true;
                missionFile.Close();
            }
        }

        private void SaveMissionButton_Click(object sender, EventArgs e)
        {
            SaveFileDialog dialog = new SaveFileDialog();
            dialog.Filter = "Mission files (*.bin)|*.bin|All files (*.*)|*.*";
            DialogResult result = dialog.ShowDialog();

            if (result == DialogResult.OK)
            {
                Stream missionOut = File.OpenWrite(dialog.FileName);
                UpdateWriteInfo();
                Mission.Write(missionOut, mission);
                missionOut.Close();
                MessageBox.Show("File saved successfully!", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void AddMissionButton_Click(object sender, EventArgs e)
        {
            MissionEntry ent = new MissionEntry();
            ent.groupBox1.Text = "Mission " + (FlowMission.Controls.Count + 1);
            FlowMission.Controls.Add(ent);
        }
    }
}
