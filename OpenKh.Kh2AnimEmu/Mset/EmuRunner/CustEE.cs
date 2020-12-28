//#define UseUnsafe
#define UseBitConverter

using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.IO;
using System.Collections;
using System.Runtime.InteropServices;

namespace OpenKh.Kh2Anim.Mset.EmuRunner
{
    public class UtUB
    {
        uint[] UL;
        bool isR0;

        public UtUB(uint[] UL, bool isR0)
        {
            this.UL = UL;
            this.isR0 = isR0;
        }

        public byte this[int x]
        {
            get
            {
                switch (x & 3)
                {
                    default: return (byte)((UL[x >> 2]));
                    case 1: return (byte)((UL[x >> 2] >> 8));
                    case 2: return (byte)((UL[x >> 2] >> 16));
                    case 3: return (byte)((UL[x >> 2] >> 24));
                }
            }
            set
            {
                if (isR0) return;// throw new IsR0Exception();
                int v = x >> 2;
                switch (x & 3)
                {
                    default: UL[v] = (UL[v] & 0xFFFFFF00U) | (value); break;
                    case 1: UL[v] = (UL[v] & 0xFFFF00FFU) | (uint)(value << 8); break;
                    case 2: UL[v] = (UL[v] & 0xFF00FFFFU) | (uint)(value << 16); break;
                    case 3: UL[v] = (UL[v] & 0x00FFFFFFU) | (uint)(value << 24); break;
                }
            }
        }
    }
    public class UtUS
    {
        uint[] UL;
        bool isR0;

        public UtUS(uint[] UL, bool isR0)
        {
            this.UL = UL;
            this.isR0 = isR0;
        }

        public ushort this[int x]
        {
            get
            {
                return ((x & 1) == 0) ? ((ushort)(UL[x >> 1])) : ((ushort)(UL[x >> 1] >> 16));
            }
            set
            {
                if (isR0) return;// throw new IsR0Exception();
                if ((x & 1) == 0)
                {
                    x >>= 1;
                    UL[x] = (UL[x] & 0xFFFF0000) | (uint)(value);
                }
                else
                {
                    x >>= 1;
                    UL[x] = (UL[x] & 0x0000FFFF) | (uint)(value << 16);
                }
            }
        }
    }
    public class UtUD
    {
        uint[] UL;
        bool isR0;

        public UtUD(uint[] UL, bool isR0)
        {
            this.UL = UL;
            this.isR0 = isR0;
        }

        public ulong this[int x]
        {
            get
            {
                x <<= 1;
                return UL[x] | ((ulong)UL[x + 1] << 32);
            }
            set
            {
                if (isR0) return;// throw new IsR0Exception();
                x <<= 1;
                UL[x] = (uint)(value);
                UL[x + 1] = (uint)(value >> 32);
            }
        }
    }
    public class UtSD
    {
        uint[] UL;
        bool isR0;

        public UtSD(uint[] UL, bool isR0)
        {
            this.UL = UL;
            this.isR0 = isR0;
        }

        public long this[int x]
        {
            get
            {
                x <<= 1;
                return (long)(UL[x] | ((ulong)UL[x + 1] << 32));
            }
            set
            {
                if (isR0) return;// throw new IsR0Exception();
                x <<= 1;
                UL[x] = (uint)(value);
                UL[x + 1] = (uint)(value >> 32);
            }
        }
    }
    public class UtSL
    {
        uint[] UL;
        bool isR0;

        public UtSL(uint[] UL, bool isR0)
        {
            this.UL = UL;
            this.isR0 = isR0;
        }

        public int this[int x]
        {
            get
            {
                return (int)UL[x];
            }
            set
            {
                if (isR0) return;// throw new IsR0Exception();
                UL[x] = (uint)value;
            }
        }
    }

    public class GPR
    {
        public readonly uint[] UL = new uint[] { 0, 0, 0, 0 };
        public readonly UtUB UB;
        public readonly UtUS US;
        public readonly UtSL SL;
        public readonly UtUD UD;
        public readonly UtSD SD;

        // --> SByte
        public byte UB0 { get { return UB[0]; } set { UB[0] = value; } } // --> Byte
        public ushort US0 { get { return US[0]; } set { US[0] = value; } } // --> Int16
        // --> UInt16
        public int SL0 { get { return SL[0]; } set { SL[0] = value; } } // --> Int32
        public uint UL0 { get { return UL[0]; } set { UL[0] = value; } } // --> UInt32
        public long SD0 { get { return SD[0]; } set { SD[0] = value; } } // --> Int64
        public ulong UD0 { get { return UD[0]; } set { UD[0] = value; } } // --> UInt64

        public ulong UD1 { get { return UD[1]; } set { UD[1] = value; } }

        public GPR(bool isR0)
        {
            UB = new UtUB(UL, isR0);
            US = new UtUS(UL, isR0);
            SL = new UtSL(UL, isR0);
            UD = new UtUD(UL, isR0);
            SD = new UtSD(UL, isR0);
        }

        public override string ToString()
        {
            return string.Format("{0:x8}_{1:x8}_{2:x8}_{3:x8}", UL[3], UL[2], UL[1], UL[0]);
        }
    }
    public class FPR
    {
        public float f = 0;

        public uint UL
        {
            get { return MobUt.F2UL(f); }
            set { f = MobUt.UL2F(value); }
        }
    }
    public class Vec : IComparable<Vec>
    {
        public Vec(bool isVF0)
        {
            this.isVF0 = isVF0;
        }
        public bool isVF0;
        public float[] F = new float[] { 0, 0, 0, 1 };
        public float x { get { return F[0]; } set { F[0] = value; } }
        public float y { get { return F[1]; } set { F[1] = value; } }
        public float z { get { return F[2]; } set { F[2] = value; } }
        public float w { get { return F[3]; } set { F[3] = value; } }

        public override string ToString()
        {
            return string.Format("{0} | {1} | {2} | {3}", F[0], F[1], F[2], F[3]);
        }

        #region IComparable<Vec> メンバ

        public int CompareTo(Vec o)
        {
            int t;
            if (0 != (t = x.CompareTo(o.x))) return t;
            if (0 != (t = y.CompareTo(o.y))) return t;
            if (0 != (t = z.CompareTo(o.z))) return t;
            if (0 != (t = w.CompareTo(o.w))) return t;
            return 0;
        }

        #endregion
    }
    public class VIv
    {
        public UInt32 UL = 0;

        public float f
        {
            get { return MobUt.UL2F(UL); }
            set { UL = MobUt.F2UL(value); }
        }
    }
    public class CustEE
    {
        public GPR r0 = new GPR(true);
        public GPR at = new GPR(false);
        public GPR v0 = new GPR(false);
        public GPR v1 = new GPR(false);
        public GPR a0 = new GPR(false);
        public GPR a1 = new GPR(false);
        public GPR a2 = new GPR(false);
        public GPR a3 = new GPR(false);
        public GPR t0 = new GPR(false);
        public GPR t1 = new GPR(false);
        public GPR t2 = new GPR(false);
        public GPR t3 = new GPR(false);
        public GPR t4 = new GPR(false);
        public GPR t5 = new GPR(false);
        public GPR t6 = new GPR(false);
        public GPR t7 = new GPR(false);
        public GPR s0 = new GPR(false);
        public GPR s1 = new GPR(false);
        public GPR s2 = new GPR(false);
        public GPR s3 = new GPR(false);
        public GPR s4 = new GPR(false);
        public GPR s5 = new GPR(false);
        public GPR s6 = new GPR(false);
        public GPR s7 = new GPR(false);
        public GPR t8 = new GPR(false);
        public GPR t9 = new GPR(false);
        public GPR k0 = new GPR(false);
        public GPR k1 = new GPR(false);
        public GPR gp = new GPR(false);
        public GPR sp = new GPR(false);
        public GPR s8 = new GPR(false);
        public GPR ra = new GPR(false);

        public GPR zero = new GPR(true);

        public GPR[] GPR
        {
            get
            {
                return new GPR[] {
                r0, at, v0, v1, a0, a1, a2, a3, t0, t1, t2, t3, t4, t5, t6, t7,
                s0, s1, s2, s3, s4, s5, s6, s7, t8, t9, k0, k1, gp, sp, s8, ra,
                };
            }
        }

        public Int64 LO = 0, HI = 0;

        // EE
        public byte[] ram = new byte[1024 * 1024 * 32];
        public byte[] spr = new byte[16384];
        // COP1, FPU
        public FPR[] fpr = new FPR[] { new FPR(), new FPR(), new FPR(), new FPR(), new FPR(), new FPR(), new FPR(), new FPR(), new FPR(), new FPR(), new FPR(), new FPR(), new FPR(), new FPR(), new FPR(), new FPR(), new FPR(), new FPR(), new FPR(), new FPR(), new FPR(), new FPR(), new FPR(), new FPR(), new FPR(), new FPR(), new FPR(), new FPR(), new FPR(), new FPR(), new FPR(), new FPR(), };
        public uint[] fprc = new uint[32];
        public FPR fpracc = new FPR();
        // COP2, VU0
        public Vec[] VF = new Vec[] { new Vec(true), new Vec(false), new Vec(false), new Vec(false), new Vec(false), new Vec(false), new Vec(false), new Vec(false), new Vec(false), new Vec(false), new Vec(false), new Vec(false), new Vec(false), new Vec(false), new Vec(false), new Vec(false), new Vec(false), new Vec(false), new Vec(false), new Vec(false), new Vec(false), new Vec(false), new Vec(false), new Vec(false), new Vec(false), new Vec(false), new Vec(false), new Vec(false), new Vec(false), new Vec(false), new Vec(false), new Vec(false), };
        public Vec Vacc = new Vec(false);
        public VIv[] VI = new VIv[] { new VIv(), new VIv(), new VIv(), new VIv(), new VIv(), new VIv(), new VIv(), new VIv(), new VIv(), new VIv(), new VIv(), new VIv(), new VIv(), new VIv(), new VIv(), new VIv(), new VIv(), new VIv(), new VIv(), new VIv(), new VIv(), new VIv(), new VIv(), new VIv(), new VIv(), new VIv(), new VIv(), new VIv(), new VIv(), new VIv(), new VIv(), new VIv(), };
        public VIv Vq;
        public VIv Vp;
        public VIv Vr;
        public VIv Vi;
        public VIv Vcf;

        uint fcr31 = 0;

        public bool fcr31_23 { get { return 0 != (fcr31 & 0x800000); } set { fcr31 = (fcr31 & (~0x800000U)) | (value ? 0x800000U : 0U); } }

        public uint pc = 0;

        public Func<uint, byte> hwRead8 = delegate (uint mem) { throw new AccessUnpreparedMemoryException(mem); };
        public Func<uint, ushort> hwRead16 = delegate (uint mem) { throw new AccessUnpreparedMemoryException(mem); };
        public Func<uint, uint> hwRead32 = delegate (uint mem) { throw new AccessUnpreparedMemoryException(mem); };
        public Func<uint, ulong> hwRead64 = delegate (uint mem) { throw new AccessUnpreparedMemoryException(mem); };

        public Action<uint, byte> hwWrite8 = delegate (uint mem, byte v) { throw new AccessUnpreparedMemoryException(mem); };
        public Action<uint, ushort> hwWrite16 = delegate (uint mem, ushort v) { throw new AccessUnpreparedMemoryException(mem); };
        public Action<uint, uint> hwWrite32 = delegate (uint mem, uint v) { throw new AccessUnpreparedMemoryException(mem); };
        public Action<uint, ulong> hwWrite64 = delegate (uint mem, ulong v) { throw new AccessUnpreparedMemoryException(mem); };

        public CustEE()
        {
            this.Vcf = VI[18];
            this.Vr = VI[20];
            this.Vi = VI[21];
            this.Vq = VI[22];
            this.Vp = VI[23];

            ram2 = new BinaryWriter(ram1 = new MemoryStream(ram, true));
            spr2 = new BinaryWriter(spr1 = new MemoryStream(spr, true));
        }

        BinaryWriter ram2, spr2;
        MemoryStream ram1, spr1;

        public void eeWrite64(uint off, ulong val)
        {
            if (false) { }
            else if ((off & 0xF0000000U) == 0x70000000U)
            {
                spr1.Position = off & 0x3FFF;
                spr2.Write(val);
                return;
            }
            else if ((off & 0xF0000000U) == 0x10000000U)
            {
                hwWrite64(off, val);
                return;
            }
            else if ((off & 0xF0000000U) == 0x00000000U)
            {
                ram1.Position = off;
                ram2.Write(val);
                return;
            }

            throw new AccessUnpreparedMemoryException(off);
        }
        public void eeWriteSingle(uint off, float val)
        {
            if (false) { }
            else if ((off & 0xF0000000U) == 0x70000000U)
            {
                spr1.Position = off & 0x3FFF;
                spr2.Write(val);
                return;
            }
            else if ((off & 0xF0000000U) == 0x10000000U)
            {
                ;
            }
            else if ((off & 0xF0000000U) == 0x00000000U)
            {
                ram1.Position = off;
                ram2.Write(val);
                return;
            }

            throw new AccessUnpreparedMemoryException(off);
        }
        public void eeWrite32(uint off, uint val)
        {
            if (false) { }
            else if ((off & 0xF0000000U) == 0x70000000U)
            {
                spr1.Position = off & 0x3FFF;
                spr2.Write(val);
                return;
            }
            else if ((off & 0xF0000000U) == 0x10000000U)
            {
                hwWrite32(off, val);
                return;
            }
            else if ((off & 0xF0000000U) == 0x00000000U)
            {
                ram1.Position = off;
                ram2.Write(val);
                return;
            }

            throw new AccessUnpreparedMemoryException(off);
        }
        public void eeWrite16(uint off, ushort val)
        {
            if (false) { }
            else if ((off & 0xF0000000U) == 0x70000000U)
            {
                spr1.Position = off & 0x3FFF;
                spr2.Write(val);
                return;
            }
            else if ((off & 0xF0000000U) == 0x10000000U)
            {
                hwWrite16(off, val);
                return;
            }
            else if ((off & 0xF0000000U) == 0x00000000U)
            {
                ram1.Position = off;
                ram2.Write(val);
                return;
            }

            throw new AccessUnpreparedMemoryException(off);
        }
        public void eeWrite8(uint off, byte val)
        {
            if (false) { }
            else if ((off & 0xF0000000U) == 0x70000000U)
            {
                spr[off & 0x3FFF] = val;
                return;
            }
            else if ((off & 0xF0000000U) == 0x10000000U)
            {
                hwWrite8(off, val);
                return;
            }
            else if ((off & 0xF0000000U) == 0x00000000U)
            {
                ram[off] = val;
                return;
            }

            throw new AccessUnpreparedMemoryException(off);
        }


        public byte eeRead8(uint off)
        {
            if (false) { }
            else if ((off & 0xF0000000U) == 0x70000000U)
            {
                return spr[off & 0x3FFF];
            }
            else if ((off & 0xF0000000U) == 0x10000000U)
            {
                return hwRead8(off);
            }
            else if ((off & 0xF0000000U) == 0x00000000U)
            {
                return ram[off];
            }

            throw new AccessUnpreparedMemoryException(off);
        }

        public ushort eeRead16(uint off)
        {
            if (false) { }
            else if ((off & 0xF0000000U) == 0x70000000U)
            {
                return BitConverter.ToUInt16(spr, (int)off & 0x3FFF);
            }
            else if ((off & 0xF0000000U) == 0x10000000U)
            {
                return hwRead16(off);
            }
            else if ((off & 0xF0000000U) == 0x00000000U)
            {
                return BitConverter.ToUInt16(ram, (int)off);
            }

            throw new AccessUnpreparedMemoryException(off);
        }

        public uint eeRead32(uint off)
        {
            if (false) { }
            else if ((off & 0xF0000000U) == 0x70000000U)
            {
                return BitConverter.ToUInt32(spr, (int)off & 0x3FFF);
            }
            else if ((off & 0xF0000000U) == 0x10000000U)
            {
                return hwRead32(off);
            }
            else if ((off & 0xF0000000U) == 0x00000000U)
            {
                return BitConverter.ToUInt32(ram, (int)off);
            }

            throw new AccessUnpreparedMemoryException(off);
        }

        public ulong eeRead64(uint off)
        {
            if (false) { }
            else if ((off & 0xF0000000U) == 0x70000000U)
            {
                return BitConverter.ToUInt64(spr, (int)off & 0x3FFF);
            }
            else if ((off & 0xF0000000U) == 0x10000000U)
            {
                return hwRead64(off);
            }
            else if ((off & 0xF0000000U) == 0x00000000U)
            {
                return BitConverter.ToUInt64(ram, (int)off);
            }

            throw new AccessUnpreparedMemoryException(off);
        }

        public float eeReadSingle(uint off)
        {
            if (false) { }
            else if ((off & 0xF0000000U) == 0x70000000U)
            {
                return BitConverter.ToSingle(spr, (int)off & 0x3FFF);
            }
            else if ((off & 0xF0000000U) == 0x10000000U)
            {
                ;
            }
            else if ((off & 0xF0000000U) == 0x00000000U)
            {
                return BitConverter.ToSingle(ram, (int)off);
            }

            throw new AccessUnpreparedMemoryException(off);
        }
    }

    public delegate TResult Func<TResult>();
    public delegate TResult Func<T, TResult>(T x);

    public delegate void Action<T1, T2>(T1 x, T2 y);

    public class MobUt
    {
        public static BitArray bita = new BitArray(2097152, false);
        public static BitArray bitr = new BitArray(2097152, false);

        public delegate void Tx8();

        public static void LQ(CustEE ee, GPR rt, uint off)
        {
            //#Debug.Assert(off <= 32U * 1024 * 1024, "Avail only within 32MB ram");
            sysr((int)(off >> 4));
            rt.UL[0] = ee.eeRead32(off + 0);
            rt.UL[1] = ee.eeRead32(off + 4);
            rt.UL[2] = ee.eeRead32(off + 8);
            rt.UL[3] = ee.eeRead32(off + 12);
        }
        public static void SQ(CustEE ee, GPR rt, uint off)
        {
            sysw((int)(off >> 4));
            ee.eeWrite32(off + 0, rt.UL[0]);
            ee.eeWrite32(off + 4, rt.UL[1]);
            ee.eeWrite32(off + 8, rt.UL[2]);
            ee.eeWrite32(off + 12, rt.UL[3]);
        }
        public static void VCLIP(CustEE ee, Vec ft, Vec fs)
        {
            uint Vcf = 0;
            float pw = +Math.Abs(MobUt.vuDouble(ft.w));
            float nw = -pw;
            if (MobUt.vuDouble(fs.x) > pw) Vcf |= 0x01;
            if (MobUt.vuDouble(fs.x) < nw) Vcf |= 0x02;
            if (MobUt.vuDouble(fs.y) > pw) Vcf |= 0x04;
            if (MobUt.vuDouble(fs.y) < nw) Vcf |= 0x08;
            if (MobUt.vuDouble(fs.z) > pw) Vcf |= 0x10;
            if (MobUt.vuDouble(fs.z) < nw) Vcf |= 0x20;
            ee.Vcf.UL = ((ee.Vcf.UL << 6) | Vcf) & 0xFFFFFFu;
        }
        public static float UL2F(uint val)
        {
#if UseBitConverter
            return BitConverter.ToSingle(BitConverter.GetBytes(val), 0);
#elif UseUnsafe
            unsafe
            {
                return *(float*)&val;
            }
#else
            MemoryStream os = new MemoryStream(new byte[] { 0, 0, 0, 0 });
            new BinaryWriter(os).Write(val);
            os.Position = 0;
            float r = new BinaryReader(os).ReadSingle();
            //Debug.Assert(!float.IsNaN(r));
            //Debug.Assert(!float.IsNegativeInfinity(r) && !float.IsPositiveInfinity(r));
            return r;
#endif
        }
        public static uint F2UL(float val)
        {
#if UseBitConverter
            return BitConverter.ToUInt32(BitConverter.GetBytes(val), 0);
#elif UseUnsafe
            unsafe
            {
                return *(uint*)&val;
            }
#else
            MemoryStream os = new MemoryStream(new byte[] { 0, 0, 0, 0 });
            //Debug.Assert(!float.IsNaN(val));
            //Debug.Assert(!float.IsNegativeInfinity(val) && !float.IsPositiveInfinity(val));
            new BinaryWriter(os).Write(val);
            os.Position = 0;
            uint r = new BinaryReader(os).ReadUInt32();
            return r;
#endif
        }

        public static void SD(CustEE ee, GPR rt, uint off)
        {
            sysw((int)(off >> 4));
            ee.eeWrite64(off, rt.UD0);
        }

        public static void LD(CustEE ee, GPR rt, uint off)
        {
            sysr((int)(off >> 4));
            rt.UD0 = ee.eeRead64(off);
        }

        public static void LWC1(CustEE ee, FPR ft, uint off)
        {
            //#Debug.Assert(off <= 32U * 1024 * 1024, "Avail only within 32MB ram");
            sysr((int)(off >> 4));
            float r = ee.eeReadSingle(off);
            //Debug.Assert(!float.IsNaN(r));
            //Debug.Assert(!float.IsNegativeInfinity(r) && !float.IsPositiveInfinity(r));
            ft.f = r;
        }

        public static void SWC1(CustEE ee, FPR ft, uint off)
        {
            //#Debug.Assert(off <= 32U * 1024 * 1024, "Avail only within 32MB ram");
            sysw((int)(off >> 4));
            ee.eeWriteSingle(off, ft.f);
        }

        public static void LQC2(CustEE ee, Vec VF, uint off)
        {
            //#Debug.Assert(off <= 32U * 1024 * 1024 && off + 16U <= 32U * 1024 * 1024, "Avail only within 32MB ram");
            sysr((int)(off >> 4));
            if (true)
            {
                float r = ee.eeReadSingle(off + 0U);
                //Debug.Assert(!float.IsNaN(r));
                //Debug.Assert(!float.IsNegativeInfinity(r) && !float.IsPositiveInfinity(r));
                if (VF != ee.VF[0]) VF.x = r;
            }
            if (true)
            {
                float r = ee.eeReadSingle(off + 4U);
                //Debug.Assert(!float.IsNaN(r));
                //Debug.Assert(!float.IsNegativeInfinity(r) && !float.IsPositiveInfinity(r));
                if (VF != ee.VF[0]) VF.y = r;
            }
            if (true)
            {
                float r = ee.eeReadSingle(off + 8U);
                //Debug.Assert(!float.IsNaN(r));
                //Debug.Assert(!float.IsNegativeInfinity(r) && !float.IsPositiveInfinity(r));
                if (VF != ee.VF[0]) VF.z = r;
            }
            if (true)
            {
                float r = ee.eeReadSingle(off + 12U);
                //Debug.Assert(!float.IsNaN(r));
                //Debug.Assert(!float.IsNegativeInfinity(r) && !float.IsPositiveInfinity(r));
                if (VF != ee.VF[0]) VF.w = r;
            }
        }

        public static void QMFC2(GPR rt, Vec vf)
        {
#if UseBitConverter
            rt.UL[0] = BitConverter.ToUInt32(BitConverter.GetBytes(vf.x), 0);
            rt.UL[1] = BitConverter.ToUInt32(BitConverter.GetBytes(vf.y), 0);
            rt.UL[2] = BitConverter.ToUInt32(BitConverter.GetBytes(vf.z), 0);
            rt.UL[3] = BitConverter.ToUInt32(BitConverter.GetBytes(vf.w), 0);
#elif UseUnsafe
            unsafe
            {
                float f;
                f = vf.x; rt.UL[0] = *(uint*)&f;
                f = vf.y; rt.UL[1] = *(uint*)&f;
                f = vf.z; rt.UL[2] = *(uint*)&f;
                f = vf.w; rt.UL[3] = *(uint*)&f;
            }
#else
            if (rt.isR0) return;
            MemoryStream os = new MemoryStream(16);
            BinaryWriter wr = new BinaryWriter(os);
            wr.Write(vf.x);
            wr.Write(vf.y);
            wr.Write(vf.z);
            wr.Write(vf.w);
            os.Position = 0;
            BinaryReader br = new BinaryReader(os);
            rt.UL[0] = br.ReadUInt32();
            rt.UL[1] = br.ReadUInt32();
            rt.UL[2] = br.ReadUInt32();
            rt.UL[3] = br.ReadUInt32();
#endif
        }

        public static void SQC2(CustEE ee, Vec VF, uint off)
        {
            sysw((int)(off >> 4));
            ee.eeWriteSingle(off + 0, VF.x);
            ee.eeWriteSingle(off + 4, VF.y);
            ee.eeWriteSingle(off + 8, VF.z);
            ee.eeWriteSingle(off + 12, VF.w);
        }

        public static void CFC2(GPR rt, VIv id)
        {
            rt.UD0 = id.UL;
        }

        public static void Latency() { }

        public static void SW(CustEE ee, GPR rt, uint off)
        {
            sysw((int)(off >> 4));
            ee.eeWrite32(off, rt.UL0);
        }

        public static void DSRL(GPR rd, GPR rt, int sa)
        {
            rd.UD0 = rt.UD0 >> sa;
        }

        public static void DSRLV(GPR rd, GPR rt, GPR rs)
        {
            rd.UD0 = rt.UD0 >> rs.UB0;
        }

        public static void SB(CustEE ee, GPR rt, uint off)
        {
            sysw((int)(off >> 4));
            ee.eeWrite8(off, rt.UB0);
        }

        public static void SLL(GPR rd, GPR rt, int sa)
        {
            rd.SD0 = (int)(rt.UL0 << sa);
        }

        public static void LW(CustEE ee, GPR rt, uint off)
        {
            sysr((int)(off >> 4));
            rt.SD0 = (int)ee.eeRead32(off);
        }

        public static void LWU(CustEE ee, GPR rt, uint off)
        {
            sysr((int)(off >> 4));
            rt.UD0 = ee.eeRead32(off);
        }

        public static void LHU(CustEE ee, GPR rt, uint off)
        {
            sysr((int)(off >> 4));
            rt.UD0 = ee.eeRead16(off);
        }

        public static void LBU(CustEE ee, GPR rt, uint off)
        {
            sysr((int)(off >> 4));
            rt.UD0 = ee.eeRead8(off);
        }

        public static void MFC1(GPR rt, FPR fs)
        {
            MemoryStream os = new MemoryStream(new byte[] { 0, 0, 0, 0 });
            new BinaryWriter(os).Write(fs.f);
            os.Position = 0;
            rt.SD0 = new BinaryReader(os).ReadInt32();
        }

        public static void SH(CustEE ee, GPR rt, uint off)
        {
            sysw((int)(off >> 4));
            ee.eeWrite16(off, rt.US0);
        }

        private static void sysw(int mark)
        {
            if (mark >= bita.Count) return;
            bita[mark] = true;
        }
        private static void sysr(int mark)
        {
            if (mark >= bita.Count) return;
            if (bita[mark]) return;
            bita[mark] = true;
            bitr[mark] = true;
        }

        public static void QMTC2(GPR rt, Vec vf)
        {
#if UseBitConverter
            vf.x = BitConverter.ToSingle(BitConverter.GetBytes(rt.UL[0]), 0);
            vf.y = BitConverter.ToSingle(BitConverter.GetBytes(rt.UL[1]), 0);
            vf.z = BitConverter.ToSingle(BitConverter.GetBytes(rt.UL[2]), 0);
            vf.w = BitConverter.ToSingle(BitConverter.GetBytes(rt.UL[3]), 0);
#elif UseUnsafe
            unsafe
            {
                uint v;
                v = rt.UL[0]; vf.x = *(float*)&v;
                v = rt.UL[1]; vf.y = *(float*)&v;
                v = rt.UL[2]; vf.z = *(float*)&v;
                v = rt.UL[3]; vf.w = *(float*)&v;
            }
#else
            if (vf.isVF0) return;
            MemoryStream os = new MemoryStream(16);
            BinaryWriter wr = new BinaryWriter(os);
            wr.Write(rt.UL[0]);
            wr.Write(rt.UL[1]);
            wr.Write(rt.UL[2]);
            wr.Write(rt.UL[3]);
            os.Position = 0;
            BinaryReader br = new BinaryReader(os);
            vf.x = br.ReadSingle();
            vf.y = br.ReadSingle();
            vf.z = br.ReadSingle();
            vf.w = br.ReadSingle();
#endif
        }

        public static void LH(CustEE ee, GPR rt, uint off)
        {
            sysr((int)(off >> 4));
            rt.SD0 = (short)ee.eeRead16(off);
        }

        public static void XOR(GPR rd, GPR rs, GPR rt)
        {
            rd.UD0 = rs.UD0 ^ rt.UD0;
        }

        public static void DSRL32(GPR rd, GPR rt, int sa)
        {
            rd.UD0 = rt.UD0 >> (32 + sa);
        }

        public static void DSLL32(GPR rd, GPR rt, int sa)
        {
            rd.UD0 = rt.UD0 << (32 + sa);
        }

        public static void CVT_W(FPR fd, FPR fs)
        {
            uint r = F2UL(fs.f);
            if ((r & 0x7F800000U) <= 0x4E800000U)
            {
                fd.f = UL2F((uint)((int)fs.f));
            }
            else if ((r & 0x80000000) != 0)
            {
                fd.f = UL2F(0x80000000);
            }
            else
            {
                fd.f = UL2F(0x7FFFFFFF);
            }
        }

        public static void SRL(GPR rd, GPR rt, int sa)
        {
            rd.UD0 = (ulong)((int)(rt.UL0 >> sa));
        }

        public static void SRA(GPR rd, GPR rt, int sa)
        {
            rd.SD0 = (int)(rt.SL0 >> sa);
        }

        public static float MUL(float v0, float v1)
        {
            return vuDouble(vuDouble(v0) * vuDouble(v1));
        }

        public static float MADD(float f0, float f1, float f2)
        {
            return vuDouble(vuDouble(f0) + (float)(vuDouble(f1) * vuDouble(f2)));
        }

        public static float OPMSUB(float f0, float f1, float f2)
        {
            return vuDouble(f0) - (float)(vuDouble(f1) * vuDouble(f2));
        }

        public static float vuDouble(float v)
        {
#if UseUnsafe
            unsafe
            {
                uint f = *(uint*)&v;
                switch (f & 0x7F800000)
                {
                    case 0x0:
                        f &= 0x80000000;
                        return *(float*)&f;
                    case 0x7F800000:
                        f = ((f & 0x80000000) | 0x7F7FFFFF);
                        return *(float*)&f;
                    default:
                        return v;
                }
            }
#else
            uint f = F2UL(v);
            switch (f & 0x7F800000) {
                case 0x0:
                    f &= 0x80000000;
                    return UL2F(f);
                case 0x7F800000:
                    return UL2F((f & 0x80000000) | 0x7F7FFFFF);
                default:
                    return UL2F(f);
            }
#endif
        }

        public static float MSUB(float f0, float f1, float f2)
        {
            return vuDouble(vuDouble(f0) - (float)(vuDouble(f1) * vuDouble(f2)));
        }

        public static void SLLV(GPR rd, GPR rt, GPR rs)
        {
            rd.SD0 = (int)(rt.SD0 << (rs.UB0 & 15));
        }

        public static void CVT_S(FPR fd, FPR fs)
        {
            fd.f = (int)fs.UL;
        }

        public static void LB(CustEE ee, GPR rt, uint off)
        {
            sysr((int)(off >> 4));
            rt.SD0 = (sbyte)ee.eeRead8(off);
        }

        [DllImport("MySSE.dll", CallingConvention = CallingConvention.StdCall)]
        static extern void MySSE_VMx16(float[] param);

        public static void VMx16(Vec vacc,
            Vec vec1, float v1f,
            Vec vec2, float v2f,
            Vec vec3, float v3f,
            Vec vec4, float v4f,
            Vec veco
            )
        {
#if UseUnsafe && false
            unsafe {
                float[] param = new float[] {
                    vec1.F[0], vec1.F[1], vec1.F[2], vec1.F[3],
                    vec2.F[0], vec2.F[1], vec2.F[2], vec2.F[3],
                    vec3.F[0], vec3.F[1], vec3.F[2], vec3.F[3],
                    vec4.F[0], vec4.F[1], vec4.F[2], vec4.F[3],
                    v1f, v1f, v1f, v1f,
                    v2f, v2f, v2f, v2f,
                    v3f, v3f, v3f, v3f,
                    v4f, v4f, v4f, v4f,
                };

                MySSE_VMx16(param);

                veco.x = param[0];
                veco.y = param[1];
                veco.z = param[2];
                veco.w = param[3];
            }
#else
            veco.F[0] = vuDouble((vacc.F[0] = vuDouble((vec1.F[0] * v1f) + (vec2.F[0] * v2f) + (vec3.F[0] * v3f))) + (vec4.F[0] * v4f));
            veco.F[1] = vuDouble((vacc.F[1] = vuDouble((vec1.F[1] * v1f) + (vec2.F[1] * v2f) + (vec3.F[1] * v3f))) + (vec4.F[1] * v4f));
            veco.F[2] = vuDouble((vacc.F[2] = vuDouble((vec1.F[2] * v1f) + (vec2.F[2] * v2f) + (vec3.F[2] * v3f))) + (vec4.F[2] * v4f));
            veco.F[3] = vuDouble((vacc.F[3] = vuDouble((vec1.F[3] * v1f) + (vec2.F[3] * v2f) + (vec3.F[3] * v3f))) + (vec4.F[3] * v4f));
#endif
        }

        public static void DSLLV(GPR rd, GPR rt, GPR rs)
        {
            rd.UD0 = (ulong)(rt.UD0 << (rs.UB0 & 63));
        }

        public static void POR(GPR rd, GPR rt, GPR rs)
        {
            rd.UD0 = rs.UD0 | rt.UD0;
            rd.UD1 = rs.UD1 | rt.UD1;
        }

        public static void PAND(GPR rd, GPR rt, GPR rs)
        {
            rd.UD0 = rs.UD0 & rt.UD0;
            rd.UD1 = rs.UD1 & rt.UD1;
        }

        public static void ADD(GPR rd, GPR rt, GPR rs)
        {
            rd.SD0 = (rs.SL0 + rt.SL0);
        }

        public static void DSLL(GPR rd, GPR rt, int sa)
        {
            rd.UD0 = (ulong)(rt.UD0 << (sa & 63));
        }

        public static void DSRA(GPR rd, GPR rt, int sa)
        {
            rd.SD0 = (long)(rt.SD0 >> sa);
        }

        public static void DSRA32(GPR rd, GPR rt, int sa)
        {
            rd.SD0 = (long)(rt.SD0 >> (32 + sa));
        }

        public static void SYSCALL(int code)
        {
            // http://gamehacking.org/?s=faqs&id=90
            Debug.WriteLine(String.Format("#SYSCALL {0:x}", code));
        }

        public static void PEXTLW(GPR rd, GPR rs, GPR rt)
        {
            uint[] res = {
                rt.UL[0],
                rs.UL[0],
                rt.UL[1],
                rs.UL[1],
            };
            for (int x = 0; x < 4; x++) rd.UL[x] = res[x];
        }

        public static void PEXTLH(GPR rd, GPR rs, GPR rt)
        {
            ushort[] res = {
                rt.US[0],
                rs.US[0],
                rt.US[1],
                rs.US[1],
                rt.US[2],
                rs.US[2],
                rt.US[3],
                rs.US[3],
            };
            for (int x = 0; x < 8; x++) rd.US[x] = res[x];
        }

        public static void PEXTLB(GPR rd, GPR rs, GPR rt)
        {
            byte[] res ={
                 rt.UB[0],
                 rs.UB[0],
                 rt.UB[1],
                 rs.UB[1],
                 rt.UB[2],
                 rs.UB[2],
                 rt.UB[3],
                 rs.UB[3],
                 rt.UB[4],
                 rs.UB[4],
                 rt.UB[5],
                 rs.UB[5],
                 rt.UB[6],
                 rs.UB[6],
                 rt.UB[7],
                 rs.UB[7],
            };
            for (int x = 0; x < 16; x++) rd.UB[x] = res[x];
        }

        public static void MULTU(CustEE ee, GPR rs, GPR rt)
        {
            Int64 r = (Int64)rs.UL0 * rt.UL0;
            ee.LO = (int)(r);
            ee.HI = (int)(r >> 32);
        }
        public static void MULT(CustEE ee, GPR rd, GPR rs, GPR rt)
        {
            Int64 r = (Int64)rs.SL0 * rt.SL0;
            ee.LO = (int)(r);
            ee.HI = (int)(r >> 32);
            rd.SD0 = r;
        }

        public static void MFC0(CustEE ee, GPR rt, int rd)
        {
            //fake
            rt.UD0 = 0;
        }

        public static void MOVZ(GPR rd, GPR rs, GPR rt)
        {
            if (rt.UD0 == 0) rd.UD0 = rs.UD0;
        }

        public static void PCPYH(GPR rd, GPR rt)
        {
            UInt64 v0 = rt.US[0];
            UInt64 v1 = rt.US[4];
            rd.UD0 = v0 | (v0 << 16) | (v0 << 32) | (v0 << 48);
            rd.UD1 = v1 | (v1 << 16) | (v1 << 32) | (v1 << 48);
        }

        public static void VRXOR(CustEE ee, float fs)
        {
            ee.Vr.UL = ee.Vr.UL ^ (F2UL(fs) & 0x7FFFFF);
        }

        public static float VRNEXT(CustEE ee, bool next)
        {
            if (next)
            {
                ee.Vr.f = new Random((int)ee.Vr.UL).Next(1, 20000) / 10000.0f;
            }
            return ee.Vr.f;
        }

        public static void PADDW(GPR rd, GPR rs, GPR rt)
        {
            for (int x = 0; x < 4; x++)
                rd.UL[x] = rs.UL[x] + rt.UL[x];
        }

        public static float MINI(float x, float y)
        {
            return Math.Min(x, y);
        }

        public static float MAX(float x, float y)
        {
            return Math.Max(x, y);
        }

        public static void PROT3W(GPR rd, GPR rt)
        {
            uint[] res = {
                 rt.UL[1],
                 rt.UL[2],
                 rt.UL[0],
                 rt.UL[3],
            };
            for (int x = 0; x < 4; x++) rd.UL[x] = res[x];
        }

        public static void PPACW(GPR rd, GPR rs, GPR rt)
        {
            uint[] res = {
                 rt.UL[0],
                 rt.UL[2],
                 rs.UL[0],
                 rs.UL[2],
            };
            for (int x = 0; x < 4; x++) rd.UL[x] = res[x];
        }

        public static void PEXTUW(GPR rd, GPR rs, GPR rt)
        {
            uint[] res = {
                 rt.UL[2],
                 rs.UL[2],
                 rt.UL[3],
                 rs.UL[3],
            };
            for (int x = 0; x < 4; x++) rd.UL[x] = res[x];
        }

        public static void PPACB(GPR rd, GPR rs, GPR rt)
        {
            byte[] res = {
                 rt.UB[0],
                 rt.UB[2],
                 rt.UB[4],
                 rt.UB[6],
                 rt.UB[8],
                 rt.UB[10],
                 rt.UB[12],
                 rt.UB[14],
                 rs.UB[0],
                 rs.UB[2],
                 rs.UB[4],
                 rs.UB[6],
                 rs.UB[8],
                 rs.UB[10],
                 rs.UB[12],
                 rs.UB[14],
            };
            for (int x = 0; x < 16; x++) rd.UB[x] = res[x];
        }
        public static void PPACH(GPR rd, GPR rs, GPR rt)
        {
            ushort[] res = {
                 rt.US[0],
                 rt.US[2],
                 rt.US[4],
                 rt.US[6],
                 rs.US[0],
                 rs.US[2],
                 rs.US[4],
                 rs.US[6],
            };
            for (int x = 0; x < 8; x++) rd.US[x] = res[x];
        }

        public static void PCPYLD(GPR rd, GPR rs, GPR rt)
        {
            UInt64 v0 = rt.UD0;
            UInt64 v1 = rs.UD0;
            rd.UD0 = v0;
            rd.UD1 = v1;
        }
        public static void PCPYUD(GPR rd, GPR rs, GPR rt)
        {
            UInt64 v0 = rs.UD1;
            UInt64 v1 = rt.UD1;
            rd.UD0 = v0;
            rd.UD1 = v1;
        }

        public static void XORI(GPR rt, GPR rs, uint imm)
        {
            rt.UL0 = rs.UL0 ^ imm;
        }

        public static void CTC2(GPR rt, VIv id)
        {
            id.UL = rt.UL0;
        }

        public static void PSRLW(GPR rd, GPR rt, int sa)
        {
            rd.UL[0] = rt.UL[0] >> sa;
            rd.UL[1] = rt.UL[1] >> sa;
            rd.UL[2] = rt.UL[2] >> sa;
            rd.UL[3] = rt.UL[3] >> sa;
        }

        // MobUtHere ---
    }

#if false
    public class Mobx {
        CustEE ee = new CustEE();
        uint pc;

        delegate void Txxxxxxxx();
        SortedList<uint, Txxxxxxxx> dicti2a = new SortedList<uint, Txxxxxxxx>();

        public void Exec() {
            dicti2a[0x011B420] = new Txxxxxxxx(_011B420);

            pc = 0x011B420;
            while (true) {
                (dicti2a[pc])();
            }
        }

        public void _011B420() {
            ee.t7.US[1] = 0x0035;
            ee.t7.SL[0] += (short)0x4260;
            MobUt.LQ(ee, ee.t0, 0x0000 + ee.t7.UL[0]);
            MobUt.LQ(ee, ee.t1, 0x0010 + ee.t7.UL[0]);
            MobUt.LQ(ee, ee.t2, 0x0020 + ee.t7.UL[0]);
            MobUt.LQ(ee, ee.t3, 0x0030 + ee.t7.UL[0]);
            MobUt.SQ(ee, ee.t0, 0x0000 + ee.t7.UL[0]);
            MobUt.SQ(ee, ee.t1, 0x0010 + ee.t7.UL[0]);
            MobUt.SQ(ee, ee.t2, 0x0020 + ee.t7.UL[0]);
            MobUt.SQ(ee, ee.t3, 0x0030 + ee.t7.UL[0]);
            pc = ee.ra.UL[0];
        }

        public void DUMMY() {
            // @0011B420   LUI t7, $0035
            this.ee.t7.UD0 = 53 << 16;

            this.ee.t7.SL[0] = (this.ee.t7.SL[0] + 16992);
            MobUt.LQ(this.ee, this.ee.t0, (0 + this.ee.t7.UL[0]));
            MobUt.LQ(this.ee, this.ee.t1, (16 + this.ee.t7.UL[0]));
            MobUt.LQ(this.ee, this.ee.t2, (32 + this.ee.t7.UL[0]));
            MobUt.LQ(this.ee, this.ee.t3, (48 + this.ee.t7.UL[0]));
            MobUt.LQ(this.ee, this.ee.t0, (0 + this.ee.a0.UL[0]));
            MobUt.LQ(this.ee, this.ee.t1, (16 + this.ee.a0.UL[0]));
            MobUt.LQ(this.ee, this.ee.t2, (32 + this.ee.a0.UL[0]));
            MobUt.LQ(this.ee, this.ee.t3, (48 + this.ee.a0.UL[0]));
            this.pc = this.ee.ra.UL[0];
            // @0011B450   MTC1 zero, $f0
            this.ee.fpr[0].f = MobUt.UL2F(this.ee.r0.UL[0]);
            this.ee.sp.SL[0] = (this.ee.sp.SL[0] + -16);

            // @0011B458   SD s0, $0000(sp)
            MobUt.SD(ee, ee.s0, 0x0000 + ee.sp.UL[0]);
            // @0011B458   SD s0, $0000(sp)
            MobUt.SD(this.ee, this.ee.s0, (0 + this.ee.sp.UL[0]));
            // @0011B45C   C.EQ.S $f12, $f0
            ee.fcr31_23 = ee.fpr[12].f == ee.fpr[0].f;
            // @0011B45C   C.EQ.S $f12, $f0
            this.ee.fcr31_23 = (this.ee.fpr[12].f == this.ee.fpr[0].f);
            // @0011B464   BC1T $0011b484
            if (ee.fcr31_23) pc = 0x0011B484;
            // @0011B464   BC1T $0011b484
            if (this.ee.fcr31_23) {
                this.pc = 1160324u;
            }
            // @0011B468   DADDU s0, a0, zero
            ee.s0.UD[0] = ee.a0.UD[0] + ee.r0.UD[0];
            // @0011B468   DADDU s0, a0, zero
            this.ee.s0.UD[0] = (this.ee.a0.UD[0] + this.ee.r0.UD[0]);

            // @0011B46C   JAL $0011bdc8
            ee.ra.UL[0] = pc + 8; pc = 0x0011B46C;
            // @0011B46C   JAL $0011bdc8
            this.ee.ra.UL[0] = 1160308u;
            this.pc = 1162696u;

            // @0011B474   MOV.S $f12, $f0
            ee.fpr[12].f = ee.fpr[0].f;
            // @0011B474   MOV.S $f12, $f0
            this.ee.fpr[12].f = this.ee.fpr[0].f;

            // @0011B488   LD ra, $0008(sp)
            MobUt.LD(ee, ee.ra, 0x0008 + ee.sp.UL[0]);
            // @0011B488   LD ra, $0008(sp)
            MobUt.LD(this.ee, this.ee.ra, (8 + this.ee.sp.UL0));

            // @0011B528   ADDIU sp, sp, $ffe0
            this.ee.sp.SD0 = ((int)((this.ee.sp.SD0 + -32)));

            // @0011B570   LWC1 $f0, $0004(s1)
            MobUt.LWC1(ee, ee.fpr[0], 4 + ee.s1.UL0);
            // @0011B570   LWC1 $f0, $0004(s1)
            MobUt.LWC1(this.ee, this.ee.fpr[0], (4 + this.ee.s1.UL0));

            // @0011B85C   SWC1 $f20, $0048(sp)
            MobUt.SWC1(this.ee, this.ee.fpr[20], (0x0048 + this.ee.s1.UL0));
            // @0011B534   SWC1 $f20, $0018(sp)
            MobUt.SWC1(this.ee, this.ee.fpr[20], (24 + this.ee.sp.UL0));

            // @0011B868   C.LT.S $f20, $f0
            ee.fcr31_23 = ee.fpr[20].f < ee.fpr[0].f;
            // @0011B868   C.LT.S $f20, $f0
            this.ee.fcr31_23 = (this.ee.fpr[20].f < this.ee.fpr[0].f);

            // @0011B884   BC1FL $0011b894
            if (!ee.fcr31_23) {
                pc = 0x0011b894;
                //xxx
            }

            // @0011B8A0   MULA.S $f13, $f13
            ee.fpracc.f = ee.fpr[13].f * ee.fpr[13].f;
            // @0011B8A0   MULA.S $f13, $f13
            this.ee.fpracc.f = (this.ee.fpr[13].f * this.ee.fpr[13].f);

            // @0011B8A4   MADD.S $f0, $f12, $f12
            ee.fpr[0].f = ee.fpracc.f + ee.fpr[12].f * ee.fpr[12].f;
            // @0011B8A4   MADD.S $f0, $f12, $f12
            this.ee.fpr[0].f = (this.ee.fpracc.f
                        + (this.ee.fpr[12].f * this.ee.fpr[12].f));

            // @0011B8B4   SQRT.S $f0, $f0
            ee.fpr[0].f = (float)Math.Sqrt(ee.fpr[0].f);
            // @0011B8B4   SQRT.S $f0, $f0
            this.ee.fpr[0].f = ((float)(System.Math.Sqrt(this.ee.fpr[0].f)));

            // @0011B8BC   BC1F $0011ba50
            if (!ee.fcr31_23) pc = 0x0011ba50;

            // @0011B8CC   NEG.S $f12, $f20
            ee.fpr[12].f = -ee.fpr[20].f;
            // @0011B8CC   NEG.S $f12, $f20
            this.ee.fpr[12].f = (0 - this.ee.fpr[20].f);

            // @0011B8FC   SUB.S $f12, $f1, $f12
            ee.fpr[12].f = ee.fpr[1].f - ee.fpr[12].f;
            // @0011B8FC   SUB.S $f12, $f1, $f12
            this.ee.fpr[12].f = (this.ee.fpr[1].f - this.ee.fpr[12].f);

            // @0011B940   ADD.S $f2, $f2, $f3
            ee.fpr[2].f = ee.fpr[2].f + ee.fpr[3].f;
            // @0011B940   ADD.S $f2, $f2, $f3
            this.ee.fpr[2].f = (this.ee.fpr[2].f + this.ee.fpr[3].f);

            // @0011B998   LQC2 vf1, $0000(sp)
            MobUt.LQC2(ee, ee.VF[1], 0x0000 + ee.sp.UL0);
            // @0011B998   LQC2 vf1, $0000(sp)
            MobUt.LQC2(this.ee, this.ee.VF[1], (0 + this.ee.sp.UL0));

            // @0011B9A0   VMUL.xyz vf1, vf1, vf2
            ee.VF[1].x = ee.VF[1].x * ee.VF[2].x;
            ee.VF[1].y = ee.VF[1].y * ee.VF[2].y;
            ee.VF[1].z = ee.VF[1].z * ee.VF[2].z;

            // @0011B9A4   VADDy.x vf1, vf1, vf1y
            ee.VF[1].x = ee.VF[1].x + ee.VF[1].y;
            // @0011B9A4   VADDy.x vf1, vf1, vf1y
            this.ee.VF[1].x = (this.ee.VF[1].x + this.ee.VF[1].y);

            // @0011B9C8   QMFC2 t0, vf1
            MobUt.QMFC2(ee.t0, ee.VF[1]);
            // @0011B9AC   QMFC2 t0, vf1
            MobUt.QMFC2(this.ee.t0, this.ee.VF[1]);

            // @0011B9D0   C.LE.S $f1, $f0
            ee.fcr31_23 = (ee.fpr[1].f <= ee.fpr[0].f);
            // @0011B9D0   C.LE.S $f1, $f0
            this.ee.fcr31_23 = (this.ee.fpr[1].f <= this.ee.fpr[0].f);

            // @00124D70   BEQ s3, t7, $00124d98
            if ((this.ee.s3.UD0 == this.ee.t7.UD0)) {
                this.pc = 1199512u;
            }

            // @00124CAC   BEQL t7, t6, $00124cc0
            if ((this.ee.t7.UD0 == this.ee.t6.UD0)) {
                this.pc = 1199296u;
                // @00124CB0   ADDIU t7, sp, $0050
                this.ee.t7.SD0 = ((int)((this.ee.sp.SD0 + 80)));
            }

            // @00124B28   SQC2 vf1, $0000(s2)
            MobUt.SQC2(this.ee, this.ee.VF[1], (0 + this.ee.s2.UL0));

            // @00124B24   VSUB.xyzw vf1, vf1, vf2
            ee.VF[1].x = ee.VF[1].x - ee.VF[2].x;
            ee.VF[1].y = ee.VF[1].y - ee.VF[2].y;
            ee.VF[1].z = ee.VF[1].z - ee.VF[2].z;
            ee.VF[1].w = ee.VF[1].w - ee.VF[2].w;

            // @00124B4C   VSQRT Q, vf2x
            ee.Vq.f = (float)Math.Sqrt(ee.VF[2].x);
            // @00124B4C   VSQRT Q, vf2x
            this.ee.Vq.f = ((float)(System.Math.Sqrt(this.ee.VF[2].x)));

            // @00124B54   CFC2 t0, $vi22
            MobUt.CFC2(ee.t0, ee.VI[22]);
            // @00124B54   CFC2 t0, $vi22
            MobUt.CFC2(this.ee.t0, this.ee.VI[22]);

            // @00124B58   VADDq.x vf2, vf0, Q
            ee.VF[2].x = ee.VF[0].x + ee.Vq.f;
            // @00124B58   VADDq.x vf2, vf0, Q
            this.ee.VF[2].x = (this.ee.VF[0].x + this.ee.Vq.f);

            // @00124B64   VDIV Q, vf0w, vf2x
            ee.Vq.f = ee.VF[0].w / ee.VF[2].x;
            // @00124B64   VDIV Q, vf0w, vf2x
            this.ee.Vq.f = (this.ee.VF[0].w / this.ee.VF[2].x);

            // @00124B6C   VMULq.xyz vf1, vf1, Q
            ee.VF[1].x = ee.VF[1].x * ee.Vq.f;
            ee.VF[1].y = ee.VF[1].y * ee.Vq.f;
            ee.VF[1].z = ee.VF[1].z * ee.Vq.f;
            // @00124B6C   VMULq.xyz vf1, vf1, Q
            this.ee.VF[1].x = (this.ee.VF[1].x * this.ee.Vq.f);
            this.ee.VF[1].y = (this.ee.VF[1].y * this.ee.Vq.f);
            this.ee.VF[1].z = (this.ee.VF[1].z * this.ee.Vq.f);

            // @00124D20   VMULAx.xyzw ACC, vf1, vf5x
            ee.Vacc.x = ee.VF[1].x * ee.VF[5].x;
            ee.Vacc.y = ee.VF[1].y * ee.VF[5].x;
            ee.Vacc.z = ee.VF[1].z * ee.VF[5].x;
            ee.Vacc.w = ee.VF[1].w * ee.VF[5].x;
            // @00124D20   VMULAx.xyzw ACC, vf1, vf5x
            this.ee.Vacc.x = (this.ee.VF[1].x * this.ee.VF[5].x);
            this.ee.Vacc.y = (this.ee.VF[1].y * this.ee.VF[5].x);
            this.ee.Vacc.z = (this.ee.VF[1].z * this.ee.VF[5].x);
            this.ee.Vacc.w = (this.ee.VF[1].w * this.ee.VF[5].x);

            // @00124D24   VMADDAy.xyzw ACC, vf2, vf5y
            ee.Vacc.x += ee.VF[2].x * ee.VF[5].y;
            ee.Vacc.y += ee.VF[2].y * ee.VF[5].y;
            ee.Vacc.z += ee.VF[2].z * ee.VF[5].y;
            ee.Vacc.w += ee.VF[2].w * ee.VF[5].y;
            // @00124D24   VMADDAy.xyzw ACC, vf2, vf5y
            this.ee.Vacc.x = (this.ee.Vacc.x
                        + (this.ee.VF[2].x * this.ee.VF[5].y));
            this.ee.Vacc.y = (this.ee.Vacc.y
                        + (this.ee.VF[2].y * this.ee.VF[5].y));
            this.ee.Vacc.z = (this.ee.Vacc.z
                        + (this.ee.VF[2].z * this.ee.VF[5].y));
            this.ee.Vacc.w = (this.ee.Vacc.w
                        + (this.ee.VF[2].w * this.ee.VF[5].y));

            // @00124D2C   VMADDw.xyzw vf5, vf4, vf5w
            ee.VF[5].x = ee.Vacc.x + ee.VF[4].x * ee.VF[5].w;
            ee.VF[5].y = ee.Vacc.y + ee.VF[4].y * ee.VF[5].w;
            ee.VF[5].z = ee.Vacc.z + ee.VF[4].z * ee.VF[5].w;
            ee.VF[5].w = ee.Vacc.w + ee.VF[4].w * ee.VF[5].w;
            // @00124D2C   VMADDw.xyzw vf5, vf4, vf5w
            this.ee.VF[5].x = (this.ee.Vacc.x
                        + (this.ee.VF[4].x * this.ee.VF[5].w));
            this.ee.VF[5].y = (this.ee.Vacc.y
                        + (this.ee.VF[4].y * this.ee.VF[5].w));
            this.ee.VF[5].z = (this.ee.Vacc.z
                        + (this.ee.VF[4].z * this.ee.VF[5].w));
            this.ee.VF[5].w = (this.ee.Vacc.w
                        + (this.ee.VF[4].w * this.ee.VF[5].w));

            // @00124D98   BNEL s1, zero, $00124e04
            if ((this.ee.s1.UD0 != this.ee.r0.UD0)) {
                this.pc = 1199620u;
                // @00124D9C   ADD.S $f0, $f23, $f23
                this.ee.fpr[0].f = (this.ee.fpr[23].f + this.ee.fpr[23].f);
            }

            // @00124E10   MSUBA.S $f23, $f23
            ee.fpracc.f = ee.fpracc.f - ee.fpr[23].f * ee.fpr[23].f;
            // @00124E10   MSUBA.S $f23, $f23
            this.ee.fpracc.f = (this.ee.fpracc.f
                        - (this.ee.fpr[23].f * this.ee.fpr[23].f));

            // @00124E14   MSUB.S $f1, $f22, $f22
            ee.fpr[1].f = ee.fpracc.f - ee.fpr[22].f * ee.fpr[22].f;
            // @00124E14   MSUB.S $f1, $f22, $f22
            this.ee.fpr[1].f = (this.ee.fpracc.f
                        - (this.ee.fpr[22].f * this.ee.fpr[22].f));

            // @00124E18   MUL.S $f0, $f0, $f22
            this.ee.fpr[0].f = (this.ee.fpr[0].f * this.ee.fpr[22].f);

            // @00124E28   DIV.S $f21, $f1, $f0
            this.ee.fpr[21].f = (this.ee.fpr[1].f / this.ee.fpr[0].f);

            // @00124E78   SW zero, $0008(s1)
            MobUt.SW(ee, ee.r0, 0x0008 + ee.s1.UL0);
            // @00124E78   SW zero, $0008(s1)
            MobUt.SW(this.ee, this.ee.r0, (8 + this.ee.s1.UL0));

            // @00125144   VOPMULA.xyz ACC, vf1, vf2
            ee.Vacc.x = ee.VF[1].y * ee.VF[2].z;
            ee.Vacc.y = ee.VF[1].z * ee.VF[2].x;
            ee.Vacc.z = ee.VF[1].x * ee.VF[2].y;
            // @00125144   VOPMULA.xyz ACC, vf1, vf2
            this.ee.Vacc.x = (this.ee.VF[1].y * this.ee.VF[2].z);
            this.ee.Vacc.y = (this.ee.VF[1].z * this.ee.VF[2].x);
            this.ee.Vacc.z = (this.ee.VF[1].x * this.ee.VF[2].y);

            // @00125148   VOPMSUB.xyz vf3xyz, vf2, vf1
            ee.VF[3].x = ee.Vacc.x - ee.VF[2].y * ee.VF[1].z;
            ee.VF[3].y = ee.Vacc.y - ee.VF[2].z * ee.VF[1].x;
            ee.VF[3].z = ee.Vacc.z - ee.VF[2].x * ee.VF[1].y;
            // @00125148   VOPMSUB.xyz vf3, vf2, vf1
            this.ee.VF[3].x = (this.ee.Vacc.x
                        - (this.ee.VF[2].y * this.ee.VF[1].z));
            this.ee.VF[3].y = (this.ee.Vacc.y
                        - (this.ee.VF[2].z * this.ee.VF[1].x));
            this.ee.VF[3].z = (this.ee.Vacc.z
                        - (this.ee.VF[2].x * this.ee.VF[1].y));

            // @00127E68   ANDI t7, t6, $0007
            ee.t7.UD0 = ee.t6.US[0] & 0x0007u;
            // @00127E68   ANDI t7, t6, $0007
            this.ee.t7.UD0 = ((ushort)((this.ee.t6.US0 & 7)));

            // @00127E78   BNE t7, zero, $00128254
            if ((this.ee.t7.UD0 != this.ee.r0.UD0)) {
                this.pc = 1213012u;
            }

            // @00128020   DSRL t2, t3, 9
            MobUt.DSRL(ee.t2, ee.t3, 9);
            // @00128020   DSRL t2, t3, 9
            MobUt.DSRL(this.ee.t2, this.ee.t3, 9);

            // @00128068   SB t6, $0021(sp)
            MobUt.SB(this.ee, this.ee.t6, (33 + this.ee.sp.UL0));

            // @0012809C   SLL t7, s0, 1
            MobUt.SLL(this.ee.t7, this.ee.s0, 1);
            // @0011B424   ADDIU t7, t7, $4260
            this.ee.t7.SD0 = ((int)((this.ee.t7.SD0 + 16992)));

            // @0012065C   ADDU t7, t7, t6
            this.ee.t7.SD0 = ((int)((this.ee.t7.SD0 + this.ee.t6.SD0)));

            // @00120670   LW t7, $0004(a0)
            MobUt.LW(this.ee, this.ee.t7, (4 + this.ee.a0.UL0));

            // @001206B4   JALR ra, v0
            ee.ra.UD0 = 0x001206B4 + 8;
            pc = ee.v0.UL0;
            // @001206B4   JALR ra, v0
            this.ee.ra.UD0 = 1181372u;
            this.pc = this.ee.v0.UL0;

            // @00128DA0   LHU a2, $0010(t7)
            MobUt.LHU(this.ee, this.ee.a2, (16 + this.ee.t7.UL0));

            // @00128DA4   SLT t6, t5, a2
            ee.t6.UD0 = Convert.ToByte(ee.t5.SD0 < ee.a2.SD0);
            // @00128DA4   SLT t6, t5, a2
            this.ee.t6.UD0 = System.Convert.ToByte((this.ee.t5.SD0 < this.ee.a2.SD0));

            // @001280A4   LBU t6, $0020(t5)
            MobUt.LBU(this.ee, this.ee.t6, (32 + this.ee.t5.UL0));

            // @00128104   SLTI t7, s0, $0003
            ee.t7.UD0 = Convert.ToByte(ee.s0.SD0 < 3);

            // @00128EA4   SLTIU t7, t6, $002c
            this.ee.t7.UD0 = System.Convert.ToByte((this.ee.t6.UD0 < 44ul));

            // @00129BA8   ORI t7, t7, $ffff
            this.ee.t7.UD0 = ((ushort)((this.ee.t7.US0 | 65535)));

            // @00129BB4   AND s4, s4, t7
            this.ee.s4.UD0 = (this.ee.s4.UD0 & this.ee.t7.UD0);

            // @00300400   MFC1 t3, $f12
            MobUt.MFC1(ee.t3, ee.fpr[12]);

            // @00145134   MFC1 t7, $f0
            MobUt.MFC1(this.ee.t7, this.ee.fpr[0]);

            // @00142018   SH s2, $0000(t6)
            MobUt.SH(this.ee, this.ee.s2, (0 + this.ee.t6.UL0));

            // @00128154   LBU t7, $ffff(s6)
            MobUt.LBU(this.ee, this.ee.t7, (4294967295u + this.ee.s6.UL0));

            // @002FAA3C   ADDI a3, a3, $ffff
            this.ee.a3.SD0 = ((int)((this.ee.a3.SD0 + -1)));

            // @002FA950   VMULx.yzw vf4, vf4, vf0x
            this.ee.VF[4].y = (this.ee.VF[4].y * this.ee.VF[0].x);
            this.ee.VF[4].z = (this.ee.VF[4].z * this.ee.VF[0].x);
            this.ee.VF[4].w = (this.ee.VF[4].w * this.ee.VF[0].x);

            // @0016274C   VADD.xyzw vf1, vf1, vf2
            this.ee.VF[1].x = (this.ee.VF[1].x + this.ee.VF[2].x);
            this.ee.VF[1].y = (this.ee.VF[1].y + this.ee.VF[2].y);
            this.ee.VF[1].z = (this.ee.VF[1].z + this.ee.VF[2].z);
            this.ee.VF[1].w = (this.ee.VF[1].w + this.ee.VF[2].w);

            // @00145848   LH t7, $000a(a1)
            MobUt.LH(this.ee, this.ee.t7, (10u + this.ee.a1.UL0));

            // @00162770   SLTU v0, zero, v0
            this.ee.v0.UD0 = System.Convert.ToByte((this.ee.r0.UD0 < this.ee.v0.UD0));

            // @001420CC   XOR t7, t7, t6
            MobUt.XOR(this.ee.t7, this.ee.t7, this.ee.t6);

            // @0012D4F4   SUBU t7, s2, t4
            this.ee.t7.SD0 = ((int)((this.ee.s2.UD0 - this.ee.t4.UD0)));

            // @00141FE8   MULT t7, t6, t5
            this.ee.t7.SD0 = (((long)(this.ee.t6.SL0)) * ((long)(this.ee.t5.SL0)));

            // @0012D2C8   BLTZ t7, $0012d33c
            if ((this.ee.t7.SD0 < 0)) {
                this.pc = 1233724u;
            }

            // @0012C808   BGEZ t4, $0012c860
            if ((this.ee.t4.SD0 >= 0)) {
                this.pc = 1230944u;
            }

            // @0012C984   BGEZL t7, $0012c9dc
            if ((this.ee.t7.SD0 >= 0)) {
                this.pc = 1231324u;
                // @0012C988   LW t6, $0004(s0)
                MobUt.LW(this.ee, this.ee.t6, (4u + this.ee.s0.UL0));
            }

            // @0012BD40   PCPYLD t2, t7, t5
            ee.t2.UD1 = ee.t7.UD0;
            ee.t2.UD0 = ee.t5.UD0;

            // @0012BD40   PCPYLD t2, t7, t5
            this.ee.t2.UD1 = this.ee.t7.UD0;
            this.ee.t2.UD0 = this.ee.t5.UD0;

            // @0012BD3C   PCPYUD t1, t4, t6
            this.ee.t1.UD0 = this.ee.t4.UD1;
            this.ee.t1.UD1 = this.ee.t6.UD1;

            // @0012BD34   PEXTUW t7, t3, t2
            this.ee.t7.UL[0] = this.ee.t2.UL[2];
            this.ee.t7.UL[1] = this.ee.t3.UL[2];
            this.ee.t7.UL[2] = this.ee.t2.UL[3];
            this.ee.t7.UL[3] = this.ee.t3.UL[3];

            // @0012AC74   DSRL32 t7, t5, 20
            MobUt.DSRL32(this.ee.t7, this.ee.t5, 20);

            // @0012AC60   DSLL32 t4, t4, 21
            MobUt.DSLL32(this.ee.t4, this.ee.t4, 21);

            // @00162328   BLEZ v0, $00162464
            if ((this.ee.v0.SD0 <= 0)) {
                this.pc = 1451108u;
            }

            // @00145300   CVT.W.S $f0, $f1
            MobUt.CVT_W(this.ee.fpr[0], this.ee.fpr[1]);

            // @00143F44   OR s0, s0, v0
            this.ee.s0.UD0 = (this.ee.s0.UD0 | this.ee.v0.UD0);

            // @0012D3B0   MOVN t5, t6, t7
            if ((this.ee.t7.UD0 != 0)) {
                this.ee.t5.UD0 = this.ee.t6.UD0;
            }
        }
    }
#endif
}
