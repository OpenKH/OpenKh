using System.IO;
using System;

namespace OpenKh.Tests.Commands
{
    internal class FileDisposer : IDisposable
    {
        private readonly string[] _fileNames;

        public FileDisposer(params string[] fileNames)
        {
            _fileNames = fileNames;
        }

        public void Dispose()
        {
            foreach (var fileName in _fileNames)
                if (File.Exists(fileName))
                    File.Delete(fileName);
        }
    }
}
