#define read_mdlx_from_ram_feature
using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using OpenKh.Common;
using System.Xml;

namespace OpenKh.Kh2.SrkAlteranatives
{
    public class Bar : BitConverter
    {
        public ushort Type;
        public string Name;
        public int Length;
        public List<Bar> Files;

        public Bar(Stream stream, ushort type, string name, int length)
        {
            this.Files = new List<Bar>(0);
            long position = stream.Position;
            this.Type = type;
            this.Name = name;
            this.Length = length;

            int child_count = 0;

            if (stream.ReadString(3, Encoding.ASCII) == "BAR")
            {
                stream.Position++;
                child_count = stream.ReadInt32();
            }

            if (child_count > 0)
            {
                for (int i = 0; i < child_count; i++)
                {
                    stream.Position = position + 0x10 + i * 0x10;
                    ushort type_ = stream.ReadUInt16(); stream.Position += 2;
                    string name_ = stream.ReadString(4,Encoding.ASCII).TrimEnd('\x0');
                    int offset_ = stream.ReadInt32();
                    int length_ = stream.ReadInt32();
                    stream.Position = offset_;
                    Bar child = new Bar(stream, type_, name_, length_);

                    this.Files.Add(child);
                }
            }
            else
            {
                int alignedLength = this.Length;
                while (alignedLength % 16 > 0) alignedLength++;
                if (this.Type == 4)
                    alignedLength += 0x30;

                stream.Position = position;
                this.Data = new byte[alignedLength];
                stream.Read(this.Data, 0, this.Length);
                switch (this.Type)
                {
                    case 9:
                        Array.Copy(new byte[0x30], 0, this.Data, 0, 0x30);
                        break;
                    case 4:
                        Array.Copy(new byte[0x30], 0, this.Data, 0, 0x30);
                        if (global::System.BitConverter.ToInt16(this.Data, 0x90) == 3)
                        {
                            int bonesCounts = global::System.BitConverter.ToInt16(this.Data, 0xA0);
                            int bonesOff = +0x90 + global::System.BitConverter.ToInt32(this.Data, 0xA4);
                            for (int i = 0; i < bonesCounts; i++)
                            {
                                short boneIndex = global::System.BitConverter.ToInt16(this.Data, bonesOff + i * 0x40);
                                Array.Copy(global::System.BitConverter.GetBytes((int)boneIndex), 0, this.Data, bonesOff + i * 0x40, 4);

                                short parentIndex = global::System.BitConverter.ToInt16(this.Data, bonesOff + 0x04 + i * 0x40);
                                Array.Copy(global::System.BitConverter.GetBytes((int)parentIndex), 0, this.Data, bonesOff + 0x04 + i * 0x40, 4);
                            }
                        }
                        break;
                    case 7:
                        int tim_header_6s = global::System.BitConverter.ToInt32(this.Data, 0x14);
                        int tim_header_6s_count = global::System.BitConverter.ToInt32(this.Data, 0x08) + 1;
                        for (int i = 0; i < tim_header_6s_count; i++)
                        {
                            int address = global::System.BitConverter.ToInt32(this.Data, tim_header_6s + i * 0x90 + 0x74);
                            if (!(stream is FileStream) && !(stream is MemoryStream))
                            address -= (int)position;
                            Array.Copy(global::System.BitConverter.GetBytes(address), 0, this.Data, tim_header_6s + i * 0x90 + 0x74, 4);
                        }
                        break;
                }
                return;
            }
        }

        public void Save(string filename)
        {
            FileStream fs = new FileStream(filename, FileMode.Create);
            BinaryWriter bw = new BinaryWriter(fs);
            bw.Write(Encoding.ASCII.GetBytes("BAR\x1"));
            bw.Write(this.Files.Count);
            bw.Write(new byte[8]);


            for (int i = 0; i < this.Files.Count; i++)
            {
                bw.Write((int)this.Files[i].Type);
                bw.Write(Encoding.ASCII.GetBytes(this.Files[i].Name));
                if (this.Files[i].Name.Length < 4)
                {
                    bw.Write(new byte[4 - this.Files[i].Name.Length]);
                }
                bw.Write(0);
                bw.Write(this.Files[i].Length);
            }
            for (int i = 0; i < this.Files.Count; i++)
            {
                fs.Position = 0x18 + i * 0x10;
                bw.Write((int)fs.Length);
                fs.Position = fs.Length;
                bw.Write(this.Files[i].GetData());
            }
            bw.Close();
        }

        public byte[] GetData()
        {
            if (this.Files.Count == 0)
                return this.Data;

            MemoryStream ms = new MemoryStream();
            BinaryWriter bw = new BinaryWriter(ms);
            bw.Write(Encoding.ASCII.GetBytes("BAR\x1"));
            bw.Write(this.Files.Count);
            bw.Write(new byte[8]);


            for (int i = 0; i < this.Files.Count; i++)
            {
                bw.Write((int)this.Files[i].Type);
                bw.Write(Encoding.ASCII.GetBytes(this.Files[i].Name));
                if (this.Files[i].Name.Length < 4)
                {
                    bw.Write(new byte[4 - this.Files[i].Name.Length]);
                }
                bw.Write(0);
                bw.Write(this.Files[i].Length);
            }
            for (int i = 0; i < this.Files.Count; i++)
            {
                ms.Position = 0x18 + i * 0x10;
                bw.Write((int)ms.Length);
                ms.Position = ms.Length;
                bw.Write(this.Files[i].Data);
            }
            byte[] output = new byte[ms.Length];
            ms.Position = 0;
            ms.Read(output,0,output.Length);
            bw.Close();
            return output;
        }
    }
}
