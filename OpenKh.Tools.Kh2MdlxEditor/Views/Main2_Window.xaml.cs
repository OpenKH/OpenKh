using Microsoft.Win32;
using OpenKh.AssimpUtils;
using OpenKh.Kh2;
using OpenKh.Tools.Common.Wpf;
using OpenKh.Tools.Kh2MdlxEditor.ViewModels;
using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Media.Imaging;

namespace OpenKh.Tools.Kh2MdlxEditor.Views
{
    public partial class Main2_Window : Window
    {
        // VIEW MODEL
        //-----------------------------------------------------------------------
        Main2_VM mainVM { get; set; } = new Main2_VM();

        // CONSTRUCTOR
        //-----------------------------------------------------------------------
        public Main2_Window()
        {
            InitializeComponent();
        }

        // ACTIONS
        //-----------------------------------------------------------------------

        // Opens the file that has been dropped on the window
        private void Window_Drop(object sender, DragEventArgs e)
        {
            try
            {
                if (e.Data.GetDataPresent(DataFormats.FileDrop))
                {
                    string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                    if (files?.FirstOrDefault() is string firstFile)
                    {
                        try
                        {
                            if (firstFile.ToLower().EndsWith(".mdlx"))
                            {
                                loadFile(firstFile);
                            }
                            else if (firstFile.ToLower().EndsWith(".fbx") || firstFile.ToLower().EndsWith(".dae"))
                            {
                                mainVM ??= new Main2_VM();
                                mainVM.replaceModel(firstFile);
                            }

                            reloadModelControl();
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"There is an error while import file: {firstFile}\n\n{ex}");
                        }
                    }
                }
            }
            catch
            {
            }
        }
        private void Menu_Open(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Multiselect = false;
            if (openFileDialog.ShowDialog() == true && openFileDialog.FileNames != null && openFileDialog.FileNames.Length > 0)
            {
                string filePath = openFileDialog.FileNames[0];
                if (filePath.ToLower().EndsWith(".mdlx"))
                {
                    loadFile(filePath);
                }
            }
        }
        private void Menu_SaveFile(object sender, EventArgs e)
        {
            SaveFile();
        }
        private void Menu_OverwriteFile(object sender, EventArgs e)
        {
            OverwriteFile();
        }

        private void Menu_ExportAsFbx(object sender, EventArgs e)
        {
            exportModel(AssimpGeneric.FileFormat.fbx);
        }
        private void Menu_ExportAsDae(object sender, EventArgs e)
        {
            exportModel(AssimpGeneric.FileFormat.collada);
        }
        private void Menu_Import(object sender, EventArgs e)
        {
            if (mainVM == null)
                return;
            Importer_Window importerWindow = new Importer_Window(mainVM, this);
            importerWindow.Show();
        }

        private void Side_Model(object sender, EventArgs e)
        {
            reloadModelControl();
        }
        private void Side_ModelBones(object sender, EventArgs e)
        {
            contentFrame.Content = new ModelBones_Control(mainVM.ModelFile);
        }
        private void Side_Texture(object sender, EventArgs e)
        {
            contentFrame.Content = new TextureFile_Control(mainVM.TextureFile);
        }
        private void Side_Collision(object sender, EventArgs e)
        {
            contentFrame.Content = new CollisionV_Control(mainVM.CollisionFile, mainVM.ModelFile, mainVM.TextureFile);
        }
        private void Side_CollisionTable(object sender, EventArgs e)
        {
            contentFrame.Content = new Collision_Control(mainVM.CollisionFile);
        }

        // FUNCTIONS
        //-----------------------------------------------------------------------

        // Loads the given file
        public void loadFile(string filePath)
        {
            contentFrame.Content = null;
            mainVM = (filePath != null) ? new Main2_VM(filePath) : new Main2_VM();
            DataContext = mainVM;

            if(mainVM.ModelFile != null)
                sideModel.Visibility = Visibility.Visible;
            else
                sideModel.Visibility = Visibility.Collapsed;

            if (mainVM.TextureFile != null)
                sideTexture.Visibility = Visibility.Visible;
            else
                sideTexture.Visibility = Visibility.Collapsed;

            if (mainVM.CollisionFile != null)
                sideCollision.Visibility = Visibility.Visible;
            else
                sideCollision.Visibility = Visibility.Collapsed;
        }

        // Saves the file
        public void SaveFile()
        {
            mainVM.buildBarFile();

            System.Windows.Forms.SaveFileDialog sfd;
            sfd = new System.Windows.Forms.SaveFileDialog();
            sfd.Title = "Save file";
            sfd.FileName = mainVM.FileName + ".out.mdlx";
            sfd.ShowDialog();
            if (sfd.FileName != "")
            {
                MemoryStream memStream = new MemoryStream();
                Bar.Write(memStream, mainVM.BarFile);
                File.WriteAllBytes(sfd.FileName, memStream.ToArray());
            }
        }
        public void OverwriteFile()
        {
            mainVM.buildBarFile();
            MemoryStream memStream = new MemoryStream();
            Bar.Write(memStream, mainVM.BarFile);
            File.WriteAllBytes(mainVM.FilePath, memStream.ToArray());
        }


        // Exports the first submodel of the loaded MDLX
        public void exportModel(AssimpGeneric.FileFormat fileFormat = AssimpGeneric.FileFormat.fbx)
        {
            if (mainVM.ModelFile != null)
            {
                Assimp.Scene scene = Kh2MdlxAssimp.getAssimpScene(mainVM.ModelFile);

                System.Windows.Forms.SaveFileDialog sfd;
                sfd = new System.Windows.Forms.SaveFileDialog();
                sfd.Title = "Export model";
                sfd.FileName = mainVM.FileName + "." + AssimpGeneric.GetFormatFileExtension(fileFormat);
                sfd.ShowDialog();
                if (sfd.FileName != "")
                {
                    string dirPath = System.IO.Path.GetDirectoryName(sfd.FileName);

                    if (!Directory.Exists(dirPath))
                        return;

                    dirPath += "\\";

                    AssimpGeneric.ExportScene(scene, fileFormat, sfd.FileName);
                    exportTextures(dirPath);
                }
            }
        }
        public void exportTextures(string filePath)
        {
            for (int i = 0; i < mainVM.TextureFile.Images.Count; i++)
            {
                ModelTexture.Texture texture = mainVM.TextureFile.Images[i];
                BitmapSource bitmapImage = texture.GetBimapSource();

                string fullPath = filePath + "Texture" + i.ToString("D4");
                string finalPath = fullPath;
                int repeat = 0;
                while (File.Exists(finalPath))
                {
                    repeat++;
                    finalPath = fullPath + " (" + repeat + ")";
                }

                AssimpGeneric.ExportBitmapSourceAsPng(bitmapImage, fullPath);
            }
        }
        public void reloadModelControl()
        {
            if (mainVM.ModelFile != null)
            {
                contentFrame.Content = new Model_Control(mainVM.ModelFile, mainVM.TextureFile, mainVM.CollisionFile);
            }
        }
    }
}
