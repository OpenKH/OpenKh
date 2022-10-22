using OpenKh.Common;
using OpenKh.Common.TreeStruc;
using OpenKh.Kh2;
using OpenKh.Kh2.Ard;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xunit;
using Xunit.Sdk;

namespace OpenKh.Tests.kh2
{
    public class EventTests
    {
        [Fact]
        public void ParseProject() =>
            AssertEvent<Event.SetProject>();

        [Fact]
        public void ParseSetActor() =>
            AssertEvent<Event.SetActor>();

        [Fact]
        public void ParseSeqActorPosition() =>
            AssertEvent<Event.SeqActorPosition>();

        [Fact]
        public void ParseSetMap() =>
            AssertEvent<Event.SetMap>();

        [Fact]
        public void ParseSeqCamera() =>
            AssertEvent<Event.SeqCamera>();

        [Fact]
        public void ParseSetEndFrame() =>
            AssertEvent<Event.SetEndFrame>();

        [Fact]
        public void ParseSeqEffect() =>
            AssertEvent<Event.SeqEffect>();

        [Fact]
        public void ParseAttachEffect() =>
            AssertEvent<Event.AttachEffect>();

        [Fact]
        public void ParseUnk0C() =>
            AssertEvent<Event.SeqBgcol>();

        [Fact]
        public void ParseUnk0D() =>
            AssertEvent(new Event.SeqPart
            {
                StartFrame = 123,
                EndFrame = 456,
                Part = new short[]
                {
                    11, 22, 33, 44
                }
            });

        [Fact]
        public void ParseUnk0E() =>
            AssertEvent<Event.SeqAlpha>();

        [Fact]
        public void ParseSetupEvent() =>
            AssertEvent<Event.SetupEvent>();

        [Fact]
        public void ParseEventStart() =>
            AssertEvent<Event.EventStart>();

        [Fact]
        public void ParseSeqFade() =>
            AssertEvent(new Event.SeqFade
            {
                FrameIndex = 123,
                Duration = 456,
                Type = Event.SeqFade.FadeType.FromWhiteVariant
            });

        [Fact]
        public void ParseSetCameraData() =>
            AssertEvent(new Event.SetCameraData
            {
                CameraId = 123,
                PositionX = new List<Event.CameraKeys>
                {
                    new Event.CameraKeys
                    {
                        Interpolation = Kh2.Motion.Interpolation.Hermite,
                        KeyFrame = 1234,
                        Value = 1,
                        TangentEaseIn = 3,
                        TangentEaseOut = 4
                    },
                    new Event.CameraKeys
                    {
                        Interpolation = Kh2.Motion.Interpolation.Linear,
                        KeyFrame = 5678,
                        Value = 5,
                        TangentEaseIn = 7,
                        TangentEaseOut = 8
                    },
                },
                PositionY = new List<Event.CameraKeys>()
                {
                    new Event.CameraKeys
                    {
                        Interpolation = Kh2.Motion.Interpolation.Nearest,
                        KeyFrame = 32767,
                        Value = 11,
                        TangentEaseIn = 33,
                        TangentEaseOut = 44
                    },
                },
                PositionZ = new List<Event.CameraKeys>(),
                LookAtX = new List<Event.CameraKeys>(),
                LookAtY = new List<Event.CameraKeys>(),
                LookAtZ = new List<Event.CameraKeys>(),
                FieldOfView = new List<Event.CameraKeys>(),
                Roll = new List<Event.CameraKeys>(),
            });

        [Fact]
        public void ParseEntryUnk14() =>
            AssertEvent<Event.EntryUnk14>();

        [Fact]
        public void ParseSeqSubtitle() =>
            AssertEvent<Event.SeqSubtitle>();

        [Fact]
        public void ParseEntryUnk16() =>
            AssertEvent<Event.BgGrupe>();

        [Fact]
        public void ParseEntryUnk17() =>
            AssertEvent<Event.SeqBlur>();

        [Fact]
        public void ParseEntryUnk18() =>
            AssertEvent<Event.SeqFocus>();

        [Fact]
        public void ParseSeqTextureAnim() =>
            AssertEvent<Event.SeqTextureAnim>();

        [Fact]
        public void ParseEntryUnk1A() =>
            AssertEvent<Event.SeqActorLeave>();

        [Fact]
        public void ParseSeqCrossFade() =>
            AssertEvent<Event.SeqCrossFade>();

        [Fact]
        public void ParseEntryUnk1D() =>
            AssertEvent<Event.SplineDataEnc>();

        [Fact]
        public void ParseEntryUnk1E() =>
            AssertEvent(new Event.SplinePoint
            {
                Id = 123,
                UnkG = 456,
                UnkH = 789,
                Entries = new List<Event.SplinePoint.Entry>()
                {
                    Helpers.CreateDummyObject<Event.SplinePoint.Entry>(),
                    Helpers.CreateDummyObject<Event.SplinePoint.Entry>(),
                    Helpers.CreateDummyObject<Event.SplinePoint.Entry>(),
                    Helpers.CreateDummyObject<Event.SplinePoint.Entry>(),
                    Helpers.CreateDummyObject<Event.SplinePoint.Entry>(),
                    Helpers.CreateDummyObject<Event.SplinePoint.Entry>(),
                    Helpers.CreateDummyObject<Event.SplinePoint.Entry>(),
                }
            });

        [Fact]
        public void ParseEntryUnk1F() =>
            AssertEvent<Event.Unk1F>();

        [Fact]
        public void ParseSeqGameSpeed() =>
            AssertEvent<Event.SeqGameSpeed>();

        [Fact]
        public void ParseEntryUnk22() =>
            AssertEvent<Event.EntryUnk22>();

        [Fact]
        public void ParseSeqVoices() =>
            AssertEvent(new Event.SeqVoices
            {
                Voices = new List<Event.SeqVoices.Voice>
                {
                    new Event.SeqVoices.Voice
                    {
                        FrameStart = 123,
                        Name = "first"
                    },
                    new Event.SeqVoices.Voice
                    {
                        FrameStart = 456,
                        Name = "second"
                    },
                }
            });

        [Fact]
        public void ParseReadAssets() =>
            AssertEvent(new Event.ReadAssets
            {
                FrameStart = 123,
                FrameEnd = 456,
                Unk06 = 789,
                Set = new List<Event.IEventEntry>
                {
                    Helpers.CreateDummyObject<Event.ReadMotion>(),
                    Helpers.CreateDummyObject<Event.ReadAudio>(),
                    Helpers.CreateDummyObject<Event.ReadActor>(),
                    Helpers.CreateDummyObject<Event.ReadEffect>(),
                    Helpers.CreateDummyObject<Event.ReadLayout>(),
                }
            });

        [Fact]
        public void ParseReadAnimation() =>
            AssertEvent<Event.ReadMotion>();

        [Fact]
        public void ParseReadAudio() =>
            AssertEvent<Event.ReadAudio>();

        [Fact]
        public void ParseSetRumble() =>
            AssertEvent<Event.SetShake>();

        [Fact]
        public void ParseEntryUnk2A() =>
            AssertEvent<Event.EntryUnk2A>();

        [Fact]
        public void ParseSeqPlayAudio() =>
            AssertEvent<Event.SeqPlayAudio>();

        [Fact]
        public void ParseSeqPlayAnimation() =>
            AssertEvent<Event.SeqPlayAnimation>();

        [Fact]
        public void ParseSeqDialog() =>
            AssertEvent<Event.SeqDialog>();

        [Fact]
        public void ParseSeqPlayBgm() =>
            AssertEvent<Event.SeqPlayBgm>();

        [Fact]
        public void ParseReadBgm() =>
            AssertEvent<Event.ReadBgm>();

        [Fact]
        public void ParseSetBgm() =>
            AssertEvent<Event.SetBgm>();

        [Fact]
        public void ParseEntryUnk36() =>
            AssertEvent<Event.EntryUnk36>();

        [Fact]
        public void ParseReadActor() =>
            AssertEvent<Event.ReadActor>();

        [Fact]
        public void ParseReadEffect() =>
            AssertEvent<Event.ReadEffect>();

        [Fact]
        public void ParseSeqLayout() =>
            AssertEvent<Event.SeqLayout>();

        [Fact]
        public void ParseReadLayout() =>
            AssertEvent<Event.ReadLayout>();

        [Fact]
        public void ParseStopEffect() =>
            AssertEvent<Event.StopEffect>();

        [Fact]
        public void ParseRunMovie() =>
            AssertEvent<Event.RunMovie>();

        [Fact]
        public void ParseUnk42() =>
            AssertEvent<Event.Unk42>();

        [Fact]
        public void ParseEntryUnk47() =>
            AssertEvent<Event.EntryUnk47>();

        [Fact]
        public void ParseHideObject() =>
            AssertEvent<Event.SeqHideObject>();

        private static void AssertEvent<T>()
            where T : class, Event.IEventEntry =>
            AssertEvent(Helpers.CreateDummyObject<T>());

        private static void AssertEvent<T>(T expected)
            where T : class, Event.IEventEntry
        {
            using var stream = new MemoryStream();
            Event.Write(stream, new Event.IEventEntry[]
            {
                expected
            });
            var actualLength = stream.Position - 2;
            if ((actualLength % 2) == 1)
                throw new AssertActualExpectedException(
                    actualLength + 1, actualLength,
                    "Offset is not aligned by 2");

            stream.Position = 0;
            var eventSet = Event.Read(stream);
            var expectedLength = stream.Position;

            Assert.Single(eventSet);
            Assert.IsType<T>(eventSet[0]);
            if (expectedLength != actualLength)
                throw new AssertActualExpectedException(
                    expectedLength, actualLength,
                    "Stream length does not match");

            var actual = eventSet[0];
            foreach (var property in typeof(T).GetProperties())
            {
                var expectedValue = property.GetValue(expected);
                var actualValue = property.GetValue(actual);
                if (expectedValue.ToString() !=
                    actualValue.ToString())
                    throw new AssertActualExpectedException(
                        expectedValue, actualValue,
                        $"Different values for '{property.Name}'");
            }
        }

        public class Massive
        {
            private static string SourceDir => Environment.GetEnvironmentVariable("KH2FM_EXTRACTION_DIR");

            [SkippableTheory]
            [MemberData(nameof(GetSource))]
            public void ReadEvent(string source)
            {
                var eventStream = new MemoryStream(ResolveSource(source), false);
                var eventEntries = Event.Read(eventStream);
                Assert.NotNull(eventEntries);
                var text = TreeStrucWriter.GetString(eventEntries);
                Console.Write("");
            }

            private byte[] ResolveSource(string source)
            {
                var sources = source.Split('\t');
                var bar = File.OpenRead(sources[0]).Using(fs => Bar.Read(fs));
                for (int x = 1; x < sources.Length; x++)
                {
                    if (sources[x].StartsWith("#"))
                    {
                        var idx = int.Parse(sources[x].Substring(1));

                        var temp = new MemoryStream();
                        bar[idx].Stream.CopyTo(temp);
                        return temp.ToArray();
                    }
                    else
                    {
                        throw new InvalidDataException();
                    }
                }
                throw new InvalidDataException();
            }

            public static IEnumerable<object[]> GetSource()
            {
                var srcDir = SourceDir;

                if (srcDir != null)
                {
                    foreach (var barFile in Directory.GetFiles(Path.Combine(srcDir, "ard"), "*.ard"))
                    {
                        foreach (var (barEntry, index) in File.OpenRead(barFile)
                            .Using(fs => Bar.Read(fs))
                            .Select((barEntry, index) => (barEntry, index))
                            .ToArray()
                            .Where(tuple => tuple.barEntry.Type == Bar.EntryType.Event && tuple.barEntry.Stream.Length != 0)
                        )
                        {
                            yield return new object[] { $"{barFile}\t#{index}" };
                        }
                    }
                }
            }
        }
    }
}
