using OpenKh.Kh2;
using System;
using System.IO;

namespace OpenKh.Tools.Kh2MdlxEditor.ViewModels
{
    public class Main_VM
    {
        // DATA
        //-----------------------------------------------------------------------
        public string FilePath { get; set; }
        public string FileName { get; set; }
        public Bar BarFile { get; set; }

        // Files for available editors
        public Mdlx ModelFile { get; set; }
        public ModelTexture TextureFile { get; set; }
        public ModelCollision CollisionFile { get; set; }

        // CONSTRUCTORS
        //-----------------------------------------------------------------------
        public Main_VM() { }
        public Main_VM(string filePath)
        {
            LoadFile(filePath);
        }

        // FUNCTIONS
        //-----------------------------------------------------------------------
        // Loads the file given a full path filename
        public void LoadFile(string filePath)
        {
            if (!isValidFilepath(filePath))
                return;

            this.FilePath = filePath;
            this.FileName = Path.GetFileNameWithoutExtension(FilePath);
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
            foreach(Bar.Entry barEntry in BarFile)
            {
                try
                {
                    switch (barEntry.Type)
                    {
                        case Bar.EntryType.Model:
                            ModelFile = Mdlx.Read(barEntry.Stream);
                            break;
                        case Bar.EntryType.ModelTexture:
                            TextureFile = ModelTexture.Read(barEntry.Stream);
                            break;
                        case Bar.EntryType.ModelCollision:
                            CollisionFile = new ModelCollision(barEntry.Stream);
                            break;
                        default:
                            break;
                    }
                }
                catch(Exception e) { }
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
                        ModelFile.Write(barEntry.Stream);
                        break;
                    case Bar.EntryType.ModelTexture:
                        TextureFile.Write(barEntry.Stream);
                        break;
                    case Bar.EntryType.ModelCollision:
                        barEntry.Stream = CollisionFile.toStream();
                        break;
                    default:
                        break;
                }
            }
        }

        public bool isValidFilepath(string filePath)
        {
            return (filePath != null && filePath.EndsWith(".mdlx"));
        }
    }
}
