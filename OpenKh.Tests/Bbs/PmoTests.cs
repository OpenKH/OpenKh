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
        private static readonly string FileName = "Bbs/res/p01ex00.pmo";

        [Fact]
        public void ReadGeometryType() => File.OpenRead(FileName).Using(stream =>
        {
            var TestPmo = Pmo.Read(stream);
            List<string> Primitive = new List<string>();
            for(int i = 0; i < TestPmo.Meshes.Count; i++)
            {
                uint flag = TestPmo.Meshes[i].SectionInfo.VertexFlags;
                flag >>= 28;
                Pmo.PrimitiveType type = (Pmo.PrimitiveType)flag;
                Primitive.Add(i + ": " + type.ToString());
            }
            Console.WriteLine(Primitive.Count);
        });

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
            Assert.Equal("585", TestPmo.Meshes[0].SectionInfo.VertexCount.ToString());
            Assert.Equal("0", TestPmo.Meshes[0].SectionInfo.TriangleStripCount.ToString());

            // mesh header 2
            Assert.Equal("141", TestPmo.Meshes[1].SectionInfo.VertexCount.ToString());
            Assert.Equal("0", TestPmo.Meshes[1].SectionInfo.TriangleStripCount.ToString());
        });

        [Fact]
        public void ReadCorrectTextures() => File.OpenRead(FileName).Using(stream =>
        {
            var TestPmo = Pmo.Read(stream);
            byte[] buffer = TestPmo.texturesData[0];
            Assert.True(buffer[0] == 0x54 && buffer[1] == 0x49 && buffer[2] == 0x4D && buffer[3] == 0x32);
        });

        [Fact]
        public void HasTriangleStrips() => File.OpenRead(FileName).Using(stream =>
        {
            var TestPmo = Pmo.Read(stream);
            bool hasTriangleStrips = false;

            for(int i = 0; i < TestPmo.Meshes.Count; i++)
            {
                if(TestPmo.Meshes[i].SectionInfo.TriangleStripCount != 0)
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

        [Fact]
        public void CheckH03EX00()
        {
            var TestPmo = Pmo.Read(File.OpenRead("../../../Bbs/res/h03ex00.pmo"));

            Assert.Equal("3", TestPmo.header.TextureCount.ToString());
        }

        [Fact]
        public void WritesBackCorrectly()
        {
            string path = "../../../Bbs/res/h_zz130";
            Stream input = File.OpenRead(path + ".pmo");
            var TestPmo = Pmo.Read(input);
            Stream output = File.Open(path + "_TEST.pmo", FileMode.Create);
            Pmo.Write(output, TestPmo);

            input.Position = 0;
            output.Position = 0;

            // Check all bytes.
            for(int i = 0; i < output.Length; i++)
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
