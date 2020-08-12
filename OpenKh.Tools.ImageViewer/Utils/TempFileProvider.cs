using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace OpenKh.Tools.ImageViewer.Utils
{
    class TempFileProvider : IDisposable
    {
        private readonly List<string> files = new List<string>();

        public string NewFile(string fileExtension = ".tmp")
        {
            var path = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N") + fileExtension);
            files.Add(path);
            return path;
        }

        public void Dispose()
        {
            foreach (var file in files)
            {
                try
                {
                    File.Delete(file);
                }
                catch
                {
                    // skip
                }
            }

            files.Clear();
        }
    }
}
