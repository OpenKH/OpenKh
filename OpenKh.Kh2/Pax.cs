using OpenKh.Common;
using OpenKh.Kh2.Utils;
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
			[Data] public ElementFlags Flags { get; set; }
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
			[Data] public BonePos BonePosition { get; set; }

            public bool FlagBind { get => BitFlag.IsFlagSet(Flags, ElementFlags.Bind); set => Flags = BitFlag.SetFlag(Flags, ElementFlags.Bind, value); }
            public bool FlagBindOnlyStart { get => BitFlag.IsFlagSet(Flags, ElementFlags.BindOnlyStart); set => Flags = BitFlag.SetFlag(Flags, ElementFlags.BindOnlyStart, value); }
            public bool FlagBindOnlyPos { get => BitFlag.IsFlagSet(Flags, ElementFlags.BindOnlyPos); set => Flags = BitFlag.SetFlag(Flags, ElementFlags.BindOnlyPos, value); }
            public bool FlagGetColor { get => BitFlag.IsFlagSet(Flags, ElementFlags.GetColor); set => Flags = BitFlag.SetFlag(Flags, ElementFlags.GetColor, value); }
            public bool FlagGetBrightness { get => BitFlag.IsFlagSet(Flags, ElementFlags.GetBrightness); set => Flags = BitFlag.SetFlag(Flags, ElementFlags.GetBrightness, value); }
            public bool FlagDestroyWhenMotionChange { get => BitFlag.IsFlagSet(Flags, ElementFlags.DestroyWhenMotionChange); set => Flags = BitFlag.SetFlag(Flags, ElementFlags.DestroyWhenMotionChange, value); }
            public bool FlagDestroyFadeout { get => BitFlag.IsFlagSet(Flags, ElementFlags.DestroyFadeout); set => Flags = BitFlag.SetFlag(Flags, ElementFlags.DestroyFadeout, value); }
            public bool FlagDestroyLoopend { get => BitFlag.IsFlagSet(Flags, ElementFlags.DestroyLoopend); set => Flags = BitFlag.SetFlag(Flags, ElementFlags.DestroyLoopend, value); }
            public bool FlagBindScale { get => BitFlag.IsFlagSet(Flags, ElementFlags.BindScale); set => Flags = BitFlag.SetFlag(Flags, ElementFlags.BindScale, value); }
            public bool FlagDestroyWhenObjectLeaves { get => BitFlag.IsFlagSet(Flags, ElementFlags.DestroyWhenObjectLeaves); set => Flags = BitFlag.SetFlag(Flags, ElementFlags.DestroyWhenObjectLeaves, value); }
            public bool FlagDestroyWhenBindEffectOff { get => BitFlag.IsFlagSet(Flags, ElementFlags.DestroyWhenBindEffectOff); set => Flags = BitFlag.SetFlag(Flags, ElementFlags.DestroyWhenBindEffectOff, value); }
            public bool FlagGetObjectFade { get => BitFlag.IsFlagSet(Flags, ElementFlags.GetObjectFade); set => Flags = BitFlag.SetFlag(Flags, ElementFlags.GetObjectFade, value); }
            public bool FlagBonePos { get => BitFlag.IsFlagSet(Flags, ElementFlags.BonePos); set => Flags = BitFlag.SetFlag(Flags, ElementFlags.BonePos, value); }
            public bool FlagBindCamera { get => BitFlag.IsFlagSet(Flags, ElementFlags.BindCamera); set => Flags = BitFlag.SetFlag(Flags, ElementFlags.BindCamera, value); }

            [Flags]
            public enum ElementFlags : uint
            {
                Bind = 0x01,
                BindOnlyStart = 0x02,
                BindOnlyPos = 0x04,
                GetColor = 0x08,
                GetBrightness = 0x10,
                DestroyWhenMotionChange = 0x20,
                DestroyFadeout = 0x40,
                DestroyLoopend = 0x80,
                BindScale = 0x100,
                DestroyWhenObjectLeaves = 0x200,
                DestroyWhenBindEffectOff = 0x400,
                GetObjectFade = 0x800,
                BonePos = 0x1000,
                BindCamera = 0x2000
            }

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
				writer.Write((uint)Flags);
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
				writer.Write(BonePosition.A);
				writer.Write(BonePosition.B);
				writer.Write(BonePosition.RatioA);
				writer.Write(BonePosition.RatioB);
				writer.Write(BonePosition.Adjust);
				writer.Write(BonePosition.Padding);
            }

            public override string ToString()
            {
                return "No: "+ EffectNumber + " | Id: " + Id + " | Gr: " + Group + " | Bone: " + BoneId + " | SEno: " + SoundEffectNumber;
            }

            public class BonePos
            {
                [Data] public ushort A { get; set; }
                [Data] public ushort B { get; set; }
                [Data] public float RatioA { get; set; }
                [Data] public float RatioB { get; set; }
                [Data] public byte Adjust { get; set; }
                [Data(Count=3)] public byte[] Padding { get; set; }
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

            Header.ElementCount = Elements.Count;
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
