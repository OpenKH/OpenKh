using OpenKh.Common;
using OpenKh.Kh2.Ard;
using System.Collections.Generic;
using System.IO;
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
            AssertEvent<Event.Unk0C>();

        [Fact]
        public void ParseUnk0D() =>
            AssertEvent(new Event.Unk0D
            {
                StartFrame = 123,
                EndFrame = 456,
                Unk = new short[]
                {
                    11, 22, 33, 44
                }
            });

        [Fact]
        public void ParseUnk0E() =>
            AssertEvent<Event.Unk0E>();

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
                PositionX = new List<Event.SetCameraData.CameraKeys>
                {
                    new Event.SetCameraData.CameraKeys
                    {
                        Interpolation = Kh2.Motion.Interpolation.Hermite,
                        KeyFrame = 1234,
                        Value = 1,
                        TangentEaseIn = 3,
                        TangentEaseOut = 4
                    },
                    new Event.SetCameraData.CameraKeys
                    {
                        Interpolation = Kh2.Motion.Interpolation.Linear,
                        KeyFrame = 5678,
                        Value = 5,
                        TangentEaseIn = 7,
                        TangentEaseOut = 8
                    },
                },
                PositionY = new List<Event.SetCameraData.CameraKeys>()
                {
                    new Event.SetCameraData.CameraKeys
                    {
                        Interpolation = Kh2.Motion.Interpolation.Nearest,
                        KeyFrame = 32767,
                        Value = 11,
                        TangentEaseIn = 33,
                        TangentEaseOut = 44
                    },
                },
                PositionZ = new List<Event.SetCameraData.CameraKeys>(),
                LookAtX = new List<Event.SetCameraData.CameraKeys>(),
                LookAtY = new List<Event.SetCameraData.CameraKeys>(),
                LookAtZ = new List<Event.SetCameraData.CameraKeys>(),
                FieldOfView = new List<Event.SetCameraData.CameraKeys>(),
                Roll = new List<Event.SetCameraData.CameraKeys>(),
            });

        [Fact]
        public void ParseEntryUnk14() =>
            AssertEvent<Event.EntryUnk14>();

        [Fact]
        public void ParseSeqSubtitle() =>
            AssertEvent<Event.SeqSubtitle>();

        [Fact]
        public void ParseEntryUnk16() =>
            AssertEvent<Event.EntryUnk16>();

        [Fact]
        public void ParseEntryUnk17() =>
            AssertEvent<Event.EntryUnk17>();

        [Fact]
        public void ParseEntryUnk18() =>
            AssertEvent<Event.EntryUnk18>();

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
            AssertEvent<Event.EntryUnk1D>();

        [Fact]
        public void ParseEntryUnk1E() =>
            AssertEvent(new Event.Unk1E
            {
                Id = 123,
                UnkG = 456,
                UnkH = 789,
                Entries = new List<Event.Unk1E.Entry>()
                {
                    Helpers.CreateDummyObject<Event.Unk1E.Entry>(),
                    Helpers.CreateDummyObject<Event.Unk1E.Entry>(),
                    Helpers.CreateDummyObject<Event.Unk1E.Entry>(),
                    Helpers.CreateDummyObject<Event.Unk1E.Entry>(),
                    Helpers.CreateDummyObject<Event.Unk1E.Entry>(),
                    Helpers.CreateDummyObject<Event.Unk1E.Entry>(),
                    Helpers.CreateDummyObject<Event.Unk1E.Entry>(),
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
    }
}
