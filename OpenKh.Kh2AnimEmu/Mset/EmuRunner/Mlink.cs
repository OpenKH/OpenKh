//#define UsePressed_eeram
//#define AllowRec1
//#define Allow_DEB_eeram01

using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Collections;
using System.Xml;
using System.IO.Compression;

namespace OpenKh.Kh2Anim.Mset.EmuRunner
{
    public class Mlink
    {
        Mobrc1 o1 = new Mobrc1();
        CustEE ee = new CustEE();

        int cntPass = 0;
        uint offMdlx04 = uint.MaxValue;

        class CnfUt
        {
            public static string findeeram
            {
                get
                {
                    var file = "rawData/ee.mset.ram.bin.gz";
                    {
                        string path = Path.Combine(Environment.CurrentDirectory, file);
                        if (File.Exists(path))
                        {
                            return path;
                        }
                    }
                    {
                        string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, file);
                        if (File.Exists(path))
                        {
                            return path;
                        }
                    }
                    throw new FileNotFoundException($"Please deploy '{file}' for MSET emulation!");
                }
            }
        }

        public void Permit(Stream fsMdlx, int cnt1, Stream fsMset, int cnt2, uint offMsetRxxx, float tick, MemoryStream os)
        {
            if (cntPass == 0)
            {
                o1.Init0();
                o1.Init1(ee);
            }
            if (cntPass == 0)
            {
#if UsePressed_eeram
                Szexp.Decode(Resources.eeramx, ee.ram, ee.ram.Length);
#else
                using (FileStream fsi = File.OpenRead(CnfUt.findeeram))
                using (GZipStream gz = new GZipStream(fsi, CompressionMode.Decompress))
                {
                    gz.Read(ee.ram, 0, 32 * 1024 * 1024);
                }
#endif
            }

            uint tmp1 = (32 * 1024 * 1024) - (768) - (65536);
            uint tmp2 = (32 * 1024 * 1024) - (768) - (65536) - (65536);
            uint tmp3 = (32 * 1024 * 1024) - (768) - (65536) - (65536) - (65536);
            uint tmp4 = (32 * 1024 * 1024) - (768) - (65536) - (65536) - (65536) - (65536);
            uint tmp5 = (32 * 1024 * 1024) - (768) - (65536) - (65536) - (65536) - (65536) - (16 * 512);
            uint tmp6 = (32 * 1024 * 1024) - (768) - (65536) - (65536) - (65536) - (65536) - (16 * 512) - (16 * 512);
            uint tmp7 = (32 * 1024 * 1024) - (768) - (65536) - (65536) - (65536) - (65536) - (16 * 512) - (16 * 512) - (16 * 512);
            uint tmp8 = (32 * 1024 * 1024) - (768) - (65536) - (65536) - (65536) - (65536) - (16 * 512) - (16 * 512) - (16 * 512) - (65536);
            uint tmp9 = (32 * 1024 * 1024) - (768) - (65536) - (65536) - (65536) - (65536) - (16 * 512) - (16 * 512) - (16 * 512) - (65536) - (65536);
            uint tmpa = (32 * 1024 * 1024) - (768) - (65536) - (65536) - (65536) - (65536) - (16 * 512) - (16 * 512) - (16 * 512) - (65536) - (65536) - (65536);
            uint tmpb = (32 * 1024 * 1024) - (768) - (65536) - (65536) - (65536) - (65536) - (16 * 512) - (16 * 512) - (16 * 512) - (65536) - (65536) - (65536) - (65536);
            //  tmp1    1abb0b0 64k [out] temp
            //  tmp2    1abb590 64k [in] st1
            //  tmp3    1ac7410 64k [out] temp
            //  tmp4    1acad50 64k [out] (4x4 matrix for mdlx bone calc)
            //  tmp5    1abb6d0 8k  [out] (Sxyz)
            //  tmp6    1abc520 8k  [out] (Rxyz)
            //  tmp7    1abd370 8k  [out] (Txyz)
            //  tmp8    3b1870  64k [out] (4x4 matrix)  size=64*(cnt2-cnt1)
            //  tmp9    3b5eb0  64k [out] (4x4 matrix)  size=64*(cnt2-cnt1)
            //  tmpa    3b6bb0  64k [out] (4x4 matrix)  size=64*(cnt2-cnt1)
            //  tmpb    3b78b0  64k [out] (s.r.t.?)

            uint Sxyz = tmp5;
            uint Rxyz = tmp6;
            uint Txyz = tmp7;

            uint offMdlxRoot = 10 * 1024 * 1024;
            if (cntPass == 0)
            {
                fsMdlx.Read(ee.ram, (int)offMdlxRoot, 5 * 1024 * 1024);

                offMdlx04 = new RelocMdlx(ee.ram, (int)offMdlxRoot, (int)offMdlxRoot, 0x354398, 0, tmp2, 1).Run();
            }

            uint s4 = tmp1; // temp;
            uint s2 = tmp2; // st1;
            uint a1 = s2;

            for (int w = 0; w < 65536; w++) ee.ram[w] = 0;

            if (true)
            {
                MemoryStream wri = new MemoryStream(ee.ram, true);
                wri.Position = s2;
                BinaryWriter wr = new BinaryWriter(wri);
                uint[] st1al = new uint[] {
                    0x0       ,0         ,0    ,0,
                    0x0       ,offMdlx04 ,0x0  ,Sxyz,
                    Rxyz      ,Txyz      ,tmp3 ,tmp4,
                    0x0       ,0x0       ,0x0  ,0,
                };
                foreach (uint ui in st1al) wr.Write(ui);
            }

            uint offMsetRoot = 15 * 1024 * 1024;
            if (cntPass == 0)
            {
                fsMset.Read(ee.ram, (int)offMsetRoot, 17 * 1024 * 1024);

                RelocMset RM = new RelocMset(ee.ram, offMsetRoot, offMsetRoot, new uint[] { 0, 0, tmp8, tmp9, tmpa, tmpb, });
                RM.Run();
            }

            //uint offMset = 0x009E0340 + 0x1D390;
            //uint offMset = offMsetRoot + 0x3370;
            uint offMset = offMsetRoot + offMsetRxxx;

            // s0, s1, s2, s4, a1
            uint s1 = offMset;
            uint a0 = offMset;
            uint s0 = offMset + 0x90;

            ee.VF[0].w = 1;

            // Opt3
            if (true)
            {
                //ee.r0.UD0 = 0U;
                ee.at.UD0 = 0U;
                ee.v0.UD0 = 0U;
                ee.v1.UD0 = 0U;
                ee.a0.UD0 = a0; // mset +0x00
                ee.a1.UD0 = a1; // info tbl
                ee.a2.UD0 = 0U;
                ee.a3.UD0 = 0U;
                ee.t0.UD0 = 0U;
                ee.t1.UD0 = 0U;
                ee.t2.UD0 = 0U;
                ee.t3.UD0 = 0U;
                ee.t4.UD0 = 0U;
                ee.t5.UD0 = 0U;
                ee.t6.UD0 = 0U;
                ee.t7.UD0 = 0U;
                ee.s0.UD0 = 0U; // s0; // mset +0x90
                ee.s1.UD0 = 0U;
                ee.s2.UD0 = 0U;
                ee.s3.UD0 = 0U;
                ee.s4.UD0 = s4; // temp?
                ee.s5.UD0 = 0U;
                ee.s6.UD0 = 0U;
                ee.s7.UD0 = 0U;
                ee.t8.UD0 = 0U;
                ee.t9.UD0 = 0U;
                ee.k0.UD0 = 0U;
                ee.k1.UD0 = 0U;
                ee.gp.UD0 = 0U;
                ee.sp.UD0 = 0x2000000U;
                ee.s8.UD0 = 0U;
                ee.ra.UD0 = 0xFFFFFFFFU;

                ee.pc = 0x128260;
                while (ee.pc != 0xFFFFFFFFU)
                {
                    if (o1.pfns.ContainsKey(ee.pc) || MobRecUt.Rec1(ee.pc, o1.pfns, ee))
                    {
                        o1.pfns[ee.pc]();
                    }
                    else throw new RecfnnotFound(ee.pc, "rc3");
                }
            }
            // Opt2
            if (true)
            {
                //ee.r0.UD0 = 0U;
                ee.at.UD0 = 0U;
                ee.v0.UD0 = 0U;
                ee.v1.UD0 = 0U;
                ee.a0.UD0 = a0; // mset +0x00
                ee.a1.UD0 = a1; // info tbl
                ee.a2.UD0 = 0U;
                ee.a3.UD0 = 0U;
                ee.t0.UD0 = 0U;
                ee.t1.UD0 = 0U;
                ee.t2.UD0 = 0U;
                ee.t3.UD0 = 0U;
                ee.t4.UD0 = 0U;
                ee.t5.UD0 = 0U;
                ee.t6.UD0 = 0U;
                ee.t7.UD0 = 0U;
                ee.s0.UD0 = s0; // mset +0x90
                ee.s1.UD0 = s1;
                ee.s2.UD0 = s2;
                ee.s3.UD0 = 0U;
                ee.s4.UD0 = s4; // temp?
                ee.s5.UD0 = 0U;
                ee.s6.UD0 = 0U;
                ee.s7.UD0 = 0U;
                ee.t8.UD0 = 0U;
                ee.t9.UD0 = 0U;
                ee.k0.UD0 = 0U;
                ee.k1.UD0 = 0U;
                ee.gp.UD0 = 0U;
                ee.sp.UD0 = 0x2000000U;
                ee.s8.UD0 = 0U;
                ee.ra.UD0 = 0xFFFFFFFFU;

                ee.fpr[12].f = tick;

                ee.pc = 0x128918;
                while (ee.pc != 0xFFFFFFFFU)
                {
                    if (o1.pfns.ContainsKey(ee.pc) || MobRecUt.Rec1(ee.pc, o1.pfns, ee))
                    {
                        o1.pfns[ee.pc]();
                    }
                    else throw new RecfnnotFound(ee.pc, "rc2");
                }
            }
            // Opt1
            if (true)
            {
                ee.at.UD0 = 0U;
                ee.v0.UD0 = 0U;
                ee.v1.UD0 = 0U;
                ee.a0.UD0 = a0; // mset +0x00
                ee.a1.UD0 = a1; // info tbl
                ee.a2.UD0 = 0U;
                ee.a3.UD0 = 0U;
                ee.t0.UD0 = 0U;
                ee.t1.UD0 = 0U;
                ee.t2.UD0 = 0U;
                ee.t3.UD0 = 0U;
                ee.t4.UD0 = 0U;
                ee.t5.UD0 = 0U;
                ee.t6.UD0 = 0U;
                ee.t7.UD0 = 0U;
                ee.s0.UD0 = s0; // mset +0x90
                ee.s1.UD0 = 0U;
                ee.s2.UD0 = 0U;
                ee.s3.UD0 = 0U;
                ee.s4.UD0 = s4; // temp?
                ee.s5.UD0 = 0U;
                ee.s6.UD0 = 0U;
                ee.s7.UD0 = 0U;
                ee.t8.UD0 = 0U;
                ee.t9.UD0 = 0U;
                ee.k0.UD0 = 0U;
                ee.k1.UD0 = 0U;
                ee.gp.UD0 = 0U;
                ee.sp.UD0 = 0x2000000U;
                ee.s8.UD0 = 0U;
                ee.ra.UD0 = 0xFFFFFFFFU;

                ee.pc = 0x129A18;
                while (ee.pc != 0xFFFFFFFFU)
                {
                    if (o1.pfns.ContainsKey(ee.pc) || MobRecUt.Rec1(ee.pc, o1.pfns, ee))
                    {
                        o1.pfns[ee.pc]();
                    }
                    else
                    {
                        throw new RecfnnotFound(ee.pc, "rc1");
                    }
                }
            }

            os.Write(ee.ram, (int)tmp4, 0x40 * cnt1);

            cntPass++;

        }

        public void Permit_DEB(Stream fsMdlx, int cnt1, Stream fsMset, int cnt2, uint offMsetRxxx, float tick, out float[] Svec, out float[] Rvec, out float[] Tvec)
        {
            if (cntPass == 0)
            {
                o1.Init0();
                o1.Init1(ee);
            }
            if (cntPass == 0)
            {
#if UsePressed_eeram
                Szexp.Decode(Resources.eeramx, ee.ram, ee.ram.Length);
#else
                using (FileStream fsi = File.OpenRead(CnfUt.findeeram))
                using (GZipStream gz = new GZipStream(fsi, CompressionMode.Decompress))
                {
                    gz.Read(ee.ram, 0, 32 * 1024 * 1024);
                }
#endif
            }

            uint tmp1 = (32 * 1024 * 1024) - (768) - (65536);
            uint tmp2 = (32 * 1024 * 1024) - (768) - (65536) - (65536);
            uint tmp3 = (32 * 1024 * 1024) - (768) - (65536) - (65536) - (65536);
            uint tmp4 = (32 * 1024 * 1024) - (768) - (65536) - (65536) - (65536) - (65536);
            uint tmp5 = (32 * 1024 * 1024) - (768) - (65536) - (65536) - (65536) - (65536) - (16 * 512);
            uint tmp6 = (32 * 1024 * 1024) - (768) - (65536) - (65536) - (65536) - (65536) - (16 * 512) - (16 * 512);
            uint tmp7 = (32 * 1024 * 1024) - (768) - (65536) - (65536) - (65536) - (65536) - (16 * 512) - (16 * 512) - (16 * 512);
            uint tmp8 = (32 * 1024 * 1024) - (768) - (65536) - (65536) - (65536) - (65536) - (16 * 512) - (16 * 512) - (16 * 512) - (65536);
            uint tmp9 = (32 * 1024 * 1024) - (768) - (65536) - (65536) - (65536) - (65536) - (16 * 512) - (16 * 512) - (16 * 512) - (65536) - (65536);
            uint tmpa = (32 * 1024 * 1024) - (768) - (65536) - (65536) - (65536) - (65536) - (16 * 512) - (16 * 512) - (16 * 512) - (65536) - (65536) - (65536);
            uint tmpb = (32 * 1024 * 1024) - (768) - (65536) - (65536) - (65536) - (65536) - (16 * 512) - (16 * 512) - (16 * 512) - (65536) - (65536) - (65536) - (65536);
            //  tmp1    1abb0b0 64k [out] temp
            //  tmp2    1abb590 64k [in] st1
            //  tmp3    1ac7410 64k [out] temp
            //  tmp4    1acad50 64k [out] (4x4 matrix for mdlx bone calc)
            //  tmp5    1abb6d0 8k  [out] (Sxyz)
            //  tmp6    1abc520 8k  [out] (Rxyz)
            //  tmp7    1abd370 8k  [out] (Txyz)
            //  tmp8    3b1870  64k [out] (4x4 matrix)  size=64*(cnt2-cnt1)
            //  tmp9    3b5eb0  64k [out] (4x4 matrix)  size=64*(cnt2-cnt1)
            //  tmpa    3b6bb0  64k [out] (4x4 matrix)  size=64*(cnt2-cnt1)
            //  tmpb    3b78b0  64k [out] (s.r.t.?)

            uint Sxyz = tmp5;
            uint Rxyz = tmp6;
            uint Txyz = tmp7;

            uint offMdlxRoot = 20 * 1024 * 1024;
            if (cntPass == 0)
            {
                fsMdlx.Read(ee.ram, (int)offMdlxRoot, 5 * 1024 * 1024);

                offMdlx04 = new RelocMdlx(ee.ram, (int)offMdlxRoot, (int)offMdlxRoot, 0x354398, 0, tmp2, 1).Run();
            }

            uint s4 = tmp1; // temp;
            uint s2 = tmp2; // st1;
            uint a1 = s2;

            for (int w = 0; w < 65536; w++) ee.ram[w] = 0;

            if (true)
            {
                MemoryStream wri = new MemoryStream(ee.ram, true);
                wri.Position = s2;
                BinaryWriter wr = new BinaryWriter(wri);
                uint[] st1al = new uint[] {
                    0x0       ,0         ,0    ,0,
                    0x0       ,offMdlx04 ,0x0  ,Sxyz,
                    Rxyz      ,Txyz      ,tmp3 ,tmp4,
                    0x0       ,0x0       ,0x0  ,0,
                };
                foreach (uint ui in st1al) wr.Write(ui);
            }

            uint offMsetRoot = 25 * 1024 * 1024;
            if (cntPass == 0)
            {
                fsMset.Read(ee.ram, (int)offMsetRoot, 5 * 1024 * 1024);

                RelocMset RM = new RelocMset(ee.ram, offMsetRoot, offMsetRoot, new uint[] { 0, 0, tmp8, tmp9, tmpa, tmpb, });
                RM.Run();
            }

            //uint offMset = 0x009E0340 + 0x1D390;
            //uint offMset = offMsetRoot + 0x3370;
            uint offMset = offMsetRoot + offMsetRxxx;

            // s0, s1, s2, s4, a1
            uint s1 = offMset;
            uint a0 = offMset;
            uint s0 = offMset + 0x90;

            ee.VF[0].w = 1;

            // Opt3
            if (true)
            {
                //ee.r0.UD0 = 0U;
                ee.at.UD0 = 0U;
                ee.v0.UD0 = 0U;
                ee.v1.UD0 = 0U;
                ee.a0.UD0 = a0; // mset +0x00
                ee.a1.UD0 = a1; // info tbl
                ee.a2.UD0 = 0U;
                ee.a3.UD0 = 0U;
                ee.t0.UD0 = 0U;
                ee.t1.UD0 = 0U;
                ee.t2.UD0 = 0U;
                ee.t3.UD0 = 0U;
                ee.t4.UD0 = 0U;
                ee.t5.UD0 = 0U;
                ee.t6.UD0 = 0U;
                ee.t7.UD0 = 0U;
                ee.s0.UD0 = 0U; // s0; // mset +0x90
                ee.s1.UD0 = 0U;
                ee.s2.UD0 = 0U;
                ee.s3.UD0 = 0U;
                ee.s4.UD0 = s4; // temp?
                ee.s5.UD0 = 0U;
                ee.s6.UD0 = 0U;
                ee.s7.UD0 = 0U;
                ee.t8.UD0 = 0U;
                ee.t9.UD0 = 0U;
                ee.k0.UD0 = 0U;
                ee.k1.UD0 = 0U;
                ee.gp.UD0 = 0U;
                ee.sp.UD0 = 0x2000000U;
                ee.s8.UD0 = 0U;
                ee.ra.UD0 = 0xFFFFFFFFU;

                ee.pc = 0x128260;
                while (ee.pc != 0xFFFFFFFFU)
                {
                    if (o1.pfns.ContainsKey(ee.pc) || MobRecUt.Rec1(ee.pc, o1.pfns, ee))
                    {
                        o1.pfns[ee.pc]();
                    }
                    else throw new RecfnnotFound(ee.pc, "rc3");
                }
            }
            // Opt2
            if (true)
            {
                //ee.r0.UD0 = 0U;
                ee.at.UD0 = 0U;
                ee.v0.UD0 = 0U;
                ee.v1.UD0 = 0U;
                ee.a0.UD0 = a0; // mset +0x00
                ee.a1.UD0 = a1; // info tbl
                ee.a2.UD0 = 0U;
                ee.a3.UD0 = 0U;
                ee.t0.UD0 = 0U;
                ee.t1.UD0 = 0U;
                ee.t2.UD0 = 0U;
                ee.t3.UD0 = 0U;
                ee.t4.UD0 = 0U;
                ee.t5.UD0 = 0U;
                ee.t6.UD0 = 0U;
                ee.t7.UD0 = 0U;
                ee.s0.UD0 = s0; // mset +0x90
                ee.s1.UD0 = s1;
                ee.s2.UD0 = s2;
                ee.s3.UD0 = 0U;
                ee.s4.UD0 = s4; // temp?
                ee.s5.UD0 = 0U;
                ee.s6.UD0 = 0U;
                ee.s7.UD0 = 0U;
                ee.t8.UD0 = 0U;
                ee.t9.UD0 = 0U;
                ee.k0.UD0 = 0U;
                ee.k1.UD0 = 0U;
                ee.gp.UD0 = 0U;
                ee.sp.UD0 = 0x2000000U;
                ee.s8.UD0 = 0U;
                ee.ra.UD0 = 0xFFFFFFFFU;

                ee.fpr[12].f = tick;

                ee.pc = 0x128918;
                while (ee.pc != 0xFFFFFFFFU)
                {
                    if (o1.pfns.ContainsKey(ee.pc) || MobRecUt.Rec1(ee.pc, o1.pfns, ee))
                    {
                        o1.pfns[ee.pc]();
                    }
                    else throw new RecfnnotFound(ee.pc, "rc2");
                }
            }

            {
                MemoryStream si = new MemoryStream(ee.ram, false);
                BinaryReader br = new BinaryReader(si);

                si.Position = Sxyz;
                Svec = new float[4 * cnt1];
                for (int a = 0; a < 4 * cnt1; a++) Svec[a] = br.ReadSingle();

                si.Position = Rxyz;
                Rvec = new float[4 * cnt1];
                for (int a = 0; a < 4 * cnt1; a++) Rvec[a] = br.ReadSingle();

                si.Position = Txyz;
                Tvec = new float[4 * cnt1];
                for (int a = 0; a < 4 * cnt1; a++) Tvec[a] = br.ReadSingle();
            }

            cntPass++;

        }

        public void DEB()
        {
#if Allow_DEB_eeram01
            MobUt.bitr[0x378368 >> 4] = true;
            MobUt.bitr[0x378378 >> 4] = true;
            MobUt.bitr[0x378378 >> 4] = true;
            MobUt.bitr[0x378398 >> 4] = true;
            MobUt.bitr[0x3783a8 >> 4] = true;
            MobUt.bitr[0x3783b8 >> 4] = true;

            BitArray bita = MobUt.bita;
            BitArray bitr = MobUt.bitr;
            if (File.Exists(Path.Combine(Settings.Default.pressdir, "save_DEB.bin"))) {
                using (FileStream fsx = File.OpenRead(Path.Combine(Settings.Default.pressdir, "save_DEB.bin"))) {
                    BinaryFormatter bf = new BinaryFormatter();
                    BitArray b;
                    bita = bita.Or(b = (BitArray)bf.Deserialize(fsx));
                    bitr = bitr.Or(b = (BitArray)bf.Deserialize(fsx));
                }
            }

            using (FileStream fs = File.Create(Path.Combine(Settings.Default.pressdir, "eeram01.bin"))) {
                byte[] noop16 = new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, };
                for (int x = 0; x < 2097152; x++) {
                    if ((bitr[x] || bitr[Math.Max(0, x - 1)] || bitr[Math.Min(2097152 - 1, x + 1)]) && x < 20 * 1024 * 1024 / 16) {
                        fs.Write(ee.ram, 16 * x, 16);
                    }
                    else {
                        fs.Write(noop16, 0, 16);
                    }
                }
            }

            if (true) {
                using (FileStream fsx = File.Create(Path.Combine(Settings.Default.pressdir, "save_DEB.bin"))) {
                    BinaryFormatter bf = new BinaryFormatter();
                    bf.Serialize(fsx, bita);
                    bf.Serialize(fsx, bitr);
                }
            }
#endif
        }
    }

    public partial class Mobrc1
    {
        void here(uint pc) { }

        internal SortedDictionary<uint, MobUt.Tx8> pfns { get { return dicti2a; } }
        internal void Init0() { initfns(); }
        internal void Init1(CustEE ee) { this.ee = ee; }
    }

    class Uteeram
    {
        class CnfUt
        {
            public static string findeeram
            {
                get
                {
                    string[] al = new string[] {
                        @"H:\Proj\khkh_xldM\MEMO\expSim\rc1\eeram.bin",
                        Path.Combine(Environment.CurrentDirectory, "eeram.bin"),
                    };
                    foreach (string s in al)
                    {
                        if (File.Exists(s)) return s;
                    }
                    return al[1];
                }
            }
        }

        public static byte[] eeram = File.ReadAllBytes(CnfUt.findeeram);
    }

    class MobRecUt
    {
        public delegate void Tx8();

        public static bool Rec1(uint addr, SortedDictionary<uint, MobUt.Tx8> dicti2a, CustEE ee)
        {
#if AllowRec1
            string dirlib = Settings.Default.dirlib;
            string flib = Myrec.Getflib(addr, dirlib);
            if (!File.Exists(flib)) {
                Myrec.Privrec1(addr, new MemoryStream(Uteeram.eeram, false), dirlib);
                if (!File.Exists(flib)) return false;
            }

            Assembly lib = Assembly.LoadFile(flib);
            Type cls1 = lib.GetType("ee1Dec.C.Class1");
            object o = Activator.CreateInstance(cls1, ee);
            MethodInfo mi = cls1.GetMethod(LabUt.addr2Funct(addr));
            MobUt.Tx8 tx8 = (MobUt.Tx8)Delegate.CreateDelegate(typeof(MobUt.Tx8), o, mi);
            dicti2a[addr] = tx8;
            System.Diagnostics.Debug.WriteLine("## " + addr.ToString("X8"));
            return true;
#else
            return false;
#endif
        }
    }
}
