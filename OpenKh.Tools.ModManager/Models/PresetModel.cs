using OpenKh.Tools.ModManager.Classes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenKh.Tools.ModManager.Models
{
    public class PresetModel
    {
        private string NAME;
        private string FOLDER_NAME;

        public string Name 
        {
            get => NAME;
            set
            {
                NAME = value;

                var _fetchNormalStr = value.ToLower().Replace(" ", "_");

                foreach (var _fetchChar in Path.GetInvalidFileNameChars())
                    _fetchNormalStr = _fetchNormalStr.Replace(_fetchChar, '-');

                FOLDER_NAME = _fetchNormalStr;
            }
        }

        public string FolderName { get => FOLDER_NAME; set => FOLDER_NAME = value; }

        public Game TargetGame { get; set; }
    }
}
