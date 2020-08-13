using OpenKh.Common;
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
            public void CheckForLength() => File.OpenRead("kh2/res/ftst.bin").Using(stream =>
            {
                var entries = Ftst.Read(stream);
                Assert.Equal(8, entries.Count);
            });

            [Fact]
            public void ShouldWriteTheExactSameFile() => File.OpenRead("kh2/res/ftst.bin").Using(stream =>
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
            public void CheckForLength() => File.OpenRead("kh2/res/item.bin").Using(stream =>
            {
                var entries = Item.Read(stream);
                Assert.Equal(535, entries.Items1.Count);
                Assert.Equal(151, entries.Items2.Count);
            });

            [Fact]
            public void ShouldWriteTheExactSameFile() => File.OpenRead("kh2/res/item.bin").Using(stream =>
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
            public void CheckNewTrsr() => File.OpenRead(@"kh2/res/trsr.bin").Using(stream =>
            {
                var table = Trsr.Read(stream);
                Assert.Equal(0x1AE, table.Count);
            });

            [Fact]
            public void ShouldWriteTheExactSameFile() => File.OpenRead("kh2/res/trsr.bin").Using(stream =>
            {
                Helpers.AssertStream(stream, x =>
                {
                    var outStream = new MemoryStream();
                    Trsr.Write(outStream, Trsr.Read(x));

                    return outStream;
                });
            });
        }
    }
}
