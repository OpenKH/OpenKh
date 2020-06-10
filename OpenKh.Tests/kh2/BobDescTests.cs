using OpenKh.Common;
using OpenKh.Kh2.Models;
using System.IO;
using Xunit;

namespace OpenKh.Tests.kh2
{
    public class BobDescTests
    {
        private const string FileName = "kh2/res/bobdesc.bin";

        [Fact]
        public void ReadCorrectValues()
        {
            var bobDesc = File.OpenRead(FileName).Using(BobDescriptor.Read);

            Assert.Equal(4, bobDesc.Count);
            Assert.Equal(1100, bobDesc[0].PositionX);
            Assert.Equal(0, bobDesc[0].PositionY);
            Assert.Equal(0, bobDesc[0].PositionZ);
            Assert.Equal(0, bobDesc[0].RotationX);
            Assert.Equal(0, bobDesc[0].RotationY);
            Assert.Equal(0, bobDesc[0].RotationZ);
            Assert.Equal(1, bobDesc[0].ScalingX);
            Assert.Equal(1, bobDesc[0].ScalingY);
            Assert.Equal(1, bobDesc[0].ScalingZ);
            Assert.Equal(0, bobDesc[0].BobIndex);
        }

        [Fact]
        public void WriteTest() => Common.FileOpenRead(FileName, stream =>
        {
            Helpers.AssertStream(stream, inStream =>
            {
                var outStream = new MemoryStream();
                BobDescriptor.Write(outStream, BobDescriptor.Read(inStream));

                return outStream;
            });
        });
    }
}
