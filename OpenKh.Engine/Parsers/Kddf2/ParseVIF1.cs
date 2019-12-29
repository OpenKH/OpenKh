using System;
using System.Collections.Generic;
using System.IO;

namespace OpenKh.Engine.Parsers.Kddf2
{
    public class VU1Mem
    {
        public byte[] vumem = new byte[16 * 1024];
    }
    public class ParseVIF1
    {
        VU1Mem vu1;
        public List<byte[]> almsmem = new List<byte[]>();

        public ParseVIF1(VU1Mem vu1)
        {
            this.vu1 = vu1;
        }

        class Reader
        {
            BinaryReader br;
            int vl;
            bool usn;

            public Reader(BinaryReader br, int vl, int usn)
            {
                this.br = br;
                this.vl = vl;
                this.usn = (usn != 0) ? true : false;
            }
            public uint Read()
            {
                switch (vl)
                {
                    case 0: // 32 bits
                        return br.ReadUInt32();
                    case 1: // 16 bits
                        if (usn) return br.ReadUInt16();
                        return (uint)(int)br.ReadInt16();
                    case 2: // 8 bits
                        if (usn) return br.ReadByte();
                        return (uint)(sbyte)br.ReadSByte();
                    default:
                        throw new NotSupportedException("vl(" + vl + ")");
                }
            }
        }

        public void Parse(MemoryStream si)
        {
            Parse(si, 0);
        }
        public void Parse(MemoryStream si, int tops)
        {
            MemoryStream os = new MemoryStream(vu1.vumem, true);
            BinaryWriter bw = new BinaryWriter(os);

            byte[] vifmask = new byte[16] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
            uint[] colval = new uint[4];
            uint[] rowval = new uint[4];

            BinaryReader br = new BinaryReader(si);
            while (si.Position < si.Length)
            {
                long curPos = si.Position;
                uint v = br.ReadUInt32();
                int cmd = ((int)(v >> 24) & 0x7F);
                switch (cmd)
                {
                    case 0x00: break; // nop
                    case 0x01:
                        { // stcycl
                            int cl = ((int)(v >> 0) & 0xFF);
                            int wl = ((int)(v >> 8) & 0xFF);
                            break;
                        }
                    case 0x02: break; // offset
                    case 0x03: break; // base
                    case 0x04: break; // itop
                    case 0x05: break; // stmod
                    case 0x06: break; // mskpath3
                    case 0x07: break; // mark
                    case 0x10: break; // flushe
                    case 0x11: break; // flush
                    case 0x13: break; // flusha
                    case 0x14: SplitMsmem(); break; // mscal
                    case 0x17: SplitMsmem(); break; // mscnt
                    case 0x15: break; // mscalf
                    case 0x20:
                        { // stmask
                            uint r1 = br.ReadUInt32();
                            for (int x = 0; x < 16; x++)
                            {
                                vifmask[x] = (byte)(((int)(r1 >> (2 * x))) & 3);
                            }
                            break;
                        }
                    case 0x30:
                        { // strow
                            rowval[0] = br.ReadUInt32();
                            rowval[1] = br.ReadUInt32();
                            rowval[2] = br.ReadUInt32();
                            rowval[3] = br.ReadUInt32();
                            break;
                        }
                    case 0x31:
                        { // stcol
                            colval[0] = br.ReadUInt32();
                            colval[1] = br.ReadUInt32();
                            colval[2] = br.ReadUInt32();
                            colval[3] = br.ReadUInt32();
                            break;
                        }
                    case 0x4A: break; // mpg
                    case 0x50:
                        { // direct
                            int imm = ((int)(v >> 0) & 0xFFFF);
                            si.Position = (si.Position + 15) & (~15);
                            si.Position += 16 * imm;
                            break;
                        }
                    case 0x51:
                        { // directhl
                            int imm = ((int)(v >> 0) & 0xFFFF);
                            si.Position = (si.Position + 15) & (~15);
                            si.Position += 16 * imm;
                            break;
                        }
                    default:
                        { // unpack or ?
                            if (0x60 == (cmd & 0x60))
                            {
                                int m = ((int)(cmd >> 4) & 1);
                                int vn = ((int)(cmd >> 2) & 0x3);
                                int vl = ((int)(cmd >> 0) & 0x3);

                                int size = ((int)(v >> 16) & 0xFF);
                                int flg = ((int)(v >> 15) & 1);
                                int usn = ((int)(v >> 14) & 1);
                                int addr = ((int)(v >> 0) & 0x3FF);

                                int cbEle = 0, cntEle = 1;
                                switch (vl)
                                {
                                    case 0: cbEle = 4; break;
                                    case 1: cbEle = 2; break;
                                    case 2: cbEle = 1; break;
                                    case 3: cbEle = 2; break;
                                }
                                switch (vn)
                                {
                                    case 0: cntEle = 1; break;
                                    case 1: cntEle = 2; break;
                                    case 2: cntEle = 3; break;
                                    case 3: cntEle = 4; break;
                                    default: throw new Exception("Ahh vn!");
                                }
                                int cbTotal = (cbEle * cntEle * size + 3) & (~3);
                                long newPos = si.Position + cbTotal;

                                os.Position = 16 * (tops + addr);
                                bool nomask = (m == 0) ? true : false;

                                Reader reader = new Reader(br, vl, usn);
                                for (int y = 0; y < size; y++)
                                {
                                    uint tmpv;
                                    switch (vn)
                                    {
                                        case 0: // S-XX
                                        default:
                                            tmpv = reader.Read();
                                            // X
                                            switch (vifmask[(y & 3) * 4 + 0] & (nomask ? 0 : 7))
                                            {
                                                case 0: bw.Write(tmpv); break;
                                                case 1: bw.Write(rowval[y & 3]); break;
                                                case 2: bw.Write(colval[0]); break;
                                                case 3: os.Position += 4; break;
                                            }
                                            // Y
                                            switch (vifmask[(y & 3) * 4 + 1] & (nomask ? 0 : 7))
                                            {
                                                case 0: bw.Write(tmpv); break;
                                                case 1: bw.Write(rowval[y & 3]); break;
                                                case 2: bw.Write(colval[1]); break;
                                                case 3: os.Position += 4; break;
                                            }
                                            // Z
                                            switch (vifmask[(y & 3) * 4 + 2] & (nomask ? 0 : 7))
                                            {
                                                case 0: bw.Write(tmpv); break;
                                                case 1: bw.Write(rowval[y & 3]); break;
                                                case 2: bw.Write(colval[2]); break;
                                                case 3: os.Position += 4; break;
                                            }
                                            // W
                                            switch (vifmask[(y & 3) * 4 + 3] & (nomask ? 0 : 7))
                                            {
                                                case 0: bw.Write(tmpv); break;
                                                case 1: bw.Write(rowval[y & 3]); break;
                                                case 2: bw.Write(colval[3]); break;
                                                case 3: os.Position += 4; break;
                                            }
                                            break;
                                        case 1: // V2-XX
                                            // X
                                            tmpv = reader.Read();
                                            switch (vifmask[(y & 3) * 4 + 0] & (nomask ? 0 : 7))
                                            {
                                                case 0: bw.Write(tmpv); break;
                                                case 1: bw.Write(rowval[y & 3]); break;
                                                case 2: bw.Write(colval[0]); break;
                                                case 3: os.Position += 4; break;
                                            }
                                            // Y
                                            tmpv = reader.Read();
                                            switch (vifmask[(y & 3) * 4 + 1] & (nomask ? 0 : 7))
                                            {
                                                case 0: bw.Write(tmpv); break;
                                                case 1: bw.Write(rowval[y & 3]); break;
                                                case 2: bw.Write(colval[1]); break;
                                                case 3: os.Position += 4; break;
                                            }
                                            // Z
                                            os.Position += 4;
                                            // W
                                            os.Position += 4;
                                            break;
                                        case 2: // V3-XX
                                            // X
                                            tmpv = reader.Read();
                                            switch (vifmask[(y & 3) * 4 + 0] & (nomask ? 0 : 7))
                                            {
                                                case 0: bw.Write(tmpv); break;
                                                case 1: bw.Write(rowval[y & 3]); break;
                                                case 2: bw.Write(colval[0]); break;
                                                case 3: os.Position += 4; break;
                                            }
                                            // Y
                                            tmpv = reader.Read();
                                            switch (vifmask[(y & 3) * 4 + 1] & (nomask ? 0 : 7))
                                            {
                                                case 0: bw.Write(tmpv); break;
                                                case 1: bw.Write(rowval[y & 3]); break;
                                                case 2: bw.Write(colval[1]); break;
                                                case 3: os.Position += 4; break;
                                            }
                                            // Z
                                            tmpv = reader.Read();
                                            switch (vifmask[(y & 3) * 4 + 2] & (nomask ? 0 : 7))
                                            {
                                                case 0: bw.Write(tmpv); break;
                                                case 1: bw.Write(rowval[y & 3]); break;
                                                case 2: bw.Write(colval[2]); break;
                                                case 3: os.Position += 4; break;
                                            }
                                            // W
                                            os.Position += 4;
                                            break;
                                        case 3: // V4-XX
                                            // X
                                            tmpv = reader.Read();
                                            switch (vifmask[(y & 3) * 4 + 0] & (nomask ? 0 : 7))
                                            {
                                                case 0: bw.Write(tmpv); break;
                                                case 1: bw.Write(rowval[y & 3]); break;
                                                case 2: bw.Write(colval[0]); break;
                                                case 3: os.Position += 4; break;
                                            }
                                            // Y
                                            tmpv = reader.Read();
                                            switch (vifmask[(y & 3) * 4 + 1] & (nomask ? 0 : 7))
                                            {
                                                case 0: bw.Write(tmpv); break;
                                                case 1: bw.Write(rowval[y & 3]); break;
                                                case 2: bw.Write(colval[1]); break;
                                                case 3: os.Position += 4; break;
                                            }
                                            // Z
                                            tmpv = reader.Read();
                                            switch (vifmask[(y & 3) * 4 + 2] & (nomask ? 0 : 7))
                                            {
                                                case 0: bw.Write(tmpv); break;
                                                case 1: bw.Write(rowval[y & 3]); break;
                                                case 2: bw.Write(colval[2]); break;
                                                case 3: os.Position += 4; break;
                                            }
                                            // W
                                            tmpv = reader.Read();
                                            switch (vifmask[(y & 3) * 4 + 3] & (nomask ? 0 : 7))
                                            {
                                                case 0: bw.Write(tmpv); break;
                                                case 1: bw.Write(rowval[y & 3]); break;
                                                case 2: bw.Write(colval[3]); break;
                                                case 3: os.Position += 4; break;
                                            }
                                            break;
                                    }
                                }

                                si.Position = newPos;
                                break;
                            }
                            break;
                        }
                }
            }
        }

        private void SplitMsmem()
        {
            almsmem.Add((byte[])vu1.vumem.Clone());
        }
    }
}
