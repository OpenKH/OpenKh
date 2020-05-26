using OpenKh.Common;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xe.IO;

namespace OpenKh.Kh2
{
    public class Imgz
	{
		private struct Entry
		{
			public int Offset, Length;
		}

		private const uint MagicCode = 0x5A474D49U;

        public IEnumerable<Imgd> Images { get; }

        public Imgz(Stream stream)
        {
            Images = Read(stream);
        }

        public static IEnumerable<Stream> OpenAsStream(Stream stream)
        {
            if (!stream.CanRead || !stream.CanSeek)
                throw new InvalidDataException($"Read or seek must be supported.");

            var reader = new BinaryReader(stream);
            var offsetBase = reader.BaseStream.Position;

            if (stream.Length < 16L || reader.ReadUInt32() != MagicCode)
                throw new InvalidDataException("Invalid header");

            var unknown = reader.ReadInt32();
            var entriesOffset = reader.ReadInt32();
            var count = reader.ReadInt32();

            stream.Position = entriesOffset;
            var entries = new List<Entry>(count);
            for (int i = 0; i < count; i++)
            {
                entries.Add(new Entry()
                {
                    Offset = reader.ReadInt32(),
                    Length = reader.ReadInt32(),
                });
            }

            return entries
                .Select(x => new SubStream(stream, x.Offset, x.Length))
                .ToList();
        }

        public static bool IsValid(Stream stream) =>
            stream.Length >= 4 && stream.SetPosition(0).ReadInt32() == MagicCode;

        public static IEnumerable<Imgd> Read(Stream stream) =>
            OpenAsStream(stream.SetPosition(0)).Select(x => Imgd.Read(x)).ToArray();


        public static void Write(Stream stream, IEnumerable<Imgd> images)
		{
			if (!stream.CanWrite)
				throw new InvalidDataException($"Read or seek must be supported.");

			var writer = new BinaryWriter(stream);
			var count = images.Count();

			writer.Write(MagicCode);
			writer.Write(0x100);
			writer.Write(0x10); // Header size;
			writer.Write(count);

			int baseOffset = (int)(stream.Position + count * 8);

			// Align by 0x10
			if ((baseOffset & 0xF) != 0)
			{
				baseOffset += 0x10 - (baseOffset & 0xF);
			}

			int currentOffset = baseOffset;

			var imgStreams = new List<MemoryStream>(count);
			foreach (var image in images)
			{
				var memStream = new MemoryStream();
				image.Write(memStream);
				imgStreams.Add(memStream);

				writer.Write(currentOffset);
				writer.Write((int)memStream.Length);

				currentOffset += (int)memStream.Length;
			}

			stream.Position = baseOffset;
			foreach (var imgStream in imgStreams)
			{
				imgStream.Position = 0;
				imgStream.WriteTo(stream);
			}
		}
    }
}
