using OpenKh.Tools.Kh2ObjectEditor.Classes;
using System;

namespace OpenKh.Tools.Kh2ObjectEditor.Views
{
    public class Main_ViewModel
    {
        public string FolderPath { get; set; }
        public string Filename { get; set; }

        public ObjectSelector_Wrapper SelectedObject { get; set; }
        public Object_Wrapper LoadedObject { get; set; }
        public MotionSelector_Wrapper LoadedMotion { get; set; }

        public void loadObject(Object_Wrapper loadedObject)
        {
            LoadedObject = loadedObject;
            if(Load != null)
            {
                Load(this, e);
            }
        }
        public void loadMotion(MotionSelector_Wrapper loadedMotion)
        {
            LoadedMotion = loadedMotion;
            if (MotionSelect != null)
            {
                MotionSelect(this, e);
            }
        }

        // Events
        public event EventHandler Load;
        public event EventHandler MotionSelect;
        public EventArgs e = null;
        public delegate void EventHandler(Main_ViewModel mainVM, EventArgs e);
    }
}
