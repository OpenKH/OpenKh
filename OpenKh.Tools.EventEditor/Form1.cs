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

namespace OpenKh.Tools.EventEditor
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        List<Event> EventLst = new List<Event>();
        Stream eventFile;

        private void UpdateParameters(List<Event> events)
        {
            FlowEvent.Controls.Clear();
            int i = 0;
            foreach(Event ev in events)
            {
                EventEntry ent = new EventEntry();
                ent.EventGBox.Text = "Entry " + (i+1);
                ent.NumericGlobalID.Value = ev.Id;
                ent.NumericEventIndex.Value = ev.EventIndex;
                ent.WorldComboBox.SelectedIndex = ev.World;
                ent.RoomComboBox.SelectedIndex = ev.Room;

                FlowEvent.Controls.Add(ent);
                i++;
            }
        }

        private void UpdateWriteInfo()
        {
            foreach (Event ev in EventLst)
            {
                foreach (EventEntry evEnt in FlowEvent.Controls)
                {
                    if(evEnt.NumericGlobalID.Value == ev.Id)
                    {
                        ev.EventIndex = decimal.ToUInt16(evEnt.NumericEventIndex.Value);
                        ev.World = (byte)evEnt.WorldComboBox.SelectedIndex;
                        ev.Room = (byte)evEnt.RoomComboBox.SelectedIndex;
                    }
                }
            }
        }

        private void LoadEventButton_Click(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "All files (*.*)|*.*";
            DialogResult result = dialog.ShowDialog();

            if (result == DialogResult.OK)
            {
                if (eventFile != null)
                    eventFile.Close();
                eventFile = File.OpenRead(dialog.FileName);
                if (Event.IsValid(eventFile))
                {
                    EventLst = Event.Read(eventFile);
                    UpdateParameters(EventLst);
                    SaveEventButton.Enabled = true;
                    AddEventButton.Enabled = true;
                }
            }
        }

        private void SaveEventButton_Click(object sender, EventArgs e)
        {
            SaveFileDialog dialog = new SaveFileDialog();
            dialog.Filter = "All files (*.*)|*.*";
            DialogResult result = dialog.ShowDialog();

            if (result == DialogResult.OK)
            {
                Stream eventOut = File.OpenWrite(dialog.FileName);
                UpdateWriteInfo();
                Event.Write(eventOut, EventLst);
                eventOut.Close();
            }

            MessageBox.Show("File saved successfully!", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void AddEventButton_Click(object sender, EventArgs e)
        {
            EventEntry evEnt = new EventEntry();
            int LastIndex = EventLst.Count - 1;
            evEnt.EventGBox.Text = "Event " + (FlowEvent.Controls.Count);
            evEnt.NumericGlobalID.Value = EventLst[LastIndex].Id + 1;
            Event nEvent = new Event();
            nEvent.Id = EventLst[LastIndex].Id;
            nEvent.Id++;
            EventLst.Add(nEvent);
            FlowEvent.Controls.Add(evEnt);
        }
    }
}
