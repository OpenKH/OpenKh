using System;
using System.IO;

namespace kh.tests
{
    public class Common
    {

        protected void FileOpenRead(string path, Action<Stream> action)
        {
            using (var stream = File.OpenRead(path))
                action(stream);
        }
    }
}
