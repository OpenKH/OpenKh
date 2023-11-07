using OpenKh.Tools.Kh2ObjectEditor.Services;
using OpenKh.Tools.Kh2ObjectEditor.Utils;
using System;
using System.IO;

namespace OpenKh.Tools.Kh2ObjectEditor.ViewModel
{
    /*
     * This is the context of the whole app.
     * All of the data stored here is accessible throughout the app.
     */
    public class App_Context
    {
        public string FolderPath { get; set; }
        public string MdlxPath { get; set; }

        public void resetData()
        {
            FolderPath = null;
            MdlxPath = null;
        }

        public void loadFolder(string folderPath)
        {
            resetData();
            FolderPath = folderPath;
            triggerFolderSelected();
        }

        public void openObject()
        {
            if (!ObjectEditorUtils.isFilePathValid(MdlxPath, "mdlx"))
                throw new FileNotFoundException("Mdlx does not exist: " + MdlxPath);

            MdlxService.Instance.LoadMdlx(MdlxPath);

            string tempMsetPath = MdlxPath.ToLower().Replace(".mdlx", ".mset");
            if (ObjectEditorUtils.isFilePathValid(tempMsetPath, "mset"))
            {
                MsetService.Instance.LoadMset(tempMsetPath);
            }

            string tempApdxPath = MdlxPath.ToLower().Replace(".mdlx", ".a.us");
            if (ObjectEditorUtils.isFilePathValid(tempApdxPath, "a.us"))
            {
                ApdxService.Instance.LoadFile(tempApdxPath);
            }

            triggerObjectSelected();
        }

        public void loadMdlx(string mdlxPath)
        {
            if (!ObjectEditorUtils.isFilePathValid(mdlxPath, "mdlx"))
                throw new FileNotFoundException("Mdlx does not exist: " + mdlxPath);

            resetData();

            MdlxPath = mdlxPath;
            openObject();
        }

        public void loadMset(string msetPath)
        {
            if (!ObjectEditorUtils.isFilePathValid(msetPath, "mset"))
                throw new FileNotFoundException("Mset does not exist: " + msetPath);

            if(MdlxPath == null)
                throw new FileNotFoundException("No Mdlx to apply Mset");

            MsetService.Instance.LoadMset(msetPath);
            triggerObjectSelected();
        }

        // Load the motion through here to apply to the viewport
        public void loadMotion(int index)
        {
            MsetService.Instance.LoadMotion(index);
            triggerMotionSelected();
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
