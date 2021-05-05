using OpenKh.Audio;
using OpenKh.Common;
using System.IO;
using Xunit;

namespace OpenKh.Tests.Audio
{
    public class ScdTests
    {
        [Fact]
        public void WriteBackTheSameFile() => File.OpenRead(@"D:\GITHUB\OpenKh\OpenKh.Command.IdxImg\bin\Debug\net5.0\_kh2\original\voice\us\event\al10101ia.win32.scd").Using(stream =>
        {
            Helpers.AssertStream(stream, inStream =>
            {
                var outStream = new MemoryStream();
                Scd.Write(outStream, Scd.Read(inStream));

                return outStream;
            });
        });
    }
}
