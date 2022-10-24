using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using static OpenKh.Kh2.Ard.Event;

namespace OpenKh.Kh2.Ard
{
    [XmlRoot("Root")]
    public class EventsXmlRoot
    {
        [XmlIgnore]
        public List<IEventEntry> Entries { get; set; }

        public static string ToXml(EventsXmlRoot root)
        {
            var writer = new StringWriter();
            var xmlWriter = XmlWriter.Create(
                writer,
                new XmlWriterSettings
                {
                    Indent = true,
                    IndentChars = " ",
                    OmitXmlDeclaration = true,
                    Encoding = Encoding.UTF8,
                }
            );
            new XmlSerializer(typeof(EventsXmlRoot)).Serialize(
                xmlWriter,
                root
            );
            return writer.ToString();
        }

        public static EventsXmlRoot FromXml(string xml)
        {
            var reader = new StringReader(xml);
            var xmlReader = XmlReader.Create(
                reader,
                new XmlReaderSettings
                {
                }
            );
            return (EventsXmlRoot)new XmlSerializer(typeof(EventsXmlRoot)).Deserialize(
                xmlReader
            );
        }

        [XmlArray("Entries")]
        [XmlArrayItem(typeof(SetProject), ElementName = "SetProject")]
        [XmlArrayItem(typeof(SetActor), ElementName = "SetActor")]
        [XmlArrayItem(typeof(SeqActorPosition), ElementName = "SeqActorPosition")]
        [XmlArrayItem(typeof(SetMap), ElementName = "SetMap")]
        [XmlArrayItem(typeof(CameraData), ElementName = "CameraData")]
        [XmlArrayItem(typeof(SeqCamera), ElementName = "SeqCamera")]
        [XmlArrayItem(typeof(EffectData), ElementName = "EffectData")]
        [XmlArrayItem(typeof(SetEndFrame), ElementName = "SetEndFrame")]
        [XmlArrayItem(typeof(SeqEffect), ElementName = "SeqEffect")]
        [XmlArrayItem(typeof(AttachEffect), ElementName = "AttachEffect")]
        [XmlArrayItem(typeof(SeqKage), ElementName = "SeqKage")]
        [XmlArrayItem(typeof(SeqBgcol), ElementName = "SeqBgcol")]
        [XmlArrayItem(typeof(SeqPart), ElementName = "SeqPart")]
        [XmlArrayItem(typeof(SeqAlpha), ElementName = "SeqAlpha")]
        [XmlArrayItem(typeof(SetupEvent), ElementName = "SetupEvent")]
        [XmlArrayItem(typeof(EventStart), ElementName = "EventStart")]
        [XmlArrayItem(typeof(JumpEvent), ElementName = "JumpEvent")]
        [XmlArrayItem(typeof(SeqFade), ElementName = "SeqFade")]
        [XmlArrayItem(typeof(SetCameraData), ElementName = "SetCameraData")]
        [XmlArrayItem(typeof(EntryUnk14), ElementName = "EntryUnk14")]
        [XmlArrayItem(typeof(SeqSubtitle), ElementName = "SeqSubtitle")]
        [XmlArrayItem(typeof(BgGrupe), ElementName = "BgGrupe")]
        [XmlArrayItem(typeof(SeqBlur), ElementName = "SeqBlur")]
        [XmlArrayItem(typeof(SeqFocus), ElementName = "SeqFocus")]
        [XmlArrayItem(typeof(SeqTextureAnim), ElementName = "SeqTextureAnim")]
        [XmlArrayItem(typeof(SeqActorLeave), ElementName = "SeqActorLeave")]
        [XmlArrayItem(typeof(SeqCrossFade), ElementName = "SeqCrossFade")]
        [XmlArrayItem(typeof(SeqIk), ElementName = "SeqIk")]
        [XmlArrayItem(typeof(SplineDataEnc), ElementName = "SplineDataEnc")]
        [XmlArrayItem(typeof(SplinePoint), ElementName = "SplinePoint")]
        [XmlArrayItem(typeof(SeqSpline), ElementName = "SeqSpline")]
        [XmlArrayItem(typeof(SeqGameSpeed), ElementName = "SeqGameSpeed")]
        [XmlArrayItem(typeof(TexFade), ElementName = "TexFade")]
        [XmlArrayItem(typeof(WideMask), ElementName = "WideMask")]
        [XmlArrayItem(typeof(SeqVoices), ElementName = "SeqVoices")]
        [XmlArrayItem(typeof(ReadAssets), ElementName = "ReadAssets")]
        [XmlArrayItem(typeof(ReadMotion), ElementName = "ReadMotion")]
        [XmlArrayItem(typeof(ReadAudio), ElementName = "ReadAudio")]
        [XmlArrayItem(typeof(SetShake), ElementName = "SetShake")]
        [XmlArrayItem(typeof(Scale), ElementName = "Scale")]
        [XmlArrayItem(typeof(Turn), ElementName = "Turn")]
        [XmlArrayItem(typeof(SeData), ElementName = "SeData")]
        [XmlArrayItem(typeof(SeqPlayAudio), ElementName = "SeqPlayAudio")]
        [XmlArrayItem(typeof(SeqPlayAnimation), ElementName = "SeqPlayAnimation")]
        [XmlArrayItem(typeof(SeqDialog), ElementName = "SeqDialog")]
        [XmlArrayItem(typeof(SeqPlayBgm), ElementName = "SeqPlayBgm")]
        [XmlArrayItem(typeof(ReadBgm), ElementName = "ReadBgm")]
        [XmlArrayItem(typeof(SetBgm), ElementName = "SetBgm")]
        [XmlArrayItem(typeof(SeqObjCamera), ElementName = "SeqObjCamera")]
        [XmlArrayItem(typeof(MusicalHeader), ElementName = "MusicalHeader")]
        [XmlArrayItem(typeof(MusicalTarget), ElementName = "MusicalTarget")]
        [XmlArrayItem(typeof(MusicalScene), ElementName = "MusicalScene")]
        [XmlArrayItem(typeof(VibData), ElementName = "VibData")]
        [XmlArrayItem(typeof(Lookat), ElementName = "Lookat")]
        [XmlArrayItem(typeof(ShadowAlpha), ElementName = "ShadowAlpha")]
        [XmlArrayItem(typeof(ReadActor), ElementName = "ReadActor")]
        [XmlArrayItem(typeof(ReadEffect), ElementName = "ReadEffect")]
        [XmlArrayItem(typeof(SeqMirror), ElementName = "SeqMirror")]
        [XmlArrayItem(typeof(SeqTreasure), ElementName = "SeqTreasure")]
        [XmlArrayItem(typeof(SeqMissionEffect), ElementName = "SeqMissionEffect")]
        [XmlArrayItem(typeof(SeqLayout), ElementName = "SeqLayout")]
        [XmlArrayItem(typeof(ReadLayout), ElementName = "ReadLayout")]
        [XmlArrayItem(typeof(StopEffect), ElementName = "StopEffect")]
        [XmlArrayItem(typeof(CacheClear), ElementName = "CacheClear")]
        [XmlArrayItem(typeof(SeqObjPause), ElementName = "SeqObjPause")]
        [XmlArrayItem(typeof(SeqBgse), ElementName = "SeqBgse")]
        [XmlArrayItem(typeof(SeqGlow), ElementName = "SeqGlow")]
        [XmlArrayItem(typeof(RunMovie), ElementName = "RunMovie")]
        [XmlArrayItem(typeof(SeqSavePoint), ElementName = "SeqSavePoint")]
        [XmlArrayItem(typeof(SeqCameraCollision), ElementName = "SeqCameraCollision")]
        [XmlArrayItem(typeof(SeqPosMove), ElementName = "SeqPosMove")]
        [XmlArrayItem(typeof(BlackFog), ElementName = "BlackFog")]
        [XmlArrayItem(typeof(Fog), ElementName = "Fog")]
        [XmlArrayItem(typeof(PlayerOffsetCamera), ElementName = "PlayerOffsetCamera")]
        [XmlArrayItem(typeof(SkyOff), ElementName = "SkyOff")]
        [XmlArrayItem(typeof(SeqHideObject), ElementName = "SeqHideObject")]
        [XmlArrayItem(typeof(Light), ElementName = "Light")]
        [XmlArrayItem(typeof(SeqMob), ElementName = "SeqMob")]
        [XmlArrayItem(typeof(Countdown), ElementName = "Countdown")]
        [XmlArrayItem(typeof(Tag), ElementName = "Tag")]
        [XmlArrayItem(typeof(WallClip), ElementName = "WallClip")]
        [XmlArrayItem(typeof(VoiceAllFadeout), ElementName = "VoiceAllFadeout")]
        public object[] EntriesProperty
        {
            get => Entries.ToArray();
            set => Entries = value.Cast<IEventEntry>().ToList();
        }
    }
}
