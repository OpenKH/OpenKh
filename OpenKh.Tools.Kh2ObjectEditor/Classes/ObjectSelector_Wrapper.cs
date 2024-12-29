using OpenKh.Tools.Kh2ObjectEditor.Utils;

namespace OpenKh.Tools.Kh2ObjectEditor.Classes
{
    public class ObjectSelector_Wrapper
    {
        public string FilePath { get; set; }
        public string FileName { get; set; } // UNIQUE KEY
        public bool HasMset { get; set; }
        public bool Selected { get; set; }

        public string GetDescription()
        {
            string description = "";
            if (ObjectDictionary.Instance.ContainsKey(FileName.ToUpper()))
            {
                description = ObjectDictionary.Instance[FileName.ToUpper()];
            }
            return description;
        }
    }
}
