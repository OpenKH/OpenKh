using OpenKh.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Xe.BinaryMapper;

namespace OpenKh.Kh2
{
    public class Pax
    {
        private const uint MagicCode = 0x5F584150U; // PAX_
        public string Name { get; set; }
        public PaxHeader Header { get; set; }
        public byte[] DebugInfo { get; set; }  // Always 8 x 16 bytes
        public List<Element> Elements { get; set; }
        public Dpx DpxPackage { get; set; }

        public class PaxHeader
        {
            [Data] public uint Signature { get; set; } // PAX_
            [Data] public int DebugInfoOffset { get; set; }
            [Data] public int ElementCount { get; set; }
            [Data] public int DpxOffset { get; set; }
        }

		public class Element
		{
            [Data] public ushort EffectNumber { get; set; }
            [Data] public ushort Id { get; set; }
			[Data] public byte Group { get; set; }
            [Data] public byte FadeoutFrame { get; set; }
            [Data] public short BoneId { get; set; }
			[Data] public ulong Category { get; set; }
			[Data] public uint Flag { get; set; }
			[Data] public float StartWait { get; set; }
			[Data] public int SoundEffectNumber { get; set; }
			[Data] public float TranslationX { get; set; }
			[Data] public float TranslationZ { get; set; }
			[Data] public float TranslationY { get; set; }
			[Data] public float RotationX { get; set; }
			[Data] public float RotationZ { get; set; }
			[Data] public float RotationY { get; set; }
			[Data] public float ScaleX { get; set; }
			[Data] public float ScaleZ { get; set; }
			[Data] public float ScaleY { get; set; }
			[Data] public int Unk40 { get; set; } // Reserve/Padding? Always 0
			[Data] public int Unk44 { get; set; } // Reserve/Padding? Always 0
            [Data] public int Unk48 { get; set; } // Reserve/Padding? Always 0
            [Data] public int Unk4C { get; set; } // Reserve/Padding? Always 0

            /*internal static Element Read(BinaryReader reader)
			{
				return new Element()
				{
                    Number = reader.ReadHalf(),
                    Id = reader.ReadInt16(),
					Unk04 = reader.ReadInt16(),
					Unk06 = reader.ReadInt16(),
					Unk08 = reader.ReadInt32(),
					Unk0C = reader.ReadInt32(),
					Unk10 = reader.ReadInt32(),
					Unk14 = reader.ReadInt32(),
					SoundEffect = reader.ReadInt32(),
					PosX = reader.ReadSingle(),
					PosZ = reader.ReadSingle(),
					PosY = reader.ReadSingle(),
					RotX = reader.ReadSingle(),
					RotZ = reader.ReadSingle(),
					RotY = reader.ReadSingle(),
					ScaleX = reader.ReadSingle(),
					ScaleZ = reader.ReadSingle(),
					ScaleY = reader.ReadSingle(),
					Unk40 = reader.ReadInt32(),
					Unk44 = reader.ReadInt32(),
					Unk48 = reader.ReadInt32(),
					Unk4C = reader.ReadInt32(),
				};
			}*/

			internal void Save(BinaryWriter writer)
			{
				writer.Write((ushort)EffectNumber);
				writer.Write((ushort)Id);
				writer.Write((byte)Group);
                writer.Write((byte)FadeoutFrame);
                writer.Write((short)BoneId);
				writer.Write(Category);
				writer.Write(Flag);
				writer.Write(StartWait);
				writer.Write(SoundEffectNumber);
				writer.Write(TranslationX);
				writer.Write(TranslationZ);
				writer.Write(TranslationY);
				writer.Write(RotationX);
				writer.Write(RotationZ);
				writer.Write(RotationY);
				writer.Write(ScaleX);
				writer.Write(ScaleZ);
				writer.Write(ScaleY);
				writer.Write(Unk40);
				writer.Write(Unk44);
				writer.Write(Unk48);
				writer.Write(Unk4C);
            }

            public override string ToString()
            {
                return "No: "+ EffectNumber + " | Id: " + Id + " | Gr: " + Group + " | Bone: " + BoneId + " | SEno: " + SoundEffectNumber;
            }
        }

        public Pax(Stream stream)
		{
			if (!stream.CanRead || !stream.CanSeek)
				throw new InvalidDataException($"Read or seek must be supported.");

            Header = BinaryMapping.ReadObject<PaxHeader>(stream);

            Elements = new List<Element>();
            for(int i = 0; i < Header.ElementCount; i++)
            {
                Elements.Add(BinaryMapping.ReadObject<Element>(stream));
            }

            DebugInfo = new byte[0x80];
            for (int i = 0; i < 0x80; i++)
            {
                DebugInfo[i] = (byte) stream.ReadByte();
            }

            stream.Position = Header.DpxOffset;

            // Load remaining data to a stream
            byte[] data = stream.ReadBytes((int)(stream.Length - stream.Position));
            MemoryStream dpxStream = new MemoryStream();
            dpxStream.Write(data, 0, data.Length);
            dpxStream.Position = 0;

            DpxPackage = new Dpx(dpxStream);
        }

        public Stream getAsStream()
        {
            Stream fileStream = new MemoryStream();

            Header.DebugInfoOffset = 16 + 80 * Elements.Count;
            Header.DpxOffset = Header.DebugInfoOffset + 128;

            BinaryMapping.WriteObject(fileStream, Header);
            foreach(Element elem in Elements)
            {
                BinaryMapping.WriteObject(fileStream, elem);
            }

            BinaryWriter writer = new BinaryWriter(fileStream);
            writer.Write(DebugInfo);

            writer.Write(((MemoryStream)DpxPackage.getAsStream()).ToArray());

            fileStream.Position = 0;
            return fileStream;
        }

        private void ReadName(BinaryReader reader)
		{
			byte[] data = new byte[0x80];
			reader.Read(data, 0, data.Length);
			Name = Encoding.UTF8.GetString(data).TrimEnd('\0');
		}

		private void ReadDpx(BinaryReader reader)
		{
		}

		public void SaveChanges(Stream stream)
		{
			SaveChanges(new BinaryWriter(stream));
		}

		public void SaveChanges(BinaryWriter writer)
		{
			var offsetBase = writer.BaseStream.Position;

			writer.BaseStream.Position += 0x10;
			SaveEntries(writer);

			int offsetName = (int)(writer.BaseStream.Position - offsetBase);
			SaveName(writer);

			int offsetDpx = (int)(writer.BaseStream.Position - offsetBase);

			writer.BaseStream.Position = offsetBase;
			writer.Write(MagicCode);
			writer.Write(offsetName);
			writer.Write(Elements.Count);
			writer.Write(offsetDpx);
		}

		public void SaveEntries(BinaryWriter writer)
		{
			foreach (var entry in Elements)
			{
				entry.Save(writer);
			}
		}

		public void SaveName(BinaryWriter writer)
		{
			var data = new byte[0x80];
			var strData = Encoding.UTF8.GetBytes(Name);
			Array.Copy(strData, data, Math.Min(data.Length, strData.Length));
			writer.Write(data);
		}
    }
}
