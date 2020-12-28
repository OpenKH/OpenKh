using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Diagnostics;

namespace OpenKh.Kh2Anim.Mset.EmuRunner
{
    class RelocMset
    {
        MemoryStream os;
        uint off;
        uint reloc;
        BinaryReader br;
        BinaryWriter wr;
        uint[] ps;

        public RelocMset(byte[] bin, uint off, uint reloc, uint[] ps)
        {
            os = new MemoryStream(bin, true);
            this.off = off;
            this.reloc = reloc;
            br = new BinaryReader(os);
            wr = new BinaryWriter(os);
            this.ps = ps;
        }
        public void Run()
        {
            relocateAnimData(off);
        }

        void relocateAnimData(uint off2)
        {
            os.Position = off2;
            wr.Write(ps[0]);
            wr.Write((uint)(off2 - off + reloc + 0x90U));
            wr.Write(ps[2]);
            wr.Write(ps[3]);
            wr.Write(ps[4]);
            wr.Write(ps[5]);

            os.Position = off2 + 0x90U + 0x10U;
            int c0 = br.ReadUInt16();
            int c1 = br.ReadUInt16();
            os.Position = off2 + 0x90U + 0x18U;
            uint pos = br.ReadUInt32();

            int ct = c1 - c0;

            uint off3 = off2 + 0x90U + pos;

            if (off3 < (uint)os.Length)
            {
                BSReloc.Run(os, off3, ct, c0);
            }
        }
    }
    class RelocMdlx
    {
        byte[] bin;
        uint off;
        uint reloc;
        uint pos0, pos1, pos2, pos3;
        MemoryStream os;
        BinaryReader br;
        BinaryWriter wr;

        public RelocMdlx(byte[] bin, int off, int reloc, uint pos0, uint pos1, uint pos2, uint pos3)
        {
            // pos0: 0x354398 --> ?
            // pos1: P_EX +0x90
            // pos2: st
            // pos3: 1
            this.bin = bin;
            this.off = (uint)off;
            this.reloc = (uint)reloc;
            os = new MemoryStream(bin, true);
            br = new BinaryReader(os);
            wr = new BinaryWriter(os);
            this.pos0 = pos0;
            this.pos1 = pos1;
            this.pos2 = pos2;
            this.pos3 = pos3;
        }

        public uint Run()
        {
            os.Position = off + 4;
            int cnt = br.ReadInt32();
            UtReloc.Rel4(os, off + 8, reloc);
            uint r = 0;
            for (int c = 0; c < cnt; c++)
            {
                os.Position = off + 16 * (1 + c);
                uint c00 = br.ReadUInt32();
                uint pos = UtReloc.Rel4(os, (uint)(off + 16 * (1 + c) + 8), reloc).oldVal;
                if (c00 == 0x04) r = ty04(pos);
            }
            return r;
        }

        uint ty04(uint pos)
        {
            os.Position = off + pos;
            wr.Write(pos0);
            wr.Write(reloc + pos + 0x90U);
            wr.Write(pos2);
            wr.Write(pos3);

            os.Position = off + pos + 0x90U + 0x10U;
            uint c10 = br.ReadUInt16();
            uint c12 = br.ReadUInt16();
            uint c14 = br.ReadUInt32();

#if true
            BSReloc.Run(os, off + pos + 0x90U + c14, (int)c10, 0);
#else
            List<Corner> alc = new List<Corner>();
            for (int t = 0; t < (int)c10; t++) {
                os.Position = off + pos + 0x90U + c14 + 64 * t;
                Corner o = new Corner();
                o.bi = br.ReadInt32();
                o.pi = br.ReadInt32();
                if (o.pi != -1) {
                    if (alc[o.pi].x3 == -1) { // no children yet
                        alc[o.pi].x3 = t;
                    }
                    else { // having children
                        int myx3 = alc[o.pi].x3;
                        alc[o.pi].x3 = t;
                        o.x1 = myx3;
                    }
                }
                alc.Add(o);
            }
            for (int t = 0; t < (int)c10; t++)
                Debug.WriteLine(alc[t]);
            for (int t = 0; t < (int)c10; t++) {
                os.Position = off + pos + 0x90U + c14 + 64 * t;
                Corner o = alc[t];
                os.Seek(+2, SeekOrigin.Current);
                wr.Write((ushort)o.x1);
                os.Seek(+2, SeekOrigin.Current);
                wr.Write((ushort)o.x3);
            }
#endif

            return reloc + pos;
        }

#if false
        class Corner {
            public int bi, pi;
            public int x1 = -1, x3 = -1;

            public override string ToString() {
                return base.ToString();
                //return string.Format("{0:X4} {1:X4} {2:X4} {3:X4}", (short)bi, (short)x1, (short)pi, (short)x3);
            }
        }
#endif
    }
    class BSReloc
    {
        class Corner
        {
            public int bi, pi;
            public int x1 = -1, x3 = -1;

            public override string ToString()
            {
                return string.Format("{0:X4} {1:X4} {2:X4} {3:X4}", (short)bi, (short)x1, (short)pi, (short)x3);
            }
        }

        public static void Run(MemoryStream os, uint off, int ct, int delta)
        {
            BinaryReader br = new BinaryReader(os);
            BinaryWriter wr = new BinaryWriter(os);

            List<Corner> alc = new List<Corner>();
            for (int t = 0; t < ct; t++)
            {
                os.Position = off + 64 * t;
                Corner o = new Corner();
                o.bi = br.ReadInt32();
                o.pi = br.ReadInt32();
                if (o.pi != -1 && o.pi - delta >= 0)
                {
                    if (alc[o.pi - delta].x3 == -1)
                    { // no children yet
                        alc[o.pi - delta].x3 = t + delta;
                    }
                    else
                    { // having children
                        int myx3 = alc[o.pi - delta].x3;
                        alc[o.pi - delta].x3 = t + delta;
                        o.x1 = myx3;
                    }
                }
                alc.Add(o);
            }
#if false
            for (int t = 0; t < ct; t++)
                Debug.WriteLine(alc[t]);
#endif
            for (int t = 0; t < ct; t++)
            {
                os.Position = off + 64 * t;
                Corner o = alc[t];
                os.Seek(+2, SeekOrigin.Current);
                wr.Write((ushort)o.x1);
                os.Seek(+2, SeekOrigin.Current);
                wr.Write((ushort)o.x3);
            }

        }
    }
    class RelRes
    {
        public uint oldVal, newVal;

        public RelRes(uint oldVal, uint newVal)
        {
            this.oldVal = oldVal;
            this.newVal = newVal;
        }
    }
    class UtReloc
    {
        public static RelRes Rel4(MemoryStream os, uint off, uint reloc)
        {
            os.Position = off;
            uint oldVal = new BinaryReader(os).ReadUInt32();
            uint newVal = oldVal + reloc;
            os.Position = off;
            new BinaryWriter(os).Write(newVal);
            return new RelRes(oldVal, newVal);
        }
    }
}
