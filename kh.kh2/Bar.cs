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
			reader.ReadInt32();
			reader.ReadInt32();

			var buffer = new byte[4];
			var tmpEntries = new List<(Entry, int, int)>(filesCount);

			for (int i = 0; i < filesCount; i++)
			{
				var type = (EntryType)reader.ReadUInt16();
				var index = reader.ReadInt16();

				reader.Read(buffer, 0, 4);
				var name = Encoding.UTF8.GetString(buffer);

				if (filter(name, type))
				{
					tmpEntries.Add((new Entry()
					{
						Type = type,
						Index = index,
						Name = name
					},
					reader.ReadInt32(), // offset
					reader.ReadInt32())); // size
				}
			}

			var entries = new List<Entry>(filesCount);
			for (int i = 0; i < filesCount; i++)
			{
				var e = tmpEntries[i];

				if (loadStream)
				{
					var length = e.Item3;
					e.Item1.Stream = new MemoryStream(length);
					reader.BaseStream.Position = e.Item2;

					if (length > 0)
					{
						reader.BaseStream.CopyTo(e.Item1.Stream, length: length);
					}
				}

				entries.Add(e.Item1);
			}

			return entries;
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
	}
}
