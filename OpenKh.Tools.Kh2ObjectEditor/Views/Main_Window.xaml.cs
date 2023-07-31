using OpenKh.Kh2;
using OpenKh.Tools.Kh2ObjectEditor.Utils;
using OpenKh.Tools.Kh2ObjectEditor.ViewModel;
using System.IO;
using System.Linq;
using System.Windows;

namespace OpenKh.Tools.Kh2ObjectEditor.Views
{
    public partial class Main_Window : Window
    {
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

                // VERSION 2
                if (Directory.Exists(firstFile))
                {
                    App_Context.Instance.loadFolder(firstFile);
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
                }
            }
        }

        private void Menu_SaveMotion(object sender, RoutedEventArgs e)
        {
            App_Context.Instance.saveMotion();
        }
        private void Menu_SaveMset(object sender, RoutedEventArgs e)
        {
            saveMset();
        }
        private void Menu_Test(object sender, RoutedEventArgs e)
        {
            App_Context checkAppContext = App_Context.Instance;
            // BREAKPOINT to check app context info
        }

        public void saveMset()
        {
            System.Windows.Forms.SaveFileDialog sfd;
            sfd = new System.Windows.Forms.SaveFileDialog();
            sfd.Title = "Save file";
            sfd.FileName = Path.GetFileNameWithoutExtension(App_Context.Instance.MsetPath) + ".out.mset";
            sfd.ShowDialog();
            if (sfd.FileName != "")
            {
                MemoryStream memStream = new MemoryStream();
                Bar.Write(memStream, App_Context.Instance.MsetBar);
                File.WriteAllBytes(sfd.FileName, memStream.ToArray());
            }
        }
    }
}
