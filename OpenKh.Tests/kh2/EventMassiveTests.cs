using OpenKh.Common;
using OpenKh.DeeperTree;
using OpenKh.Kh2;
using OpenKh.Kh2.Ard;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using static OpenKh.Kh2.Ard.Event;

namespace OpenKh.Tests.kh2
{
    public class EventMassiveTests
    {
        private static readonly TreeWriter _treeWriter = new TreeWriterBuilder()
            .Build();

        private static readonly TreeReader _treeReader = new TreeReaderBuilder()
            .AddType(nameof(SetProject), typeof(SetProject))
            .AddType(nameof(SetActor), typeof(SetActor))
            .AddType(nameof(SeqActorPosition), typeof(SeqActorPosition))
            .AddType(nameof(SetMap), typeof(SetMap))
            .AddType(nameof(CameraData), typeof(CameraData))
            .AddType(nameof(SeqCamera), typeof(SeqCamera))
            .AddType(nameof(EffectData), typeof(EffectData))
            .AddType(nameof(SetEndFrame), typeof(SetEndFrame))
            .AddType(nameof(SeqEffect), typeof(SeqEffect))
            .AddType(nameof(AttachEffect), typeof(AttachEffect))
            .AddType(nameof(SeqKage), typeof(SeqKage))
            .AddType(nameof(SeqBgcol), typeof(SeqBgcol))
            .AddType(nameof(SeqPart), typeof(SeqPart))
            .AddType(nameof(SeqAlpha), typeof(SeqAlpha))
            .AddType(nameof(SetupEvent), typeof(SetupEvent))
            .AddType(nameof(EventStart), typeof(EventStart))
            .AddType(nameof(JumpEvent), typeof(JumpEvent))
            .AddType(nameof(SeqFade), typeof(SeqFade))
            .AddType(nameof(SetCameraData), typeof(SetCameraData))
            .AddType(nameof(EntryUnk14), typeof(EntryUnk14))
            .AddType(nameof(SeqSubtitle), typeof(SeqSubtitle))
            .AddType(nameof(BgGrupe), typeof(BgGrupe))
            .AddType(nameof(SeqBlur), typeof(SeqBlur))
            .AddType(nameof(SeqFocus), typeof(SeqFocus))
            .AddType(nameof(SeqTextureAnim), typeof(SeqTextureAnim))
            .AddType(nameof(SeqActorLeave), typeof(SeqActorLeave))
            .AddType(nameof(SeqCrossFade), typeof(SeqCrossFade))
            .AddType(nameof(SeqIk), typeof(SeqIk))
            .AddType(nameof(SplineDataEnc), typeof(SplineDataEnc))
            .AddType(nameof(SplinePoint), typeof(SplinePoint))
            .AddType(nameof(SeqSpline), typeof(SeqSpline))
            .AddType(nameof(SeqGameSpeed), typeof(SeqGameSpeed))
            .AddType(nameof(WideMask), typeof(WideMask))
            .AddType(nameof(SeqVoices), typeof(SeqVoices))
            .AddType(nameof(ReadAssets), typeof(ReadAssets))
            .AddType(nameof(ReadMotion), typeof(ReadMotion))
            .AddType(nameof(ReadAudio), typeof(ReadAudio))
            .AddType(nameof(SetShake), typeof(SetShake))
            .AddType(nameof(Turn), typeof(Turn))
            .AddType(nameof(SeData), typeof(SeData))
            .AddType(nameof(SeqPlayAudio), typeof(SeqPlayAudio))
            .AddType(nameof(SeqPlayAnimation), typeof(SeqPlayAnimation))
            .AddType(nameof(SeqDialog), typeof(SeqDialog))
            .AddType(nameof(SeqPlayBgm), typeof(SeqPlayBgm))
            .AddType(nameof(ReadBgm), typeof(ReadBgm))
            .AddType(nameof(SetBgm), typeof(SetBgm))
            .AddType(nameof(Lookat), typeof(Lookat))
            .AddType(nameof(ReadActor), typeof(ReadActor))
            .AddType(nameof(ReadEffect), typeof(ReadEffect))
            .AddType(nameof(SeqLayout), typeof(SeqLayout))
            .AddType(nameof(ReadLayout), typeof(ReadLayout))
            .AddType(nameof(StopEffect), typeof(StopEffect))
            .AddType(nameof(SeqBgse), typeof(SeqBgse))
            .AddType(nameof(RunMovie), typeof(RunMovie))
            .AddType(nameof(SeqPosMove), typeof(SeqPosMove))
            .AddType(nameof(SeqHideObject), typeof(SeqHideObject))
            .AddType(nameof(VibData), typeof(VibData))
            .AddType(nameof(ShadowAlpha), typeof(ShadowAlpha))
            .AddType(nameof(BlackFog), typeof(BlackFog))
            .AddType(nameof(SeqMirror), typeof(SeqMirror))
            .AddType(nameof(Scale), typeof(Scale))
            .AddType(nameof(CacheClear), typeof(CacheClear))
            .AddType(nameof(TexFade), typeof(TexFade))
            .AddType(nameof(Light), typeof(Light))
            .AddType(nameof(SeqMob), typeof(SeqMob))
            .AddType(nameof(Fog), typeof(Fog))
            .AddType(nameof(SkyOff), typeof(SkyOff))
            .AddType(nameof(MusicalHeader), typeof(MusicalHeader))
            .AddType(nameof(MusicalScene), typeof(MusicalScene))
            .AddType(nameof(MusicalTarget), typeof(MusicalTarget))
            .AddType(nameof(Tag), typeof(Tag))
            .AddType(nameof(PlayerOffsetCamera), typeof(PlayerOffsetCamera))
            .AddType(nameof(SeqCameraCollision), typeof(SeqCameraCollision))
            .AddType(nameof(VoiceAllFadeout), typeof(VoiceAllFadeout))
            .AddType(nameof(WallClip), typeof(WallClip))
            .AddType(nameof(SeqGlow), typeof(SeqGlow))

            .AddType("Voice", typeof(SeqVoices.Voice))
            .AddType("CameraKeys", typeof(CameraKeys))
            .AddType("Point", typeof(SplinePoint.Point))
            .AddType("Data", typeof(Light.Data))
            .Build();

        private static string SourceDir => Environment.GetEnvironmentVariable("KH2FM_EXTRACTION_DIR");
        private static string SaveDir => Path.Combine(Environment.CurrentDirectory, "EventTests");

        [SkippableTheory(Skip = "Skipping this due to disordered 'offset' data. Some files (al00.ard al03.ard eh19.ard mu06.ard po00.ard tt05.ard) cannot pass regression test.")]
        [MemberData(nameof(GetSource))]
        public void EventRegression(string source)
        {
            var src = new BarEntrySource(source);
            var eventStream = src.GetMemoryStream();
            var eventEntries = Event.Read(eventStream);
            Assert.NotNull(eventEntries);
            var newEventStream = new MemoryStream(eventStream.Capacity);
            Event.Write(newEventStream, eventEntries);

            if (src.GetRelativePart() is string relative)
            {
                {
                    var saveTo = Path.Combine(Environment.CurrentDirectory, "EventRegression", relative + ".new.event");
                    Directory.CreateDirectory(Path.GetDirectoryName(saveTo));
                    File.WriteAllBytes(saveTo, newEventStream.ToArray());
                }
                {
                    var saveTo = Path.Combine(Environment.CurrentDirectory, "EventRegression", relative + ".org.event");
                    Directory.CreateDirectory(Path.GetDirectoryName(saveTo));
                    File.WriteAllBytes(saveTo, eventStream.ToArray());
                }
            }

            Assert.Equal(
                expected: eventStream.ToArray(),
                actual: newEventStream.ToArray()
            );
        }

        public static IEnumerable<object[]> GetSource()
        {
            var srcDir = SourceDir;

            if (srcDir != null)
            {
                foreach (var barFile in new string[0]
                    .Concat(Directory.GetFiles(Path.Combine(srcDir, "ard"), "*.ard"))
                )
                {
                    foreach (var source in BarEntrySourceRef.FromFile(barFile, srcDir)
                        .Where(one => one.Entry.Type == Bar.EntryType.Event && one.Entry.Stream.Length != 0)
                        .Select(one => one.GetSource().Source)
                    )
                    {
                        yield return new object[] { source };
                    }
                }
            }
        }
    }
}
