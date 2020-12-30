using OpenKh.Common;
using OpenKh.Bbs;
using System.IO;
using Xunit;

namespace OpenKh.Tests.Bbs
{
    public class PmoTests
    {
        private static readonly string FileName = "Bbs/res/p01ex00.pmo";

        [Fact]
        public void ReadCorrectHeader() => File.OpenRead(FileName).Using(stream =>
        {
            var TestPmo = Pmo.Read(stream);
            Assert.Equal(0x4F4D50, (int)TestPmo.header.MagicCode);
            Assert.Equal(0x1FD80, (int)TestPmo.header.SkeletonOffset);
            Assert.Equal(0xC5C0, (int)TestPmo.header.MeshOffset1);
        });

        [Fact]
        public void ReadCorrectTextureBlock() => File.OpenRead(FileName).Using(stream =>
        {
            var TestPmo = Pmo.Read(stream);
            Assert.Equal("p01ex00_00", TestPmo.textureInfo[0].TextureName);
            Assert.Equal(0xE040, (int)TestPmo.textureInfo[0].TextureOffset);
            Assert.Equal("p01ex00_M", TestPmo.textureInfo[2].TextureName);
            Assert.Equal(0x1f540, (int)TestPmo.textureInfo[2].TextureOffset);
        });

        [Fact]
        public void ReadCorrectMeshGroup() => File.OpenRead(FileName).Using(stream =>
        {
            var TestPmo = Pmo.Read(stream);

            // mesh header 1
            Assert.Equal("585", TestPmo.meshSection[0].VertexCount.ToString());
            Assert.Equal("0", TestPmo.meshSection[0].TriangleStripCount.ToString());

            // mesh header 2
            Assert.Equal("141", TestPmo.meshSection[1].VertexCount.ToString());
            Assert.Equal("0", TestPmo.meshSection[1].TriangleStripCount.ToString());
        });

        [Fact]
        public void ReadCorrectTextures() => File.OpenRead(FileName).Using(stream =>
        {
            var TestPmo = Pmo.Read(stream);
            byte[] buffer = TestPmo.Textures[0];
            Assert.True(buffer[0] == 0x54 && buffer[1] == 0x49 && buffer[2] == 0x4D && buffer[3] == 0x32);
        });

        [Fact]
        public void HasTriangleStrips() => File.OpenRead(FileName).Using(stream =>
        {
            var TestPmo = Pmo.Read(stream);
            bool hasTriangleStrips = false;

            for(int i = 0; i < TestPmo.meshSection.Count; i++)
            {
                if(TestPmo.meshSection[i].TriangleStripCount != 0)
                {
                    hasTriangleStrips = true;
                    break;
                }
            }

            Assert.True(hasTriangleStrips);
        });

        [Fact]
        public void ReadBoneHeader() => File.OpenRead(FileName).Using(stream =>
        {
            var TestPmo = Pmo.Read(stream);
            Assert.Equal((uint)0x4e4f42, TestPmo.skeletonHeader.MagicValue);
            Assert.Equal((uint)0x35, TestPmo.skeletonHeader.JointCount);
        });

        [Fact]
        public void ReadJoints() => File.OpenRead(FileName).Using(stream =>
        {
            var TestPmo = Pmo.Read(stream);
            Assert.Equal("Root", TestPmo.jointList[0].JointName);
            Assert.Equal(0x1, TestPmo.jointList[1].JointIndex);
        });
    }
}
