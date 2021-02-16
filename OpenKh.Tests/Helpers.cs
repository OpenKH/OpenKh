using OpenKh.Common;
using OpenKh.Kh2;
using System;
using System.IO;
using System.Linq;
using Xunit;
using Xunit.Sdk;

namespace OpenKh.Tests
{
    internal class AssertBarException : XunitException
    {
        public AssertBarException(string barEntry, XunitException innerException) :
            base($"Failed assertion for BAR {barEntry}", innerException)
        { }
    }

    public static class Helpers
    {
        public const string Kh2DataPath = ".tests/kh2_data/";

        private class AssertFileNameException : XunitException
        {
            public AssertFileNameException(string fileName, Exception innerException) :
                base($"Failed assertion for file ${fileName}", innerException)
            { }
        }

        public static void Dump(this Stream stream, string path) =>
            File.Create(path).Using(outStream =>
            {
                stream.Position = 0;
                stream.CopyTo(outStream);
            });

        public static void AssertStream(Stream expectedStream, Func<Stream, Stream> funcGenerateNewStream)
        {
            var expectedData = expectedStream.ReadAllBytes();
            var actualStream = funcGenerateNewStream(new MemoryStream(expectedData));
            var actualData = actualStream.ReadAllBytes();

            var length = Math.Min(expectedData.Length, actualData.Length);
            for (var i = 0; i < length; i++)
            {
                var ch1 = expectedData[i];
                var ch2 = actualData[i];
                Assert.True(ch1 == ch2, $"Expected {ch1:X02} but found {ch2:X02} at {i:X}");
            }

            Assert.Equal(expectedData.Length, actualData.Length);
        }

        public static void ForAllFiles(string gamePath, Action<string> action)
        {
            Skip.IfNot(Directory.Exists(gamePath));

            Directory.GetFiles(gamePath, "*", SearchOption.AllDirectories)
                .AsParallel()
                .ForAll(fileName =>
                {
                    try
                    {
                        action(fileName);
                    }
                    catch (Exception ex)
                    {
                        throw new AssertFileNameException(fileName, ex);
                    }
                });
        }

        public static void ForBarEntries(
            string fileName,
            Func<Bar.Entry, bool> predicate,
            Action<string, Bar.Entry> action)
        {
            if (!File.OpenRead(fileName).Using(stream => Bar.IsValid(stream)))
                return;

            foreach (var entry in File
                .OpenRead(fileName)
                .Using(stream => Bar.Read(stream))
                .Where(x => x.Index == 0)
                .Where(predicate))
            {
                try
                {
                    action(fileName, entry);
                }
                catch (XunitException ex)
                {
                    throw new AssertBarException(entry.Name, ex);
                }
            }
        }

        public static void UseAsset(string assetName, Action<Stream> action) =>
            File.OpenRead(Path.Combine($"Common/res/", assetName)).Using(x => action(x));

        public static void Dump(this byte[] data, string path) =>
            new MemoryStream(data).Using(x => x.Dump(path));

        public static T CreateDummyObject<T>()
            where T : class
        {
            var dummyByte = (byte)12;
            var dummyShort = (short)123;
            var dummyInt = 1234;
            var dummyFloat = 12.5f;
            var dummyString = "dumy";

            var instance = Activator.CreateInstance<T>();
            foreach (var property in typeof(T).GetProperties())
            {
                var propType = property.PropertyType;
                if (propType == typeof(byte))
                {
                    property.SetValue(instance, dummyByte);
                    dummyByte *= 2;
                }
                else if (propType == typeof(short))
                {
                    property.SetValue(instance, dummyShort);
                    dummyShort *= 2;
                }
                else if (propType == typeof(ushort))
                {
                    property.SetValue(instance, (ushort)dummyShort);
                    dummyShort *= 2;
                }
                else if (propType == typeof(int))
                {
                    property.SetValue(instance, dummyInt);
                    dummyInt *= 2;
                }
                else if (propType == typeof(uint))
                {
                    property.SetValue(instance, dummyInt);
                    dummyInt *= 2;
                }
                else if (propType == typeof(float))
                {
                    property.SetValue(instance, dummyFloat);
                    dummyFloat *= 2;
                }
                else if (propType == typeof(string))
                {
                    property.SetValue(instance, dummyString);
                    dummyString += dummyString;
                }
                else
                    throw new NotImplementedException(
                        $"Type {propType.FullName} can not be mocked.");
            }

            return instance;
        }
    }
}
