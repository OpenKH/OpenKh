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

		public enum EntryType
		{
			Dummy = 0,
			Msg = 2,
			Ai = 3,
			Tim2 = 7,
            SpawnPoint = 12,
            SpawnScript = 13,
			Bar = 17,
			Pax = 18,
            AnimationLoader = 22,
			Imgd = 24,
			Seqd = 25,
            Layout = 28,
            Imgz = 29,
			Seb = 31,
			Wd = 32,
            RawBitmap = 36,
            Vibration = 47,
			Vag = 48,
		}

		public class Entry
		{
			public EntryType Type { get; set; }

			public int Index { get; set; }

			public string Name { get; set; }

			public Stream Stream { get; set; }
        }

        public static List<Entry> Open(Stream stream, Func<string, EntryType, bool> filter)
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
                        Stream = fileStream
                    };
                })
                .ToList();
        }

        public static List<Entry> Open(Stream stream)
		{
			return Open(stream, (name, type) => true);
		}

		public static int Count(Stream stream, Func<string, EntryType, bool> filter)
		{
			return Open(stream, filter).Count;
		}

		public static void Save(Stream stream, IEnumerable<Entry> entries)
		{
			if (!stream.CanWrite || !stream.CanSeek)
				throw new InvalidDataException($"Write or seek must be supported.");

			var writer = new BinaryWriter(stream);
			var entriesCount = entries.Count();

			writer.Write(MagicCode);
			writer.Write(entriesCount);
			writer.Write(0);
			writer.Write(0);

			int offset = 0x10 + entriesCount * 0x10;
			foreach (var entry in entries)
			{
				var normalizedName = entry.Name ?? "xxxx";
				if (normalizedName.Length < 4)
					normalizedName = $"{entry.Name}\0\0\0\0";
				else if (normalizedName.Length > 4)
					normalizedName = entry.Name.Substring(0, 4);

				writer.Write((ushort)entry.Type);
				writer.Write((ushort)entry.Index);
				writer.Write(Encoding.UTF8.GetBytes(normalizedName), 0, 4);
				writer.Write(offset);
				writer.Write((int)entry.Stream.Length);

				offset += (int)entry.Stream.Length;
			}

			foreach (var entry in entries)
			{
				entry.Stream.Position = 0;
				entry.Stream.CopyTo(writer.BaseStream);
			}
		}

        public static bool IsValid(Stream stream) =>
            stream.Length >= 4 && new BinaryReader(stream).PeekInt32() == MagicCode;

    }
}
