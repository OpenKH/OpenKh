using OpenKh.Kh2;
using OpenKh.Kh2.Models;
using OpenKh.Tools.Kh2ObjectEditor.Classes;
using OpenKh.Tools.Kh2ObjectEditor.Utils;
using System;
using System.Collections.Generic;
using System.IO;

namespace OpenKh.Tools.Kh2ObjectEditor.ViewModel
{
    /*
     * This is the context of the whole app.
     * All of the data stored here is accessible throughout the app.
     */
    public class App_Context
    {
        // PHASE 1: Load input
        public string FolderPath { get; set; }
        public string MdlxPath { get; set; }
        public string MsetPath { get; set; }

        // PHASE 2: Open selected object
        public Bar MdlxBar { get; set; }
        public Bar MsetBar { get; set; }
        public ModelSkeletal ModelFile { get; set; }
        public ModelCollision CollisionFile { get; set; }
        public ModelTexture TextureFile { get; set; }
        public List<MotionSelector_Wrapper> MsetEntries { get; set; }

        // PHASE 3: Open selected motion
        public MotionSelector_Wrapper LoadedMotion { get; set; }
        public AnimationBinary AnimBinary { get; set; }


        public void resetData()
        {
            FolderPath = null;
            MdlxPath = null;
            MsetPath = null;
            MdlxBar = null;
            MsetBar = null;
            ModelFile = null;
            CollisionFile = null;
            TextureFile = null;
            MsetEntries = null;
            LoadedMotion = null;
        }
        public void resetObject()
        {
            MdlxBar = null;
            MsetBar = null;
            ModelFile = null;
            CollisionFile = null;
            TextureFile = null;
            MsetEntries = null;
            LoadedMotion = null;
        }

        public void loadFolder(string folderPath)
        {
            resetData();
            FolderPath = folderPath;
            triggerFolderSelected();
        }

        public void loadMdlx(string mdlxPath)
        {
            if (!ObjectEditorUtils.isFilePathValid(mdlxPath, "mdlx"))
                throw new FileNotFoundException("Mdlx does not exist: " + mdlxPath);

            resetData();

            MdlxPath = mdlxPath;
            string tempMsetPath = MdlxPath.ToLower().Replace(".mdlx", ".mset");
            if (ObjectEditorUtils.isFilePathValid(tempMsetPath, "mset"))
            {
                MsetPath = tempMsetPath;
            }

            openObject();
        }

        public void loadMset(string msetPath)
        {
            if (!ObjectEditorUtils.isFilePathValid(msetPath, "mset"))
                throw new FileNotFoundException("Mset does not exist: " + msetPath);

            if(MdlxPath == null)
                throw new FileNotFoundException("No Mdlx to apply Mset");

            MsetPath = msetPath;

            openObject();
        }

        public void loadObject(string mdlxPath)
        {
            if (!ObjectEditorUtils.isFilePathValid(mdlxPath, "mdlx"))
                throw new FileNotFoundException("Mdlx does not exist: " + mdlxPath);

            MdlxPath = mdlxPath;
            string tempMsetPath = MdlxPath.ToLower().Replace(".mdlx", ".mset");
            if (ObjectEditorUtils.isFilePathValid(tempMsetPath, "mset"))
            {
                MsetPath = tempMsetPath;
            }

            openObject();
        }

        public void loadMotion(MotionSelector_Wrapper motion)
        {
            LoadedMotion = motion;
            openMotion();
        }

        public void openObject()
        {
            resetObject();

            // MDLX
            if (MdlxPath != null)
            {
                if (!ObjectEditorUtils.isFilePathValid(MdlxPath, "mdlx"))
                    throw new FileNotFoundException("Mdlx does not exist: " + MdlxPath);

                using var streamMdlx = File.Open(MdlxPath, FileMode.Open);
                if (!Bar.IsValid(streamMdlx))
                    throw new Exception("File is not a valid MDLX: " + MdlxPath);

                MdlxBar = Bar.Read(streamMdlx);

                foreach (Bar.Entry barEntry in MdlxBar)
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
                            default:
                                break;
                        }
                    }
                    catch (Exception e) { }
                }
            }

            // MSET
            if (MsetPath != null)
            {
                if (!ObjectEditorUtils.isFilePathValid(MsetPath, "mset"))
                    throw new FileNotFoundException("Mset does not exist: " + MsetPath);

                using var streamMset = File.Open(MsetPath, FileMode.Open);
                if (!Bar.IsValid(streamMset))
                    throw new Exception("File is not a valid MSET: " + MsetPath);

                MsetBar = Bar.Read(streamMset);

                MsetEntries = new List<MotionSelector_Wrapper>();
                for (int i = 0; i < MsetBar.Count; i++)
                {
                    MsetEntries.Add(new MotionSelector_Wrapper(i, MsetBar[i]));
                }
            }

            triggerObjectSelected();
        }

        public void openMotion()
        {
            AnimBinary = new AnimationBinary(LoadedMotion.Entry.Stream);
            triggerMotionSelected();
        }

        public void saveMotion()
        {
            if (App_Context.Instance.AnimBinary == null)
                return;

            Stream saveStream = App_Context.Instance.AnimBinary.toStream();
            saveStream.Position = 0;

            App_Context.Instance.MsetEntries[App_Context.Instance.LoadedMotion.Index].Entry.Stream = saveStream;
            App_Context.Instance.MsetBar[App_Context.Instance.LoadedMotion.Index].Stream = saveStream;
        }

        // EVENTS
        public EventArgs e = null;
        public delegate void EventHandler(App_Context app_Context, EventArgs e);

        public event EventHandler Event_FolderSelected;
        public void triggerFolderSelected() {
            if (Event_FolderSelected != null)
                Event_FolderSelected(this, e);
        }

        public event EventHandler Event_ObjectSelected;
        public void triggerObjectSelected()
        {
            if (Event_ObjectSelected != null)
                Event_ObjectSelected(this, e);
        }

        public event EventHandler Event_MotionSelected;
        public void triggerMotionSelected()
        {
            if (Event_MotionSelected != null)
                Event_MotionSelected(this, e);
        }

        // SINGLETON
        private App_Context() { }
        private static App_Context instance = null;
        public static App_Context Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new App_Context();
                }
                return instance;
            }
        }
    }
}
