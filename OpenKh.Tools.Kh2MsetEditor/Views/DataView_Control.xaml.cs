using OpenKh.Kh2;
using OpenKh.Tools.Kh2MsetEditor.ViewModels;
using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using static OpenKh.Tools.Kh2MsetEditor.ViewModels.DataView_VM;

namespace OpenKh.Tools.Kh2MsetEditor.Views
{
    /// <summary>
    /// Interaction logic for DataView_Control.xaml
    /// </summary>
    public partial class DataView_Control : UserControl
    {
        // VIEW MODEL
        //-----------------------------------------------------------------------
        DataView_VM mainViewModel { get; set; }
        Bar.Entry loadedAnimationBinary { get; set; }

        // CONSTRUCTOR
        //-----------------------------------------------------------------------
        public DataView_Control()
        {
            InitializeComponent();
        }

        // ACTIONS
        //-----------------------------------------------------------------------

        // Opens the file that has been dropped on the window
        private void Window_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                string firstFile = files?.FirstOrDefault();
                loadFile(firstFile);
            }
        }
        private void Menu_SaveFile(object sender, EventArgs e)
        {
            saveFile();
        }

        // Loads the required control in the frame on BAR item click
        public void ListViewItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            AnbEntryWrapper anbWrapper = ((ListViewItem)sender).Content as AnbEntryWrapper;
            openBarEntry(anbWrapper.Entry);
        }
        public void ListViewItem_SelectionChange(object sender, SelectionChangedEventArgs args)
        {
            if ((sender as ListView).SelectedItem == null)
            {
                return;
            }
            AnbEntryWrapper anbWrapper = (sender as ListView).SelectedItem as AnbEntryWrapper;
            openBarEntry(anbWrapper.Entry);
        }

        public void DummyFilter_Enable(object sender, RoutedEventArgs e)
        {
            if (mainViewModel == null)
                return;
            mainViewModel.filterDummies = true;
            mainViewModel.loadViewList();
        }
        public void DummyFilter_Disable(object sender, RoutedEventArgs e)
        {
            if (mainViewModel == null)
                return;
            mainViewModel.filterDummies = false;
            mainViewModel.loadViewList();
        }

        // FUNCTIONS
        //-----------------------------------------------------------------------

        // Loads the given file
        public void loadFile(string filePath)
        {
            contentFrame.Content = null;
            mainViewModel = (filePath != null) ? new DataView_VM(filePath) : new DataView_VM();
            DataContext = mainViewModel;
        }

        // Saves the file
        public void saveFile()
        {
            saveLoadedAnimationBinary();
            mainViewModel.buildBarFile();

            System.Windows.Forms.SaveFileDialog sfd;
            sfd = new System.Windows.Forms.SaveFileDialog();
            sfd.Title = "Save file";
            sfd.FileName = mainViewModel.FileName + ".out.mset";
            sfd.ShowDialog();
            if (sfd.FileName != "")
            {
                MemoryStream memStream = new MemoryStream();
                Bar.Write(memStream, mainViewModel.BarFile);
                File.WriteAllBytes(sfd.FileName, memStream.ToArray());
            }
        }

        // Loads the User Control required for the given bar entry
        public void openBarEntry(Bar.Entry barEntry)
        {
            saveLoadedAnimationBinary();
            if (barEntry == null || barEntry.Stream.Length == 0)
            {
                contentFrame.Content = null;
                return;
            }

            switch (barEntry.Type)
            {
                case Bar.EntryType.Anb:
                    contentFrame.Content = new AnimBin_Control(barEntry.Stream);
                    loadedAnimationBinary = barEntry;
                    break;
                default:
                    break;
            }
        }

        public void saveLoadedAnimationBinary()
        {
            if (contentFrame.Content == null)
                return;

            Stream saveStream = (contentFrame.Content as AnimBin_Control).getAnimationBinaryAsStream();
            saveStream.Position = 0;

            loadedAnimationBinary.Stream = saveStream;
        }
    }
}
