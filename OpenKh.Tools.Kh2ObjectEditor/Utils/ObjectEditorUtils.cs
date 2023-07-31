using System.IO;

namespace OpenKh.Tools.Kh2ObjectEditor.Utils
{
    public class ObjectEditorUtils
    {
        public static bool isFilePathValid(string filepath, string extension)
        {
            if (!File.Exists(filepath))
                return false;
            if (!filepath.ToLower().EndsWith("."+extension))
                return false;

            return true;
        }
    }
}
