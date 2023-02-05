using OpenKh.Kh2.Models.VIF;
using OpenKh.Tools.Kh2MdlxEditor.Utils;
using OpenKh.Tools.Kh2MdlxEditor.ViewModels;
using System;
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

        private void Button_Import(object sender, EventArgs e)
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
                        ErrorMessage.Content = "Loading...";
                        MdlxEditorImporter.KEEP_ORIGINAL_SHADOW = importerVM.KeepShadow;
                        VifProcessor.VERTEX_LIMIT = importerVM.VertexLimitPerPacket;
                        VifProcessor.MEMORY_LIMIT = importerVM.MemoryLimitPerPacket;
                        importerVM.MainVM.replaceModel(sfd.FileName);
                        ErrorMessage.Content = "Finished";
                        if(mainWindow != null)
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
                    
            }
        }
    }
}
