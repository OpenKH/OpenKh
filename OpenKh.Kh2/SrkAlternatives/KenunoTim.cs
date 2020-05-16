using System;
using System.Drawing;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using OpenKh.Imaging;
using OpenKh.Common.Ps2;
using TexUt2;

namespace OpenKh.Kh2.SrkAlternatives
{
    public class KenunoTim
    {
        byte[] data_;
        public MTex tex;
        public KenunoTim(byte[] data)
        {
            this.data_ = data;
            this.tex = ParseTex();
        }

        public class MTex
        {
            public Bitmap[] pics;
            public int[] alIndirIndex = new int[0];

            public MTex(List<Bitmap> pics)
            {
                this.pics = pics.ToArray();
            }
        }

        private MTex ParseTex()
        {
            MemoryStream si = new MemoryStream(this.data_, false);
            BinaryReader br = new BinaryReader(si);

            int v00 = br.ReadInt32();
            if (v00 == 0)
            {
                // TIMf
                si.Position = 0;
                return ParseTex_TIMf(si, br);
            }
            else if (v00 == -1)
            {
                // TIMc .. TIMformat collection
                int v04 = br.ReadInt32();

                List<int> aloff = new List<int>();
                for (int x = 0; x < v04; x++)
                    aloff.Add(br.ReadInt32());

                aloff.Add(this.data_.Length);

                List<MTex> alres = new List<MTex>();

                for (int x = 0; x < v04; x++)
                {
                    si.Position = aloff[x];
                    byte[] bin = br.ReadBytes(aloff[x + 1] - aloff[x]);
                    
                    MemoryStream siF = new MemoryStream(bin, false);
                    BinaryReader brF = new BinaryReader(siF);

                    alres.Add(ParseTex_TIMf(siF, brF));
                }
                return alres.Count != 0 ? alres[0] : null;
            }
            else throw new NotSupportedException("Unknown v00 .. " + v00);
        }

        public MTex ParseTex_TIMf(MemoryStream si, BinaryReader br)
        {
            int v00 = br.ReadInt32();
            int v04 = br.ReadInt32();
            int v08 = br.ReadInt32(); // v08: cnt-pal1? each-size=0x90
            int v0c = br.ReadInt32(); // v0c: cnt-pal2? each-size=0xA0
            int v10 = br.ReadInt32(); // v10: off-pal2-tbl
            int v14 = br.ReadInt32(); // v14: off-pal1
            int v18 = br.ReadInt32(); // v18: off-pal2
            int v1c = br.ReadInt32(); // v1c: off-tex1?
            int v20 = br.ReadInt32(); // v20: off-tex2?

            SortedDictionary<int, int> map2to1 = new SortedDictionary<int, int>();

            si.Position = v10;
            for (int p1 = 0; p1 < v0c; p1++)
            {
                map2to1[p1] = br.ReadByte();
            }

            Texctx tc = new Texctx();

            List<int> offPalfrom1 = new List<int>();

            for (int p1 = 0; p1 < v08 + 1; p1++)
            {
                //int offp1 = v14 + 0x90 * p1;
                //si.Position = offp1 + 0x20;
                //tc.Do1(si);
                //int offPal = tc.offTex;
                //offPalfrom1.Add(offPal);
            }

            List<Bitmap> pics = new List<Bitmap>();
            for (int p2 = 0; p2 < v0c; p2++)
            {
                int offp1pal = v14;
                si.Position = offp1pal + 0x20;
                tc.Do1(si);
                int offPal = tc.offBin;

                int offp1tex = v14 + 0x90 * (1 + map2to1[p2]);
                si.Position = offp1tex + 0x20;
                tc.Do1(si);
                int offTex = tc.offBin;

                int offp2 = v18 + 0xA0 * p2 + 0x20;
                si.Position = offp2;
                STim st = tc.Do2(si);
                
                pics.Add(st.Generate());
            }
            return new MTex(pics);
        }

        class MI
        {
            public SortedDictionary<string, int> col2off = new SortedDictionary<string, int>();

            public void Add(string col, int off)
            {
                col2off[col] = off;
            }
        }

        class Texctx
        {
            public byte[] gs = new byte[1024 * 1024 * 4];
            public int t0PSM;
            public int offBin;

            public void Do1(Stream si)
            {
                BinaryReader br = new BinaryReader(si);
                UInt64 cm;

                UInt64 r50 = br.ReadUInt64(); cm = br.ReadUInt64(); Debug.Assert(0x50 == cm, cm.ToString("X16") + " ≠ 0x50"); // 0x50 BITBLTBUF
                int SBP = ((int)(r50)) & 0x3FFF;
                int SBW = ((int)(r50 >> 16)) & 0x3F;
                int SPSM = ((int)(r50 >> 24)) & 0x3F;
                int DBP = ((int)(r50 >> 32)) & 0x3FFF;
                int DBW = ((int)(r50 >> 48)) & 0x3F;
                int DPSM = ((int)(r50 >> 56)) & 0x3F;
                Trace.Assert(SBP == 0);
                Trace.Assert(SBW == 0);
                Trace.Assert(SPSM == 0);
                Trace.Assert(DPSM == 0 || DPSM == 19 || DPSM == 20);

                UInt64 r51 = br.ReadUInt64(); cm = br.ReadUInt64(); Debug.Assert(0x51 == cm, cm.ToString("X16") + " ≠ 0x51"); // 0x51 TRXPOS
                int SSAX = ((int)(r51)) & 0x7FF;
                int SSAY = ((int)(r51 >> 16)) & 0x7FF;
                int DSAX = ((int)(r51 >> 32)) & 0x7FF;
                int DSAY = ((int)(r51 >> 48)) & 0x7FF;
                int DIR = ((int)(r51 >> 59)) & 3;
                Trace.Assert(SSAX == 0);
                Trace.Assert(SSAY == 0); //!
                Trace.Assert(DSAX == 0); //!
                Trace.Assert(DSAY == 0); //!
                Trace.Assert(DIR == 0); //!

                UInt64 r52 = br.ReadUInt64(); cm = br.ReadUInt64(); Debug.Assert(0x52 == cm, cm.ToString("X16") + " ≠ 0x52"); // 0x52 TRXREG
                int RRW = ((int)(r52 >> 0)) & 0xFFF;
                int RRH = ((int)(r52 >> 32)) & 0xFFF;

                UInt64 r53 = br.ReadUInt64(); cm = br.ReadUInt64(); Debug.Assert(0x53 == cm, cm.ToString("X16") + " ≠ 0x53"); // 0x53 TRXDIR
                int XDIR = ((int)(r53)) & 2;
                Trace.Assert(XDIR == 0);

                int eop = br.ReadUInt16();
                Trace.Assert(8 != (eop & 0x8000));

                si.Position += 18;
                offBin = br.ReadInt32();

                int cbTex = (eop & 0x7FFF) << 4;
                int MyDBH = cbTex / 8192 / DBW;

                {
                    byte[] binTmp = new byte[Math.Max(8192, cbTex)]; // decoder needs at least 8kb
                    si.Position = offBin;
                    si.Read(binTmp, 0, cbTex);
                    byte[] binDec;
                    if (DPSM == 0)
                    {
                        binDec = OpenKh.Kh2.Ps2.Encode32(binTmp, DBW, MyDBH);
                    }
                    else if (DPSM == 19)
                    {
                        binDec = OpenKh.Kh2.Ps2.Encode8(binTmp, DBW / 2, cbTex / 8192 / (DBW / 2));
                    }
                    else if (DPSM == 20)
                    {
                        binDec = OpenKh.Kh2.Ps2.Encode4(binTmp, DBW / 2, cbTex / 8192 / Math.Max(1, DBW / 2));
                    }
                    else
                    {
                        throw new NotSupportedException("DPSM = " + DPSM + "?");
                    }
                    Array.Copy(binDec, 0, gs, 256 * DBP, cbTex);
                }

                Debug.WriteLine(string.Format("# p1 {0:x4}      {1,6} {2}", DBP, cbTex, DPSM));
            }

            public STim Do2(Stream si)
            {
                BinaryReader br = new BinaryReader(si);
                UInt64 command;

                UInt64 r3f = br.ReadUInt64(); Trace.Assert((command = br.ReadUInt64()) == 0x3F); // 0x3F TEXFLUSH
                UInt64 r34 = br.ReadUInt64(); Trace.Assert((command = br.ReadUInt64()) == 0x34); // 0x34 MIPTBP1_1
                UInt64 r36 = br.ReadUInt64(); Trace.Assert((command = br.ReadUInt64()) == 0x36); // 0x36 MIPTBP2_1
                UInt64 r16 = br.ReadUInt64(); Trace.Assert((command = br.ReadUInt64()) == 0x16); // 0x16 TEX2_1
                int t2PSM = ((int)(r16 >> 20)) & 0x3F;
                int t2CBP = ((int)(r16 >> 37)) & 0x3FFF;
                int t2CPSM = ((int)(r16 >> 51)) & 0xF;
                int t2CSM = ((int)(r16 >> 55)) & 0x1;
                int t2CSA = ((int)(r16 >> 56)) & 0x1F;
                int t2CLD = ((int)(r16 >> 61)) & 0x7;
                Trace.Assert(t2PSM == 19); // PSMT8
                Trace.Assert(t2CPSM == 0); // PSMCT32
                Trace.Assert(t2CSM == 0); // CSM1
                Trace.Assert(t2CSA == 0);
                Trace.Assert(t2CLD == 4);

                UInt64 r14 = br.ReadUInt64(); Trace.Assert((command = br.ReadUInt64()) == 0x14);// 0x14 TEX1_1
                int t1LCM = ((int)(r14 >> 0)) & 1;
                int t1MXL = ((int)(r14 >> 2)) & 7;
                int t1MMAG = ((int)(r14 >> 5)) & 1;
                int t1MMIN = ((int)(r14 >> 6)) & 7;
                int t1MTBA = ((int)(r14 >> 9)) & 1;
                int t1L = ((int)(r14 >> 19)) & 3;
                int t1K = ((int)(r14 >> 32)) & 0xFFF;

                UInt64 r06 = br.ReadUInt64(); Trace.Assert((command = br.ReadUInt64()) == 0x06);// 0x06 TEX0_1
                int t0TBP0 = ((int)(r06 >> 0)) & 0x3FFF;
                int t0TBW = ((int)(r06 >> 14)) & 0x3F;
                t0PSM = ((int)(r06 >> 20)) & 0x3F;
                int t0TW = ((int)(r06 >> 26)) & 0xF;
                int t0TH = ((int)(r06 >> 30)) & 0xF;
                int t0TCC = ((int)(r06 >> 34)) & 0x1;
                int t0TFX = ((int)(r06 >> 35)) & 0x3;
                int t0CBP = ((int)(r06 >> 37)) & 0x3FFF;
                int t0CPSM = ((int)(r06 >> 51)) & 0xF;
                int t0CSM = ((int)(r06 >> 55)) & 0x1;
                int t0CSA = ((int)(r06 >> 56)) & 0x1F;
                int t0CLD = ((int)(r06 >> 61)) & 0x7;
                Trace.Assert(t0PSM == 19 || t0PSM == 20);
                Trace.Assert(t0TCC == 1);
                Trace.Assert(t0CPSM == 0);
                Trace.Assert(t0CSM == 0);
                //Trace.Assert(t0CSA == 0);
                Trace.Assert(t0CLD == 0);

                UInt64 r08 = br.ReadUInt64(); Trace.Assert((command = br.ReadUInt64()) == 0x08);// 0x08 CLAMP_1
                int c1WMS = ((int)(r08 >> 0)) & 0x3;
                int c1WMT = ((int)(r08 >> 2)) & 0x3;
                int c1MINU = ((int)(r08 >> 4)) & 0x3FF;
                int c1MAXU = ((int)(r08 >> 14)) & 0x3FF;
                int c1MINV = ((int)(r08 >> 24)) & 0x3FF;
                int c1MAXV = ((int)(r08 >> 34)) & 0x3FF;

                int sizetbp0 = (1 << t0TW) * (1 << t0TH);
                byte[] buftbp0 = new byte[Math.Max(8192, sizetbp0)]; // needs at least 8kb
                Array.Copy(gs, 256 * t0TBP0, buftbp0, 0, Math.Min(gs.Length - 256 * t0TBP0, Math.Min(buftbp0.Length, sizetbp0)));
                byte[] bufcbpX = new byte[8192];
                Array.Copy(gs, 256 * t0CBP, bufcbpX, 0, bufcbpX.Length);

                Debug.WriteLine(string.Format("# p2 {0:x4} {1:x4} {2,2}", t0TBP0, t0CBP, t0CSA));

                STim st = null;
                if (t0PSM == 0x13) st = TexUt2.TexUt2.Decode8(buftbp0, bufcbpX, t0TBW, 1 << t0TW, 1 << t0TH);
                if (t0PSM == 0x14) st = TexUt2.TexUt2.Decode4Ps(buftbp0, bufcbpX, t0TBW, 1 << t0TW, 1 << t0TH, t0CSA);
                if (st != null)
                {
                    st.tfx = (TFX)t0TFX;
                    st.tcc = (TCC)t0TCC;
                    st.wms = (WM)c1WMS;
                    st.wmt = (WM)c1WMT;
                    st.minu = c1MINU;
                    st.maxu = c1MAXU;
                    st.minv = c1MINV;
                    st.maxv = c1MAXV;
                }
                return st;
            }
        }
        class Texi : Hexi
        {
            public STim st;

            public Texi(int off, STim st)
                : base(off)
            {
                this.st = st;
            }
            public Texi(int off, MI mi, STim st)
                : base(off, mi)
            {
                this.st = st;
            }
        }

        class Vifi : Hexi
        {
            public byte[] vifpkt;

            public Vifi(int off, byte[] vifpkt)
                : base(off)
            {
                this.vifpkt = vifpkt;
            }
            public Vifi(int off, MI mi, byte[] vifpkt)
                : base(off, mi)
            {
                this.vifpkt = vifpkt;
            }
        }
        class Hexi
        {
            public int off, len;
            public MI mi = null;

            public Hexi(int off)
            {
                this.off = off;
                this.len = 0;
            }
            public Hexi(int off, int len)
            {
                this.off = off;
                this.len = len;
            }
            public Hexi(int off, MI mi)
            {
                this.off = off;
                this.mi = mi;
            }
        }



    }
}
