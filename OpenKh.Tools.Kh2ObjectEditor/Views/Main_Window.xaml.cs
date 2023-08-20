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
            Mdlx_Service checkMdlxService = Mdlx_Service.Instance;
            Mset_Service checkMsetService = Mset_Service.Instance;
            Apdx_Service checkApdxService = Apdx_Service.Instance;
            // BREAKPOINT to check app context info
        }
        private void Menu_Test_RunCode(object sender, RoutedEventArgs e)
        {
            Testing_Service.Instance.runCode();
        }
        private void Menu_Test_Effects(object sender, RoutedEventArgs e)
        {
            Testing_Service.Instance.testApdx();
        }
        private void Menu_Test_Mdlxs(object sender, RoutedEventArgs e)
        {
            Testing_Service.Instance.testMdlx();
        }
        private void Menu_Test_Msets(object sender, RoutedEventArgs e)
        {
            Testing_Service.Instance.testMset();
        }

        private void Menu_Save_Mdlx(object sender, RoutedEventArgs e)
        {
            Mdlx_Service.Instance.saveFile();
        }
        private void Menu_Save_Mset(object sender, RoutedEventArgs e)
        {
            Mset_Service.Instance.saveFile();
        }
        private void Menu_Save_Apdx(object sender, RoutedEventArgs e)
        {
            Apdx_Service.Instance.saveFile();
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
                        Testing_Service.Instance.FolderPath = firstFile;
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
                        Apdx_Service.Instance.loadFile(firstFile);
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
                        if(Mdlx_Service.Instance.ModelFile != null)
                        {
                            Attachment_Service.Instance.loadAttachment(openFileDialog.FileName, 178);
                        }
                        else
                        {
                            Attachment_Service.Instance.loadAttachment(openFileDialog.FileName);
                        }
                    }
                }
            }
        }

        
    }
}
