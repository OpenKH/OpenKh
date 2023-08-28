using OpenKh.Kh2;
using OpenKh.Tools.Kh2ObjectEditor.Utils;
using System;
using System.Collections.Generic;
using System.IO;

namespace OpenKh.Tools.Kh2ObjectEditor.Services
{
    public class Mdlx_Service
    {
        // Apdx File
        public string MdlxPath { get; set; }
        public Bar MdlxBar { get; set; }
        public Kh2.Models.ModelSkeletal ModelFile { get; set; }
        public ModelCollision CollisionFile { get; set; }
        public ModelTexture TextureFile { get; set; }
        public Stream BdxFile { get; set; }
        // May also contain Pax files (Uncommon, 46/2121 files)


        // Note: Copied as JSON in order to not copy the reference
        public string CopiedCollision { get; set; }
        public List<string> CopiedCollisionList { get; set; }

        public void loadMdlx(string mdlxPath)
        {
            if (!ObjectEditorUtils.isFilePathValid(mdlxPath, "mdlx"))
                throw new FileNotFoundException("Mdlx does not exist: " + mdlxPath);

            MdlxPath = mdlxPath;

            using var streamMdlx = File.Open(MdlxPath, FileMode.Open);
            if (!Bar.IsValid(streamMdlx))
                throw new Exception("File is not a valid MDLX: " + MdlxPath);

            MdlxBar = Bar.Read(streamMdlx);

            Mset_Service.reset();
            Apdx_Service.reset();

            foreach (Bar.Entry barEntry in MdlxBar)
            {
                switch (barEntry.Type)
                {
                    case Bar.EntryType.Model:
                        ModelFile = Kh2.Models.ModelSkeletal.Read(barEntry.Stream);
                        break;
                    case Bar.EntryType.ModelTexture:
                        TextureFile = ModelTexture.Read(barEntry.Stream);
                        break;
                    case Bar.EntryType.ModelCollision:
                        CollisionFile = new ModelCollision(barEntry.Stream);
                        break;
                    case Bar.EntryType.Bdx:
                        BdxFile = barEntry.Stream;
                        break;
                    default:
                        break;
                }
            }
        }

        public void saveModel()
        {
            foreach (Bar.Entry barEntry in MdlxBar)
            {
                switch (barEntry.Type)
                {
                    case Bar.EntryType.Model:
                        barEntry.Stream.Position = 0;
                        ModelFile.Write(barEntry.Stream);
                        barEntry.Stream.Position = 0;
                        break;
                    case Bar.EntryType.ModelTexture:
                        barEntry.Stream.Position = 0;
                        TextureFile.Write(barEntry.Stream);
                        barEntry.Stream.Position = 0;
                        break;
                    case Bar.EntryType.ModelCollision:
                        barEntry.Stream.Position = 0;
                        barEntry.Stream = CollisionFile.toStream();
                        barEntry.Stream.Position = 0;
                        break;
                    case Bar.EntryType.Bdx:
                        barEntry.Stream.Position = 0;
                        barEntry.Stream = new MemoryStream();
                        BdxFile.CopyTo(barEntry.Stream);
                        barEntry.Stream.Position = 0;
                        break;
                    default:
                        break;
                }
            }
        }

        public void saveFile()
        {
            saveModel();
            System.Windows.Forms.SaveFileDialog sfd;
            sfd = new System.Windows.Forms.SaveFileDialog();
            sfd.Title = "Save file";
            sfd.FileName = Path.GetFileNameWithoutExtension(MdlxPath) + ".out.mdlx";
            sfd.ShowDialog();
            if (sfd.FileName != "")
            {
                MemoryStream memStream = new MemoryStream();
                Bar.Write(memStream, MdlxBar);
                File.WriteAllBytes(sfd.FileName, memStream.ToArray());
            }
        }

        // SINGLETON
        private Mdlx_Service() { }
        private static Mdlx_Service instance = null;
        public static Mdlx_Service Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new Mdlx_Service();
                }
                return instance;
            }
        }
        public static void reset()
        {
            string copiedCollision = instance.CopiedCollision;
            List<string> copiedCollisionList = instance.CopiedCollisionList;

            instance = new Mdlx_Service();
            instance.CopiedCollision = copiedCollision;
            instance.CopiedCollisionList = copiedCollisionList;
        }
    }
}
