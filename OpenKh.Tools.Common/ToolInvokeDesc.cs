using OpenKh.Kh2;

namespace OpenKh.Tools.Common
{
    public class ToolInvokeDesc
    {
        public enum ContentChangeInfo
        {
            None,
            File,
            Entry
        }

        public string Title { get; set; }

        public string ActualFileName { get; set; }

        public Bar.Entry SelectedEntry { get; set; }

        /// <summary>
        /// Once the tool is closed, this field will say which input resource
        /// has been modified.
        /// </summary>
        public ContentChangeInfo ContentChange { get; set; }
    }
}
