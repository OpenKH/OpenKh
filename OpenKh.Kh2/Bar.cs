using OpenKh.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace OpenKh.Kh2
{
	public class Bar : List<Bar.Entry>
	{
		private const uint MagicCode = 0x01524142U;
        private const int HeaderSize = 0x10;
        private const int EntrySize = 0x10;

        private static Dictionary<EntryType, int> _alignments = new Dictionary<EntryType, int>
        {
            [EntryType.Model] = 0x10,
            [EntryType.ModelTexture] = 0x80,
            [EntryType.Motion] = 0x10,
            [EntryType.PS2Image] = 0x40,
            [EntryType.CameraOctalTree] = 0x10,
            [EntryType.CollisionOctalTree] = 0x10,
            [EntryType.ColorOctalTree] = 0x10,
            [EntryType.AnimationBinary] = 0x10,
            [EntryType.PAX] = 0x10,
            [EntryType.MapCollision2] = 0x10,
            [EntryType.Motionset] = 0x10,
            [EntryType.ImageData] = 0x10,
            [EntryType.SequenceData] = 0x10,
            [EntryType.LayoutData] = 0x10,
            [EntryType.ImageZip] = 0x10,
            [EntryType.AnimationMap] = 0x10,
            [EntryType.SeBlock] = 0x40,
            [EntryType.InstrumentData] = 0x40,
            [EntryType.IopVoice] = 0x40,
            [EntryType.RawBitmap] = 0x80,
            [EntryType.MemoryCard] = 0x40,
            [EntryType.WrappedCollisionData] = 0x10,
            [EntryType.UNK39] = 0x10,
            [EntryType.Minigame] = 0x10,
            [EntryType.Progress] = 0x10,
            [EntryType.BarUnknown] = 0x10,
            [EntryType.SonyADPCM] = 0x10,
        };

        public enum EntryType
		{
            DUMMY,
            Binary,
            List,
            BDX,
            Model,
            DrawOctalTree,
            CollisionOctalTree,
            ModelTexture,
            DPX,
            Motion,
            PS2Image,
            CameraOctalTree,
            AreaDataSpawn,
            AreaDataScript,
            FogColor,
            ColorOctalTree,
            MotionTriggers,
            AnimationBinary,
            PAX,
            MapCollision2,
            Motionset,
            BgObjPlacement,
            Event,
            ModelCollision,
            ImageData,
            SequenceData,
            UNK26,
            UN27K,
            LayoutData,
            ImageZip,
            AnimationMap,
            SeBlock,
            InstrumentData,
            UNK33,
            IopVoice,
            UNK35,
            RawBitmap,
            MemoryCard,
            WrappedCollisionData,
            UNK39,
            UNK40,
            UNK41,
            Minigame,
            JiminyData,
            Progress,
            Synthesis,
            BarUnknown,
            Vibration,
            SonyADPCM
        }

        public enum MotionsetType
        {
            Default,
            Player,
            Raw
        }

		public class Entry
		{
			public EntryType Type { get; set; }

			public bool Duplicate { get; set; }

			public string Name { get; set; }

            public int Offset { get; set; }

            public Stream Stream { get; set; }
        }

        public MotionsetType Motionset { get; set; }

        public class BarContainer
        {
            /// <summary>
            /// Used by p_ex msets.
            /// </summary>
            public int Flags = 0;

            public List<Entry> Entries { get; set; } = new List<Entry>();
        }

        public static Bar Read(Stream stream, Func<string, EntryType, bool> predicate)
        {
            if (!stream.CanRead || !stream.CanSeek)
                throw new InvalidDataException($"Read or seek must be supported.");

            var reader = new BinaryReader(stream);
            if (stream.Length < 16L || reader.ReadUInt32() != MagicCode)
                throw new InvalidDataException("Invalid header");

            int entryCount = reader.ReadInt32();
            reader.ReadInt32(); // always zero
            
            var motionsetType = (MotionsetType)reader.ReadInt32();
            var binarc = new Bar()
            {
                Motionset = motionsetType
            };

            binarc.AddRange(Enumerable.Range(0, entryCount)
                .Select(x => new
                {
                    Type = (EntryType)reader.ReadUInt16(),
                    Duplicate = reader.ReadInt16() == 1 ? true : false,
                    Name = Encoding.UTF8.GetString(reader.ReadBytes(4)),
                    Offset = reader.ReadInt32(),
                    Size = reader.ReadInt32()
                })
                .ToList() // Needs to be consumed
                .Where(x => predicate(x.Name, x.Type))
                .Select(x =>
                {
                    reader.BaseStream.Position = x.Offset;
                    var data = reader.ReadBytes(x.Size);

                    var fileStream = new MemoryStream();
                    fileStream.Write(data, 0, data.Length);
                    fileStream.Position = 0;

                    var name = x.Name.Split('\0').FirstOrDefault();

                    return new Entry
                    {
                        Type = x.Type,
                        Duplicate = x.Duplicate,
                        Name = name,
                        Offset = x.Offset,
                        Stream = fileStream
                    };
                }));

            return binarc;
        }

        public static Bar Read(Stream stream) => Read(stream, (name, type) => true);

        public static void Write(Stream stream, Bar binarc) =>
            Write(stream, binarc, binarc.Motionset);

        public static void Write(Stream stream, IEnumerable<Entry> entries, MotionsetType motionset = MotionsetType.Default)
		{
			if (!stream.CanWrite || !stream.CanSeek)
				throw new InvalidDataException($"Write or seek must be supported.");

			var writer = new BinaryWriter(stream);
			var entriesCount = entries.Count();

			writer.Write(MagicCode);
			writer.Write(entriesCount);
			writer.Write(0);
			writer.Write((int)motionset);

			var offset = HeaderSize + entriesCount * EntrySize;
            var dicLink = new Dictionary<(string name, EntryType type), (int offset, int length)>();
            var myEntries = entries.ToList();

            foreach (var entry in myEntries)
			{
				var normalizedName = entry.Name ?? "xxxx";
				if (normalizedName.Length < 4)
					normalizedName = $"{entry.Name}\0\0\0\0";
				else if (normalizedName.Length > 4)
					normalizedName = entry.Name.Substring(0, 4);

                offset = Align(offset, entry);

                writer.Write((ushort)entry.Type);
				writer.Write(entry.Duplicate == true ? (ushort)1 : (ushort)0);
				writer.Write(Encoding.UTF8.GetBytes(normalizedName), 0, 4);

                if (entry.Duplicate != false)
                {
                    var linkInfo = dicLink[(entry.Name, entry.Type)];
                    entry.Offset = linkInfo.offset;

                    writer.Write(linkInfo.offset);
                    writer.Write(linkInfo.length);
                }
                else
                {
                    dicLink[(entry.Name, entry.Type)] = (offset, (int)entry.Stream.Length);
                    entry.Offset = offset;

                    writer.Write(offset);
                    writer.Write((int)entry.Stream.Length);

                    offset += (int)entry.Stream.Length;
                }

            }

			foreach (var entry in myEntries)
            {
                writer.BaseStream.Position = entry.Offset;
                if (entry.Duplicate == false)
                {
                    entry.Stream.Position = 0;
                    entry.Stream.CopyTo(writer.BaseStream);
                }
            }
		}

        private static int Align(int offset, Entry entry)
        {
            if (!_alignments.TryGetValue(entry.Type, out var alignment))
            {
                var stream = entry.Stream;
                var magicCode = stream.Length >= 4 ?
                    stream.SetPosition(0).ReadUInt32() : 0;

                alignment = magicCode == MagicCode ? 0x80 : 4;
            }

            return Helpers.Align(offset, alignment);
        }

        public static bool IsValid(Stream stream) =>
            stream.Length >= 4 && new BinaryReader(stream).PeekInt32() == MagicCode;

    }
}
