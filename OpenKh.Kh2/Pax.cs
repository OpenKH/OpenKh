using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace OpenKh.Kh2
{
    public partial class Pax
    {
        public class Entry
        {
            public int Effect { get; set; }
            public int Caster { get; set; }
            public int Unk04 { get; set; }
            public int Unk06 { get; set; }
            public int Unk08 { get; set; }
            public int Unk0C { get; set; }
            public int Unk10 { get; set; }
            public int Unk14 { get; set; }
            public int SoundEffect { get; set; }
            public float PosX { get; set; }
            public float PosZ { get; set; }
            public float PosY { get; set; }
            public float RotX { get; set; }
            public float RotZ { get; set; }
            public float RotY { get; set; }
            public float ScaleX { get; set; }
            public float ScaleZ { get; set; }
            public float ScaleY { get; set; }
            public int Unk40 { get; set; }
            public int Unk44 { get; set; }
            public int Unk48 { get; set; }
            public int Unk4C { get; set; }

            internal static Entry Read(BinaryReader reader)
            {
                return new Entry()
                {
                    Effect = reader.ReadInt16(),
                    Caster = reader.ReadInt16(),
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
            }

            internal void Save(BinaryWriter writer)
            {
                writer.Write((short)Effect);
                writer.Write((short)Caster);
                writer.Write((short)Unk04);
                writer.Write((short)Unk06);
                writer.Write(Unk08);
                writer.Write(Unk0C);
                writer.Write(Unk10);
                writer.Write(Unk14);
                writer.Write(SoundEffect);
                writer.Write(PosX);
                writer.Write(PosZ);
                writer.Write(PosY);
                writer.Write(RotX);
                writer.Write(RotZ);
                writer.Write(RotY);
                writer.Write(ScaleX);
                writer.Write(ScaleZ);
                writer.Write(ScaleY);
                writer.Write(Unk40);
                writer.Write(Unk44);
                writer.Write(Unk48);
                writer.Write(Unk4C);
            }
        }

        private const uint MagicCode = 0x5F584150U;

        public Pax(Stream stream)
        {
            if (!stream.CanRead || !stream.CanSeek)
                throw new InvalidDataException($"Read or seek must be supported.");

            var reader = new BinaryReader(stream);
            var offsetBase = reader.BaseStream.Position;

            if (stream.Length < 16L || reader.ReadUInt32() != MagicCode)
                throw new InvalidDataException("Invalid header");

            var offsetName = reader.ReadInt32();
            var entriesCount = reader.ReadInt32();
            var offsetDpx = reader.ReadInt32();

            Entries = new List<Entry>(entriesCount);
            for (int i = 0; i < entriesCount; i++)
            {
                Entries.Add(Entry.Read(reader));
            }

            reader.BaseStream.Position = offsetBase + offsetName;
            ReadName(reader);

            reader.BaseStream.Position = offsetBase + offsetDpx;
            //Dpx = new Dpx(reader);
        }

        public string Name { get; set; }

        public List<Entry> Entries { get; set; }

        public Dpx Dpx { get; set; }


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
            //Dpx.SaveChanges(writer);

            writer.BaseStream.Position = offsetBase;
            writer.Write(MagicCode);
            writer.Write(offsetName);
            writer.Write(Entries.Count);
            writer.Write(offsetDpx);
        }

        public void SaveEntries(BinaryWriter writer)
        {
            foreach (var entry in Entries)
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
