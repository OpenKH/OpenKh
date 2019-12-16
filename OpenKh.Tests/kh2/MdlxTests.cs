using OpenKh.Common;
using OpenKh.Kh2;
using System.IO;
using System.Linq;
using Xunit;

namespace OpenKh.Tests.kh2
{
    public class MdlxTests
    {
        private const string FileName = "kh2/res/p_ex.vif";
        private const string MapFileName = "kh2/res/map.vif";

        [Fact]
        public void ShouldReadTheCorrectAmountOfSubModels() => File.OpenRead(FileName).Using(stream =>
        {
            var model = Mdlx.Read(stream).SubModels;

            Assert.Equal(2, model.Count);
        });

        [Fact]
        public void ShouldReadVifPackets() => File.OpenRead(FileName).Using(stream =>
        {
            var dmaChain = Mdlx.Read(stream).SubModels[0].DmaChains;
            Assert.Equal(26, dmaChain[0].DmaVifs.Count);
            Assert.Equal(4, dmaChain.Count);
            Assert.Equal(58, dmaChain.Sum(x => x.DmaVifs.Count));

            var dmaVif = dmaChain[0].DmaVifs[0];
            Assert.Equal(0, dmaVif.TextureIndex);
            Assert.Equal(10, dmaVif.Alaxi.Length);
            Assert.Equal(53, dmaVif.Alaxi[0]);
            Assert.Equal(22, dmaVif.Alaxi[9]);
            Assert.Equal(1600, dmaVif.VifPacket.Length);
            Assert.Equal(1, dmaVif.VifPacket[0]);
            Assert.Equal(248, dmaVif.VifPacket[324]);
        });

        [Fact]
        public void ShouldReadBones() => File.OpenRead(FileName).Using(stream =>
        {
            var bones = Mdlx.Read(stream).SubModels[0].Bones;
            Assert.Equal(228, bones.Count);

            var bone = bones[0];
            Assert.Equal(0, bone.Index);
            Assert.Equal(-1, bone.Parent);
            Assert.Equal(0, bone.Unk08);
            Assert.Equal(3, bone.Unk0c);
            Assert.Equal(1, bone.ScaleX);
            Assert.Equal(0, bone.RotationX);
            Assert.Equal(0, bone.TranslationX);
            Assert.Equal(1, bone.ScaleY);
            Assert.Equal(4.71, bone.RotationY, 2);
            Assert.Equal(102.62, bone.TranslationY, 2);
            Assert.Equal(1, bone.ScaleZ);
            Assert.Equal(4.71, bone.RotationZ, 2);
            Assert.Equal(0, bone.TranslationZ);
            Assert.Equal(0, bone.ScaleW);
            Assert.Equal(0, bone.RotationW);
            Assert.Equal(0, bone.TranslationW);
        });

        [Fact]
        public void ShouldWriteBackTheExactSameFile() => File.OpenRead(FileName).Using(stream =>
        {
            Helpers.AssertStream(stream, inStream =>
            {
                var mdlx = Mdlx.Read(inStream);

                var outStream = new MemoryStream();
                mdlx.Write(outStream);

                return outStream;
            });
        });

        [Fact]
        public void alb1t2() => File.OpenRead(MapFileName).Using(stream =>
        {
            var alb1t2 = Mdlx.Read(stream).MapModel.alb1t2;
            Assert.Equal(97, alb1t2.Count);

            Assert.Equal(0, alb1t2[0]);
            Assert.Equal(1, alb1t2[1]);
            Assert.Equal(2, alb1t2[2]);
            Assert.Equal(96, alb1t2[96]);
        });

        [Fact]
        public void alb2() => File.OpenRead(MapFileName).Using(stream =>
        {
            var alb2 = Mdlx.Read(stream).MapModel.alb2;
            Assert.Equal(9, alb2.Count);

            Assert.Equal(20, alb2[0].Length);
            Assert.Equal(1, alb2[0][0]);
            Assert.Equal(91, alb2[0][19]);

            Assert.Equal(3, alb2[8].Length);
            Assert.Equal(30, alb2[8][0]);
            Assert.Equal(68, alb2[8][2]);
        });

        [Fact]
        public void alvifpkt() => File.OpenRead(MapFileName).Using(stream =>
        {
            var alvifpkt = Mdlx.Read(stream).MapModel.VifPackets;
            Assert.Equal(97, alvifpkt.Count);

            var packet1 = alvifpkt[0];
            Assert.Equal(1, packet1.TextureId);
            Assert.Equal(1184, packet1.VifPacket.Length);
            Assert.Equal(1, packet1.VifPacket[0]);
            Assert.Equal(0, packet1.VifPacket[10]);
            Assert.Equal(64, packet1.VifPacket[100]);
            Assert.Equal(76, packet1.VifPacket[1170]);

            var packet2 = alvifpkt[96];
            Assert.Equal(2, packet2.TextureId);
            Assert.Equal(960, packet2.VifPacket.Length);
            Assert.Equal(1, packet2.VifPacket[0]);
            Assert.Equal(117, packet2.VifPacket[500]);
        });
    }
}
