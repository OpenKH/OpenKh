using OpenKh.Kh2;
using OpenKh.Tools.Kh2MsetEditor.ViewModels;
using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using static OpenKh.Tools.Kh2MsetEditor.ViewModels.DataView_VM;

namespace OpenKh.Tools.Kh2MsetEditor.Views
{
    public partial class Transfer_Control : UserControl
    {
        Transfer_VM TransferViewModel { get; set; }

        public Transfer_Control()
        {
            InitializeComponent();
            TransferViewModel =  new Transfer_VM();
            DataContext = TransferViewModel;
        }

        private void Drop_To(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                string firstFile = files?.FirstOrDefault();
                if (!firstFile.EndsWith(".mset"))
                    return;
                TransferViewModel.loadToFile(firstFile);
                toViewList.ItemsSource = TransferViewModel.toEntryList_View;
                ToBoneCountBox.GetBindingExpression(TextBox.TextProperty).UpdateTarget();
            }
        }
        private void Drop_From(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                string firstFile = files?.FirstOrDefault();
                if (!firstFile.EndsWith(".mset"))
                    return;
                TransferViewModel.loadFromFile(firstFile);
                fromViewList.ItemsSource = TransferViewModel.fromEntryList_View;
            }
        }
        private void Drop_Bones(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                string firstFile = files?.FirstOrDefault();
                if (!firstFile.EndsWith(".csv"))
                    return;
                TransferViewModel.loadBoneTransferFile(firstFile);

                BoneTransferTable.Items.Refresh();
            }
        }

        private void Transfer_Button(object sender, RoutedEventArgs e)
        {
            TransferViewModel.transferAnb();
            TransferViewModel.loadToViewList();
        }

        private void Menu_SaveFile(object sender, EventArgs e)
        {
            saveFile();
        }

        public void ListViewItem_ToChanged(object sender, SelectionChangedEventArgs args)
        {
            if ((sender as ListView).SelectedItem == null)
            {
                return;
            }
            AnbEntryWrapper anbWrapper = (sender as ListView).SelectedItem as AnbEntryWrapper;
            TransferViewModel.toSelected = anbWrapper.Entry;
        }
        public void ListViewItem_FromChanged(object sender, SelectionChangedEventArgs args)
        {
            if ((sender as ListView).SelectedItem == null)
            {
                return;
            }
            AnbEntryWrapper anbWrapper = (sender as ListView).SelectedItem as AnbEntryWrapper;
            TransferViewModel.fromSelected = anbWrapper.Entry;

            if (TransferViewModel.fromSelected.Stream.Length < 1) {
                return;
            }
            TransferViewModel.fromSelected.Stream.Position = 0;

            AnimationBinary fromAnb = new AnimationBinary(TransferViewModel.fromSelected.Stream);
            if(TransferViewModel.FromBoneCount != fromAnb.MotionFile.InterpolatedMotionHeader.BoneCount)
            {
                TransferViewModel.FromBoneCount = fromAnb.MotionFile.InterpolatedMotionHeader.BoneCount;
                TransferViewModel.loadBoneTransferWrappers();
                BoneTransferTable.ItemsSource = TransferViewModel.boneTransferWrappers;
            }
        }

        public void saveFile()
        {
            System.Windows.Forms.SaveFileDialog sfd;
            sfd = new System.Windows.Forms.SaveFileDialog();
            sfd.Title = "Save file";
            sfd.FileName = "transfer.out.mset";
            sfd.ShowDialog();
            if (sfd.FileName != "")
            {
                MemoryStream memStream = new MemoryStream();
                Bar.Write(memStream, TransferViewModel.ToMset, TransferViewModel.ToMset.Motionset);
                File.WriteAllBytes(sfd.FileName, memStream.ToArray());
            }
        }
    }
}
