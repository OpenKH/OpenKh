using OpenKh.Kh2;
using OpenKh.Tools.Kh2ObjectEditor.Utils;
using System;
using System.IO;
using System.Windows.Forms;

namespace OpenKh.Tools.Kh2ObjectEditor.Services
{
    public class Mset_Service
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

        public void loadMset(string msetPath)
        {
            if (!ObjectEditorUtils.isFilePathValid(msetPath, "mset"))
                throw new FileNotFoundException("Mset does not exist: " + msetPath);

            MsetPath = msetPath;

            using var streamMset = File.Open(MsetPath, FileMode.Open);
            if (!Bar.IsValid(streamMset))
                throw new Exception("File is not a valid MSET: " + MsetPath);

            MsetBar = Bar.Read(streamMset);
        }

        public void loadMotion(int motionIndex)
        {
            LoadedMotionId = motionIndex;
            loadCurrentMotion();
        }
        public void loadCurrentMotion()
        {
            MsetBar[LoadedMotionId].Stream.Position = 0;
            LoadedMotion = new AnimationBinary(MsetBar[LoadedMotionId].Stream);
            MsetBar[LoadedMotionId].Stream.Position = 0;
        }

        public void saveMotion()
        {
            MsetBar[LoadedMotionId].Stream = LoadedMotion.toStream();
        }

        public void saveFile()
        {
            System.Windows.Forms.SaveFileDialog sfd;
            sfd = new System.Windows.Forms.SaveFileDialog();
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

        // SINGLETON
        private Mset_Service() { }
        private static Mset_Service instance = null;
        public static Mset_Service Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new Mset_Service();
                }
                return instance;
            }
        }
        public static void reset()
        {
            instance = new Mset_Service();
        }
    }
}
