using OpenKh.Kh2Anim.Legacy;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace OpenKh.Kh2Anim.Mset
{
    public class AnimReader
    {
        public AnimModel model = new AnimModel();
        public int cntb1, cntb2;

        public AnimReader(Stream si)
        {
            var posTbl = new PosTbl(si);
            int baseoff = 0;
            int tbloff = posTbl.tbloff;

            cntb1 = posTbl.va0;
            cntb2 = posTbl.va2;

            var br = new BinaryReader(si);

            // min
            br.ReadSingle();
            br.ReadSingle();
            br.ReadSingle();
            br.ReadSingle();
            // max
            br.ReadSingle();
            br.ReadSingle();
            br.ReadSingle();
            br.ReadSingle();

            FrameLoop = br.ReadSingle();
            FrameEnd = br.ReadSingle();
            FramePerSecond = br.ReadSingle();
            FrameCount = br.ReadSingle();

            int cnt9 = 0, cnt10 = 0, cnt12 = 0;
            if (true)
            { // cnt9
                si.Position = baseoff + tbloff + posTbl.vc0 - baseoff; // t2
                for (int i2 = 0; i2 < posTbl.vc4; i2++)
                {
                    br.ReadByte();
                    br.ReadByte();
                    br.ReadByte();
                    int tcx = br.ReadByte();
                    int tx = br.ReadUInt16();
                    cnt9 = Math.Max(cnt9, tx + tcx);
                }
                si.Position = baseoff + tbloff + posTbl.vc8 - baseoff; // t2x
                for (int i2 = 0; i2 < posTbl.vcc; i2++)
                {
                    br.ReadByte();
                    br.ReadByte();
                    br.ReadByte();
                    int tcx = br.ReadByte();
                    int tx = br.ReadUInt16();
                    cnt9 = Math.Max(cnt9, tx + tcx);
                }

                if (true)
                { // cnt10, cnt12
                    si.Position = baseoff + tbloff + posTbl.vd0 - baseoff; // t9
                    for (int i9 = 0; i9 < cnt9; i9++)
                    {
                        br.ReadUInt16();
                        int ti10 = br.ReadUInt16(); cnt10 = Math.Max(cnt10, ti10 + 1);
                        int ti12a = br.ReadUInt16(); cnt12 = Math.Max(cnt12, ti12a + 1);
                        int ti12b = br.ReadUInt16(); cnt12 = Math.Max(cnt12, ti12b + 1);
                    }
                }
            }
            int cntt8 = 0;
            if (true)
            {
                si.Position = baseoff + tbloff + posTbl.ve0 - baseoff; // t3
                for (int i3 = 0; i3 < posTbl.ve4; i3++)
                {
                    br.ReadUInt16();
                    br.ReadUInt16();
                    br.ReadUInt16();
                    int ti8 = br.ReadInt16(); cntt8 = Math.Max(cntt8, ti8 + 1);
                    br.ReadUInt16();
                    br.ReadUInt16();
                }
            }

            int off1 = tbloff + posTbl.vb4; int cnt1 = posTbl.vb8;
            si.Position = off1;
            for (int a1 = 0; a1 < cnt1; a1++)
            {
                int c00 = br.ReadUInt16();
                int c02 = br.ReadUInt16();
                float c04 = br.ReadSingle();
                model.t1List.Add(new T1(c00, c02, c04));
            }

            int off10 = tbloff + posTbl.vd8;
            si.Position = off10;
            model.t10List = new float[cnt10];
            for (int a = 0; a < cnt10; a++)
            {
                model.t10List[a] = br.ReadSingle();
            }

            int off11 = tbloff + posTbl.vd4; int cnt11 = posTbl.vb0;
            si.Position = off11;
            model.t11List = new float[cnt11];
            for (int a = 0; a < cnt11; a++)
            {
                model.t11List[a] = br.ReadSingle();
            }

            int off12 = tbloff + posTbl.vdc;
            si.Position = off12;
            model.t12List = new float[cnt12];
            for (int a = 0; a < cnt12; a++)
            {
                model.t12List[a] = br.ReadSingle();
            }

            int off9 = tbloff + posTbl.vd0;
            si.Position = off9;
            for (int a = 0; a < cnt9; a++)
            {
                int c00 = br.ReadUInt16();
                int c02 = br.ReadUInt16();
                int c04 = br.ReadUInt16();
                int c06 = br.ReadUInt16();
                model.t9List.Add(new T9(c00, c02, c04, c06));
            }

            int off2 = tbloff + posTbl.vc0; int cnt2 = posTbl.vc4;
            si.Position = off2;
            for (int a = 0; a < cnt2; a++)
            {
                int c00 = br.ReadByte();
                int c01 = br.ReadByte();
                int c02 = br.ReadByte();
                int c03 = br.ReadByte();
                int c04 = br.ReadUInt16(); // t9_xxxx
                T2 o2 = new T2(c00, c01, c02, c03, c04);
                model.t2List.Add(o2);
            }
            for (int a = 0; a < cnt2; a++)
            {
                T2 o2 = model.t2List[a];
                for (int b = 0; b < o2.c03; b++)
                {
                    T9 o9 = model.t9List[o2.c04 + b];

                    int t9c00 = o9.c00; // t11_xxxx
                    int t9c02 = o9.c02; // t10_xxxx
                    int t9c04 = o9.c04; // t12_xxxx
                    int t9c06 = o9.c06; // t12_xxxx

                    o2.al9f.Add(new T9f(o2.c04 + b, model.t11List[t9c00 >> 2], model.t10List[t9c02], model.t12List[t9c04], model.t12List[t9c06]));
                }
            }

            int off2x = tbloff + posTbl.vc8; int cnt2x = posTbl.vcc;
            si.Position = off2x;
            for (int a = 0; a < cnt2x; a++)
            {
                int c00 = br.ReadByte();
                int c01 = br.ReadByte();
                int c02 = br.ReadByte();
                int c03 = br.ReadByte();
                int c04 = br.ReadUInt16(); // t9_xxxx
                T2 o2 = new T2(c00, c01, c02, c03, c04);
                model.t2xList.Add(o2);
            }
            for (int a = 0; a < cnt2x; a++)
            {
                T2 o2 = model.t2xList[a];
                for (int b = 0; b < o2.c03; b++)
                {
                    T9 o9 = model.t9List[o2.c04 + b];

                    int t9c00 = o9.c00; // t11_xxxx
                    int t9c02 = o9.c02; // t10_xxxx
                    int t9c04 = o9.c04; // t12_xxxx
                    int t9c06 = o9.c06; // t12_xxxx

                    o2.al9f.Add(new T9f(o2.c04 + b, model.t11List[t9c00 >> 2], model.t10List[t9c02], model.t12List[t9c04], model.t12List[t9c06]));
                }
            }

            int off3 = tbloff + posTbl.ve0; int cnt3 = posTbl.ve4;
            si.Position = off3;
            for (int a3 = 0; a3 < cnt3; a3++)
            {
                int c00 = br.ReadByte();
                int c01 = br.ReadByte();
                int c02 = br.ReadUInt16();
                int c04 = br.ReadUInt16();
                int c06 = br.ReadUInt16();
                uint c08 = br.ReadUInt32();
                model.t3List.Add(new T3(c00, c01, c02, c04, c06, c08));
            }

            int off4 = tbloff + posTbl.vac; int cnt4 = posTbl.va2;
            si.Position = off4;
            for (int a4 = 0; a4 < cnt4; a4++)
            {
                int c00 = br.ReadUInt16();
                int c02 = br.ReadUInt16();
                model.t4List.Add(new T4(c00, c02));
            }

            model.off5 = tbloff + posTbl.va8; model.cnt5 = (posTbl.va2 - posTbl.va0);
            si.Position = model.off5;
            for (int a5 = 0; a5 < model.cnt5; a5++)
            {
                AxBone o = new AxBone();
                o.cur = br.ReadUInt16();
                o.parent = br.ReadUInt16();
                o.v08 = br.ReadUInt16();
                o.v0c = br.ReadUInt16();
                br.ReadUInt64();
                o.x1 = br.ReadSingle();
                o.y1 = br.ReadSingle();
                o.z1 = br.ReadSingle();
                o.w1 = br.ReadSingle();
                o.x2 = br.ReadSingle();
                o.y2 = br.ReadSingle();
                o.z2 = br.ReadSingle();
                o.w2 = br.ReadSingle();
                o.x3 = br.ReadSingle();
                o.y3 = br.ReadSingle();
                o.z3 = br.ReadSingle();
                o.w3 = br.ReadSingle();
                model.t5List.Add(o);
            }
        }

        public float FrameLoop { get; }
        public float FrameEnd { get; }
        public float FramePerSecond { get; }
        public float FrameCount { get; }
    }
}
