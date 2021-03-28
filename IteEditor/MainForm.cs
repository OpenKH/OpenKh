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

namespace OpenKh.Tools.IteEditor
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }

        public Ite ite = new Ite();
        Stream iteFile;

        private void UpdateParameters(Ite ite)
        {
            int cnt = 1;
            for (int i = 0; i < ite.header.WeaponDataCount; i++)
            {
                IteEntry itEntry = new IteEntry();
                itEntry.ITE_GBox.Text = "ITC Entry " + cnt;
                itEntry.ItemComboBox.SelectedItem = (Item.Type)ite.WeaponList[i].ItemID;
                FlowWeapons.Controls.Add(itEntry);
                cnt++;
            }

            for (int i = 0; i < ite.header.FlavorDataCount; i++)
            {
                IteEntry itEntry = new IteEntry();
                itEntry.ITE_GBox.Text = "ITC Entry " + cnt;
                itEntry.ItemComboBox.SelectedItem = (Item.Type)ite.FlavorList[i].ItemID;
                FlowFlavors.Controls.Add(itEntry);
                cnt++;
            }

            for (int i = 0; i < ite.header.KeyItemDataCount; i++)
            {
                IteEntry itEntry = new IteEntry();
                itEntry.ITE_GBox.Text = "ITC Entry " + cnt;
                itEntry.ItemComboBox.SelectedItem = (Item.Type)ite.KeyItemList[i].ItemID;
                FlowKeyItem.Controls.Add(itEntry);
                cnt++;
            }

            for (int i = 0; i < ite.header.KeyItemHideDataCount; i++)
            {
                IteEntry itEntry = new IteEntry();
                itEntry.ITE_GBox.Text = "ITC Entry " + cnt;
                itEntry.ItemComboBox.SelectedItem = (Item.Type)ite.KeyItemHideList[i].ItemID;
                FlowKeyItemHide.Controls.Add(itEntry);
                cnt++;
            }

            for (int i = 0; i < ite.header.SynthesisDataCount; i++)
            {
                IteEntry itEntry = new IteEntry();
                itEntry.ITE_GBox.Text = "ITC Entry " + cnt;
                itEntry.ItemComboBox.SelectedItem = (Item.Type)ite.SynthesisList[i].ItemID;
                FlowSynthesis.Controls.Add(itEntry);
                cnt++;
            }

        }

        private void LoadITEButton_Click(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "Item files (*.ite)|*.ite|All files (*.*)|*.*";
            DialogResult result = dialog.ShowDialog();

            if (result == DialogResult.OK)
            {
                if (iteFile != null)
                    iteFile.Close();
                iteFile = File.OpenRead(dialog.FileName);
                ite = Ite.Read(iteFile);
                UpdateParameters(ite);
                //SaveITEButton.Enabled = true;
            }
        }
    }
}
