using OpenKh.Common;
using OpenKh.Kh2;
using System.Collections.Generic;
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

        public static IEnumerable<object[]> MsetSource()
        {
            if (!Directory.Exists(Path.Combine(Helpers.Kh2DataPath, "obj")))
            {
                yield return new object[] { "", -1 };
                yield break;
            }

            foreach (var msetFile in Directory.GetFiles(Helpers.Kh2DataPath, "obj/*.mset", SearchOption.AllDirectories))
            {
                foreach (var (msetEntry, index) in File.OpenRead(msetFile).Using(Bar.Read)
                    .Select((msetEntry, index) => (msetEntry, index))
                    .Where(pair => pair.msetEntry.Type == Bar.EntryType.Anb && pair.msetEntry.Stream.Length != 0)
                )
                {
                    foreach (var anbEntry in Bar.Read(msetEntry.Stream)
                        .Where(it => it.Type == Bar.EntryType.Motion)
                    )
                    {
                        if (anbEntry.Stream.Length != 0)
                        {
                            yield return new object[] {
                                    msetFile.Substring(Helpers.Kh2DataPath.Length).Replace("\\", "/"),
                                    index,
                                };
                        }
                    }
                }
            }
        }

        [SkippableTheory, MemberData(nameof(MsetSource))]
        public void Batch_WriteMset(string msetFile, int index)
        {
            Skip.If(index == -1, "No MSET file found");

            var msetEntry = File.OpenRead(Path.Combine(Helpers.Kh2DataPath, msetFile))
                .Using(Bar.Read)
                .Skip(index)
                .First();

            Assert.Equal(Bar.EntryType.Anb, msetEntry.Type);

            if (msetEntry != null)
            {
                var anbEntry = Bar.Read(msetEntry.Stream)
                    .First(it => it.Type == Bar.EntryType.Motion);
                Helpers.AssertStream(anbEntry.Stream, inStream =>
                {
                    var outStream = new MemoryStream();
                    Motion.Write(outStream, Motion.Read(inStream));

                    return outStream;
                });

            }
        }

        public static IEnumerable<object[]> AnbSource()
        {
            if (!Directory.Exists(Path.Combine(Helpers.Kh2DataPath, "anb")))
            {
                yield return new object[] { "", "" };
                yield break;
            }

            foreach (var anbFile in Directory.GetFiles(Helpers.Kh2DataPath, "anm/*.anb", SearchOption.AllDirectories))
            {
                foreach (var entry in File.OpenRead(anbFile).Using(Bar.Read)
                    .Where(it => it.Type == Bar.EntryType.Motion)
                )
                {
                    yield return new object[] { anbFile.Substring(Helpers.Kh2DataPath.Length).Replace("\\", "/"), entry.Name };
                }
            }
        }

        [SkippableTheory, MemberData(nameof(AnbSource))]
        public void Batch_WriteAnb(string anbFile, string motionName)
        {
            Skip.If(anbFile.Length == 0, "No ANB file found");

            var entry = File.OpenRead(Path.Combine(Helpers.Kh2DataPath, anbFile))
                .Using(Bar.Read)
                .FirstOrDefault(it => it.Name == motionName && it.Type == Bar.EntryType.AnimationLoader);

            Helpers.AssertStream(entry.Stream, inStream =>
            {
                var outStream = new MemoryStream();
                Motion.Write(outStream, Motion.Read(inStream));

                return outStream;
            });
        }
    }
}
