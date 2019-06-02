using System;
using System.IO;
using Xunit;

namespace OpenKh.Tests
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

        public static void AssertStream(Stream expectedStream, Func<Stream, Stream> funcGenerateNewStream)
        {
            var expectedData = ReadBytes(expectedStream);
            var actualStream = funcGenerateNewStream(new MemoryStream(expectedData));
            var actualData = ReadBytes(actualStream);

            Assert.Equal(expectedData.Length, actualData.Length);
            Assert.Equal(expectedData, actualData);
        }

        public static byte[] ReadBytes(Stream stream)
        {
            var data = new byte[stream.Length];
            stream.Position = 0;
            stream.Read(data, 0, data.Length);
            stream.Position = 0;

            return data;
        }

        public static void UseAsset(string assetName, Action<Stream> action) =>
            File.OpenRead(Path.Combine($"_Assets", assetName)).Using(x => action(x));

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
