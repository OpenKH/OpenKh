using System;
using System.IO;
using System.Linq;
using Assimp;
using OpenKh.AssimpUtils;
using OpenKh.Kh2;
using OpenKh.Kh2.Models;
using OpenKh.Kh2.Models.VIF;
using OpenKh.Tools.Kh2MdlxEditor.Utils;
using OpenKh.Tools.Kh2MdlxEditor.Views;

namespace OpenKh.Tools.Kh2MdlxEditor.ViewModels
{
    public class Main2_VM
    {
        // DATA
        //-----------------------------------------------------------------------
        public string FilePath { get; set; }
        public string FileName { get; set; }
        public Bar BarFile { get; set; }

        private Main2_Window mainWindow { get; set; }
        
        // Files for available editors
        public ModelSkeletal ModelFile { get; set; }
        public ModelTexture TextureFile { get; set; }
        public ModelCollision CollisionFile { get; set; }

        // CONSTRUCTORS
        //-----------------------------------------------------------------------
        public Main2_VM(Main2_Window mainWindow)
        {
            this.mainWindow = mainWindow;
        }
        public Main2_VM(Main2_Window mainWindow, string filePath)
        {
            LoadFile(filePath);
            this.mainWindow = mainWindow;
        }

        // FUNCTIONS
        //-----------------------------------------------------------------------
        // Loads the file given a full path filename
        public void LoadFile(string filePath)
        {
            if (!isValidFilepath(filePath))
                return;

            FilePath = filePath;
            FileName = Path.GetFileNameWithoutExtension(FilePath);
            LoadFile();
        }
        public void LoadFile()
        {
            if (!isValidFilepath(FilePath))
                return;

            using var stream = File.Open(FilePath, FileMode.Open);
            if (!Bar.IsValid(stream))
                return;

            BarFile = Bar.Read(stream);
            LoadSubFiles();
        }
        public void LoadSubFiles()
        {
            foreach (Bar.Entry barEntry in BarFile)
            {
                try
                {
                    switch (barEntry.Type)
                    {
                        case Bar.EntryType.Model:
                            ModelFile = ModelSkeletal.Read(barEntry.Stream);
                            break;
                        case Bar.EntryType.ModelTexture:
                            TextureFile = ModelTexture.Read(barEntry.Stream);
                            break;
                        case Bar.EntryType.ModelCollision:
                            CollisionFile = new ModelCollision(barEntry.Stream);
                            break;
                    }
                }
                catch (Exception e) { }
            }
        }

        // Builds the MDLX with the edited data
        public void buildBarFile()
        {
            foreach (Bar.Entry barEntry in BarFile)
            {
                switch (barEntry.Type)
                {
                    case Bar.EntryType.Model:
                        barEntry.Stream = new MemoryStream();
                        ModelFile.Write(barEntry.Stream);
                        break;
                    case Bar.EntryType.ModelTexture:
                        barEntry.Stream = new MemoryStream();
                        TextureFile.Write(barEntry.Stream);
                        break;
                    case Bar.EntryType.ModelCollision:
                        barEntry.Stream = CollisionFile.toStream();
                        break;
                }
            }
        }

        public bool isValidFilepath(string filePath)
        {
            return (filePath != null && filePath.EndsWith(".mdlx"));
        }

        public void reloadModelFromFbx()
        {
            // Switching the order of meshes and directly exporting the mdlx right now doesn't work.
            // the exported mdlx file is corrupt. It can still be opened in this editor, and exported to fbx,
            // but cannot be opened in another mdlx viewer or used in the game (the game crashes immediately).
            //
            // However, if you change the mesh order, export the fbx, and import the same fbx, it works.
            // So, we're going to do that. By doing that process here in the code, we also keep the texture
            // animations.
            //
            // Caveat: We need to store it in a temporary file, and this sucks.
            // just creating a Assimp scene and passing it to the Assimp importer doesn't work - the model
            // gets corrupted in some circumstances. (for me it happened with P_EX100)
            // The exporting process to a temporary file actually produces a different scene,
            // which is not corrupted and is otherwise identical.
            // 
            // I'd like this to not have to reload the entire window, but might complicate the code.
            Scene scene = Kh2MdlxAssimp.getAssimpScene(ModelFile);
            
            string fileName = Path.GetTempPath() + Guid.NewGuid() + ".fbx";
            AssimpGeneric.ExportScene(scene, AssimpGeneric.FileFormat.fbx, fileName);

            scene = AssimpGeneric.getAssimpSceneFromFile(fileName);

            MdlxEditorImporter.materialToTexture = ModelFile.Groups
                .Select((item, index) => new { item.Header.TextureIndex, index })
                .ToDictionary(x => x.index, x => (int)x.TextureIndex);
            
            ModelFile = MdlxEditorImporter.replaceMeshModelSkeletal(scene, ModelFile, null);
            mainWindow.reloadModelControl();
        }
        
        public void replaceModel(string filePath)
        {
            Scene scene = AssimpGeneric.getAssimpSceneFromFile(filePath);

            TextureFile = MdlxEditorImporter.createModelTexture(scene, filePath);

            ModelFile = MdlxEditorImporter.replaceMeshModelSkeletal(scene, ModelFile, filePath);
        }
    }
}
