using OpenKh.Common.Archives;
using System;
using System.IO;
using Xunit;

namespace OpenKh.Tests.Archives
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
            Assert.Throws<ArgumentNullException>(() => HdAsset.New().Stream = null);
        }

        [Fact]
        public void CannotAssignNullEntriesListStream()
        {
            Assert.Throws<ArgumentNullException>(() => HdAsset.New().Entries = null);
        }

        [Fact]
        public void CannotAssignNullEntryStreamListStream()
        {
            Assert.Throws<ArgumentNullException>(() => new HdAsset.Entry
            {
                Stream = null
            });
        }
    }
}
