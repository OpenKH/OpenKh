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
            [EntryType.Tim2] = 0x40,
            [EntryType.CameraCollision] = 0x10,
            [EntryType.MapCollision] = 0x10,
            [EntryType.LightData] = 0x10,
            [EntryType.Anb] = 0x10,
            [EntryType.Pax] = 0x10,
            [EntryType.MapCollision2] = 0x10,
            [EntryType.Motionset] = 0x10,
            [EntryType.Imgd] = 0x10,
            [EntryType.Seqd] = 0x10,
            [EntryType.Layout] = 0x10,
            [EntryType.Imgz] = 0x10,
            [EntryType.AnimationMap] = 0x10,
            [EntryType.Seb] = 0x40,
            [EntryType.Wd] = 0x40,
            [EntryType.IopVoice] = 0x40,
            [EntryType.RawBitmap] = 0x80,
            [EntryType.MemoryCard] = 0x40,
            [EntryType.WrappedCollisionData] = 0x10,
            [EntryType.Unknown39] = 0x10,
            [EntryType.Minigame] = 0x10,
            [EntryType.Progress] = 0x10,
            [EntryType.BarUnknown] = 0x10,
            [EntryType.Vag] = 0x10,
        };

        public enum EntryType
		{
			Dummy = 0,
			Binary = 1,
			List = 2,
			Bdx = 3,
			Model = 4,
            MeshOcclusion = 5,
            MapCollision = 6,
			ModelTexture = 7,
			Dpx = 8,
			Motion = 9,
			Tim2 = 10,
            CameraCollision = 11,
            SpawnPoint = 12,
            SpawnScript = 13,
            FogColor = 14,
            LightData = 15,
            MotionTriggers = 16,
			Anb = 17,
			Pax = 18,
            MapCollision2 = 19,
            Motionset = 20,
            BgObjPlacement = 21,
            AnimationLoader = 22,
            ModelCollision = 23,
			Imgd = 24,
			Seqd = 25,
            Layout = 28,
            Imgz = 29,
			AnimationMap = 30,
			Seb = 31,
			Wd = 32,
			Unknown33,
			IopVoice = 34,
            RawBitmap = 36,
            MemoryCard = 37,
            WrappedCollisionData = 38,
            Unknown39,
            Unknown40,
            Unknown41,
            Minigame = 42,
            JimiData,
            Progress = 44,
            Synthesis,
            BarUnknown = 46,
            Vibration = 47,
			Vag = 48,
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

			public int Index { get; set; }

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
                    Index = reader.ReadInt16(),
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
                        Index = x.Index,
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
				writer.Write((ushort)entry.Index);
				writer.Write(Encoding.UTF8.GetBytes(normalizedName), 0, 4);

                if (entry.Index != 0)
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
                if (entry.Index == 0)
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
