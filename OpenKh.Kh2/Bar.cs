using OpenKh.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace OpenKh.Kh2
{
	public static class Bar
	{
		private const uint MagicCode = 0x01524142U;
        private const int HeaderSize = 0x10;
        private const int EntrySize = 0x10;

        private static Dictionary<EntryType, int> _alignmentsByType = new Dictionary<EntryType, int>
        {
            [EntryType.Vif] = 0x10,
            [EntryType.Tim2] = 0x80,
            [EntryType.Texture] = 0x40,
            [EntryType.CameraCollision] = 0x10,
            [EntryType.MapCollision] = 0x10,
            [EntryType.LightData] = 0x10,
            [EntryType.Bar] = 0x10,
            [EntryType.Pax] = 0x10,
            [EntryType.AnimationLimit] = 0x10,
            [EntryType.Imgd] = 0x10,
            [EntryType.Seqd] = 0x10,
            [EntryType.Layout] = 0x10,
            [EntryType.AnimationMap] = 0x10,
            [EntryType.Seb] = 0x40,
            [EntryType.Wd] = 0x40,
            [EntryType.IopVoice] = 0x40,
            [EntryType.WrappedCollisionData] = 0x10,
            [EntryType.Minigame] = 0x10,
            [EntryType.BarUnknown] = 0x10,
        };

        public enum EntryType
		{
			Dummy = 0,
			Binary = 2,
			Ai = 3,
			Vif = 4,
            MapCollision = 6,
			Tim2 = 7,
			Texture = 10,
            CameraCollision = 11,
            SpawnPoint = 12,
            SpawnScript = 13,
            LightData = 15,
			Bar = 17,
			Pax = 18,
            AnimationLimit = 20,
            AnimationLoader = 22,
			Imgd = 24,
			Seqd = 25,
            Layout = 28,
            Imgz = 29,
			AnimationMap = 30,
			Seb = 31,
			Wd = 32,
			IopVoice = 34,
            RawBitmap = 36,
            WrappedCollisionData = 38,
            Minigame = 42,
            BarUnknown = 46,
            Vibration = 47,
			Vag = 48,
		}

		public class Entry
		{
			public EntryType Type { get; set; }

			public int Index { get; set; }

			public string Name { get; set; }

            public int Offset { get; set; }

            public Stream Stream { get; set; }
        }

        public static List<Entry> Read(Stream stream, Func<string, EntryType, bool> filter)
        {
            if (!stream.CanRead || !stream.CanSeek)
                throw new InvalidDataException($"Read or seek must be supported.");

            var reader = new BinaryReader(stream);
            if (stream.Length < 16L || reader.ReadUInt32() != MagicCode)
                throw new InvalidDataException("Invalid header");

            int filesCount = reader.ReadInt32();
            reader.ReadInt32(); // padding
            reader.ReadInt32(); // padding

            return Enumerable.Range(0, filesCount)
                .Select(x => new
                {
                    Type = (EntryType)reader.ReadUInt16(),
                    Index = reader.ReadInt16(),
                    Name = Encoding.UTF8.GetString(reader.ReadBytes(4)),
                    Offset = reader.ReadInt32(),
                    Size = reader.ReadInt32()
                })
                .ToList() // Needs to be consumed
                .Where(x => filter(x.Name, x.Type))
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
                        Index = x.Index,
                        Name = name,
                        Offset = x.Offset,
                        Stream = fileStream
                    };
                })
                .ToList();
        }

        public static List<Entry> Read(Stream stream)
		{
			return Read(stream, (name, type) => true);
		}

		public static int Count(Stream stream, Func<string, EntryType, bool> filter)
		{
			return Read(stream, filter).Count;
		}

		public static void Write(Stream stream, IEnumerable<Entry> entries)
		{
			if (!stream.CanWrite || !stream.CanSeek)
				throw new InvalidDataException($"Write or seek must be supported.");

			var writer = new BinaryWriter(stream);
			var entriesCount = entries.Count();

			writer.Write(MagicCode);
			writer.Write(entriesCount);
			writer.Write(0);
			writer.Write(0);

            var entryOffsets = new int[entriesCount];

			var offset = HeaderSize + entriesCount * EntrySize;
            var myEntries = entries.Select((Entry, Index) => new { Entry, Index });
            foreach (var e in myEntries)
			{
                var entry = e.Entry;
				var normalizedName = entry.Name ?? "xxxx";
				if (normalizedName.Length < 4)
					normalizedName = $"{entry.Name}\0\0\0\0";
				else if (normalizedName.Length > 4)
					normalizedName = entry.Name.Substring(0, 4);

                offset = Align(offset, entry);

                writer.Write((ushort)entry.Type);
				writer.Write((ushort)entry.Index);
				writer.Write(Encoding.UTF8.GetBytes(normalizedName), 0, 4);

                if (entry.Index != 0)
                {
                    var linkIndex = myEntries
                        .First(x => x.Entry.Offset == entry.Offset).Index;

                    var linkOffset = entryOffsets[linkIndex];
                    writer.Write(linkOffset);
                    entryOffsets[e.Index] = linkOffset;
                }
                else
                {
				    writer.Write(offset);
                    entryOffsets[e.Index] = offset;
                    offset += (int)entry.Stream.Length;
                }

				writer.Write((int)entry.Stream.Length);
            }

            var entryOffsetIndex = 0;
            Entry lastWrittenEntry = null;
			foreach (var entry in entries)
            {
                writer.BaseStream.Position = entryOffsets[entryOffsetIndex++];
                if (entry.Index == 0)
                {
                    entry.Stream.Position = 0;
                    entry.Stream.CopyTo(writer.BaseStream);
                    lastWrittenEntry = entry;
                }
            }
		}

        private static int Align(int offset, Entry entry)
        {
            if (!_alignmentsByType.TryGetValue(entry.Type, out var alignment))
            {
                var stream = entry.Stream;
                var magicCode = stream.Length >= 4 ?
                    stream.SetPosition(0).ReadUInt32() : 0;

                alignment = magicCode == MagicCode ? 0x80 : 4;
            }

            return Align(offset, alignment);
        }

        private static int Align(int offset, int alignment)
        {
            var misalignment = offset % alignment;
            return misalignment > 0 ? offset + alignment - misalignment : offset;
        }

        public static bool IsValid(Stream stream) =>
            stream.Length >= 4 && new BinaryReader(stream).PeekInt32() == MagicCode;

    }
}
