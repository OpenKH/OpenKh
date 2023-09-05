using Microsoft.Win32;
using OpenKh.Tools.Kh2ObjectEditor.Services;
using OpenKh.Tools.Kh2ObjectEditor.Utils;
using OpenKh.Tools.Kh2ObjectEditor.ViewModel;
using System.IO;
using System.Linq;
using System.Windows;

namespace OpenKh.Tools.Kh2ObjectEditor.Views
{
    public partial class Main_Window : Window
    {
        private static readonly bool MODE_DEBUG = false;
        private void Menu_Test(object sender, RoutedEventArgs e)
        {
            App_Context checkAppContext = App_Context.Instance;
            MdlxService checkMdlxService = MdlxService.Instance;
            MsetService checkMsetService = MsetService.Instance;
            ApdxService checkApdxService = ApdxService.Instance;
            // BREAKPOINT to check app context info
        }
        private void Menu_Test_RunCode(object sender, RoutedEventArgs e)
        {
            TestingService.Instance.RunCode();
        }
        private void Menu_Test_Effects(object sender, RoutedEventArgs e)
        {
            TestingService.Instance.TestApdx();
        }
        private void Menu_Test_Mdlxs(object sender, RoutedEventArgs e)
        {
            TestingService.Instance.TestMdlx();
        }
        private void Menu_Test_Msets(object sender, RoutedEventArgs e)
        {
            TestingService.Instance.TestMset();
        }

        private void Menu_Save_Mdlx(object sender, RoutedEventArgs e)
        {
            MdlxService.Instance.SaveFile();
        }
        private void Menu_Save_Mset(object sender, RoutedEventArgs e)
        {
            MsetService.Instance.SaveFile();
        }
        private void Menu_Save_Apdx(object sender, RoutedEventArgs e)
        {
            ApdxService.Instance.SaveFile();
        }
        private void Menu_Overwrite_Mdlx(object sender, RoutedEventArgs e)
        {
            MdlxService.Instance.OverwriteFile();
        }
        private void Menu_Overwrite_Mset(object sender, RoutedEventArgs e)
        {
            MsetService.Instance.OverwriteFile();
        }
        private void Menu_Overwrite_Apdx(object sender, RoutedEventArgs e)
        {
            ApdxService.Instance.OverwriteFile();
        }

        public Main_Window()
        {
            InitializeComponent();
        }

        private void Window_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                string firstFile = files?.FirstOrDefault();

                if (Directory.Exists(firstFile))
                {
                    if (MODE_DEBUG)
                    {
                        TestingService.Instance.FolderPath = firstFile;
                    }
                    else
                    {
                        App_Context.Instance.loadFolder(firstFile);
                    }
                }
                else if(File.Exists(firstFile))
                {
                    if (ObjectEditorUtils.isFilePathValid(firstFile, "mdlx"))
                    {
                        App_Context.Instance.loadMdlx(firstFile);
                    }
                    else if (ObjectEditorUtils.isFilePathValid(firstFile, "mset"))
                    {
                        App_Context.Instance.loadMset(firstFile);
                    }
                    else if (firstFile.Contains(".a."))
                    {
                        ApdxService.Instance.LoadFile(firstFile);
                    }
                }
            }
        }

        
        private void Menu_AttachMdlx(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Mdlx file | *.mdlx";
            bool? success = openFileDialog.ShowDialog();

            if(success == true)
            {
                if (Directory.Exists(openFileDialog.FileName))
                {
                    return;
                }
                else if (File.Exists(openFileDialog.FileName))
                {
                    if (ObjectEditorUtils.isFilePathValid(openFileDialog.FileName, "mdlx"))
                    {
                        if(MdlxService.Instance.ModelFile != null)
                        {
                            AttachmentService.Instance.LoadAttachment(openFileDialog.FileName, 178);
                        }
                        else
                        {
                            AttachmentService.Instance.LoadAttachment(openFileDialog.FileName);
                        }
                    }
                }
            }
        }

        
    }
}
