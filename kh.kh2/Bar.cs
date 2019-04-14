using kh.common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Xe;

namespace kh.kh2
{
	public static class Bar
	{
		private const uint MagicCode = 0x01524142U;

		public enum EntryType
		{
			Dummy = 0x00,
			Msg = 0x02,
			Ai = 0x03,
			Tim2 = 0x07,
			Bar = 0x11,
			Pax = 0x12,
			Imgd = 0x18,
			Seqd = 0x19,
			Imgz = 0x1d,
			Seb = 0x1f,
			Wd = 0x20,
            Vibration = 0x2f,
			Vag = 0x30,
		}

		public class Entry
		{
			public EntryType Type { get; set; }

			public int Index { get; set; }

			public string Name { get; set; }

			public Stream Stream { get; set; }
		}

		private static List<Entry> Open(Stream stream, Func<string, EntryType, bool> filter, bool loadStream)
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

                    return new Entry
                    {
                        Type = x.Type,
                        Index = x.Index,
                        Name = x.Name,
                        Stream = fileStream
                    };
                })
                .ToList();
		}

		public static IEnumerable<Entry> Open(Stream stream)
		{
			return Open(stream, (name, type) => true);
		}

		public static IEnumerable<Entry> Open(Stream stream, Func<string, EntryType, bool> filter)
		{
			return Open(stream, filter, true);
		}

		public static int Count(Stream stream, Func<string, EntryType, bool> filter)
		{
			return Open(stream, filter, false).Count;
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

        public static bool IsValid(Stream stream) => new BinaryReader(stream).PeekInt32() == MagicCode;

    }
}
