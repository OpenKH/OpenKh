using OpenKh.Kh2;
using System.Collections.ObjectModel;
using System.IO;

namespace OpenKh.Tools.Kh2MsetEditor.ViewModels
{
    public class DataView_VM
    {
        // DATA
        //-----------------------------------------------------------------------
        public string FilePath { get; set; }
        public string FileName { get; set; }
        public Bar BarFile { get; set; }
        public ObservableCollection<AnbEntryWrapper> entryList_View { get; set; }

        // SETTINGS
        //-----------------------------------------------------------------------
        public bool filterDummies = false;

        // CONSTRUCTORS
        //-----------------------------------------------------------------------
        public DataView_VM() { }
        public DataView_VM(string filePath)
        {
            LoadFile(filePath);
        }

        public class AnbEntryWrapper
        {
            public int Index { get; set; }
            public Bar.Entry Entry { get; set; }
            public string Name { get; set; }

            public AnbEntryWrapper(int index, Bar.Entry entry)
            {
                Index = index;
                Entry = entry;
                Name = "[" + Index + "] " + Entry.Name + " [" + (MotionSet.MotionName)(index / 4) + "]";
            }
        }

        // FUNCTIONS
        //-----------------------------------------------------------------------
        // Loads the file given a full path filename
        public void LoadFile(string filePath)
        {
            if (!isValidFilepath(filePath))
                return;

            this.FilePath = filePath;
            this.FileName = Path.GetFileNameWithoutExtension(FilePath);
            LoadFile();
        }
        public void LoadFile()
        {
            if (!isValidFilepath(FilePath))
                return;

            using var stream = File.Open(FilePath, FileMode.Open);
            if (!Bar.IsValid(stream))
                return;

            BarFile = Bar.Read(stream);
            loadViewList();
        }
        public void loadViewList()
        {
            if (entryList_View == null)
                entryList_View = new ObservableCollection<AnbEntryWrapper>();

            entryList_View.Clear();
            int barIndex = 0;
            foreach (Bar.Entry barEntry in BarFile)
            {
                entryList_View.Add(new AnbEntryWrapper(barIndex, barEntry));
                barIndex++;
            }

            if (filterDummies)
            {
                for (int i = entryList_View.Count - 1; i >= 0; i--)
                {
                    if (entryList_View[i].Entry.Name == "DUMM")
                    {
                        entryList_View.RemoveAt(i);
                    }
                }
            }
        }

        // Builds the MSET with the edited data
        public void buildBarFile()
        {
            foreach (Bar.Entry barEntry in BarFile)
            {
                switch (barEntry.Type)
                {
                    case Bar.EntryType.Motion:
                        //Motion.Write(barEntry.Stream, MotionFile);
                        break;
                    default:
                        break;
                }
            }
        }

        public static bool isValidFilepath(string filePath)
        {
            return (filePath != null && filePath.EndsWith(".mset"));
        }
    }
}
