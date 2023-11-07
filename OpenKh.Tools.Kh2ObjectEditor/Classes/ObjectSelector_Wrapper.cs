namespace OpenKh.Tools.Kh2ObjectEditor.Classes
{
    public class ObjectSelector_Wrapper
    {
        public string FilePath { get; set; }
        public string FileName { get; set; } // UNIQUE KEY
        public bool HasMset { get; set; }
        public bool Selected { get; set; }
    }
}
