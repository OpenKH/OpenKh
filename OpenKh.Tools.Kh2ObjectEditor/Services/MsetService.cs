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
        public Bar MsetBar { get; set; }
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
            if (!Bar.IsValid(streamMset))
                throw new Exception("File is not a valid MSET: " + MsetPath);

            MsetBar = Bar.Read(streamMset);
        }

        public void LoadMotion(int motionIndex)
        {
            LoadedMotionId = motionIndex;
            LoadCurrentMotion();
        }
        public void LoadCurrentMotion()
        {
            MsetBar[LoadedMotionId].Stream.Position = 0;
            LoadedMotion = new AnimationBinary(MsetBar[LoadedMotionId].Stream);
            MsetBar[LoadedMotionId].Stream.Position = 0;
        }

        public void SaveMotion()
        {
            MsetBar[LoadedMotionId].Stream = LoadedMotion.toStream();
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
                MemoryStream memStream = new MemoryStream();
                Bar.Write(memStream, MsetBar);
                File.WriteAllBytes(sfd.FileName, memStream.ToArray());
            }
        }

        public void OverwriteFile()
        {
            if (MsetPath == null)
                return;

            MemoryStream memStream = new MemoryStream();
            Bar.Write(memStream, MsetBar);
            File.WriteAllBytes(MsetPath, memStream.ToArray());
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
