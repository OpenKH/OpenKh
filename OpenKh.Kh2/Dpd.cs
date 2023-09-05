using OpenKh.Common;
using OpenKh.Kh2.Utils;
using System.Collections.Generic;
using System.IO;
using Xe.BinaryMapper;

namespace OpenKh.Kh2
{
    public partial class Dpd
	{
		private const uint Version = 0x00000096U;

        public List<ParticleData> ParticleDataList { get; set; }
        public List<Texture> TexturesList { get; set; }
        public List<Shape> ShapesList { get; set; }
        public List<DpdModel> ModelsList { get; set; }
        public List<DpdVsf> VsfList { get; set; }
        public EtcData DpdEtcData { get; set; }

        public class EtcData
        {
            [Data] public int TexVramMin { get; set; }
            [Data] public int TexVramMax { get; set; }
            [Data] public int ClutVramMin { get; set; }
            [Data] public int ClutVramMax { get; set; }
        }

        public Dpd(Stream stream)
		{
			if (!stream.CanRead || !stream.CanSeek)
				throw new InvalidDataException($"Read or seek must be supported.");

			var reader = new BinaryReader(stream);
			if (stream.Length < 16L || reader.ReadUInt32() != Version)
				throw new InvalidDataException("Invalid header");

            // dpd_h_init_datainfo
            List<int> offsetParticleData = ReadOffsetsList(reader);
			List<int> offsetTextures = ReadOffsetsList(reader);
			List<int> offsetShapes = ReadOffsetsList(reader);
			List<int> offsetModels = ReadOffsetsList(reader);
            List<int> offsetVsf = ReadOffsetsList(reader);

            DpdEtcData = BinaryMapping.ReadObject<EtcData>(stream);

            // PARTICLE DATA
			// pppInitPdt
			foreach (var offset in offsetParticleData) // RESEARCH
			{
				// 9648E0
				pppInitPdt(reader, offset, 0x354BF0);
			}
            ParticleDataList = new List<ParticleData>();
            for (int i = 0; i < offsetParticleData.Count; i++)
            {
                stream.Position = offsetParticleData[i];
                ParticleDataList.Add(new ParticleData(stream));
            }

            // TEXTURE
            TexturesList = new List<Texture>();
            for (int i = 0; i < offsetTextures.Count; i++)
            {
                stream.Position = offsetTextures[i];
                TexturesList.Add(new Texture(stream));
            }

            // SHAPE
            ShapesList = new List<Shape>();
            for (int i = 0; i < offsetShapes.Count; i++)
            {
                stream.Position = offsetShapes[i];
                ShapesList.Add(new Shape(stream));
            }

            // MODEL
            ModelsList = new List<DpdModel>();
            for (int i = 0; i < offsetModels.Count; i++)
            {
                int nextOffset = i + 1 < offsetModels.Count ? i+1 : 0;
                if(i + 1 < offsetModels.Count)
                {
                    nextOffset = offsetModels[i + 1];
                }
                else if (offsetVsf.Count > 0)
                {
                    nextOffset = offsetVsf[0];
                }
                else
                {
                    nextOffset = (int)stream.Length;
                }

                stream.Position = offsetModels[i];
                int size = nextOffset - (int)stream.Position;
                byte[] model = stream.ReadBytes(size);
                MemoryStream modelStream = new MemoryStream(model);

                ModelsList.Add(new DpdModel(modelStream));
            }

            // VSF
            VsfList = new List<DpdVsf>();
            for (int i = 0; i < offsetVsf.Count; i++)
            {
                stream.Position = offsetVsf[i];
                VsfList.Add(new DpdVsf(stream));
            }

            stream.Position = 0;
        }

        public Stream getAsStream()
        {
            Stream fileStream = new MemoryStream();
            BinaryWriter writer = new BinaryWriter(fileStream, System.Text.Encoding.UTF8, true);

            long POINTER_particleDataOffsets = 0;
            List<long> particleDataOffsets = new List<long>();

            long POINTER_textureOffsets = 0;
            List<long> textureOffsets = new List<long>();

            long POINTER_shapeOffsets = 0;
            List<long> shapeOffsets = new List<long>();

            long POINTER_modelOffsets = 0;
            List<long> modelOffsets = new List<long>();

            long POINTER_vsfOffsets = 0;
            List<long> vsfOffsets = new List<long>();

            // BUILD FILES
            List<Stream> particleDataStreams = new List<Stream>();
            foreach(ParticleData particleData in ParticleDataList)
            {
                particleDataStreams.Add(particleData.getAsStream());
            }
            List<Stream> textureStreams = new List<Stream>();
            foreach (Texture texture in TexturesList)
            {
                textureStreams.Add(texture.getAsStream());
            }
            List<Stream> shapeStreams = new List<Stream>();
            foreach (Shape shape in ShapesList)
            {
                shapeStreams.Add(shape.getAsStream());
            }
            List<Stream> modelStreams = new List<Stream>();
            foreach (DpdModel model in ModelsList)
            {
                modelStreams.Add(model.getAsStream());
            }
            List<Stream> vsfStreams = new List<Stream>();
            foreach (DpdVsf vsf in VsfList)
            {
                vsfStreams.Add(vsf.getAsStream());
            }

            // WRITE FILE
            // Version and counts
            writer.Write(Version);

            writer.Write(particleDataStreams.Count);
            POINTER_particleDataOffsets = fileStream.Position;
            foreach (Stream stream in particleDataStreams)
            {
                writer.Write((int)0);
            }
            writer.Write(textureStreams.Count);
            POINTER_textureOffsets = fileStream.Position;
            foreach (Stream stream in textureStreams)
            {
                writer.Write((int)0);
            }
            writer.Write(shapeStreams.Count);
            POINTER_shapeOffsets = fileStream.Position;
            foreach (Stream stream in shapeStreams)
            {
                writer.Write((int)0);
            }
            writer.Write(modelStreams.Count);
            POINTER_modelOffsets = fileStream.Position;
            foreach (Stream stream in modelStreams)
            {
                writer.Write((int)0);
            }
            writer.Write(vsfStreams.Count);
            POINTER_vsfOffsets = fileStream.Position;
            foreach (Stream stream in vsfStreams)
            {
                writer.Write((int)0);
            }

            // Padding and reserved
            BinaryMapping.WriteObject(fileStream, DpdEtcData);
            ReadWriteUtils.alignStreamToByte(fileStream, 16);
            ReadWriteUtils.addBytesToStream(fileStream, 16, 0x00);

            // Files
            foreach (Stream stream in particleDataStreams)
            {
                particleDataOffsets.Add(fileStream.Position);
                writer.Write(((MemoryStream)stream).ToArray());
            }
            foreach (Stream stream in textureStreams)
            {
                textureOffsets.Add(fileStream.Position);
                writer.Write(((MemoryStream)stream).ToArray());
            }
            foreach (Stream stream in shapeStreams)
            {
                shapeOffsets.Add(fileStream.Position);
                writer.Write(((MemoryStream)stream).ToArray());
            }
            foreach (Stream stream in modelStreams)
            {
                modelOffsets.Add(fileStream.Position);
                writer.Write(((MemoryStream)stream).ToArray());
            }
            foreach (Stream stream in vsfStreams)
            {
                vsfOffsets.Add(fileStream.Position);
                writer.Write(((MemoryStream)stream).ToArray());
            }

            // Write offsets
            fileStream.Position = POINTER_particleDataOffsets;
            foreach (long offset in particleDataOffsets)
            {
                writer.Write((int)offset);
            }
            fileStream.Position = POINTER_textureOffsets;
            foreach (long offset in textureOffsets)
            {
                writer.Write((int)offset);
            }
            fileStream.Position = POINTER_shapeOffsets;
            foreach (long offset in shapeOffsets)
            {
                writer.Write((int)offset);
            }
            fileStream.Position = POINTER_modelOffsets;
            foreach (long offset in modelOffsets)
            {
                writer.Write((int)offset);
            }
            fileStream.Position = POINTER_vsfOffsets;
            foreach (long offset in vsfOffsets)
            {
                writer.Write((int)offset);
            }

            fileStream.Position = 0;
            return fileStream;
        }

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
