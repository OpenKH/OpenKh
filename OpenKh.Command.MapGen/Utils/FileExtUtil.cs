using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace OpenKh.Command.MapGen.Utils
{
    static class FileExtUtil
    {
        public static bool IsExtension(string path, string ext) => 
            Path.GetExtension(path).Equals(ext, StringComparison.InvariantCultureIgnoreCase);

    }
}
