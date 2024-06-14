using OpenKh.Kh2;

namespace OpenKh.Tools.Kh2ObjectEditor.Classes
{
    public class MotionSelector_Wrapper
    {
        public int Index { get; set; }
        public Bar.Entry Entry { get; set; }
        public string Name { get; set; }
        public bool IsDummy { get { return Name.Contains("DUMM") || Entry.Stream.Length == 0; } }

        public MotionSelector_Wrapper(int index, Bar.Entry entry)
        {
            Index = index;
            Entry = entry;
            setName();
        }
        public void setName()
        {
            Name = "[" + Index + "] " + Entry.Name + " [" + (MotionSet.MotionName)(Index / 4) + "]";
            if (Entry.Type == Bar.EntryType.Motionset)
                Name += "<RC>";
            if (Entry.Stream.Length == 0)
                Name += "<DUMMY>";
        }
    }
}
