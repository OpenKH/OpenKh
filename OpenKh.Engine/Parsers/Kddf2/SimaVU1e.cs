using OpenKh.Engine.Maths;
using System;
using System.IO;

namespace OpenKh.Engine.Parsers.Kddf2
{
    public class SimaVU1e
    {
        public static Body1e Sima(VU1Mem vu1mem, Matrix[] Ma, int tops, int top2, int tsel, int[] alaxi, Matrix Mv)
        {
            MemoryStream si = new MemoryStream(vu1mem.vumem, true);
            BinaryReader br = new BinaryReader(si);

            si.Position = 16 * (tops);

            int v00 = br.ReadInt32();
            if (v00 != 1 && v00 != 2) throw new ProtInvalidTypeException();
            int v04 = br.ReadInt32();
            int v08 = br.ReadInt32();
            int v0c = br.ReadInt32();
            int v10 = br.ReadInt32(); // cnt box2
            int v14 = br.ReadInt32(); // off box2 {tx ty vi fl}
            int v18 = br.ReadInt32(); // off box1
            int v1c = br.ReadInt32(); // off matrices
            int v20 = (v00 == 1) ? br.ReadInt32() : 0; // cntvertscolor
            int v24 = (v00 == 1) ? br.ReadInt32() : 0; // offvertscolor
            int v28 = (v00 == 1) ? br.ReadInt32() : 0; // cnt spec
            int v2c = (v00 == 1) ? br.ReadInt32() : 0; // off spec
            int v30 = br.ReadInt32(); // cnt verts 
            int v34 = br.ReadInt32(); // off verts
            int v38 = br.ReadInt32(); // 
            int v3c = br.ReadInt32(); // cnt box1

            si.Position = 16 * (tops + v18);
            int[] box1 = new int[v3c];
            for (int x = 0; x < box1.Length; x++)
            {
                box1[x] = br.ReadInt32();
            }

            Body1e body1 = new Body1e();
            body1.t = tsel;
            body1.alvertraw = new Vector4[v30];
            body1.avail = (v28 == 0) && (v00 == 1);
            body1.alalni = new MJ1[v30][];

            MJ1[] alni = new MJ1[v30];

            int vi = 0;
            si.Position = 16 * (tops + v34);
            for (int x = 0; x < box1.Length; x++)
            {
                int ct = box1[x];
                for (int t = 0; t < ct; t++, vi++)
                {
                    float fx = br.ReadSingle();
                    float fy = br.ReadSingle();
                    float fz = br.ReadSingle();
                    float fw = br.ReadSingle();
                    Vector4 v4 = new Vector4(fx, fy, fz, fw);
                    body1.alvertraw[vi] = Vector4.Transform(v4, Mv);
                    body1.alalni[vi] = new MJ1[] { alni[vi] = new MJ1(alaxi[x], vi, fw) };
                }
            }

            body1.aluv = new Vector2[v10];
            body1.alvi = new int[v10];
            body1.alfl = new int[v10];

            int mini = int.MaxValue, maxi = int.MinValue;

            si.Position = 16 * (tops + v14);
            for (int x = 0; x < v10; x++)
            {
                int tx = br.ReadUInt16() / 16; br.ReadUInt16();
                int ty = br.ReadUInt16() / 16; br.ReadUInt16();
                body1.aluv[x] = new Vector2(tx / 256.0f, ty / 256.0f);
                int locvi = body1.alvi[x] = br.ReadUInt16(); br.ReadUInt16();
                body1.alfl[x] = br.ReadUInt16(); br.ReadUInt16();

                mini = Math.Min(mini, locvi);
                maxi = Math.Max(maxi, locvi);
            }

            if (v28 != 0)
            {
                si.Position = 16 * (tops + v2c);
                int vt0 = br.ReadInt32();
                int vt1 = br.ReadInt32();
                int vt2 = br.ReadInt32();
                int vt3 = br.ReadInt32();
                int vt4 = 0;
                if (v28 >= 5)
                {
                    vt4 = br.ReadInt32();
                    br.ReadInt32();
                    br.ReadInt32();
                    br.ReadInt32();
                }

                MJ1[][] localalalni = new MJ1[v30][];
                int xi = 0;
                for (xi = 0; xi < vt0; xi++)
                {
                    int ai = br.ReadInt32();
                    localalalni[xi] = (new MJ1[] { alni[ai] });
                }
                if (v28 >= 2)
                {
                    //Debug.Fail("v28: " + v28);

                    si.Position = (si.Position + 15) & (~15);
                    for (int x = 0; x < vt1; x++, xi++)
                    {
                        int i0 = br.ReadInt32();
                        int i1 = br.ReadInt32();
                        localalalni[xi] = (new MJ1[] { alni[i0], alni[i1] });
                    }
                }
                if (v28 >= 3)
                {
                    //Debug.Fail("v28: " + v28);

                    si.Position = (si.Position + 15) & (~15);
                    for (int x = 0; x < vt2; x++, xi++)
                    {
                        int i0 = br.ReadInt32();
                        int i1 = br.ReadInt32();
                        int i2 = br.ReadInt32();
                        localalalni[xi] = (new MJ1[] { alni[i0], alni[i1], alni[i2] });
                    }
                }
                if (v28 >= 4)
                {
                    //Debug.Fail("v28: " + v28);

                    si.Position = (si.Position + 15) & (~15);
                    for (int x = 0; x < vt3; x++, xi++)
                    {
                        int i0 = br.ReadInt32();
                        int i1 = br.ReadInt32();
                        int i2 = br.ReadInt32();
                        int i3 = br.ReadInt32();
                        localalalni[xi] = (new MJ1[] { alni[i0], alni[i1], alni[i2], alni[i3] });
                    }
                }
                if (v28 >= 5)
                {
                    si.Position = (si.Position + 15) & (~15);
                    for (int x = 0; x < vt4; x++, xi++)
                    {
                        int i0 = br.ReadInt32();
                        int i1 = br.ReadInt32();
                        int i2 = br.ReadInt32();
                        int i3 = br.ReadInt32();
                        int i4 = br.ReadInt32();
                        localalalni[xi] = (new MJ1[] { alni[i0], alni[i1], alni[i2], alni[i3], alni[i4] });
                    }
                }
                if (v28 >= 6)
                {
                    throw new Exception("v28: " + v28);
                }
                for (int t = mini; t <= maxi; t++)
                {
                    if (localalalni[t] == null)
                        throw new Exception($"localalani {t} is null");
                }
                body1.alalni = localalalni;
            }

            return body1;
        }
    }
}
