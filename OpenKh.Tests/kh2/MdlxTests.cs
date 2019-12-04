using OpenKh.Common;
using OpenKh.Kh2;
using System.IO;
using Xunit;

namespace OpenKh.Tests.kh2
{
    public class MdlxTests
    {
        private const string FileName = "kh2/res/p_ex.vif";

        [Fact]
        public void ShouldReadTheCorrectAmountOfSubModels() => File.OpenRead(FileName).Using(stream =>
        {
            var model = Mdlx.Read(stream).SubModels;

            Assert.Equal(2, model.Count);
        });

        [Fact]
        public void ShouldReadVifPackets() => File.OpenRead(FileName).Using(stream =>
        {
            var dmaVifs = Mdlx.Read(stream).SubModels[0].DmaVifs;
            Assert.Equal(58, dmaVifs.Count);
            
            var dmaVif = dmaVifs[0];
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
    }
}
