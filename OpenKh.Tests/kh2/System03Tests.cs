using OpenKh.Common;
using OpenKh.Kh2.SystemData;
using System.IO;
using Xunit;

namespace OpenKh.Tests.kh2
{
    public class System03Tests
    {
        public class ArifTests
        {
            [Fact]
            public void CheckForLength() => File.OpenRead("kh2/res/arif.bin").Using(stream =>
            {
                var worlds = Arif.Read(stream);

                Assert.Equal(19, worlds.Count);
                Assert.Equal(64, worlds[0].Count);
                Assert.Equal(2, worlds[1].Count);
                Assert.Equal(42, worlds[2].Count);
            });

            [Fact]
            public void ShouldWriteTheExactSameFile() => File.OpenRead("kh2/res/arif.bin").Using(stream =>
            {
                Helpers.AssertStream(stream, x =>
                {
                    var outStream = new MemoryStream();
                    Arif.Write(outStream, Arif.Read(x));

                    return outStream;
                });
            });
        }

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

        public class MemtTests
        {
            private const string MemtVanilla = @"kh2/res/memt_eu.bin";
            private const string MemtFinalMix = @"kh2/res/memt_fm.bin";

            [Theory]
            [InlineData(MemtVanilla)]
            [InlineData(MemtFinalMix)]
            public void Read(string fileName) => File.OpenRead(fileName).Using(stream =>
            {
                var table = Memt.Read(stream);
                Assert.Equal(0x5, table.Entries.Count);
                Assert.Equal(0x7, table.MemberIndexCollection.Length);
            });

            [Theory]
            [InlineData(MemtVanilla)]
            [InlineData(MemtFinalMix)]
            public void Write(string fileName) => File.OpenRead(fileName).Using(stream =>
            {
                Helpers.AssertStream(stream, x =>
                {
                    var outStream = new MemoryStream();
                    Memt.Write(outStream, Memt.Read(x));

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
