using OpenKh.Common;
using OpenKh.Patcher;
using System.IO;
using Xunit;

namespace OpenKh.Tests.Patcher
{
    public class MetadataTests
    {
        [Fact]
        public void ReadModMetadataTest()
        {
            var metadata = File.OpenRead("Patcher/res/sample-simple-mod.yml").Using(Metadata.Read);

            Assert.Equal("My awesome untitled mod", metadata.Title);
            Assert.Equal(1, metadata.Specifications);

            Assert.Single(metadata.Dependencies);
            Assert.Equal("openkh/is-awesome", metadata.Dependencies[0].Name);

            Assert.NotNull(metadata.Assets);
            Assert.Single(metadata.Assets);
            Assert.Equal("my/file/path.bin", metadata.Assets[0].Name);
        }
    }
}
