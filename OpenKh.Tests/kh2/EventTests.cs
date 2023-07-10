using OpenKh.Common;
using OpenKh.DeeperTree;
using OpenKh.Kh2;
using OpenKh.Kh2.Ard;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Xunit;
using Xunit.Sdk;
using static OpenKh.Kh2.Ard.Event;

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
        public void ParseCameraData() =>
            AssertEvent<Event.CameraData>();

        [Fact]
        public void ParseSeqCamera() =>
            AssertEvent<Event.SeqCamera>();

        [Fact]
        public void ParseEffectData() =>
            AssertEvent<Event.EffectData>();

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
        public void ParseSeqKage() =>
            AssertEvent<Event.SeqKage>();

        [Fact]
        public void ParseSeqBgcol() =>
            AssertEvent<Event.SeqBgcol>();

        [Fact]
        public void ParseSeqPart() =>
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
        public void ParseSeqAlpha() =>
            AssertEvent<Event.SeqAlpha>();

        [Fact]
        public void ParseSetupEvent() =>
            AssertEvent<Event.SetupEvent>();

        [Fact]
        public void ParseEventStart() =>
            AssertEvent<Event.EventStart>();

        [Fact]
        public void ParseJumpEvent() =>
            AssertEvent<Event.JumpEvent>();

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
        public void ParseBgGrupe() =>
            AssertEvent<Event.BgGrupe>();

        [Fact]
        public void ParseSeqBlur() =>
            AssertEvent<Event.SeqBlur>();

        [Fact]
        public void ParseSeqFocus() =>
            AssertEvent<Event.SeqFocus>();

        [Fact]
        public void ParseSeqTextureAnim() =>
            AssertEvent<Event.SeqTextureAnim>();

        [Fact]
        public void ParseSeqActorLeave() =>
            AssertEvent<Event.SeqActorLeave>();

        [Fact]
        public void ParseSeqCrossFade() =>
            AssertEvent<Event.SeqCrossFade>();

        [Fact]
        public void ParseSeqIk() =>
            AssertEvent<Event.SeqIk>();

        [Fact]
        public void ParseSplineDataEnc() =>
            AssertEvent<Event.SplineDataEnc>(
                new SplineDataEnc
                {
                    PutId = 123,
                    TransOfs = 789,
                    Keys = new List<CameraKeys>()
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
                }
            );

        [Fact]
        public void ParseSplinePoint() =>
            AssertEvent(new Event.SplinePoint
            {
                Id = 123,
                Type = 456,
                SplineLng = 789,
                Points = new List<Event.SplinePoint.Point>()
                {
                    Helpers.CreateDummyObject<Event.SplinePoint.Point>(),
                    Helpers.CreateDummyObject<Event.SplinePoint.Point>(),
                    Helpers.CreateDummyObject<Event.SplinePoint.Point>(),
                    Helpers.CreateDummyObject<Event.SplinePoint.Point>(),
                    Helpers.CreateDummyObject<Event.SplinePoint.Point>(),
                    Helpers.CreateDummyObject<Event.SplinePoint.Point>(),
                    Helpers.CreateDummyObject<Event.SplinePoint.Point>(),
                }
            });

        [Fact]
        public void ParseSeqSpline() =>
            AssertEvent<Event.SeqSpline>();

        [Fact]
        public void ParseSeqGameSpeed() =>
            AssertEvent<Event.SeqGameSpeed>();

        [Fact]
        public void ParseTexFade() =>
            AssertEvent<Event.TexFade>();

        [Fact]
        public void ParseWideMask() =>
            AssertEvent<Event.WideMask>();

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
        public void ParseScale() =>
            AssertEvent<Event.Scale>();

        [Fact]
        public void ParseTurn() =>
            AssertEvent<Event.Turn>();

        [Fact]
        public void ParseSeData() =>
            AssertEvent<Event.SeData>();

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
        public void ParseSeqObjCamera() =>
            AssertEvent<Event.SeqObjCamera>();

        [Fact]
        public void ParseMusicalHeader() =>
            AssertEvent<Event.MusicalHeader>();

        [Fact]
        public void ParseMusicalTarget() =>
            AssertEvent<Event.MusicalTarget>();

        [Fact]
        public void ParseMusicalScene() =>
            AssertEvent<Event.MusicalScene>();

        [Fact]
        public void ParseVibData() =>
            AssertEvent<Event.VibData>(
                new VibData
                {
                    Frame = 12,
                    Dummy = 34,
                    Data = new byte[] { 1, 2, 3, 4 },
                }
            );

        [Fact]
        public void ParseLookat() =>
            AssertEvent<Event.Lookat>();

        [Fact]
        public void ParseShadowAlpha() =>
            AssertEvent<Event.ShadowAlpha>();

        [Fact]
        public void ParseReadActor() =>
            AssertEvent<Event.ReadActor>();

        [Fact]
        public void ParseReadEffect() =>
            AssertEvent<Event.ReadEffect>();

        [Fact]
        public void ParseSeqMirror() =>
            AssertEvent<Event.SeqMirror>();

        [Fact]
        public void ParseSeqTreasure() =>
            AssertEvent<Event.SeqTreasure>();

        [Fact]
        public void ParseSeqMissionEffect() =>
            AssertEvent<Event.SeqMissionEffect>();

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
        public void ParseCacheClear() =>
            AssertEvent<Event.CacheClear>(
                new CacheClear
                {
                    PutId = Enumerable.Range(0, 96).Select(idx => (byte)idx).ToArray(),
                    Frame = 12345,
                }
            );

        [Fact]
        public void ParseSeqObjPause() =>
            AssertEvent<Event.SeqObjPause>();

        [Fact]
        public void ParseSeqBgse() =>
            AssertEvent<Event.SeqBgse>();

        [Fact]
        public void ParseSeqGlow() =>
            AssertEvent<Event.SeqGlow>();

        [Fact]
        public void ParseRunMovie() =>
            AssertEvent<Event.RunMovie>();

        [Fact]
        public void ParseSeqSavePoint() =>
            AssertEvent<Event.SeqSavePoint>();

        [Fact]
        public void ParseSeqCameraCollision() =>
            AssertEvent<Event.SeqCameraCollision>();

        [Fact]
        public void ParseSeqPosMove() =>
            AssertEvent<Event.SeqPosMove>();

        [Fact]
        public void ParseBlackFog() =>
            AssertEvent<Event.BlackFog>();

        [Fact]
        public void ParseFog() =>
            AssertEvent<Event.Fog>();

        [Fact]
        public void ParsePlayerOffsetCamera() =>
            AssertEvent<Event.PlayerOffsetCamera>();

        [Fact]
        public void ParseSkyOff() =>
            AssertEvent<Event.SkyOff>();

        [Fact]
        public void ParseHideObject() =>
            AssertEvent<Event.SeqHideObject>();

        [Fact]
        public void ParseCountdown() =>
            AssertEvent<Event.Countdown>();

        [Fact]
        public void ParseTag() =>
            AssertEvent<Event.Tag>();

        [Fact]
        public void ParseWallClip() =>
            AssertEvent<Event.WallClip>();

        [Fact]
        public void ParseVoiceAllFadeout() =>
            AssertEvent<Event.VoiceAllFadeout>();

        [Fact]
        public void ParseLight() =>
            AssertEvent<Event.Light>(
                new Light
                {
                    WorkNum = 12345,
                    LightData = new List<Light.Data>()
                    {
                        new Light.Data
                        {
                            PutId = 1,
                            StartFrame = 2,
                            EndFrame = 3,
                            CamNum = 4,
                            SubNum = 5,
                            Position = new Light.LightParamPosition
                            {
                                Pos = Enumerable.Range(0, 9).Select(it => (float)it).ToArray(),
                                Color = Enumerable.Range(0, 12).Select(it => (float)it).ToArray(),
                            },
                        }
                    },
                }
            );

        [Fact]
        public void ParseSeqMob() =>
            AssertEvent<Event.SeqMob>();

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
                throw NotEqualException.ForEqualValues(
                    $"{actualLength + 1}", $"{actualLength}",
                    "Offset is not aligned by 2");

            stream.Position = 0;
            var eventSet = Event.Read(stream);
            var expectedLength = stream.Position;

            Assert.Single(eventSet);
            Assert.IsType<T>(eventSet[0]);
            if (expectedLength != actualLength)
                throw NotEqualException.ForEqualValues(
                    $"{expectedLength}", $"{actualLength}",
                    "Stream length does not match");

            var actual = eventSet[0];
            foreach (var property in typeof(T).GetProperties())
            {
                var expectedValue = property.GetValue(expected);
                var actualValue = property.GetValue(actual);
                if (!IsValueEqual(expectedValue, actualValue))
                    throw NotEqualException.ForEqualValues(
                        $"{expectedValue}", $"{actualValue}",
                        $"Different values for '{property.Name}'");
            }
        }

        private static bool IsValueEqual(object expectedValue, object actualValue)
        {
            if ((expectedValue == null) != (actualValue == null))
            {
                return false;
            }
            if (expectedValue == null)
            {
                return true;
            }

            if (expectedValue.GetType() != actualValue.GetType())
            {
                return false;
            }

            return JsonSerializer.Serialize(expectedValue) == JsonSerializer.Serialize(actualValue);
        }
    }
}
