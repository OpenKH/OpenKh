using OpenKh.Common;
using OpenKh.Kh2;
using OpenKh.Kh2.System;
using System.IO;
using Xunit;

namespace OpenKh.Tests.kh2
{
    public class System03Tests
    {
        public class FtstTests
        {
            [Fact]
            public void CheckForLength() => Common.FileOpenRead("kh2/res/ftst.bin", stream =>
            {
                var entries = Ftst.Read(stream);
                Assert.Equal(8, entries.Count);
            });

            [Fact]
            public void ShouldWriteTheExactSameFile() => Common.FileOpenRead("kh2/res/ftst.bin", stream =>
            {
                Helpers.AssertStream(stream, x =>
                {
                    var outStream = new MemoryStream();

                    Ftst.Write(outStream, Ftst.Read(x));

                    return outStream;
                });
            });
        }

        public class ItemTests
        {
            [Fact]
            public void CheckForLength() => Common.FileOpenRead("kh2/res/item.bin", stream =>
            {
                var entries = Item.Read(stream);
                Assert.Equal(535, entries.Items1.Count);
                Assert.Equal(151, entries.Items2.Count);
            });

            [Fact]
            public void ShouldWriteTheExactSameFile() => Common.FileOpenRead("kh2/res/item.bin", stream =>
            {
                Helpers.AssertStream(stream, x =>
                {
                    var outStream = new MemoryStream();

                    Item.Read(x).Write(outStream);

                    return outStream;
                });
            });
        }

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
}
