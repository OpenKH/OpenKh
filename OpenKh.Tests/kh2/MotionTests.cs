using OpenKh.Common;
using OpenKh.Kh2;
using System.IO;
using Xunit;

namespace OpenKh.Tests.kh2
{
    public class MotionTests
    {
        private const string RawFileName = "./kh2/res/raw.motion";

        [Fact]
        public void ReadRawMotion()
        {
            var motion = File.OpenRead(RawFileName).Using(Motion.Read);

            Assert.True(motion.IsRaw);
            Assert.NotNull(motion.Raw);

            Assert.Equal(30, motion.Raw.FramePerSecond);
        }

        [Theory]
        [InlineData(RawFileName)]
        public void WriteBackTheSameFile(string fileName) =>
            File.OpenRead(fileName).Using(stream =>
        {
            Helpers.AssertStream(stream, inStream =>
            {
                var outStream = new MemoryStream();
                Motion.Write(outStream, Motion.Read(inStream));

                return outStream;
            });
        });
    }
}
