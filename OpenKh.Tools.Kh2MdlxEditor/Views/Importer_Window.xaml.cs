using OpenKh.Tools.Kh2MdlxEditor.ViewModels;
using System;
using System.Linq;
using System.Windows;

namespace OpenKh.Tools.Kh2MdlxEditor.Views
{
    public partial class Importer_Window : Window
    {
        Importer_VM importerVM { get; set; }
        Main2_Window? mainWindow { get; set; } // To reload the model viewport

        public Importer_Window()
        {
            InitializeComponent();
        }
        public Importer_Window(Main2_VM mainVM, Main2_Window? mainWindow = null)
        {
            importerVM = new Importer_VM(mainVM);
            InitializeComponent();
            this.DataContext = importerVM;
            this.mainWindow = mainWindow;
        }

        private void Button_LoadModel(object sender, EventArgs e)
        {
            System.Windows.Forms.OpenFileDialog sfd;
            sfd = new System.Windows.Forms.OpenFileDialog();
            sfd.Title = "Select file";
            sfd.ShowDialog();
            if (sfd.FileName != "")
            {
                if (sfd.FileName.ToLower().EndsWith(".fbx") || sfd.FileName.ToLower().EndsWith(".dae"))
                {
                    try
                    {
                        loadModel(sfd.FileName);
                    }
                    catch (Exception exception)
                    {
                        ErrorMessage.Content = "Error: " + exception.Message;
                    }
                }

            }
        }

        private void Drop_LoadModel(object sender, DragEventArgs e)
        {
            try
            {
                if (e.Data.GetDataPresent(DataFormats.FileDrop))
                {
                    string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                    string firstFile = files?.FirstOrDefault();

                    if (firstFile.ToLower().EndsWith(".fbx") || firstFile.ToLower().EndsWith(".dae"))
                    {
                        loadModel(firstFile);
                    }
                }
            }
            catch (Exception exception)
            {
                ErrorMessage.Content = "Error opening file: " + exception.Message;
            }
        }

        private void Button_Import(object sender, EventArgs e)
        {
            try
            {
                ErrorMessage.Content = "Loading...";
                importerVM.ImportModel();
                ErrorMessage.Content = "Finished";
                if (mainWindow != null)
                {
                    mainWindow.reloadModelControl();
                }
                this.Close();
            }
            catch (Exception exception)
            {
                ErrorMessage.Content = "Error: " + exception.Message;
            }
        }

        private void loadModel(string filepath)
        {
            importerVM.loadModel(filepath);
        }
    }
}
