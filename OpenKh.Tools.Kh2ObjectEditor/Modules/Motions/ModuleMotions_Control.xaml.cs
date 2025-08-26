using Microsoft.Win32;
using OpenKh.AssimpUtils;
using OpenKh.Kh2;
using OpenKh.Tools.Kh2ObjectEditor.Classes;
using OpenKh.Tools.Kh2ObjectEditor.Modules.Motions;
using OpenKh.Tools.Kh2ObjectEditor.Services;
using OpenKh.Tools.Kh2ObjectEditor.ViewModel;
using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace OpenKh.Tools.Kh2ObjectEditor.Views
{
    public partial class ModuleMotions_Control : UserControl
    {
        public ModuleMotions_VM ThisVM { get; set; }
        public ModuleMotions_Control()
        {
            InitializeComponent();
            ThisVM = new ModuleMotions_VM();
            DataContext = ThisVM;
        }

        private void Button_ApplyFilters(object sender, System.Windows.RoutedEventArgs e)
        {
            ThisVM.applyFilters();
        }

        private void list_doubleCLick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            MotionSelector_Wrapper item = (MotionSelector_Wrapper)(sender as ListView).SelectedItem;


            // Reaction Command
            if (item.Entry.Type == BinaryArchive.EntryType.Motionset)
            {
                System.Windows.Forms.MessageBox.Show("This is a Motion Set", "Motion couldn't be loaded", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                return;
            }
            // No motion
            else if(item.Entry.Type != BinaryArchive.EntryType.Anb)
            {
                System.Windows.Forms.MessageBox.Show("This is not a Motion or Motion Set", "Motion couldn't be loaded", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                return;
            }

            // Invalid
            if (item == null || item.Name.Contains("DUMM"))
            {
                return;
            }
            // Unnamed dummy
            if (item.LinkedSubfile.Length == 0)
            {
                System.Windows.Forms.MessageBox.Show("This motion is a dummy (No data)", "Motion couldn't be loaded", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                return;
            }

            try
            {
                App_Context.Instance.loadMotion(item.Index);
                //Mset_Service.Instance.loadMotion(item.Index);
                openMotionTabs(MsetService.Instance.LoadedMotion);
            }
            catch (System.Exception exc)
            {
                System.Windows.Forms.MessageBox.Show("There was an error", "Animation couldn't be loaded", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                return;
            }
        }

        private void openMotionTabs(AnimationBinary animBinary)
        {
            Frame_Metadata.Content = new MotionMetadata_Control(animBinary);
            Frame_Triggers.Content = new MotionTriggers_Control(animBinary);
        }
        public void Motion_Rename(object sender, RoutedEventArgs e)
        {
            if (MotionList.SelectedItem != null)
            {
                MotionSelector_Wrapper item = (MotionSelector_Wrapper)MotionList.SelectedItem;
                MotionRenameWindow newWindow = new MotionRenameWindow(item.Index, this);
                newWindow.Show();
            }
        }
        public void Motion_AddNew(object sender, RoutedEventArgs e)
        {
            if (MotionList.SelectedItem != null)
            {

                MotionSelector_Wrapper item = (MotionSelector_Wrapper)MotionList.SelectedItem;

                BinaryArchive.Entry newMotionEntry = new BinaryArchive.Entry();
                newMotionEntry.Name = "NDUM";
                newMotionEntry.Type = BinaryArchive.EntryType.Anb;
                newMotionEntry.Link = -1;

                MsetService.Instance.MsetBinarc.Entries.Insert(item.Index + 1, newMotionEntry);

                ThisVM.loadMotions();
                ThisVM.applyFilters();
            }
        }
        public void Motion_Remove(object sender, RoutedEventArgs e)
        {
            if (MotionList.SelectedItem != null)
            {
                MotionSelector_Wrapper item = (MotionSelector_Wrapper)MotionList.SelectedItem;

                MsetService.Instance.MsetBinarc.Entries.RemoveAt(item.Index);

                ThisVM.loadMotions();
                ThisVM.applyFilters();
            }
        }
        public void Motion_Copy(object sender, RoutedEventArgs e)
        {
            if (MotionList.SelectedItem != null)
            {
                MotionSelector_Wrapper item = (MotionSelector_Wrapper)MotionList.SelectedItem;
                ThisVM.Motion_Copy(item.Index);
            }
        }
        public void Motion_Replace(object sender, RoutedEventArgs e)
        {
            if (MotionList.SelectedItem != null)
            {
                MotionSelector_Wrapper item = (MotionSelector_Wrapper)MotionList.SelectedItem;
                ThisVM.Motion_Replace(item.Index);
            }
        }
        public void Motion_Export(object sender, RoutedEventArgs e)
        {
            if (MotionList.SelectedItem == null || MdlxService.Instance.ModelFile == null)
            {
                return;
            }

            MotionSelector_Wrapper item = (MotionSelector_Wrapper)MotionList.SelectedItem;
            AnimationBinary animation;
            using (MemoryStream memStream = new MemoryStream(item.LinkedSubfile))
            {
                animation = new AnimationBinary(memStream);
            }

            Kh2.Models.ModelSkeletal model = null;
            foreach(Bar.Entry barEntry in MdlxService.Instance.MdlxBar)
            {
                if(barEntry.Type == Bar.EntryType.Model)
                {
                    model = Kh2.Models.ModelSkeletal.Read(barEntry.Stream);
                    barEntry.Stream.Position = 0;
                }
            }
            Assimp.Scene scene = Kh2MdlxAssimp.getAssimpScene(model);
            Kh2MdlxAssimp.AddAnimation(scene, MdlxService.Instance.MdlxBar, animation);

            System.Windows.Forms.SaveFileDialog sfd;
            sfd = new System.Windows.Forms.SaveFileDialog();
            sfd.Title = "Export animated model";
            sfd.FileName = MdlxService.Instance.MdlxPath + "." + AssimpGeneric.GetFormatFileExtension(AssimpGeneric.FileFormat.fbx);
            sfd.ShowDialog();
            if (sfd.FileName != "")
            {
                string dirPath = Path.GetDirectoryName(sfd.FileName);

                if (!Directory.Exists(dirPath))
                    return;

                dirPath += "\\";

                AssimpGeneric.ExportScene(scene, AssimpGeneric.FileFormat.fbx, sfd.FileName);
            }
        }
        public void Motion_Import(object sender, RoutedEventArgs e)
        {
            if (MotionList.SelectedItem == null) {
                return;
            }

            Frame_Metadata.Content = new ContentControl();
            Frame_Triggers.Content = new ContentControl();

            MotionSelector_Wrapper item = (MotionSelector_Wrapper)MotionList.SelectedItem;

            try
            {
                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.Multiselect = false;
                if (openFileDialog.ShowDialog() == true && openFileDialog.FileNames != null && openFileDialog.FileNames.Length > 0)
                {
                    string filePath = openFileDialog.FileNames[0];
                    if (filePath.ToLower().EndsWith(".fbx"))
                    {
                        ThisVM.Motion_Import(item.Index, filePath);
                    }
                }

                App_Context.Instance.loadMotion(item.Index);
                openMotionTabs(MsetService.Instance.LoadedMotion);
            }
            catch (Exception exception) { }
        }
        public void MotionEntry_Export(object sender, RoutedEventArgs e)
        {
            if (MotionList.SelectedItem == null) {
                return;
            }

            MotionSelector_Wrapper item = (MotionSelector_Wrapper)MotionList.SelectedItem;

            string fileExtension = ".mentry";
            if(item.Entry.Type == BinaryArchive.EntryType.Anb)
            {
                fileExtension = ".anb";
            }
            if (item.Entry.Type == BinaryArchive.EntryType.Motionset)
            {
                fileExtension = ".mset";
            }

            try
            {
                System.Windows.Forms.SaveFileDialog sfd;
                sfd = new System.Windows.Forms.SaveFileDialog();
                sfd.Title = "Export motion entry";
                sfd.FileName = item.Entry.Name + fileExtension;
                sfd.ShowDialog();
                if (sfd.FileName != "")
                {
                    File.WriteAllBytes(sfd.FileName, item.LinkedSubfile);
                }
            }
            catch (Exception exc) { }
        }
        public void ANB_Replace(object sender, RoutedEventArgs e)
        {
            Entry_Replace(BinaryArchive.EntryType.Anb);
        }
        public void MSET_Replace(object sender, RoutedEventArgs e)
        {
            Entry_Replace(BinaryArchive.EntryType.Motionset);
        }
        public void Entry_Replace(BinaryArchive.EntryType entryType)
        {
            if (MotionList.SelectedItem == null) {
                return;
            }

            MotionSelector_Wrapper item = (MotionSelector_Wrapper)MotionList.SelectedItem;

            try
            {
                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.Multiselect = false;
                if (openFileDialog.ShowDialog() == true && openFileDialog.FileNames != null && openFileDialog.FileNames.Length > 0)
                {
                    string filePath = openFileDialog.FileNames[0];
                    ThisVM.Motion_ImportEntry(item.Index, filePath, entryType);
                }

                // Unneeded and will crash if null
                //openMotionTabs(MsetService.Instance.LoadedMotion);
            }
            catch (Exception exception) { }
        }
        public void Motion_MoveUp(object sender, RoutedEventArgs e)
        {
            if (MotionList.SelectedItem != null)
            {
                MotionSelector_Wrapper item = (MotionSelector_Wrapper)MotionList.SelectedItem;

                int index = item.Index;

                if(index == 0)
                {
                    return;
                }

                BinaryArchive.Entry tempEntry = MsetService.Instance.MsetBinarc.Entries[item.Index];

                MsetService.Instance.MsetBinarc.Entries.RemoveAt(item.Index);

                MsetService.Instance.MsetBinarc.Entries.Insert(index - 1, tempEntry);

                ThisVM.loadMotions();
                ThisVM.applyFilters();

                MotionList.SelectedItem = MotionList.Items[index - 1];
            }
        }
        public void Motion_MoveDown(object sender, RoutedEventArgs e)
        {
            if (MotionList.SelectedItem != null)
            {
                MotionSelector_Wrapper item = (MotionSelector_Wrapper)MotionList.SelectedItem;

                int index = item.Index;

                if (index >= MotionList.Items.Count - 1)
                {
                    return;
                }

                BinaryArchive.Entry tempEntry = MsetService.Instance.MsetBinarc.Entries[item.Index];

                MsetService.Instance.MsetBinarc.Entries.RemoveAt(item.Index);

                MsetService.Instance.MsetBinarc.Entries.Insert(index + 1, tempEntry);

                ThisVM.loadMotions();
                ThisVM.applyFilters();

                MotionList.SelectedItem = MotionList.Items[index + 1];
            }
        }

        private void Button_TEST(object sender, System.Windows.RoutedEventArgs e)
        {
            ThisVM.TestMsetIngame();
        }

        private void FilterName_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                ThisVM.FilterName = FilterName.Text;
                ThisVM.applyFilters();
            }
        }

        private void FilterName_TextChanged(object sender, TextChangedEventArgs e)
        {

            ThisVM.FilterName = FilterName.Text;
            ThisVM.applyFilters();
        }

        private void HideDummies_Checked(object sender, RoutedEventArgs e)
        {
            ThisVM.FilterHideDummies = true;
            ThisVM.applyFilters();
        }

        private void HideDummies_Unchecked(object sender, RoutedEventArgs e)
        {
            ThisVM.FilterHideDummies = false;
            ThisVM.applyFilters();
        }
    }
}
