using OpenKh.Common;
using OpenKh.Common.Archives;
using System;
using System.IO;
using System.Linq;
using Xunit;

namespace OpenKh.Tests.Common
{
    public class HdAssetTests
    {
        [Fact]
        public void WriteBackTheSameReadStream() => Helpers.UseAsset("ps4archive.bin", stream =>
        {
            Helpers.AssertStream(stream, x =>
            {
                var asset = HdAsset.Read(stream);

                var outStream = new MemoryStream();
                asset.Write(outStream);

                return outStream;
            });
        });

        [Fact]
        public void CannotAssignNullMainStream()
        {
            Assert.Throws<ArgumentNullException>(() => new HdAsset().Stream = null);
        }

        [Fact]
        public void CannotAssignNullEntriesListStream()
        {
            Assert.Throws<ArgumentNullException>(() => new HdAsset().Entries = null);
        }

        [Fact]
        public void CannotAssignNullEntryStreamListStream()
        {
            Assert.Throws<ArgumentNullException>(() => new HdAsset.Entry
            {
                Stream = null
            });
        }

        [Fact]
        public void IsValid() => Helpers.UseAsset("ps4archive.bin", stream =>
        {
            Assert.True(HdAsset.IsValid(stream));
        });

        [Fact]
        public void IsValidWhenTheMinimumNecessaryInformationAreThere()
        {
            var hdasset = new HdAsset();

            using var stream = new MemoryStream();
            hdasset.Write(stream);

            Assert.True(HdAsset.IsValid(stream.SetPosition(0)));
        }

        [Fact]
        public void IsNotValidWhenTheHeaderSizeIsTooSmall()
        {
            var stream = new MemoryStream(new byte[15]);
            Assert.False(HdAsset.IsValid(stream));
        }

        [Fact]
        public void IsNotValidWhenTheInnerStreamLengthIsTooBig()
        {
            var hdasset = new HdAsset();
            hdasset.Stream = new MemoryStream(new byte[] { 2, 3, 4, 5, 6, 7, 8 });

            using var stream = new MemoryStream();
            hdasset.Write(stream);

            stream.SetLength(stream.Length - 1);

            Assert.False(HdAsset.IsValid(stream.SetPosition(0)));
        }

        [Fact]
        public void IsNotValidIfThereAreTooManyHdAssets()
        {
            var hdasset = new HdAsset();
            hdasset.Stream = new MemoryStream(new byte[] { 2, 3, 4, 5, 6, 7, 8 });
            hdasset.Entries = Enumerable
                .Range(0, 1024)
                .Select(x => new HdAsset.Entry()
                {
                    Name = "Test",
                    Stream = new MemoryStream()
                })
                .ToList();

            using var stream = new MemoryStream();
            hdasset.Write(stream);

            stream.SetLength(stream.Length - 1);

            Assert.False(HdAsset.IsValid(stream.SetPosition(0)));
        }
    }
}
