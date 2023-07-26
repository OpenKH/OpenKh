using OpenKh.Kh2;

namespace OpenKh.Tools.Kh2ObjectEditor.Classes
{
    public class MotionSelector_Wrapper
    {
        public int Index { get; set; }
        public Bar.Entry Entry { get; set; }
        public string Name { get; set; }
        public bool IsDummy { get { return Name.Contains("DUMM"); } }

        public MotionSelector_Wrapper(int index, Bar.Entry entry)
        {
            Index = index;
            Entry = entry;
            Name = "[" + Index + "] " + Entry.Name + " [" + (MotionSet.MotionName)(index / 4) + "]";
        }


    }
}
