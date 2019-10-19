using Xunit;
using OpenKh.Kh2.System;

namespace OpenKh.Tests.kh2
{
    public class TrsrTests
    {
        [Fact]
        public void CheckNewTrsr() => Common.FileOpenRead(@"kh2/res/trsr.bin", x => x.Using(stream =>
        {
            var table = BaseSystem<Trsr>.Read(stream);
            Assert.Equal(0x1AE, table.Count);
        }));
    }
}