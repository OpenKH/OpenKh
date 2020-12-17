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
                PositionX = new List<Event.SetCameraData.CameraValue>
                {
                    new Event.SetCameraData.CameraValue
                    {
                        Value = 1,
                        Speed = 2,
                        Unk08 = 3,
                        Unk0C = 4
                    },
                    new Event.SetCameraData.CameraValue
                    {
                        Value = 5,
                        Speed = 6,
                        Unk08 = 7,
                        Unk0C = 8
                    },
                },
                PositionY = new List<Event.SetCameraData.CameraValue>()
                {
                    new Event.SetCameraData.CameraValue
                    {
                        Value = 11,
                        Speed = 22,
                        Unk08 = 33,
                        Unk0C = 44
                    },
                },
                PositionZ = new List<Event.SetCameraData.CameraValue>(),
                LookAtX = new List<Event.SetCameraData.CameraValue>(),
                LookAtY = new List<Event.SetCameraData.CameraValue>(),
                LookAtZ = new List<Event.SetCameraData.CameraValue>(),
                FieldOfView = new List<Event.SetCameraData.CameraValue>(),
                Roll = new List<Event.SetCameraData.CameraValue>(),
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
        public void ParseEntryUnk1A() =>
            AssertEvent<Event.EntryUnk1A>();

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
                Unk02 = 123,
                Unk04 = 456,
                Unk06 = 789,
                Set = new List<Event.IEventEntry>
                {
                    Helpers.CreateDummyObject<Event.ReadMotion>(),
                    Helpers.CreateDummyObject<Event.ReadAudio>(),
                    Helpers.CreateDummyObject<Event.ReadActor>(),
                    Helpers.CreateDummyObject<Event.ReadEffect>(),
                }
            });

        [Fact]
        public void ParseReadAnimation() =>
            AssertEvent<Event.ReadMotion>();

        [Fact]
        public void ParseReadAudio() =>
            AssertEvent<Event.ReadAudio>();

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
        public void ParseEntryUnk2E() =>
            AssertEvent<Event.EntryUnk2E>();

        [Fact]
        public void ParseEntryUnk2F() =>
            AssertEvent<Event.EntryUnk2F>();

        [Fact]
        public void ParseEntryUnk30() =>
            AssertEvent<Event.EntryUnk30>();

        [Fact]
        public void ParseReadActor() =>
            AssertEvent<Event.ReadActor>();

        [Fact]
        public void ParseReadEffect() =>
            AssertEvent<Event.ReadEffect>();

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
