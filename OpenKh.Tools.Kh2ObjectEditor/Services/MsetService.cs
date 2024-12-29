using OpenKh.Kh2;
using OpenKh.Tools.Kh2ObjectEditor.Utils;
using System;
using System.IO;
using System.Windows.Forms;

namespace OpenKh.Tools.Kh2ObjectEditor.Services
{
    public class MsetService
    {
        // Apdx File
        public string MsetPath { get; set; }
        //public Bar MsetBar { get; set; }
        public BinaryArchive MsetBinarc { get; set; }
        //public List<AnimationBinary> Motions { get; set; }
        //public List<Bar> MotionSets { get; set; } // Reaction Commands
        // public List<Motion> OtherMotions  { get; set; } // Some msets have 0x09 entries

        // Selection
        public int LoadedMotionId { get; set; }
        public AnimationBinary LoadedMotion { get; set; }

        public void LoadMset(string msetPath)
        {
            if (!ObjectEditorUtils.isFilePathValid(msetPath, "mset"))
                throw new FileNotFoundException("Mset does not exist: " + msetPath);

            MsetPath = msetPath;

            using var streamMset = File.Open(MsetPath, FileMode.Open);
            if (!BinaryArchive.IsValid(streamMset))
                throw new Exception("File is not a valid MSET: " + MsetPath);

            MsetBinarc = BinaryArchive.Read(streamMset);
        }

        public void LoadMotion(int motionIndex)
        {
            LoadedMotionId = motionIndex;
            LoadCurrentMotion();
            ViewerService.Instance.LoadMotion();
        }
        public void LoadCurrentMotion()
        {
            BinaryArchive.Entry motionEntry = MsetBinarc.Entries[LoadedMotionId];
            using MemoryStream fileStream = new MemoryStream(MsetBinarc.Subfiles[motionEntry.Link]);
            LoadedMotion = new AnimationBinary(fileStream);
        }

        public void SaveMotion()
        {
            BinaryArchive.Entry motionEntry = MsetBinarc.Entries[LoadedMotionId];
            MemoryStream motionStream = (MemoryStream)LoadedMotion.toStream();
            MsetBinarc.Subfiles[motionEntry.Link] = motionStream.ToArray();
        }

        public void SaveFile()
        {
            SaveFileDialog sfd;
            sfd = new SaveFileDialog();
            sfd.Title = "Save file";
            sfd.FileName = Path.GetFileNameWithoutExtension(MsetPath) + ".out.mset";
            sfd.ShowDialog();
            if (sfd.FileName != "")
            {
                File.WriteAllBytes(sfd.FileName, MsetBinarc.getAsByteArray());
            }
        }

        public void OverwriteFile()
        {
            if (MsetPath == null)
                return;

            File.WriteAllBytes(MsetPath, MsetBinarc.getAsByteArray());
        }

        // SINGLETON
        private MsetService() { }
        private static MsetService _instance = null;
        public static MsetService Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new MsetService();
                }
                return _instance;
            }
        }
        public static void Reset()
        {
            _instance = new MsetService();
        }
    }
}
