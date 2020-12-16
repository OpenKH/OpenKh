using OpenKh.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Xe.BinaryMapper;

namespace OpenKh.Kh2.Ard
{
    public static class Event
    {
        private static readonly Dictionary<int, Func<Stream, IEventEntry>> _entryType = new Dictionary<int, Func<Stream, IEventEntry>>()
        {
            [0x00] = EntryProject.Read,
            [0x01] = EntryActor.Read,
            [0x02] = EntryActorPosition.Read,
            [0x03] = EntryMap.Read,
            [0x06] = EntryCameraTimeline.Read,
            [0x08] = SetEndFrame.Read,
            [0x09] = SeqEffect.Read,
            [0x0A] = AttachEffect.Read,
            [0x0F] = SetupEvent.Read,
            [0x10] = EventStart.Read,
            [0x12] = SeqFade.Read,
            [0x13] = SetCameraData.Read,
            [0x14] = EntryUnk14.Read,
            [0x15] = EntrySubtitle.Read,
            [0x16] = EntryUnk16.Read,
            [0x17] = EntryUnk17.Read,
            [0x1A] = EntryUnk1A.Read,
            [0x22] = EntryUnk22.Read,
            [0x23] = EntryVoices.Read,
            [0x24] = EntryLoadAssets.Read,
            [0x2A] = EntryUnk2A.Read,
            [0x2B] = EntryPlayBgm.Read,
            [0x2C] = EntryRunAnimation.Read,
            [0x2D] = EntryDialog.Read,
            [0x2E] = EntryUnk2E.Read,
            [0x2F] = EntryUnk2F.Read,
            [0x30] = EntryUnk30.Read,
        };

        private static readonly Dictionary<int, Func<Stream, IEventLoad>> _loadType = new Dictionary<int, Func<Stream, IEventLoad>>()
        {
            [0x25] = LoadAnimation.Read,
            [0x26] = LoadVoice.Read,
            [0x38] = LoadObject.Read,
            [0x39] = LoadPax.Read,
        };

        public interface IEventEntry
        { }

        public interface IEventLoad
        { }

        public class EntryProject : IEventEntry // unused
        {
            public int Unk00 { get; set; }
            public int Unk02 { get; set; }
            public int Version { get; set; }
            public int ObjCameraType { get; set; }
            public string Name { get; set; }

            private EntryProject(Stream stream)
            {
                Unk00 = stream.ReadInt16();
                Unk02 = stream.ReadInt16();
                Version = stream.ReadByte(); // Always PS2_BIN_VER (2)
                ObjCameraType = stream.ReadByte();
                Name = ReadCStyleString(stream); // Can't be larger than 32 bytes
            }

            public override string ToString() =>
                $"Project: {Name}, Version {Version}, Camera type {ObjCameraType}, ({Unk00:X}, {Unk02:X})";

            public static IEventEntry Read(Stream stream) => new EntryProject(stream);
        }

        public class EntryActor : IEventEntry // sub_22F528
        {
            public int ObjectEntry { get; set; }
            public int ActorId { get; set; }
            public string Name { get; set; }

            private EntryActor(Stream stream)
            {
                ObjectEntry = stream.ReadInt16();
                ActorId = stream.ReadInt16();
                Name = ReadCStyleString(stream);
            }

            public override string ToString() =>
                $"Actor: ObjEntry {ObjectEntry:X}, Name {Name}, ActorID {ActorId}";

            public static IEventEntry Read(Stream stream) => new EntryActor(stream);
        }
        
        public class EntryActorPosition : IEventEntry
        {
            public float Unk00 { get; set; }
            public float PositionX { get; set; }
            public float PositionY { get; set; }
            public float PositionZ { get; set; }
            public float RotationX { get; set; }
            public float RotationY { get; set; }
            public float RotationZ { get; set; }
            public float Unk1C { get; set; }
            public int ActorId { get; set; }
            public int Frame { get; set; }

            private EntryActorPosition(Stream stream)
            {
                Unk00 = stream.ReadSingle();
                PositionX = stream.ReadSingle();
                PositionY = stream.ReadSingle();
                PositionZ = stream.ReadSingle();
                RotationX = stream.ReadSingle();
                RotationY = stream.ReadSingle();
                RotationZ = stream.ReadSingle();
                Unk1C = stream.ReadSingle();
                ActorId = stream.ReadInt16();
                Frame = stream.ReadInt16();
            }

            public override string ToString() =>
                $"ActorPosition: Frame {Frame}, ActorID {ActorId}, Pos({PositionX}, {PositionY}, {PositionZ}) Rot({RotationX}, {RotationY}, {RotationZ}) Unk({Unk00}, {Unk1C})";

            public static IEventEntry Read(Stream stream) => new EntryActorPosition(stream);
        }

        public class EntryMap : IEventEntry // unused
        {
            public int Place { get; set; }
            public string World { get; set; }

            private EntryMap(Stream stream)
            {
                Place = stream.ReadInt16();
                World = ReadCStyleString(stream);
            }

            public override string ToString() =>
                $"Map: {World}{Place:D02}";

            public static IEventEntry Read(Stream stream) => new EntryMap(stream);
        }

        public class EventStart : IEventEntry // sub_22D3A8
        {
            public int FadeIn { get; set; } // dword_35DE40

            private EventStart(Stream stream)
            {
                FadeIn = stream.ReadInt16();
                stream.ReadInt16();
            }

            public override string ToString() =>
                $"FadeIn: Fade in for {FadeIn} frames";

            public static IEventEntry Read(Stream stream) => new EventStart(stream);
        }

        public class SeqFade: IEventEntry
        {
            public enum FadeType
            {
                FromBlack,
                ToBlack,
                ToWhite,
                FromBlackVariant,
                ToBlackVariant,
                FromWhite,
                ToWhiteVariant,
                FromWhiteVariant,
            }

            public int FrameIndex { get; set; }
            public int Duration { get; set; }
            public FadeType Type { get; set; }

            private SeqFade(Stream stream)
            {
                FrameIndex = stream.ReadInt16();
                Duration = stream.ReadInt16();
                Type = (FadeType)stream.ReadInt16();
            }

            public override string ToString() =>
                $"Fade: Frame {FrameIndex}, Duration {Duration}, {Type}";

            public static IEventEntry Read(Stream stream) => new SeqFade(stream);
        }

        public class EntryUnk14 : IEventEntry
        {
            public int Unk00 { get; set; }

            private EntryUnk14(Stream stream)
            {
                Unk00 = stream.ReadInt16();
            }

            public override string ToString() =>
                $"Unk14: {Unk00}";

            public static IEventEntry Read(Stream stream) => new EntryUnk14(stream);
        }

        public class EntrySubtitle : IEventEntry
        {
            public int FrameStart { get; set; }
            public int Index { get; set; }
            public int MessageId { get; set; }
            public int HideFlag { get; set; }

            private EntrySubtitle(Stream stream)
            {
                FrameStart = stream.ReadInt16();
                Index = stream.ReadInt16();
                MessageId = stream.ReadInt16();
                HideFlag = stream.ReadInt16();
            }

            public override string ToString() =>
                $"Subtitle: Frame {FrameStart}, MsgId {MessageId}, Index {Index}, Hide {HideFlag != 0}";

            public static IEventEntry Read(Stream stream) => new EntrySubtitle(stream);
        }

        public class EntryUnk16 : IEventEntry
        {
            public int Unk00 { get; set; }
            public int Unk02 { get; set; }

            private EntryUnk16(Stream stream)
            {
                Unk00 = stream.ReadInt16();
                Unk02 = stream.ReadInt16();
            }

            public override string ToString() =>
                $"Unk16: {Unk00} {Unk02}";

            public static IEventEntry Read(Stream stream) => new EntryUnk16(stream);
        }

        public class EntryUnk17 : IEventEntry
        {
            public int Unk00 { get; set; }
            public int Unk02 { get; set; }
            public int Unk04 { get; set; }
            public int Unk06 { get; set; }
            public int Unk08 { get; set; }
            public int Unk0A { get; set; }
            public int Unk0C { get; set; }
            public int Unk0E { get; set; }

            private EntryUnk17(Stream stream)
            {
                Unk00 = stream.ReadInt16();
                Unk02 = stream.ReadInt16();
                Unk04 = stream.ReadInt16();
                Unk06 = stream.ReadInt16();
                Unk08 = stream.ReadInt16();
                Unk0A = stream.ReadInt16();
                Unk0C = stream.ReadInt16();
                Unk0E = stream.ReadInt16();
            }

            public override string ToString() =>
                $"Unk17: {Unk00}, {Unk02} {Unk04} {Unk06} {Unk08} {Unk0A} {Unk0C} {Unk0E}";

            public static IEventEntry Read(Stream stream) => new EntryUnk17(stream);
        }

        public class EntryUnk1A : IEventEntry
        {
            public int Unk00 { get; set; }
            public int Unk02 { get; set; }

            private EntryUnk1A(Stream stream)
            {
                Unk00 = stream.ReadInt16();
                Unk02 = stream.ReadInt16();
            }

            public override string ToString() =>
                $"Unk1A: {Unk00} {Unk02}";

            public static IEventEntry Read(Stream stream) => new EntryUnk1A(stream);
        }

        public class EntryUnk22 : IEventEntry
        {
            public int Unk00 { get; set; }
            public int Unk02 { get; set; }

            private EntryUnk22(Stream stream)
            {
                Unk00 = stream.ReadInt16();
                Unk02 = stream.ReadInt16();
            }

            public override string ToString() =>
                $"Unk22: {Unk00} {Unk02}";

            public static IEventEntry Read(Stream stream) => new EntryUnk22(stream);
        }

        public class EntryVoices : IEventEntry
        {
            public class Voice
            {
                public int FrameStart { get; set; }
                public string Name { get; set; }

                public override string ToString() =>
                    $"Frame {FrameStart}, {Name}";
            }

            public List<Voice> Voices { get; set; }

            private EntryVoices(Stream stream)
            {
                var voiceCount = stream.ReadInt32();
                Voices = new List<Voice>(voiceCount);
                for (var i = 0; i < voiceCount; i++)
                {
                    var voice = new Voice();
                    var startPos = stream.Position;
                    stream.ReadInt32(); // It always supposed to be 0
                    voice.FrameStart = stream.ReadInt16();
                    stream.ReadByte();
                    voice.Name = ReadCStyleString(stream);
                    Voices.Add(voice);
                    stream.Position = startPos + 0x20;
                }
            }

            public override string ToString() =>
                $"Voices:{string.Join("\n\t", Voices)}";

            public static IEventEntry Read(Stream stream) => new EntryVoices(stream);
        }

        public class EntryCameraTimeline : IEventEntry // ignored
        {
            public int CameraId { get; set; }
            public int FrameStart { get; set; }
            public int FrameEnd { get; set; }
            public int Unk06 { get; set; }

            private EntryCameraTimeline(Stream stream)
            {
                CameraId = stream.ReadInt16();
                FrameStart = stream.ReadInt16();
                FrameEnd = stream.ReadInt16();
                Unk06 = stream.ReadInt16();
            }

            public override string ToString() =>
                $"CameraTimeline: CameraID {CameraId}, Frame start {FrameStart}, Frame end {FrameEnd}, {Unk06}";

            public static IEventEntry Read(Stream stream) => new EntryCameraTimeline(stream);
        }

        public class SetEndFrame : IEventEntry // sub_22D3B8
        {
            public int EndFrame { get; set; } // dword_35DE28

            private SetEndFrame(Stream stream)
            {
                EndFrame = stream.ReadInt16();
                stream.ReadInt16();
            }

            public override string ToString() =>
                $"End frame: {EndFrame}";

            public static IEventEntry Read(Stream stream) => new SetEndFrame(stream);
        }

        public class SeqEffect : IEventEntry
        {
            public int FrameStart { get; set; }
            public int FrameLoop { get; set; }
            public int EffectId { get; set; }
            public int PaxId { get; set; }
            public int PaxEntryIndex { get; set; }
            public int EndType { get; set; }
            public int FadeFrame { get; set; }

            private SeqEffect(Stream stream)
            {
                FrameStart = stream.ReadInt16();
                FrameLoop = stream.ReadInt16();
                EffectId = stream.ReadInt16();
                PaxId = stream.ReadInt16();
                PaxEntryIndex = stream.ReadInt16();
                EndType = stream.ReadInt16();
                FadeFrame = stream.ReadInt16();
            }

            public override string ToString() =>
                $"Effect: Frame start {FrameStart}, Frame loop {FrameLoop}, Effect ID {EffectId}, PAX ID {PaxId}, PAX entry index {PaxEntryIndex}, End type {EndType}, Frame fade {FadeFrame}";

            public static IEventEntry Read(Stream stream) => new SeqEffect(stream);
        }

        public class AttachEffect : IEventEntry // unused
        {
            public int FrameStart { get; set; }
            public int FrameEnd { get; set; } // maybe unused?
            public int AttachEffectId { get; set; }
            public short ActorId { get; set; }
            public short BoneIndex { get; set; }
            public short PaxEntryIndex { get; set; }
            public short Type { get; set; }

            private AttachEffect(Stream stream)
            {
                FrameStart = stream.ReadInt16();
                FrameEnd = stream.ReadInt16();
                AttachEffectId = stream.ReadInt16();
                ActorId = stream.ReadInt16();
                BoneIndex = stream.ReadInt16();
                PaxEntryIndex = stream.ReadInt16();
                Type = stream.ReadInt16();
            }

            public override string ToString() =>
                $"Attach effect: Frame start {FrameStart}, Frame end {FrameEnd}, Attach effect ID {AttachEffectId}, ActorID {ActorId}, Bone index {BoneIndex}, PAX entry {PaxEntryIndex}, Type {Type}";

            public static IEventEntry Read(Stream stream) => new AttachEffect(stream);
        }

        public class SetupEvent : IEventEntry // sub_22d358
        {
            private SetupEvent(Stream stream)
            {
            }

            public override string ToString() =>
                $"Setup event";

            public static IEventEntry Read(Stream stream) => new SetupEvent(stream);
        }

        public class SetCameraData : IEventEntry
        {
            private class Header
            {
                [Data] public short Count { get; set; }
                [Data] public short Index { get; set; }
            }

            public class CameraValue
            {
                [Data] public float Speed { get; set; }
                [Data] public float Value { get; set; }
                [Data] public float Unk08 { get; set; }
                [Data] public float Unk0C { get; set; }

                public override string ToString() =>
                    $"Value {Value}, Speed {Speed}, {Unk08}, {Unk0C}";
            }

            public int CameraId { get; set; }
            public List<CameraValue> PositionY { get; set; }
            public List<CameraValue> PositionZ { get; set; }
            public List<CameraValue> LookAtX { get; set; }
            public List<CameraValue> LookAtY { get; set; }
            public List<CameraValue> LookAtZ { get; set; }
            public List<CameraValue> Roll { get; set; }
            public List<CameraValue> FieldOfView { get; set; }
            public List<CameraValue> PositionX { get; set; }

            private SetCameraData(Stream stream)
            {
                CameraId = stream.ReadInt32();
                var headers = Enumerable
                    .Range(0, 8)
                    .Select(x => BinaryMapping.ReadObject<Header>(stream))
                    .ToList();
                var valueCount = headers.Max(x => x.Index + x.Count);
                var values = Enumerable
                    .Range(0, valueCount)
                    .Select(x => BinaryMapping.ReadObject<CameraValue>(stream))
                    .ToList();

                PositionY = AssignValues(headers[0], values);
                PositionZ = AssignValues(headers[1], values);
                LookAtX = AssignValues(headers[2], values);
                LookAtY = AssignValues(headers[3], values);
                LookAtZ = AssignValues(headers[4], values);
                Roll = AssignValues(headers[5], values);
                FieldOfView = AssignValues(headers[6], values);
                PositionX = AssignValues(headers[7], values);
            }

            private static List<CameraValue> AssignValues(Header header, IList<CameraValue> values) =>
                Enumerable.Range(0, header.Count).Select(i => values[header.Index + i]).ToList();

            public override string ToString()
            {
                var sb = new StringBuilder();
                sb.AppendLine($"Camera: ID {CameraId}");
                sb.AppendLine($"\tPositionX: {ToString(PositionY)}");
                sb.AppendLine($"\tPositionY: {ToString(PositionZ)}");
                sb.AppendLine($"\tPositionZ: {ToString(LookAtX)}");
                sb.AppendLine($"\tChannel3: {ToString(LookAtY)}");
                sb.AppendLine($"\tChannel4: {ToString(LookAtZ)}");
                sb.AppendLine($"\tChannel5: {ToString(Roll)}");
                sb.AppendLine($"\tChannel6: {ToString(FieldOfView)}");
                sb.AppendLine($"\tChannel7: {ToString(PositionX)}");
                return sb.ToString();
            }

            private string ToString(IList<CameraValue> values)
            {
                if (values.Count == 1)
                    return values[0].ToString();
                return string.Join("\n\t\t", values.Select(x => x.ToString()));
            }

            public static IEventEntry Read(Stream stream) => new SetCameraData(stream);
        }

        public class EntryLoadAssets : IEventEntry
        {
            public int Unk02 { get; set; }
            public int Unk04 { get; set; }
            public int Unk06 { get; set; }
            public List<IEventLoad> Loads { get; set; }

            private EntryLoadAssets(Stream stream)
            {
                var itemCount = stream.ReadInt16();
                Unk02 = stream.ReadInt16();
                Unk04 = stream.ReadInt16();
                Unk06 = stream.ReadInt16();

                Loads = new List<IEventLoad>();
                for (var i = 0; i < itemCount; i++)
                {
                    var startPosition = stream.Position;
                    var type = stream.ReadInt16();
                    var blockLength = stream.ReadInt16();
                    if (_loadType.TryGetValue(type, out var read))
                        Loads.Add(read(stream));
                    else
                        Console.Error.WriteLine($"No Load implementation for {type:X02}");
                    stream.Position = startPosition + blockLength;
                }
            }

            public override string ToString() =>
                $"LoadAssets: {string.Join("\n\t", Loads.Select(x => x.ToString()))}";

            public static IEventEntry Read(Stream stream) => new EntryLoadAssets(stream);
        }

        public class EntryUnk2A : IEventEntry // sub_232A48
        {
            public int Unk00 { get; set; }
            public int Unk02 { get; set; }
            public int Unk04 { get; set; }

            private EntryUnk2A(Stream stream)
            {
                Unk00 = stream.ReadInt16();
                Unk02 = stream.ReadInt16();
                Unk04 = stream.ReadInt16();
            }

            public override string ToString() =>
                $"Unk2A: {Unk00} {Unk02} {Unk04}";

            public static IEventEntry Read(Stream stream) => new EntryUnk2A(stream);
        }

        public class EntryPlayBgm : IEventEntry
        {
            public int Unk00 { get; set; }
            public int Unk02 { get; set; }
            public int Unk04 { get; set; }
            public int Unk06 { get; set; }
            public int FrameStart { get; set; }
            public int Unk0A { get; set; }

            private EntryPlayBgm(Stream stream)
            {
                Unk00 = stream.ReadInt16();
                Unk02 = stream.ReadInt16();
                Unk04 = stream.ReadInt16();
                Unk06 = stream.ReadInt16();
                FrameStart = stream.ReadInt16();
                Unk0A = stream.ReadInt16();
            }

            public override string ToString() =>
                $"PlayBGM: {Unk00}, {Unk02}, {Unk04}, {Unk06}, Frame {FrameStart}, {Unk0A}";

            public static IEventEntry Read(Stream stream) => new EntryPlayBgm(stream);
        }

        public class EntryRunAnimation : IEventEntry
        {
            public int FrameStart { get; set; }
            public int FrameEnd { get; set; }
            public int Unk06 { get; set; }
            public int Unk08 { get; set; }
            public int ActorId { get; set; }
            public int Unk0C { get; set; }
            public string Path { get; set; }

            private EntryRunAnimation(Stream stream)
            {
                FrameStart = stream.ReadInt16();
                FrameEnd = stream.ReadInt32();
                Unk06 = stream.ReadInt16();
                Unk08 = stream.ReadInt16();
                ActorId = stream.ReadInt16();
                Unk0C = stream.ReadInt16();
                Path = ReadCStyleString(stream);
            }

            public override string ToString() =>
                $"RunAnimation: Frame start {FrameStart}, Frame end {FrameEnd}, {Unk06}, {Unk08}, ActorID {ActorId}, {Unk0C}, {Path}";

            public static IEventEntry Read(Stream stream) => new EntryRunAnimation(stream);
        }

        public class EntryDialog : IEventEntry
        {
            public int FrameIndex { get; set; }
            public int Unk02 { get; set; }
            public int MessageId { get; set; }
            public int Unk06 { get; set; }
            public int Unk08 { get; set; }

            private EntryDialog(Stream stream)
            {
                FrameIndex = stream.ReadInt16();
                Unk02 = stream.ReadInt16();
                MessageId = stream.ReadInt16();
                Unk06 = stream.ReadInt16();
                Unk08 = stream.ReadInt16();
            }

            public override string ToString() =>
                $"Dialog: Frame index {FrameIndex}, {Unk02}, MsgID {MessageId}, {Unk06}, {Unk08}";

            public static IEventEntry Read(Stream stream) => new EntryDialog(stream);
        }

        public class EntryUnk2E : IEventEntry
        {
            public int Unk00 { get; set; }
            public int Unk02 { get; set; }
            public int Unk04 { get; set; }

            private EntryUnk2E(Stream stream)
            {
                Unk00 = stream.ReadInt16();
                Unk02 = stream.ReadInt16();
                Unk04 = stream.ReadInt16();
            }

            public override string ToString() =>
                $"Unk2E: {Unk00} {Unk02} {Unk04}";

            public static IEventEntry Read(Stream stream) => new EntryUnk2E(stream);
        }

        public class EntryUnk2F : IEventEntry
        {
            public int Unk00 { get; set; }
            public int Unk02 { get; set; }

            private EntryUnk2F(Stream stream)
            {
                Unk00 = stream.ReadInt16();
                Unk02 = stream.ReadInt16();
            }

            public override string ToString() =>
                $"Unk2F: {Unk00} {Unk02}";

            public static IEventEntry Read(Stream stream) => new EntryUnk2F(stream);
        }

        public class EntryUnk30 : IEventEntry
        {
            public int Unk00 { get; set; }
            public int Unk02 { get; set; }
            public int Unk04 { get; set; }

            private EntryUnk30(Stream stream)
            {
                Unk00 = stream.ReadInt16();
                Unk02 = stream.ReadInt16();
                Unk04 = stream.ReadInt16();
            }

            public override string ToString() =>
                $"Unk30: {Unk00} {Unk02} {Unk04}";

            public static IEventEntry Read(Stream stream) => new EntryUnk30(stream);
        }

        public class LoadObject : IEventLoad
        {
            public int ObjectId { get; set; }
            public int ActorId { get; set; }
            public string Name { get; set; }

            private LoadObject(Stream stream)
            {
                ObjectId = stream.ReadInt16();
                ActorId = stream.ReadInt16();
                Name = ReadCStyleString(stream);
            }

            public override string ToString() =>
                $"Object: ObjectEntry {ObjectId:X04}, Name {Name}, ActorID {ActorId}";

            public static IEventLoad Read(Stream stream) => new LoadObject(stream);
        }

        public class LoadPax : IEventLoad
        {
            public int Id { get; set; }
            public string Name { get; set; }

            private LoadPax(Stream stream)
            {
                Id = stream.ReadInt16();
                Name = ReadCStyleString(stream);
            }

            public override string ToString() =>
                $"PAX: Id {Id}, Name {Name}";

            public static IEventLoad Read(Stream stream) => new LoadPax(stream);
        }

        public class LoadAnimation : IEventLoad
        {
            public int ObjectId { get; set; }
            public int ActorId { get; set; }
            public int UnknownIndex { get; set; }
            public string Name { get; set; }

            private LoadAnimation(Stream stream)
            {
                ObjectId = stream.ReadInt16();
                ActorId = stream.ReadInt16();
                UnknownIndex = stream.ReadInt16();
                Name = ReadCStyleString(stream);
            }

            public override string ToString() =>
                $"Animation: ObjectEntry {ObjectId:X04}, ActorID {ActorId}, Unk? {UnknownIndex}, Path {Name}";

            public static IEventLoad Read(Stream stream) => new LoadAnimation(stream);
        }

        public class LoadVoice : IEventLoad
        {
            public string Name { get; set; }

            private LoadVoice(Stream stream)
            {
                Name = ReadCStyleString(stream);
            }

            public override string ToString() =>  $"Voice {Name}";

            public static IEventLoad Read(Stream stream) => new LoadVoice(stream);
        }

        public static List<IEventEntry> Read(Stream stream)
        {
            var entries = new List<IEventEntry>();

            int blockLength;
            while ((blockLength = stream.ReadInt16()) > 0)
            {
                var startPosition = stream.Position - 2;
                var type = stream.ReadInt16();
                if (_entryType.TryGetValue(type, out var read))
                    entries.Add(read(stream));
                else
                    Console.Error.WriteLine($"No Event implementation for {type:X02}");

                if (stream.Position != startPosition + blockLength)
                    stream.Position = stream.Position;
                stream.Position = startPosition + blockLength;
            }

            var str = string.Join("\n", entries.Select(x => x.ToString()));
            return entries;
        }

        private static string ReadCStyleString(Stream stream)
        {
            var sb = new StringBuilder();
            while (stream.Position < stream.Length)
            {
                var ch = stream.ReadByte();
                if (ch == 0)
                    break;

                sb.Append((char)ch);
            }

            return sb.ToString();
        }
    }
}
