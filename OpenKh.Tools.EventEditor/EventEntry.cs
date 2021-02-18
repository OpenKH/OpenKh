using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using OpenKh.Bbs;

namespace OpenKh.Tools.EventEditor
{
    public partial class EventEntry : UserControl
    {
        public EventEntry()
        {
            InitializeComponent();
            WorldComboBox.DataSource = Constants.WorldNames;
            RoomComboBox.DataSource = Constants.Rooms_DP;
        }

        private void WorldComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch (WorldComboBox.SelectedIndex)
            {
                case 1:
                    RoomComboBox.DataSource = Constants.Rooms_DP;
                    break;
                case 2:
                    RoomComboBox.DataSource = Constants.Rooms_SW;
                    break;
                case 3:
                    RoomComboBox.DataSource = Constants.Rooms_CD;
                    break;
                case 4:
                    RoomComboBox.DataSource = Constants.Rooms_SB;
                    break;
                case 5:
                    RoomComboBox.DataSource = Constants.Rooms_YT;
                    break;
                case 6:
                    RoomComboBox.DataSource = Constants.Rooms_RG;
                    break;
                case 7:
                    RoomComboBox.DataSource = Constants.Rooms_JB;
                    break;
                case 8:
                    RoomComboBox.DataSource = Constants.Rooms_HE;
                    break;
                case 9:
                    RoomComboBox.DataSource = Constants.Rooms_LS;
                    break;
                case 10:
                    RoomComboBox.DataSource = Constants.Rooms_DI;
                    break;
                case 11:
                    RoomComboBox.DataSource = Constants.Rooms_PP;
                    break;
                case 12:
                    RoomComboBox.DataSource = Constants.Rooms_DC;
                    break;
                case 13:
                    RoomComboBox.DataSource = Constants.Rooms_KG;
                    break;
                case 15:
                    RoomComboBox.DataSource = Constants.Rooms_VS;
                    break;
                case 16:
                    RoomComboBox.DataSource = Constants.Rooms_BD;
                    break;
                case 17:
                    RoomComboBox.DataSource = Constants.Rooms_WM;
                    break;
            }
        }
    }
}
