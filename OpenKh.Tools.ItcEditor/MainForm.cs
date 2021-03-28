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

namespace OpenKh.Tools.ItcEditor
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }

        Itc itc = new Itc();
        Stream itcFile;

        private void UpdateParameters(Itc itc)
        {
            foreach (TabPage tCon in ItcTabControl.TabPages)
            {
                tCon.Controls[0].Controls.Clear();
            }

            for (int i = 0; i < itc.header.ItemsTotal; i++)
            {
                FlowLayoutPanel currentFPanel = new FlowLayoutPanel();
                switch (itc.AllITC[i].WorldID)
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

                ItcEntry itcEntry = new ItcEntry();
                itcEntry.ITC_GBox.Text = "ITC Entry " + (i + 1);
                itcEntry.NumericCollectionID.Value = itc.AllITC[i].CollectionID;
                itcEntry.ItemIDComboBox.SelectedItem = (Item.Type)itc.AllITC[i].ItemID;
                currentFPanel.Controls.Add(itcEntry);
            }
        }

        private void UpdateWriteInfo()
        {
            for (int i = 0; i < itc.header.ItemsTotal; i++)
            {
                ushort CollID = itc.AllITC[i].CollectionID;

                foreach (TabPage page in ItcTabControl.TabPages)
                {
                    foreach (ItcEntry ent in page.Controls[0].Controls)
                    {
                        if (ent.NumericCollectionID.Value == CollID)
                        {
                            Item.Type nItem = (Item.Type)ent.ItemIDComboBox.SelectedItem;
                            ushort ItemVal = (ushort)nItem;
                            itc.AllITC[i].ItemID = ItemVal;
                            break;
                        }
                    }
                }
            }
        }

        private void LoadITCButton_Click(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "Item Collection files (*.itc)|*.itc|All files (*.*)|*.*";
            DialogResult result = dialog.ShowDialog();

            if (result == DialogResult.OK)
            {
                if (itcFile != null)
                    itcFile.Close();
                itcFile = File.OpenRead(dialog.FileName);
                itc = Itc.Read(itcFile);
                UpdateParameters(itc);
                SaveITCButton.Enabled = true;
            }
        }

        private void SaveITCButton_Click(object sender, EventArgs e)
        {
            SaveFileDialog dialog = new SaveFileDialog();
            dialog.Filter = "Item Collection files (*.itc)|*.itc|All files (*.*)|*.*";
            DialogResult result = dialog.ShowDialog();

            if (result == DialogResult.OK)
            {
                Stream itcOut = File.OpenWrite(dialog.FileName);
                UpdateWriteInfo();
                Itc.Write(itcOut, itc);
                itcOut.Close();
            }

            MessageBox.Show("File saved successfully!", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}
