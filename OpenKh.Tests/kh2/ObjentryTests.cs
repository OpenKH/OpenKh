using OpenKh.Kh2;
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
    }
}
