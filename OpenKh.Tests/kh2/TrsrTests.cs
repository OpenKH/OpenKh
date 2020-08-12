using Xunit;
using OpenKh.Common;
using OpenKh.Kh2.System;
using OpenKh.Kh2;

namespace OpenKh.Tests.kh2
{
    public class TrsrTests
    {
        [Fact]
        public void CheckNewTrsr() => Common.FileOpenRead(@"kh2/res/trsr.bin", x => x.Using(stream =>
        {
            var table = BaseTable<Trsr>.Read(stream);
            Assert.Equal(0x1AE, table.Count);
        }));
    }
}