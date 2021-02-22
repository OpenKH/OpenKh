using System.Collections.Generic;
using System.IO;
using System.Linq;

using OpenKh.Common.Utils;

namespace OpenKh.Kh2
{
	public partial class Dpd
	{
		private const uint Version = 0x00000096U;

		public List<int> OffsetPdt { get; set; }
		public List<int> OffsetTextures { get; set; }
		public List<int> Offset3 { get; set; }
		public List<int> Offset4 { get; set; }
		public List<int> Offset5 { get; set; }

		public Dpd(Stream stream)
		{
			if (!stream.CanRead || !stream.CanSeek)
				throw new InvalidDataException($"Read or seek must be supported.");

			var reader = new BinaryReader(stream);
			if (stream.Length < 16L || reader.ReadUInt32() != Version)
				throw new InvalidDataException("Invalid header");

			// dpd_h_init_datainfo
			OffsetPdt = ReadOffsetsList(reader);
			OffsetTextures = ReadOffsetsList(reader);
			Offset3 = ReadOffsetsList(reader);
			Offset4 = ReadOffsetsList(reader);
			Offset5 = ReadOffsetsList(reader);

			// pppInitPdt
			// OffsetPdt, Table 1
			foreach (var offset in OffsetPdt)
			{
				// 9648E0
				pppInitPdt(reader, offset, 0x354BF0);
			}

			Textures = OffsetTextures
				.Select(x => new Texture(new BinaryReader(new SubStream(reader.BaseStream, x, 0))));
		}

		public IEnumerable<Texture> Textures { get; }

		private void pppInitPdt(BinaryReader reader, int offset, int unk)
		{
			int a0, a1, a2, a3, a4, a5, a6, a7;
			int t0, t1, t2, t3, t4, t5, t6, t7;
			int v0, v1, v2, v3;
			// a0 = offset
			// a1 = unk

			a1 = unk;
			a0 = offset + 0x110;
			t7 = ReadInt32(reader, a0 + 0x08);
			t4 = a0 + 0x10;
			t6 = ReadInt32(reader, a0 + 0x0C);
			t0 = a0 + t7;

			a3 = a0 + t6;
			t5 = ReadInt32(reader, t4);
			a2 = ReadInt32(reader, t0);
			v1 = ReadInt32(reader, a3);
			t0 += 4;

			if (t5 != 0)
			{
				do
				{
					t7 = a0 + ReadInt32(reader, t4);
					var count = ReadInt16(reader, t4 + 0x26);
					// sw      $t7, 0($t4)

					for (var i = 0; i < count; i++)
					{
						t5 = a1 + ReadInt32(reader, t4 + i * 0x10 + 0x28) * 0x48;
						t6 = a0 + ReadInt32(reader, t4 + i * 0x10 + 0x30);
						t7 = a0 + ReadInt32(reader, t4 + i * 0x10 + 0x34);
						// sw      $t5, 0x28($t4)
						// sw      $t6, 0x30($t4)
						// sw      $t7, 0x34($t4)
					}

					t6 = a0 + ReadInt32(reader, t4);
					t7 = ReadInt32(reader, t6);
					t4 = t6;
				} while (t7 != 0);

				// sw      $zero, 0($t4)
				for (var i = 0; i < a2; i++)
				{
					t7 = a1 + ReadInt32(reader, t0 + i * 4) * 0x48;
					// sw      $t7, 0($t5)
				}

				t5 = a3;
				t4 = v1;

				do
				{
					t7 = ReadInt32(reader, t5);
					t4--;
					t7 += a0;
					// sw      $t7, 0($t5)
					t5 += 4;
				} while (t4 != 0);
			}
		}


		private List<int> ReadOffsetsList(BinaryReader reader)
		{
			int count = reader.ReadInt32();
			var list = new List<int>(count);

			for (int i = 0; i < count; i++)
			{
				list.Add(reader.ReadInt32());
			}

			return list;
		}

		private void WriteOffsetsList(BinaryWriter writer, List<int> list)
		{
			writer.Write(list.Count);
			for (int i = 0; i < list.Count; i++)
			{
				writer.Write(list[i]);
			}
		}

		public int ReadInt16(BinaryReader reader, int offset)
		{
			var old = reader.BaseStream.Position;
			reader.BaseStream.Position = offset;
			var data = reader.ReadInt16();
			reader.BaseStream.Position = old;
			return data;
		}

		public int ReadInt32(BinaryReader reader, int offset)
		{
			var old = reader.BaseStream.Position;
			reader.BaseStream.Position = offset;
			var data = reader.ReadInt32();
			reader.BaseStream.Position = old;
			return data;
		}
	}
}
