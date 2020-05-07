using OpenKh.Kh2;
using System.IO;
using System.Linq;
using Xunit;

namespace OpenKh.Tests.kh2
{
    public class ObjentryTests
    {
        [Fact]
        public void HasRightEntryCount() => Common.FileOpenRead("kh2/res/00objentry.bin", stream =>
        {
            var table = BaseTable<Objentry>.Read(stream);
            Assert.Equal(0x076C, table.Count);
        });

        [Fact]
        public void WriteBackTheSameFile() => Common.FileOpenRead("kh2/res/00objentry.bin", stream =>
        {
            Helpers.AssertStream(stream, inStream =>
            {
                var outStream = new MemoryStream();
                Objentry.Write(outStream, Objentry.Read(inStream));

                return outStream;
            });
        });

        [Fact]
        public void GroupByUnknown02() => Common.FileOpenRead("kh2/res/00objentry.bin", stream =>
        {
            var table = BaseTable<Objentry>.Read(stream);
            var grouped = table.Items.GroupBy(x => x.Unknown5e).ToList();
            Assert.Equal(0x076C, table.Count);
        });
    }
}
