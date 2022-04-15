using OpenKh.Kh2;
using OpenKh.Tools.Kh2MsetEditor.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using static OpenKh.Tools.Kh2MsetEditor.ViewModels.Main_VM;

namespace OpenKh.Tools.Kh2MsetEditor.Views
{
    public partial class Main_Window : Window
    {
        // VIEW MODEL
        //-----------------------------------------------------------------------
        Main_VM mainViewModel { get; set; }
        Bar.Entry loadedAnimationBinary { get; set; }

        // CONSTRUCTOR
        //-----------------------------------------------------------------------
        public Main_Window()
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

                if (true)
                {
                    loadFile(firstFile);
                }
                // TESTING
                else
                {
                    //analyzeFiles(files.ToList());
                    parseData(firstFile);
                }
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
            if((sender as ListView).SelectedItem == null){
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
            mainViewModel = (filePath != null) ? new Main_VM(filePath) : new Main_VM();
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
            if (barEntry == null || barEntry.Stream.Length == 0)
            {
                contentFrame.Content = null;
                return;
            }

            switch (barEntry.Type)
            {
                case Bar.EntryType.Anb:
                    saveLoadedAnimationBinary();
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

        /////////////////////////// TESTING ///////////////////////////
        // not available for the releases, only for research purposes
        private void Menu_Test(object sender, EventArgs e)
        {
            Debug.WriteLine("Enum2: " + (MotionSet.MotionName)2);
            Debug.WriteLine("BREAKPOINT");
        }
        public class triggerData
        {
            public byte trigger;
            public HashSet<byte> paramSize = new HashSet<byte>();
        }
        public void parseData(string filePath)
        {
            if (filePath == null || !filePath.EndsWith(".mset"))
                return;

            using var stream = File.Open(filePath, FileMode.Open);
            if (!Bar.IsValid(stream))
                return;

            Bar binarc = Bar.Read(stream);

            // MSET BAR entries (ANBs)
            foreach (Bar.Entry barEntry in binarc)
            {
                if (barEntry.Stream.Length == 0) // DUMMY
                    continue;

                if (barEntry.Type == Bar.EntryType.Anb)
                {
                    if (!Bar.IsValid(barEntry.Stream))
                        continue;

                    AnimationBinary anb = new AnimationBinary(barEntry.Stream);
                    /*foreach(Motion.StaticPoseTable pose in anb.MotionFile.Interpolated.StaticPose)
                    {
                        Debug.WriteLine("Static pose: " + pose.ToString());
                    }
                    foreach (Motion.BoneAnimationTable boneAnim in anb.MotionFile.Interpolated.ModelBoneAnimation)
                    {
                        Debug.WriteLine("Bone anim: " + boneAnim.ToString());
                    }*/
                    Debug.WriteLine("BREAKPOINT - check data");
                }
            }
        }
        public void analyzeFiles(List<string> list_filePaths)
        {
            int totalFiles = 0;

            Dictionary<byte, triggerData> triggerDataRange = new Dictionary<byte, triggerData>();
            Dictionary<byte, triggerData> triggerDataFrame = new Dictionary<byte, triggerData>();

            int filesWithFrame13_1Param = 0;
            int filesWithFrame13_2Params = 0;
            //List<string> filesFalse = new List<string>();

            foreach (string filePath in list_filePaths)
            {
                totalFiles++;
                Debug.WriteLine("Reading file: " + filePath);

                bool hasFrame13_1Param = false;
                bool hasFrame13_2Params = false;

                if (filePath == null || !filePath.EndsWith(".mset"))
                    continue;

                if (filePath.EndsWith("EX_0010.mset")) // Trigger param size in bytes for some reason
                    continue;

                using var stream = File.Open(filePath, FileMode.Open);
                if (!Bar.IsValid(stream))
                    continue;

                if (stream.Length == 0) // DUMMY
                    continue;

                Bar binarc = Bar.Read(stream); // ANB

                bool hasNonAnb = false;
                foreach (Bar.Entry barEntry in binarc)
                {
                    if (!Bar.IsValid(barEntry.Stream))
                        continue;

                    if (barEntry.Type == Bar.EntryType.Anb)
                    {
                        Bar anbBar = Bar.Read(barEntry.Stream);

                        foreach(Bar.Entry anbEntry in anbBar)
                        {
                            if(anbEntry.Type == Bar.EntryType.MotionTriggers && anbEntry.Stream.Length > 0)
                            {
                                MotionTrigger motionTriggerEntry = new MotionTrigger(anbEntry.Stream);

                                foreach(MotionTrigger.RangeTrigger rangeTrigger in motionTriggerEntry.RangeTriggerList)
                                {
                                    if (!triggerDataRange.ContainsKey(rangeTrigger.Trigger))
                                        triggerDataRange.Add(rangeTrigger.Trigger, new triggerData());

                                    triggerDataRange[rangeTrigger.Trigger].trigger = rangeTrigger.Trigger;
                                    triggerDataRange[rangeTrigger.Trigger].paramSize.Add(rangeTrigger.ParamSize);
                                }

                                foreach (MotionTrigger.FrameTrigger frameTrigger in motionTriggerEntry.FrameTriggerList)
                                {
                                    if (!triggerDataFrame.ContainsKey(frameTrigger.Trigger))
                                        triggerDataFrame.Add(frameTrigger.Trigger, new triggerData());

                                    triggerDataFrame[frameTrigger.Trigger].trigger = frameTrigger.Trigger;
                                    triggerDataFrame[frameTrigger.Trigger].paramSize.Add(frameTrigger.ParamSize);

                                    if (frameTrigger.Trigger == 13 && frameTrigger.ParamSize == 1)
                                        hasFrame13_1Param = true;
                                    if (frameTrigger.Trigger == 13 && frameTrigger.ParamSize == 2)
                                        hasFrame13_2Params = true;
                                }
                            }
                        }
                    }
                }
                if (hasFrame13_1Param) filesWithFrame13_1Param++;
                if (hasFrame13_2Params) filesWithFrame13_2Params++;
            }
            Debug.WriteLine("totalFiles: " + totalFiles);
            Debug.WriteLine("filesWithFrame13_1Param: " + filesWithFrame13_1Param);
            Debug.WriteLine("filesWithFrame13_2Params: " + filesWithFrame13_2Params);
            foreach(byte triggerRange in triggerDataRange.Keys)
            {
                Debug.WriteLine("Range Trigger [" + triggerRange + "] <" + string.Join(";",triggerDataRange[triggerRange].paramSize) + ">");
            }
            foreach (byte triggerFrame in triggerDataFrame.Keys)
            {
                Debug.WriteLine("Frame Trigger [" + triggerFrame + "] <" + string.Join(";", triggerDataFrame[triggerFrame].paramSize) + ">");
            }
        }
    }
}
