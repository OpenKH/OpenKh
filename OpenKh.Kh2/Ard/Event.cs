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
        private const short Terminator = 0;
        private static readonly IBinaryMapping Mapping =
            MappingConfiguration.DefaultConfiguration()
                .ForType<string>(CStringReader, CStringWriter)
                .ForType<SeqVoices>(ReadVoices, WriteVoices)
                .ForType<SetCameraData>(ReadSetCameraData, WriteSetCameraData)
                .ForType<ReadAssets>(ReadLoadAssets, WriteLoadAssets)
                .Build();

        private static readonly Dictionary<int, Type> _idType = new Dictionary<int, Type>()
        {
            [0x00] = typeof(SetProject),
            [0x01] = typeof(SetActor),
            [0x02] = typeof(SeqActorPosition),
            [0x03] = typeof(SetMap),
            [0x06] = typeof(SeqCamera),
            [0x08] = typeof(SetEndFrame),
            [0x09] = typeof(SeqEffect),
            [0x0A] = typeof(AttachEffect),
            [0x0C] = typeof(Unk0C),
            [0x0D] = typeof(Unk0D),
            [0x0F] = typeof(SetupEvent),
            [0x10] = typeof(EventStart),
            [0x12] = typeof(SeqFade),
            [0x13] = typeof(SetCameraData),
            [0x14] = typeof(EntryUnk14),
            [0x15] = typeof(SeqSubtitle),
            [0x16] = typeof(EntryUnk16),
            [0x17] = typeof(EntryUnk17),
            [0x18] = typeof(EntryUnk18),
            [0x19] = typeof(SeqTextureAnim),
            [0x1A] = typeof(EntryUnk1A),
            [0x1B] = typeof(SeqCrossFade),
            [0x20] = typeof(SeqGameSpeed),
            [0x22] = typeof(EntryUnk22),
            [0x23] = typeof(SeqVoices),
            [0x24] = typeof(ReadAssets),
            [0x25] = typeof(ReadMotion),
            [0x26] = typeof(ReadAudio),
            [0x27] = typeof(SetShake),
            [0x2A] = typeof(EntryUnk2A),
            [0x2B] = typeof(SeqPlayAudio),
            [0x2C] = typeof(SeqPlayAnimation),
            [0x2D] = typeof(SeqDialog),
            [0x2E] = typeof(SeqPlayBgm),
            [0x2F] = typeof(ReadBgm),
            [0x30] = typeof(SetBgm),
            [0x36] = typeof(EntryUnk36),
            [0x38] = typeof(ReadActor),
            [0x39] = typeof(ReadEffect),
            [0x3F] = typeof(StopEffect),
            [0x44] = typeof(RunMovie),
            [0x47] = typeof(EntryUnk47),
            [0x4D] = typeof(SeqHideObject),
        };

        private static readonly Dictionary<Type, int> _typeId =
            _idType.ToDictionary(x => x.Value, x => x.Key);

        private class SetCameraDataHeader
        {
            [Data] public short Index { get; set; }
            [Data] public short Count { get; set; }
        }

        public interface IEventEntry
        {
        }

        public class SetProject : IEventEntry // unused
        {
            [Data] public short Unk00 { get; set; }
            [Data] public short Unk02 { get; set; }

            // Always PS2_BIN_VER (2)
            [Data] public byte Version { get; set; }
            [Data] public byte ObjCameraType { get; set; }

            // Can't be greater than 32 bytes
            [Data] public string Name { get; set; }

            public override string ToString() =>
                $"{typeof(SetProject)}: {Name}, Version {Version}, Camera type {ObjCameraType}, ({Unk00:X}, {Unk02:X})";
        }

        public class SetActor : IEventEntry // sub_22F528
        {
            [Data] public short ObjectEntry { get; set; }
            [Data] public short ActorId { get; set; }
            [Data] public string Name { get; set; }

            public override string ToString() =>
                $"{typeof(SetActor)}: ObjEntry {ObjectEntry:X}, Name {Name}, ActorID {ActorId}";
        }
        
        public class SeqActorPosition : IEventEntry
        {
            [Data] public short Unk00 { get; set; }
            [Data] public short Unk02 { get; set; }
            [Data] public float PositionX { get; set; }
            [Data] public float PositionY { get; set; }
            [Data] public float PositionZ { get; set; }
            [Data] public float RotationX { get; set; }
            [Data] public float RotationY { get; set; }
            [Data] public float RotationZ { get; set; }
            [Data] public float Unk1C { get; set; }
            [Data] public short ActorId { get; set; }
            [Data] public short Frame { get; set; }

            public override string ToString() =>
                $"{typeof(SeqActorPosition)}: Frame {Frame}, ActorID {ActorId}, Pos({PositionX}, {PositionY}, {PositionZ}) Rot({RotationX}, {RotationY}, {RotationZ}) Unk({Unk00}, {Unk02}, {Unk1C})";
        }

        public class SetMap : IEventEntry // unused
        {
            [Data] public short Place { get; set; }
            [Data] public string World { get; set; }

            public override string ToString() =>
                $"{typeof(SetMap)}: {World}{Place:D02}";
        }

        public class SeqCamera : IEventEntry // ignored
        {
            [Data] public short CameraId { get; set; }
            [Data] public short FrameStart { get; set; }
            [Data] public short FrameEnd { get; set; }
            [Data] public short Unk06 { get; set; }

            public override string ToString() =>
                $"{typeof(SeqCamera)}: CameraID {CameraId}, Frame start {FrameStart}, Frame end {FrameEnd}, {Unk06}";
        }

        public class SetEndFrame : IEventEntry // sub_22D3B8
        {
            [Data] public short EndFrame { get; set; } // dword_35DE28
            [Data] public short Unused { get; set; }

            public override string ToString() =>
                $"{typeof(SetEndFrame)}: {EndFrame}";
        }

        public class SeqEffect : IEventEntry
        {
            [Data] public short FrameStart { get; set; }
            [Data] public short FrameLoop { get; set; }
            [Data] public short EffectId { get; set; }
            [Data] public short PaxId { get; set; }
            [Data] public short PaxEntryIndex { get; set; }
            [Data] public short EndType { get; set; }
            [Data] public short FadeFrame { get; set; }

            public override string ToString() =>
                $"{typeof(SeqEffect)}: Frame start {FrameStart}, Frame loop {FrameLoop}, Effect ID {EffectId}, PAX ID {PaxId}, PAX entry index {PaxEntryIndex}, End type {EndType}, Frame fade {FadeFrame}";
        }

        public class AttachEffect : IEventEntry
        {
            [Data] public short FrameStart { get; set; }
            [Data] public short FrameEnd { get; set; } // maybe unused?
            [Data] public short AttachEffectId { get; set; }
            [Data] public short ActorId { get; set; }
            [Data] public short BoneIndex { get; set; }
            [Data] public short PaxEntryIndex { get; set; }
            [Data] public short Type { get; set; }

            public override string ToString() =>
                $"{typeof(AttachEffect)}: Frame start {FrameStart}, Frame end {FrameEnd}, Attach effect ID {AttachEffectId}, ActorID {ActorId}, Bone index {BoneIndex}, PAX entry {PaxEntryIndex}, Type {Type}";
        }

        public class EventStart : IEventEntry // sub_22D3A8
        {
            [Data] public short FadeIn { get; set; } // dword_35DE40
            [Data] public short Unused { get; set; }

            public override string ToString() =>
                $"{typeof(EventStart)}: Fade in for {FadeIn} frames";
        }

        public class SeqFade: IEventEntry
        {
            public enum FadeType : short
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

            [Data] public short FrameIndex { get; set; }
            [Data] public short Duration { get; set; }
            [Data] public FadeType Type { get; set; }

            public override string ToString() =>
                $"{typeof(SeqFade)}: Frame {FrameIndex}, Duration {Duration}, {Type}";
        }

        public class SetCameraData : IEventEntry
        {
            public class CameraValue
            {
                [Data] public float Speed { get; set; }
                [Data] public float Value { get; set; }
                [Data] public float Unk08 { get; set; }
                [Data] public float Unk0C { get; set; }

                public override string ToString() =>
                    $"Value {Value}, Speed {Speed}, {Unk08}, {Unk0C}";
            }

            public short CameraId { get; set; }
            public List<CameraValue> PositionY { get; set; }
            public List<CameraValue> PositionZ { get; set; }
            public List<CameraValue> LookAtX { get; set; }
            public List<CameraValue> LookAtY { get; set; }
            public List<CameraValue> LookAtZ { get; set; }
            public List<CameraValue> Roll { get; set; }
            public List<CameraValue> FieldOfView { get; set; }
            public List<CameraValue> PositionX { get; set; }

            public override string ToString()
            {
                var sb = new StringBuilder();
                sb.AppendLine($"{typeof(SetCameraData)}: ID {CameraId}");
                sb.AppendLine($"\tPositionX: {ToString(PositionX)}");
                sb.AppendLine($"\tPositionY: {ToString(PositionY)}");
                sb.AppendLine($"\tPositionZ: {ToString(PositionZ)}");
                sb.AppendLine($"\tLookAtX: {ToString(LookAtX)}");
                sb.AppendLine($"\tLookAtY: {ToString(LookAtY)}");
                sb.AppendLine($"\tLookAtZ: {ToString(LookAtZ)}");
                sb.AppendLine($"\tRoll: {ToString(Roll)}");
                sb.AppendLine($"\tFOV: {ToString(FieldOfView)}");
                return sb.ToString();
            }

            private string ToString(IList<CameraValue> values)
            {
                if (values.Count == 1)
                    return values[0].ToString();
                return string.Join("\n\t\t", values.Select(x => x.ToString()));
            }

        }

        public class EntryUnk14 : IEventEntry
        {
            [Data] public short Unk00 { get; set; }

            public override string ToString() =>
                $"{typeof(EntryUnk14)}: {Unk00}";
        }

        public class SeqSubtitle : IEventEntry
        {
            [Data] public short FrameStart { get; set; }
            [Data] public short Index { get; set; }
            [Data] public short MessageId { get; set; }
            [Data] public short HideFlag { get; set; }

            public override string ToString() =>
                $"{typeof(SeqSubtitle)}: Frame {FrameStart}, MsgId {MessageId}, Index {Index}, Hide {HideFlag != 0}";
        }

        public class EntryUnk16 : IEventEntry
        {
            [Data] public short Unk00 { get; set; }
            [Data] public short Unk02 { get; set; }

            public override string ToString() =>
                $"{typeof(EntryUnk16)}: {Unk00} {Unk02}";
        }

        public class EntryUnk17 : IEventEntry
        {
            [Data] public short Unk00 { get; set; }
            [Data] public short Unk02 { get; set; }
            [Data] public short Unk04 { get; set; }
            [Data] public short Unk06 { get; set; }
            [Data] public short Unk08 { get; set; }
            [Data] public short Unk0A { get; set; }
            [Data] public short Unk0C { get; set; }
            [Data] public short Unk0E { get; set; }

            public override string ToString() =>
                $"{typeof(EntryUnk17)}: {Unk00}, {Unk02} {Unk04} {Unk06} {Unk08} {Unk0A} {Unk0C} {Unk0E}";
        }

        public class EntryUnk18 : IEventEntry
        {
            [Data] public short Frame { get; set; }
            [Data] public short Unk02 { get; set; }
            [Data] public short Unk04 { get; set; }
            [Data] public short Unk06 { get; set; }

            public override string ToString() =>
                $"{typeof(EntryUnk18)}: Frame {Frame}, {Unk02} {Unk04} {Unk06}";
        }

        public class SeqTextureAnim : IEventEntry
        {
            [Data] public short Frame { get; set; }
            [Data] public short Unk02 { get; set; }
            [Data] public short Unk04 { get; set; }
            [Data] public short Unk06 { get; set; }

            public override string ToString() =>
                $"{typeof(SeqTextureAnim)}: Frame {Frame}, {Unk02}, {Unk04}, {Unk06}";
        }

        public class EntryUnk1A : IEventEntry
        {
            [Data] public short Unk00 { get; set; }
            [Data] public short Unk02 { get; set; }

            public override string ToString() =>
                $"{typeof(EntryUnk1A)}: {Unk00} {Unk02}";
        }

        public class SeqCrossFade : IEventEntry
        {
            [Data] public short Frame { get; set; }
            [Data] public short Duration { get; set; }

            public override string ToString() =>
                $"{typeof(SeqCrossFade)}: {Frame}, {Duration}";
        }

        public class SeqGameSpeed : IEventEntry
        {
            [Data] public short Frame { get; set; }
            [Data] public short Unused { get; set; }
            [Data] public float Speed { get; set; }

            public override string ToString() =>
                $"{typeof(SeqGameSpeed)}: {Frame} {Speed}";
        }

        public class EntryUnk22 : IEventEntry
        {
            [Data] public short Unk00 { get; set; }
            [Data] public short Unk02 { get; set; }

            public override string ToString() =>
                $"{typeof(EntryUnk22)}: {Unk00} {Unk02}";
        }

        public class SeqVoices : IEventEntry
        {
            public class Voice
            {
                public short FrameStart { get; set; }
                public string Name { get; set; }

                public override string ToString() =>
                    $"Frame {FrameStart}, {Name}";
            }

            public List<Voice> Voices { get; set; }

            public override string ToString() =>
                $"{typeof(SeqVoices)}:\n\t{string.Join("\n\t", Voices)}";
        }

        public class Unk0C : IEventEntry
        {
            [Data] public short StartFrame { get; set; }
            [Data] public short EndFrame { get; set; }
            [Data] public short Unk04 { get; set; }
            [Data] public short Unk08 { get; set; }

            public override string ToString() =>
                $"{typeof(Unk0C)}: Frame {StartFrame}, {Unk04}, {Unk08}";
        }

        public class Unk0D : IEventEntry
        {
            [Data] public short StartFrame { get; set; }
            [Data] public short EndFrame { get; set; }
            [Data(Count = 34)] public short[] Unk { get; set; }

            public override string ToString() =>
                $"{typeof(Unk0D)}: Frame {StartFrame}, ({string.Join(", ", Unk)})";
        }

        public class SetupEvent : IEventEntry // sub_22d358
        {
            public override string ToString() =>
                $"{typeof(SetupEvent)}";
        }

        public class ReadAssets : IEventEntry
        {
            public short Unk02 { get; set; }
            public short Unk04 { get; set; }
            public short Unk06 { get; set; }
            public List<IEventEntry> Set { get; set; }

            public override string ToString() =>
                $"{typeof(ReadAssets)}:\n\t{string.Join("\n\t", Set.Select(x => x.ToString()))}";
        }

        public class EntryUnk2A : IEventEntry // sub_232A48
        {
            [Data] public short Unk00 { get; set; }
            [Data] public short Unk02 { get; set; }
            [Data] public short Unk04 { get; set; }

            public override string ToString() =>
                $"{typeof(EntryUnk2A)}: {Unk00} {Unk02} {Unk04}";
        }

        public class SeqPlayAudio : IEventEntry
        {
            [Data] public short Unk00 { get; set; }
            [Data] public short Unk02 { get; set; }
            [Data] public short Unk04 { get; set; }
            [Data] public short Unk06 { get; set; }
            [Data] public short FrameStart { get; set; }
            [Data] public short Unk0A { get; set; }

            public override string ToString() =>
                $"{typeof(SeqPlayAudio)}: {Unk00}, {Unk02}, {Unk04}, {Unk06}, Frame {FrameStart}, {Unk0A}";
        }

        public class SeqPlayAnimation : IEventEntry
        {
            [Data] public short FrameStart { get; set; }
            [Data] public short FrameEnd { get; set; }
            [Data] public short Unk04 { get; set; }
            [Data] public short Unk06 { get; set; }
            [Data] public short Unk08 { get; set; }
            [Data] public short ActorId { get; set; }
            [Data] public short Unk0C { get; set; }
            [Data] public string Path { get; set; }

            public override string ToString() =>
                $"{typeof(SeqPlayAnimation)}: Frame start {FrameStart}, Frame end {FrameEnd}, {Unk04}, {Unk06}, {Unk08}, ActorID {ActorId}, {Unk0C}, {Path}";
        }

        public class SeqDialog : IEventEntry
        {
            [Data] public short FrameIndex { get; set; }
            [Data] public short Unk02 { get; set; }
            [Data] public short MessageId { get; set; }
            [Data] public short Unk06 { get; set; }
            [Data] public short Unk08 { get; set; }

            public override string ToString() =>
                $"{typeof(SeqDialog)}: Frame index {FrameIndex}, {Unk02}, MsgID {MessageId}, {Unk06}, {Unk08}";
        }

        public class SeqPlayBgm : IEventEntry
        {
            [Data] public short Frame { get; set; }
            [Data] public short BankIndex { get; set; }
            [Data] public byte VolumeStartIndex { get; set; }
            [Data] public byte VolumeEndIndex { get; set; }
            [Data] public byte FadeType { get; set; }
            [Data] public byte Unused { get; set; }

            public override string ToString() =>
                $"{typeof(SeqPlayBgm)}: Frame {Frame}, Bank {BankIndex}, Volume start {VolumeStartIndex}, Volume end {VolumeEndIndex}, Fade type {FadeType}";
        }

        public class ReadBgm : IEventEntry
        {
            [Data] public short Unk00 { get; set; }
            [Data] public short BgmId { get; set; }

            public override string ToString() =>
                $"{typeof(ReadBgm)}: BGM ID {BgmId}, {Unk00}";
        }

        public class SetBgm : IEventEntry
        {
            [Data] public short Frame { get; set; }
            [Data] public short BankIndex { get; set; }
            [Data] public short BgmId { get; set; }

            public override string ToString() =>
                $"{typeof(SetBgm)}: BGM ID {BgmId}, {Frame} {BankIndex}";
        }

        public class EntryUnk36 : IEventEntry
        {
            [Data] public short Frame { get; set; }
            [Data] public short Unk02 { get; set; }
            [Data] public float Unk04 { get; set; }
            [Data] public float Unk08 { get; set; }
            [Data] public float Unk0c { get; set; }

            public override string ToString() =>
                $"{typeof(EntryUnk36)}: Frame {Frame}, {Unk02}, {Unk04}, {Unk08}, {Unk0c}";
        }

        public class ReadActor : IEventEntry
        {
            [Data] public short ObjectId { get; set; }
            [Data] public short ActorId { get; set; }
            [Data] public string Name { get; set; }

            public override string ToString() =>
                $"{typeof(ReadActor)}: ObjectEntry {ObjectId:X04}, Name {Name}, ActorID {ActorId}";
        }

        public class ReadEffect : IEventEntry
        {
            [Data] public short Id { get; set; }
            [Data] public string Name { get; set; }

            public override string ToString() =>
                $"{typeof(ReadEffect)}: Id {Id}, Name {Name}";
        }

        public class ReadMotion : IEventEntry
        {
            [Data] public short ObjectId { get; set; }
            [Data] public short ActorId { get; set; }
            [Data] public short UnknownIndex { get; set; }
            [Data] public string Name { get; set; }

            public override string ToString() =>
                $"{typeof(ReadMotion)}: ObjectEntry {ObjectId:X04}, ActorID {ActorId}, Unk? {UnknownIndex}, Path {Name}";
        }

        public class ReadAudio : IEventEntry
        {
            [Data] public string Name { get; set; }

            public override string ToString() => $"{typeof(ReadAudio)} {Name}";
        }

        public class SetShake : IEventEntry
        {
            [Data] public short Frame { get; set; }
            [Data] public short Type { get; set; }
            [Data] public short Width { get; set; }
            [Data] public short Height { get; set; }
            [Data] public short Depth { get; set; }
            [Data] public short Duration { get; set; }

            public override string ToString() =>
                $"{typeof(SetShake)}: Frame {Frame}, Type {Type}, Width {Width}, Height {Height}, Depth {Depth}, Duration {Duration}";
        }

        public class StopEffect : IEventEntry
        {
            [Data] public short Frame { get; set; }
            [Data] public short Unused { get; set; }

            public override string ToString() =>
                $"{typeof(StopEffect)}: Frame {Frame}";
        }

        public class RunMovie : IEventEntry
        {
            [Data] public short Frame { get; set; }
            [Data] public string Name { get; set; }

            public override string ToString() =>
                $"{typeof(RunMovie)}: Frame {Frame}, Name {Name}";
        }

        public class EntryUnk47 : IEventEntry
        {
            [Data] public short Unk00 { get; set; }
            [Data] public short Unk02 { get; set; }
            [Data] public float StartPositionX { get; set; }
            [Data] public float StartPositionY { get; set; }
            [Data] public float StartPositionZ { get; set; }
            [Data] public float EndPositionX { get; set; }
            [Data] public float EndPositionY { get; set; }
            [Data] public float EndPositionZ { get; set; }
            [Data] public float RotationX { get; set; }
            [Data] public float RotationY { get; set; }
            [Data] public float RotationZ { get; set; }
            [Data] public short Unk28 { get; set; }
            [Data] public short Unk2A { get; set; }

            public override string ToString() =>
                $"{typeof(EntryUnk47)}: {Unk00}, {Unk02}, {Unk28}, {Unk2A}, StartPos({StartPositionX}, {StartPositionY}, {StartPositionZ}), EndPos({EndPositionX}, {EndPositionY}, {EndPositionZ}), Rot({RotationX}, {RotationY}, {RotationZ})";
        }

        public class SeqHideObject : IEventEntry
        {
            [Data] public short Frame { get; set; }
            [Data] public short Type { get; set; }

            public override string ToString() =>
                $"{typeof(SeqHideObject)}: Frame {Frame}, Type {Type}";
        }

        public static List<IEventEntry> Read(Stream stream)
        {
            var entries = new List<IEventEntry>();

            int blockLength;
            while (stream.Position + 3 < stream.Length)
            {
                blockLength = stream.ReadInt16();
                if (blockLength == Terminator)
                    break;

                var startPosition = stream.Position - 2;
                var type = stream.ReadInt16();
                if (_idType.TryGetValue(type, out var entryType))
                    entries.Add(Mapping.ReadObject(stream, (IEventEntry)Activator.CreateInstance(entryType)));
                else
                    Console.Error.WriteLine($"No Event implementation for {type:X02}");

                if (stream.Position != startPosition + blockLength)
                    stream.Position = stream.Position;
                stream.Position = startPosition + blockLength;
            }

            var str = string.Join("\n", entries.Select(x => x.ToString()));
            return entries;
        }

        public static void Write(Stream stream,
            IEnumerable<IEventEntry> eventSet)
        {
            foreach (var item in eventSet)
            {
                var startPosition = stream.Position;
                stream.Position += 4;
                Mapping.WriteObject(stream, item);
                var nextPosition = stream.AlignPosition(2).Position;
                
                var id = _typeId[item.GetType()];
                var length = stream.Position - startPosition;
                stream.Position = startPosition;
                stream.Write((short)(length));
                stream.Write((short)id);
                stream.Position = nextPosition;
            }

            stream.Write(Terminator);
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

        private static void WriteCStyleString(Stream stream, string str)
        {
            foreach (var ch in str)
                stream.WriteByte((byte)ch);
            stream.WriteByte(0);
        }

        private static void CStringWriter(MappingWriteArgs args) =>
            WriteCStyleString(args.Writer.BaseStream, (string)args.Item);

        private static object CStringReader(MappingReadArgs args) =>
            ReadCStyleString(args.Reader.BaseStream);

        private static object ReadVoices(MappingReadArgs args)
        {
            var voiceCount = args.Reader.ReadInt32();
            var voices = new List<SeqVoices.Voice>(voiceCount);
            for (var i = 0; i < voiceCount; i++)
            {
                var voice = new SeqVoices.Voice();
                var startPos = args.Reader.BaseStream.Position;
                args.Reader.ReadInt32(); // It always supposed to be 0
                voice.FrameStart = args.Reader.ReadInt16();
                args.Reader.ReadByte();
                voice.Name = ReadCStyleString(args.Reader.BaseStream);
                voices.Add(voice);
                args.Reader.BaseStream.Position = startPos + 0x20;
            }

            return new SeqVoices
            {
                Voices = voices
            };
        }

        private static void WriteVoices(MappingWriteArgs args)
        {
            var item = (SeqVoices)args.Item;
            args.Writer.Write(item.Voices.Count);
            foreach (var voice in item.Voices)
            {
                var startPos = args.Writer.BaseStream.Position;
                args.Writer.Write(0);
                args.Writer.Write(voice.FrameStart);
                args.Writer.Write((byte)0);
                WriteCStyleString(args.Writer.BaseStream, voice.Name);
                args.Writer.BaseStream.Position = startPos + 0x20;
            }

            var endPosition = args.Writer.BaseStream.Position;
            if (endPosition > args.Writer.BaseStream.Length)
                args.Writer.BaseStream.SetLength(endPosition);
        }

        private static object ReadSetCameraData(MappingReadArgs args)
        {
            List<SetCameraData.CameraValue> AssignValues(SetCameraDataHeader header, IList<SetCameraData.CameraValue> values) =>
                Enumerable.Range(0, header.Count).Select(i => values[header.Index + i]).ToList();

            var cameraId = args.Reader.ReadInt16();
            var headers = Enumerable
                .Range(0, 8)
                .Select(x => BinaryMapping.ReadObject<SetCameraDataHeader>(args.Reader.BaseStream))
                .ToList();
            args.Reader.BaseStream.AlignPosition(4);
            var valueCount = headers.Max(x => x.Index + x.Count);
            var values = Enumerable
                .Range(0, valueCount)
                .Select(x => BinaryMapping.ReadObject<SetCameraData.CameraValue>(args.Reader.BaseStream))
                .ToList();

            return new SetCameraData
            {
                CameraId = cameraId,
                PositionX = AssignValues(headers[0], values),
                PositionY = AssignValues(headers[1], values),
                PositionZ = AssignValues(headers[2], values),
                LookAtX = AssignValues(headers[3], values),
                LookAtY = AssignValues(headers[4], values),
                LookAtZ = AssignValues(headers[5], values),
                Roll = AssignValues(headers[6], values),
                FieldOfView = AssignValues(headers[7], values),
            };
        }

        private static void WriteSetCameraData(MappingWriteArgs args)
        {
            void WriteHeader(Stream stream,
                List<SetCameraData.CameraValue> values,
                int startIndex) =>
                Mapping.WriteObject(stream, new SetCameraDataHeader
                {
                    Index = (short)startIndex,
                    Count = (short)values.Count
                });

            void WriteData(Stream stream, List<SetCameraData.CameraValue> values)
            {
                foreach (var value in values)
                    Mapping.WriteObject(stream, value);
            }


            var item = args.Item as SetCameraData;
            args.Writer.Write(item.CameraId);
            if (item.CameraId == 10)
                item = item;

            var channelCountList = new List<int>(8)
            {
                item.PositionX.Count,
                item.PositionY.Count,
                item.PositionZ.Count,
                item.LookAtX.Count,
                item.LookAtY.Count,
                item.LookAtZ.Count,
                item.Roll.Count,
                item.FieldOfView.Count,
            };
            var indexList = new List<int>(8);
            var startIndex = 0;
            foreach (var channelCount in channelCountList)
            {
                indexList.Add(startIndex);
                startIndex += channelCount;
            }

            WriteHeader(args.Writer.BaseStream, item.PositionX, indexList[0]);
            WriteHeader(args.Writer.BaseStream, item.PositionY, indexList[1]);
            WriteHeader(args.Writer.BaseStream, item.PositionZ, indexList[2]);
            WriteHeader(args.Writer.BaseStream, item.LookAtX, indexList[3]);
            WriteHeader(args.Writer.BaseStream, item.LookAtY, indexList[4]);
            WriteHeader(args.Writer.BaseStream, item.LookAtZ, indexList[5]);
            WriteHeader(args.Writer.BaseStream, item.Roll, indexList[6]);
            WriteHeader(args.Writer.BaseStream, item.FieldOfView, indexList[7]);

            args.Writer.BaseStream.AlignPosition(4);
            WriteData(args.Writer.BaseStream, item.PositionX);
            WriteData(args.Writer.BaseStream, item.PositionY);
            WriteData(args.Writer.BaseStream, item.PositionZ);
            WriteData(args.Writer.BaseStream, item.LookAtX);
            WriteData(args.Writer.BaseStream, item.LookAtY);
            WriteData(args.Writer.BaseStream, item.LookAtZ);
            WriteData(args.Writer.BaseStream, item.Roll);
            WriteData(args.Writer.BaseStream, item.FieldOfView);
        }

        private static object ReadLoadAssets(MappingReadArgs args)
        {
            var reader = args.Reader;
            var itemCount = reader.ReadInt16();
            var unk02 = reader.ReadInt16();
            var unk04 = reader.ReadInt16();
            var unk06 = reader.ReadInt16();

            var loadSet = new List<IEventEntry>();
            for (var i = 0; i < itemCount; i++)
            {
                var startPosition = reader.BaseStream.Position;
                var type = reader.ReadInt16();
                var length = reader.ReadInt16();
                if (_idType.TryGetValue(type, out var entryType))
                    loadSet.Add(Mapping.ReadObject(reader.BaseStream,
                        (IEventEntry)Activator.CreateInstance(entryType)));
                else
                    Console.Error.WriteLine($"No Load implementation for {type:X02}");
                reader.BaseStream.Position = startPosition + length;
            }

            return new ReadAssets
            {
                Unk02 = unk02,
                Unk04 = unk04,
                Unk06 = unk06,
                Set = loadSet
            };
        }

        private static void WriteLoadAssets(MappingWriteArgs args)
        {
            var item = args.Item as ReadAssets;
            args.Writer.Write((short)item.Set.Count);
            args.Writer.Write(item.Unk02);
            args.Writer.Write(item.Unk04);
            args.Writer.Write(item.Unk06);

            var stream = args.Writer.BaseStream;
            foreach (var loadItem in item.Set)
            {
                var startPosition = stream.Position;
                stream.Position += 4;
                Mapping.WriteObject(stream, loadItem);
                var nextPosition = stream.AlignPosition(2).Position;

                var id = _typeId[loadItem.GetType()];
                var length = stream.Position - startPosition;
                stream.Position = startPosition;
                stream.Write((short)id);
                stream.Write((short)length);
                stream.Position = nextPosition;
            }
        }
    }
}
