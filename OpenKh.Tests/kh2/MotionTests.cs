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

        public class UseAssetAnbFiles
        {
            private static string KH2Dir = ".tests/kh2_data/";

            public static IEnumerable<object[]> Source()
            {
                foreach (var anbFile in Directory.GetFiles(KH2Dir, "anm/*.anb", SearchOption.AllDirectories))
                {
                    foreach (var entry in File.OpenRead(anbFile).Using(Bar.Read)
                        .Where(it => it.Type == Bar.EntryType.Motion)
                    )
                    {
                        yield return new object[] { anbFile.Substring(KH2Dir.Length).Replace("\\", "/"), entry.Name };
                    }
                }
            }

            //[Theory, MemberData(nameof(Source))]
            public void MotionTests(string anbFile, string motionName)
            {
                var entry = File.OpenRead(Path.Combine(KH2Dir, anbFile))
                    .Using(Bar.Read)
                    .SingleOrDefault(it => it.Name == motionName && it.Type == Bar.EntryType.Event);

                if (entry != null)
                {
                    Motion.Read(entry.Stream);
                }
            }
        }

        public class UseAssetMsetFiles
        {
            private static string KH2Dir = ".tests/kh2_data/";

            public static IEnumerable<object[]> Source()
            {
                foreach (var msetFile in Directory.GetFiles(KH2Dir, "obj/*.mset", SearchOption.AllDirectories))
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
                                    msetFile.Substring(KH2Dir.Length).Replace("\\", "/"),
                                    index,
                                };
                            }
                        }
                    }
                }
            }

            //[Theory, MemberData(nameof(Source))]
            public void MotionTests(string msetFile, int index)
            {
                var msetEntry = File.OpenRead(Path.Combine(KH2Dir, msetFile))
                    .Using(Bar.Read)
                    .Skip(index)
                    .First();

                Assert.Equal(Bar.EntryType.Anb, msetEntry.Type);

                if (msetEntry != null)
                {
                    var anbEntry = Bar.Read(msetEntry.Stream)
                        .Single(it => it.Type == Bar.EntryType.Motion);

                    Motion.Read(anbEntry.Stream);
                }
            }
        }
    }
}
