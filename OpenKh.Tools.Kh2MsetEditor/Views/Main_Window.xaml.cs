using System;
using System.Windows;

namespace OpenKh.Tools.Kh2MsetEditor.Views
{
    public partial class Main_Window : Window
    {
        public Main_Window()
        {
            InitializeComponent();
            contentFrame.Content = new DataView_Control();
        }

        public void Menu_ModeDataViewer(object sender, EventArgs e) {
            contentFrame.Content = new DataView_Control();
        }
        public void Menu_ModeTransfer(object sender, EventArgs e) {
            contentFrame.Content = new Transfer_Control();
        }
    }
}
