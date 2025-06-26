using CommunityToolkit.Mvvm.ComponentModel;
using OpenKh.Kh2;
using OpenKh.Tools.Kh2ObjectEditor.Utils;
using System;
using System.IO;
using System.Windows.Forms;

namespace OpenKh.Tools.Kh2ObjectEditor.Services
{
    public partial class MdlxService : ObservableObject
    {
        // Apdx File
        [ObservableProperty] public string mdlxPath;
        public Bar MdlxBar { get; set; }
        public Kh2.Models.ModelSkeletal ModelFile { get; set; }
        public ModelCollision CollisionFile { get; set; }
        public ModelTexture TextureFile { get; set; }
        public Stream BdxFile { get; set; }
        // May also contain Pax files (Uncommon, 46/2121 files)

        public void LoadMdlx(string mdlxPath)
        {
            if (!ObjectEditorUtils.isFilePathValid(mdlxPath, "mdlx"))
                throw new FileNotFoundException("Mdlx does not exist: " + mdlxPath);

            MdlxPath = mdlxPath;

            using var streamMdlx = File.Open(MdlxPath, FileMode.Open);
            if (!Bar.IsValid(streamMdlx))
                throw new Exception("File is not a valid MDLX: " + MdlxPath);

            MdlxBar = Bar.Read(streamMdlx);

            MsetService.Reset();
            ApdxService.Reset();

            ModelFile = null;
            TextureFile = null;
            CollisionFile = null;
            BdxFile = null;
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

            ViewerService.Instance.Render();
        }

        public void SaveModel()
        {
            foreach (Bar.Entry barEntry in MdlxBar)
            {
                switch (barEntry.Type)
                {
                    case Bar.EntryType.Model:
                        barEntry.Stream = new MemoryStream();
                        ModelFile.Write(barEntry.Stream);
                        barEntry.Stream.Position = 0;
                        break;
                    case Bar.EntryType.ModelTexture:
                        barEntry.Stream = new MemoryStream();
                        TextureFile.Write(barEntry.Stream);
                        barEntry.Stream.Position = 0;
                        break;
                    case Bar.EntryType.ModelCollision:
                        barEntry.Stream = CollisionFile.toStream();
                        barEntry.Stream.Position = 0;
                        break;
                    case Bar.EntryType.Bdx:
                        barEntry.Stream = new MemoryStream();
                        BdxFile.CopyTo(barEntry.Stream);
                        barEntry.Stream.Position = 0;
                        break;
                    default:
                        break;
                }
            }
        }

        public void SaveFile()
        {
            SaveModel();
            SaveFileDialog sfd;
            sfd = new SaveFileDialog();
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

        public void OverwriteFile()
        {
            if (MdlxPath == null)
                return;

            SaveModel();

            MemoryStream memStream = new MemoryStream();
            Bar.Write(memStream, MdlxBar);
            File.WriteAllBytes(MdlxPath, memStream.ToArray());
        }

        // SINGLETON
        private MdlxService() { }
        private static MdlxService _instance = null;
        public static MdlxService Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new MdlxService();
                }
                return _instance;
            }
        }
        public static void Reset()
        {
            _instance = new MdlxService();
        }
    }
}
