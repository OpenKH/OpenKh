using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenKh.Tools.ModManager.Models
{
    public class PreferenceModel
    {
        public string Title { get; set; }
        public string Key { get; set; }
        public string Type { get; set; }
        public string Description { get; set; }
        public string Category { get; set; }

        public string[]? Options { get; set; }
        public object Value { get; set; }

        public bool IsValueBoolean => Type == "bool";
        public bool IsValueComboBox => Type == "options";
    }
}
