using OpenKh.Common;
using OpenKh.Kh2;
using System.IO;
using System.Linq;
using Xunit;

namespace OpenKh.Tests.kh2
{
    public class ObjentryTests
    {
        [Fact]
        public void HasRightEntryCount() => File.OpenRead("kh2/res/00objentry.bin").Using(stream =>
        {
            var table = Objentry.Read(stream);
            Assert.Equal(0x076C, table.Count);
        });

        [Fact]
        public void WriteBackTheSameFile() => File.OpenRead("kh2/res/00objentry.bin").Using(stream =>
        {
            Helpers.AssertStream(stream, inStream =>
            {
                var outStream = new MemoryStream();
                Objentry.Write(outStream, Objentry.Read(inStream));

                return outStream;
            });
        });

        [Fact]
        public void GroupByUnknown02() => File.OpenRead("kh2/res/00objentry.bin").Using(stream =>
        {
            var table = Objentry.Read(stream);
            var grouped = table.GroupBy(x => x.Unknown5e).ToList();
            Assert.Equal(0x076C, table.Count);
        });
    }
}
