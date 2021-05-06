using OpenKh.Audio;
using OpenKh.Common;
using System.IO;
using Xunit;

namespace OpenKh.Tests.Audio
{
    public class ScdTests
    {
        [Theory]
        [InlineData(@"D:\GITHUB\OpenKh\OpenKh.Command.IdxImg\bin\Debug\net5.0\_kh2\original\bgm\music050.win32.scd")]
        [InlineData(@"D:\GITHUB\OpenKh\OpenKh.Command.IdxImg\bin\Debug\net5.0\_kh2\original\voice\us\event\al10101ia.win32.scd")]
        [InlineData(@"D:\GITHUB\OpenKh\OpenKh.Command.IdxImg\bin\Debug\net5.0\_kh2\original\voice\us\battle\tt0_roxas.win32.scd")]
        [InlineData(@"D:\GITHUB\OpenKh\OpenKh.Command.IdxImg\bin\Debug\net5.0\_kh2\original\voice\us\event\lmmu3-1-2.win32.scd")]
        [InlineData(@"D:\GITHUB\OpenKh\OpenKh.Command.IdxImg\bin\Debug\net5.0\_kh2\original\voice\us\battle\big_xemnas.win32.scd")]
        public void WriteBackTheSameFile(string fileName) => File.OpenRead(fileName).Using(stream =>
        {
            Helpers.AssertStream(stream, inStream =>
            {
                var outStream = new MemoryStream();
                var file = Scd.Read(inStream);
                file.Write(outStream);

                using var fileStream = File.Create($"C:\\temp\\{Path.GetFileName(fileName)}");
                file.Write(fileStream);

                return outStream;
            });
        });
    }
}
