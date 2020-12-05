using OpenKh.Common;
using OpenKh.Kh2;
using System.IO;
using System.Linq;
using Xunit;

namespace OpenKh.Tests.kh2
{
    public class MotionTests
    {
        private const string RawFileName = "./kh2/res/raw.motion";
        private const string InterpolatedFileName = "./kh2/res/interpolated.motion";

        [Fact]
        public void ReadRawMotion()
        {
            var motion = File.OpenRead(RawFileName).Using(Motion.Read);

            Assert.True(motion.IsRaw);
            Assert.NotNull(motion.Raw);
            Assert.Null(motion.Interpolated);

            Assert.Equal(30, motion.Raw.FramePerSecond);
        }

        [Fact]
        public void ReadInterpolatedMotion()
        {
            var motion = File.OpenRead(InterpolatedFileName).Using(Motion.Read);

            Assert.False(motion.IsRaw);
            Assert.Null(motion.Raw);
            Assert.NotNull(motion.Interpolated);
        }

        [Theory]
        [InlineData(RawFileName)]
        [InlineData(InterpolatedFileName)]
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

        [SkippableFact]
        public void Full_WriteBackTheSameFiles() =>
            Helpers.ForAllFiles(".tests/kh2_data", fileName =>
        {
            Helpers.ForBarEntries(fileName,
                x => x.Type == Bar.EntryType.Anb && x.Stream.Length > 0, (fileName, entry) =>
            {
                if (!Bar.IsValid(entry.Stream))
                    return;

                var motionEntry = Bar.Read(entry.Stream)
                    .FirstOrDefault(x => x.Type == Bar.EntryType.Motion);
                if (motionEntry == null)
                    return;

                Helpers.AssertStream(motionEntry.Stream, inStream =>
                {
                    var outStream = new MemoryStream();
                    Motion.Write(outStream, Motion.Read(inStream));

                    return outStream;
                });
            });
        });
    }
}
