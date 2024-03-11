using OpenKh.AssimpUtils;
using OpenKh.Engine.Parsers;
using OpenKh.Kh2;
using OpenKh.Tools.Common.Wpf;
using OpenKh.Tools.Kh2MdlxEditor.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Windows;
using System.Windows.Controls;
//using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace OpenKh.Tools.Kh2MdlxEditor.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class Main_Window : Window
    {
        // VIEW MODEL
        //-----------------------------------------------------------------------
        Main_VM mainWiewModel { get; set; }

        // CONSTRUCTOR
        //-----------------------------------------------------------------------
        public Main_Window()
        {
            InitializeComponent();
            loadFile(null);
        }

        // ACTIONS
        //-----------------------------------------------------------------------

        // Opens the file that has been dropped on the window
        private void Window_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[]) e.Data.GetData(DataFormats.FileDrop);
                string firstFile = files?.FirstOrDefault();

                if (true) {
                    loadFile(firstFile);
                }
                // TESTING
                else {
                    //OpenMdlxFile(firstFile);
                    analyzeFiles(files.ToList());
                }
            }
        }
        private void Menu_SaveFile(object sender, EventArgs e)
        {
            saveFile();
        }

        private void Menu_ExportAsFbx(object sender, EventArgs e)
        {
            exportModel(AssimpGeneric.FileFormat.fbx);
        }
        private void Menu_ExportAsDae(object sender, EventArgs e)
        {
            exportModel(AssimpGeneric.FileFormat.collada);
        }

        // Loads the required control in the frame on BAR item click
        public void ListViewItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            openBarEntry(((ListViewItem)sender).Content as Bar.Entry);
        }

        // FUNCTIONS
        //-----------------------------------------------------------------------

        // Loads the given file
        public void loadFile(string filePath)
        {
            contentFrame.Content = null;
            mainWiewModel = (filePath != null) ? new Main_VM(filePath) : new Main_VM();
            DataContext = mainWiewModel;
        }

        // Saves the file
        public void saveFile()
        {
            mainWiewModel.buildBarFile();

            System.Windows.Forms.SaveFileDialog sfd;
            sfd = new System.Windows.Forms.SaveFileDialog();
            sfd.Title = "Save file";
            sfd.FileName = mainWiewModel.FileName + ".out.mdlx";
            sfd.ShowDialog();
            if (sfd.FileName != "")
            {
                MemoryStream memStream = new MemoryStream();
                Bar.Write(memStream, mainWiewModel.BarFile);
                File.WriteAllBytes(sfd.FileName, memStream.ToArray());
            }
        }

        // Loads the User Control required for the given bar entry
        public void openBarEntry(Bar.Entry barEntry)
        {
            if (barEntry == null)
            {
                contentFrame.Content = null;
                return;
            }

            switch (barEntry.Type)
            {
                case Bar.EntryType.Model:
                    contentFrame.Content = new ModelFile_Control(mainWiewModel.ModelFile);
                    break;
                case Bar.EntryType.ModelTexture:
                    contentFrame.Content = new TextureFile_Control(mainWiewModel.TextureFile);
                    break;
                case Bar.EntryType.ModelCollision:
                    contentFrame.Content = new Collision_Control(mainWiewModel.CollisionFile);
                    break;
                default:
                    break;
            }
        }

        // Exports the first submodel of the loaded MDLX
        public void exportModel(AssimpGeneric.FileFormat fileFormat = AssimpGeneric.FileFormat.fbx)
        {
            if(mainWiewModel.ModelFile != null)
            {
                MdlxParser mParser = new MdlxParser(mainWiewModel.ModelFile);
                Assimp.Scene scene = Kh2MdlxAssimp.getAssimpScene(mParser);

                System.Windows.Forms.SaveFileDialog sfd;
                sfd = new System.Windows.Forms.SaveFileDialog();
                sfd.Title = "Export model";
                sfd.FileName = mainWiewModel.FileName + "." + AssimpGeneric.GetFormatFileExtension(fileFormat);
                sfd.ShowDialog();
                if (sfd.FileName != "")
                {
                    string dirPath = Path.GetDirectoryName(sfd.FileName);

                    if (!Directory.Exists(dirPath))
                        return;

                    dirPath += "\\";

                    AssimpGeneric.ExportScene(scene, fileFormat, sfd.FileName);
                    exportTextures(mainWiewModel.TextureFile.Images, dirPath);
                }
            }
        }
        /* Changed to static to be used in "MsetLoader's FBX exporter without duplicating the method. " */
        public static void exportTextures(List<ModelTexture.Texture> textures, string filePath)
        {
            for(int i = 0; i < textures.Count; i++)
            {
                ModelTexture.Texture texture = textures[i];
                BitmapSource bitmapImage = texture.GetBimapSource();

                string fullPath = filePath + "Texture" + i;
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


        /////////////////////////// TESTING ///////////////////////////
        // not available for the releases, only for research purposes
        private void Menu_Test(object sender, EventArgs e)
        {
            Debug.WriteLine("BREAKPOINT");
        }
        public void OpenMdlxFile(string filePath)
        {
            using var stream = File.Open(filePath, FileMode.Open);
            Bar binarc = Bar.Read(stream);

            ObservableCollection<Bar.Entry> testColl = new ObservableCollection<Bar.Entry>(binarc);
            testColl[0].Name = "Changed";
            Debug.WriteLine("TEST: " + binarc[0].Name);
            Debug.WriteLine("TEST: " + testColl[0].Name);

            if (binarc != null)
            {
                Mdlx modelFile;
                ModelTexture textureFile;
                ModelCollision collisionFile;

                foreach (Bar.Entry barEntry in binarc)
                {
                    switch (barEntry.Type)
                    {
                        case Bar.EntryType.Model:
                            modelFile = Mdlx.Read(barEntry.Stream);
                            break;
                        case Bar.EntryType.ModelTexture:
                            textureFile = ModelTexture.Read(barEntry.Stream);
                            break;
                        case Bar.EntryType.ModelCollision:
                            collisionFile = new ModelCollision(barEntry.Stream);
                            break;
                        default:
                            break;
                    }
                }
                Debug.WriteLine("BREAKPOINT");
            }
        }
        public void analyzeFiles(List<string> list_filePaths)
        {
            int totalFiles = 0;
            int validFiles = 0;
            List<string> filesFalse = new List<string>();

            foreach (string filePath in list_filePaths)
            {
                totalFiles++;

                if (filePath == null || !filePath.EndsWith(".mdlx"))
                    continue;

                using var stream = File.Open(filePath, FileMode.Open);
                if (!Bar.IsValid(stream)) 
                    continue;

                Bar binarc = Bar.Read(stream);


                foreach (Bar.Entry barEntry in binarc)
                {
                    switch (barEntry.Type)
                    {
                        case Bar.EntryType.Model:
                            //Mdlx modelFile = Mdlx.Read(barEntry.Stream);
                            break;
                        case Bar.EntryType.ModelTexture:
                            //ModelTexture textureFile = ModelTexture.Read(barEntry.Stream);
                            break;
                        case Bar.EntryType.ModelCollision:
                            ModelCollision collisionFile = new ModelCollision(barEntry.Stream);
                            validFiles++;
                            if (collisionFile.Enable != 1)
                            {
                                filesFalse.Add(filePath);
                                Debug.WriteLine("FALSE: " + filePath + " ["+ collisionFile.Enable + "]");
                            }
                            break;
                        default:
                            break;
                    }
                }
            }
            Debug.WriteLine("totalFiles: " + totalFiles);
            Debug.WriteLine("validFiles: " + validFiles);
            Debug.WriteLine("filesFalse count: " + filesFalse.Count);
        }
    }
}
