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
using OpenKh.Bbs.SystemData;

namespace OpenKh.Tools.ItbEditor
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }

        Itb itb = new Itb();
        Stream itbFile;

        private void UpdateParameters(Itb itb)
        {
            foreach (TabPage tCon in ItbTabControl.TabPages)
            {
                tCon.Controls[0].Controls.Clear();
            }

            for (int i = 0; i < itb.header.ItemsTotal; i++)
            {
                FlowLayoutPanel currentFPanel = new FlowLayoutPanel();
                switch (itb.AllITB[i].WorldID)
                {
                    case 1:
                        currentFPanel = FlowDP;
                        break;
                    case 2:
                        currentFPanel = FlowSW;
                        break;
                    case 3:
                        currentFPanel = FlowCD;
                        break;
                    case 4:
                        currentFPanel = FlowSB;
                        break;
                    case 5:
                        currentFPanel = FlowYT;
                        break;
                    case 6:
                        currentFPanel = FlowRG;
                        break;
                    case 7:
                        currentFPanel = FlowJB;
                        break;
                    case 8:
                        currentFPanel = FlowHE;
                        break;
                    case 9:
                        currentFPanel = FlowLS;
                        break;
                    case 10:
                        currentFPanel = FlowDI;
                        break;
                    case 11:
                        currentFPanel = FlowPP;
                        break;
                    case 12:
                        currentFPanel = FlowDC;
                        break;
                    case 13:
                        currentFPanel = FlowKG;
                        break;
                    case 15:
                        currentFPanel = FlowVS;
                        break;
                    case 16:
                        currentFPanel = FlowBD;
                        break;
                    case 17:
                        currentFPanel = FlowWM;
                        break;
                }

                ItbEntry itbEntry = new ItbEntry();
                itbEntry.ITB_GBox.Text = "ITB Entry " + (i + 1);
                itbEntry.NumericTreasureBoxID.Value = itb.AllITB[i].TreasureBoxID;
                itbEntry.ItemKindComboBox.SelectedIndex = itb.AllITB[i].ItemKind;

                switch (itbEntry.ItemKindComboBox.SelectedIndex)
                {
                    case 0:
                        itbEntry.ItemIDComboBox.DataSource = Enum.GetValues(typeof(Item.Type));
                        itbEntry.ItemIDComboBox.SelectedItem = (Item.Type)itb.AllITB[i].ItemID;
                        break;
                    case 1:
                        itbEntry.ItemIDComboBox.SelectedItem = (Command.Type)itb.AllITB[i].ItemID;
                        break;
                }


                itbEntry.NumericReportID.Value = itb.AllITB[i].ReportID;
                currentFPanel.Controls.Add(itbEntry);
            }
        }

        private void UpdateWriteInfo()
        {
            for (int i = 0; i < itb.header.ItemsTotal; i++)
            {
                ushort TrsrID = itb.AllITB[i].TreasureBoxID;

                foreach (TabPage page in ItbTabControl.TabPages)
                {
                    foreach (ItbEntry ent in page.Controls[0].Controls)
                    {
                        if (ent.NumericTreasureBoxID.Value == TrsrID)
                        {
                            itb.AllITB[i].ItemKind = (byte)ent.ItemKindComboBox.SelectedIndex;

                            switch (itb.AllITB[i].ItemKind)
                            {
                                case 0:
                                    Item.Type nItem = (Item.Type)ent.ItemIDComboBox.SelectedItem;
                                    ushort ItemVal = (ushort)nItem;
                                    itb.AllITB[i].ItemID = ItemVal;
                                    break;
                                case 1:
                                    Command.Type nCommand = (Command.Type)ent.ItemIDComboBox.SelectedItem;
                                    ushort CommandVal = (ushort)nCommand;
                                    itb.AllITB[i].ItemID = CommandVal;
                                    break;
                            }

                            itb.AllITB[i].ReportID = (byte)ent.NumericReportID.Value;
                            break;
                        }
                    }
                }
            }
        }

        private void LoadITCButton_Click(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "Item Treasure Box files (*.itb)|*.itb|All files (*.*)|*.*";
            DialogResult result = dialog.ShowDialog();

            if (result == DialogResult.OK)
            {
                if (itbFile != null)
                    itbFile.Close();
                itbFile = File.OpenRead(dialog.FileName);
                itb = Itb.Read(itbFile);
                UpdateParameters(itb);
                SaveITBButton.Enabled = true;
                NewChestButton.Enabled = true;
            }
        }

        private void SaveITCButton_Click(object sender, EventArgs e)
        {
            SaveFileDialog dialog = new SaveFileDialog();
            dialog.Filter = "Item Treasure Box files (*.itb)|*.itb|All files (*.*)|*.*";
            DialogResult result = dialog.ShowDialog();

            if (result == DialogResult.OK)
            {
                Stream itbOut = File.OpenWrite(dialog.FileName);
                UpdateWriteInfo();
                Itb.Write(itbOut, itb);
                itbOut.Close();
            }

            MessageBox.Show("File saved successfully!", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void NewChestButton_Click(object sender, EventArgs e)
        {
            itb.header.ItemsTotal++;

            switch (ItbTabControl.SelectedIndex)
            {
                case 1:
                    itb.header.ItemCountDP++;
                    break;
                case 2:
                    itb.header.ItemCountSW++;
                    break;
                case 3:
                    itb.header.ItemCountCD++;
                    break;
                case 4:
                    itb.header.ItemCountSB++;
                    break;
                case 5:
                    itb.header.ItemCountYT++;
                    break;
                case 6:
                    itb.header.ItemCountRG++;
                    break;
                case 7:
                    itb.header.ItemCountJB++;
                    break;
                case 8:
                    itb.header.ItemCountHE++;
                    break;
                case 9:
                    itb.header.ItemCountLS++;
                    break;
                case 10:
                    itb.header.ItemCountDI++;
                    break;
                case 11:
                    itb.header.ItemCountPP++;
                    break;
                case 12:
                    itb.header.ItemCountDC++;
                    break;
                case 13:
                    itb.header.ItemCountKG++;
                    break;
                case 14:
                    itb.header.ItemCountVS++;
                    break;
                case 15:
                    itb.header.ItemCountBD++;
                    break;
                case 16:
                    itb.header.ItemCountWM++;
                    break;
            }

            ItbEntry itEntry = new ItbEntry();
            Itb.ITBData LastData = itb.AllITB[itb.AllITB.Count - 1];
            Itb.ITBData nData = new Itb.ITBData();
            itEntry.ITB_GBox.Text = "ITB Entry " + (LastData.TreasureBoxID + 1);
            itEntry.NumericTreasureBoxID.Value = LastData.TreasureBoxID + 1;
            nData.TreasureBoxID = LastData.TreasureBoxID;
            nData.TreasureBoxID++;
            ItbTabControl.SelectedTab.Controls[0].Controls.Add(itEntry);
            itb.AllITB.Add(nData);
        }
    }
}
