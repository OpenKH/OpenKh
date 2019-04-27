using System;
using System.IO;

namespace kh.tests
{
    public static class Helpers
    {
        public static void Using<T>(this T disposable, Action<T> action)
            where T : IDisposable
        {
            using (disposable)
                action(disposable);
        }

        public static TResult Using<T, TResult>(this T disposable, Func<T, TResult> func)
            where T : IDisposable
        {
            using (disposable)
                return func(disposable);
        }

        public static void Dump(this Stream stream, string path) =>
            File.OpenWrite(path).Using(outStream =>
            {
                stream.Position = 0;
                stream.CopyTo(outStream);
            });

        public static void Dump(this byte[] data, string path) =>
            new MemoryStream(data).Using(x => x.Dump(path));
    }

    public class Common
    {
        public static void FileOpenRead(string path, Action<Stream> action)
        {
            File.OpenRead(path).Using(x => action(x));
        }
    }
}
