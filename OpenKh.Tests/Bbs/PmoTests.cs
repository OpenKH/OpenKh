using OpenKh.Common;
using OpenKh.Bbs;
using System.IO;
using Xunit;
using System.Collections.Generic;
using System.Collections.Specialized;
using System;

namespace OpenKh.Tests.Bbs
{
    public class PmoTests
    {
        private static readonly string FileName = "Bbs/res/bbs-dummy.pmo";

        [Fact]
        public void ReadCorrectHeader() => File.OpenRead(FileName).Using(stream =>
        {
            var TestPmo = Pmo.Read(stream);
            Assert.Equal(0x4F4D50, (int)TestPmo.header.MagicCode);
            Assert.Equal(0, (int)TestPmo.header.SkeletonOffset);
            Assert.Equal(0xC0, (int)TestPmo.header.MeshOffset0);
            Assert.Equal(0, (int)TestPmo.header.MeshOffset1);
        });

        [Fact]
        public void ReadCorrectTextureBlock() => File.OpenRead(FileName).Using(stream =>
        {
            var TestPmo = Pmo.Read(stream);
            Assert.Equal("Dummy_tex02", TestPmo.textureInfo[0].TextureName);
            Assert.Equal(0x2300, (int)TestPmo.textureInfo[0].TextureOffset);
        });

        [Fact]
        public void ReadCorrectMeshGroup() => File.OpenRead(FileName).Using(stream =>
        {
            var TestPmo = Pmo.Read(stream);

            // mesh header 1
            Assert.Equal("544", TestPmo.Meshes[0].SectionInfo.VertexCount.ToString());
            Assert.Equal("16", TestPmo.Meshes[0].SectionInfo.TriangleStripCount.ToString());
        });

        [Fact]
        public void ReadBoneHeader() => File.OpenRead(FileName).Using(stream =>
        {
            var TestPmo = Pmo.Read(stream);
            if (TestPmo.header.SkeletonOffset == 0)
            {
                Assert.True(true);
            }
            else
            {
                Assert.Equal((uint)0x4e4f42, TestPmo.skeletonHeader.MagicValue);
                Assert.Equal((uint)0x35, TestPmo.skeletonHeader.BoneCount);
            }
        });

        [Fact]
        public void WritesBackCorrectly()
        {
            Stream input = File.OpenRead(FileName);
            var TestPmo = Pmo.Read(input);
            Stream output = File.Open("Bbs/res/bbs-dummy_TEST.pmo", FileMode.Create);
            Pmo.Write(output, TestPmo);

            input.Position = 0;
            output.Position = 0;

            // Check all bytes.
            for (int i = 0; i < output.Length; i++)
            {
                if (input.ReadByte() != output.ReadByte())
                {
                    long position = output.Position;
                    Assert.False(true);
                }
            }

            Assert.True(true);
        }
    }
}
