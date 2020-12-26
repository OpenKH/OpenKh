using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace OpenKh.Kh2Anim.Mset
{
    public class PosTbl
    {
        public int tbloff = 0x90;

        public int va0;
        public int va2; // cnt t4
        public int va8; // off t5 (each 64 bytes)  { cnt_t5 = va2 -va0 }
        public int vac; // off t4 (each 4 bytes)
        public int vb0; // cnt t11
        public int vb4; // off t1 (each 8 bytes)
        public int vb8; // cnt t1
        public int vc0; // off t2 (each 6 bytes)
        public int vc4; // cnt t2
        public int vc8; // off t2` (each 6 bytes)
        public int vcc; // cnt t2`
        public int vd0; // off t9 (each 8 bytes)
        public int vd4; // off t11 (each 4 bytes)
        public int vd8; // off t10 (each 4 bytes)
        public int vdc; // off t12 (each 4 bytes)
        public int ve0; // off t3 (each 12 bytes)
        public int ve4; // cnt t3
        public int ve8;
        public int vec; // off t8 (each 48 bytes)  { cnt_t8 = cnt_t2` }
        public int vf0; // off t7 (each 8 bytes)
        public int vf4; // cnt t7
        public int vf8; // off t6 (each 12 bytes)
        public int vfc; // cnt t6

        public PosTbl(Stream si)
        {
            BinaryReader br = new BinaryReader(si);
            int off = tbloff - 0x90;

            // Clone→
            si.Position = off + 0xA0;
            va0 = br.ReadUInt16();
            va2 = br.ReadUInt16(); // cnt t4
            si.Position = off + 0xA8;
            va8 = br.ReadInt32(); // off t5 (each 64 bytes)  { cnt_t5 = va2 -va0 }
            vac = br.ReadInt32(); // off t4 (each 4 bytes)

            si.Position = off + 0xB0;
            vb0 = br.ReadInt32(); // cnt t11
            vb4 = br.ReadInt32(); // off t1 (each 8 bytes)
            vb8 = br.ReadInt32(); // cnt t1
            si.Position = off + 0xC0;
            vc0 = br.ReadInt32(); // off t2 (each 6 bytes)
            vc4 = br.ReadInt32(); // cnt t2
            vc8 = br.ReadInt32(); // off t2` (each 6 bytes)
            vcc = br.ReadInt32(); // cnt t2`
            si.Position = off + 0xD0;
            vd0 = br.ReadInt32(); // off t9 (each 8 bytes)
            vd4 = br.ReadInt32(); // off t11 (each 4 bytes)
            vd8 = br.ReadInt32(); // off t10 (each 4 bytes)
            vdc = br.ReadInt32(); // off t12 (each 4 bytes)
            si.Position = off + 0xE0;
            ve0 = br.ReadInt32(); // off t3 (each 12 bytes)
            ve4 = br.ReadInt32(); // cnt t3
            ve8 = br.ReadInt32();
            vec = br.ReadInt32(); // off t8 (each 48 bytes)  { cnt_t8 = cnt_t2` }
            si.Position = off + 0xF0;
            vf0 = br.ReadInt32(); // off t7 (each 8 bytes)
            vf4 = br.ReadInt32(); // cnt t7
            vf8 = br.ReadInt32(); // off t6 (each 12 bytes)
            vfc = br.ReadInt32(); // cnt t6
            // ←Clone
        }
    }
}
