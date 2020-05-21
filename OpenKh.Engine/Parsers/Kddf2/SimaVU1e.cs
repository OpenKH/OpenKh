using OpenKh.Engine.Maths;
using System;
using System.IO;

namespace OpenKh.Engine.Parsers.Kddf2
{
    public class SimaVU1e
    {
        public static Body1e Sima(byte[] vu1mem, Matrix[] Ma, int tops, int top2, int tsel, int[] alaxi, Matrix Mv)
        {
            MemoryStream si = new MemoryStream(vu1mem, true);
            BinaryReader br = new BinaryReader(si);

            si.Position = 16 * (tops);

            var vpu = VpuPacket.Header(si);

            if (vpu.Type != 1 && vpu.Type != 2) throw new ProtInvalidTypeException();
            int v04 = vpu.Unknown04;
            int v08 = vpu.Unknown08;
            int v0c = vpu.Unknown1cLocation;
            int offMatrices = vpu.Unknown1cLocation; // off matrices

            // Shady logic here???? It MIGHT cause problems:
            // If vpu.Type != 1, those 0x10 bytes will be skipped. Therefore,
            // stuff that it's at 0x40, will be at 0x30 instead.
            int cntvertscolor = (vpu.Type == 1) ? vpu.ColorCount : 0; // cntvertscolor
            int offvertscolor = (vpu.Type == 1) ? vpu.ColorLocation : 0; // offvertscolor
            int specCnt = (vpu.Type == 1) ? vpu.Unknown28 : 0; // cnt spec
            int specOff = (vpu.Type == 1) ? vpu.Unknown2c : 0; // off spec

            int v38 = vpu.Unknown38; // 

            si.Position = 16 * (tops + vpu.UnkBoxLocation);
            int[] box1 = new int[vpu.UnkBoxCount];
            for (int x = 0; x < box1.Length; x++)
            {
                box1[x] = br.ReadInt32();
            }

            Body1e body1 = new Body1e();
            body1.t = tsel;
            body1.alvertraw = new Vector4[vpu.VertexCount];
            body1.avail = (specCnt == 0) && (vpu.Type == 1);
            body1.alalni = new MJ1[vpu.VertexCount][];

            MJ1[] alni = new MJ1[vpu.VertexCount];

            int vi = 0;
            si.Position = 16 * (tops + vpu.VertexLocation);
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

            body1.aluv = new Vector2[vpu.IndexCount];
            body1.alvi = new int[vpu.IndexCount];
            body1.alfl = new int[vpu.IndexCount];

            int mini = int.MaxValue, maxi = int.MinValue;

            si.Position = 16 * (tops + vpu.IndexLocation);
            for (int x = 0; x < vpu.IndexCount; x++)
            {
                int tx = br.ReadUInt16() / 16; br.ReadUInt16();
                int ty = br.ReadUInt16() / 16; br.ReadUInt16();
                body1.aluv[x] = new Vector2(tx / 256.0f, ty / 256.0f);
                int locvi = body1.alvi[x] = br.ReadUInt16(); br.ReadUInt16();
                body1.alfl[x] = br.ReadUInt16(); br.ReadUInt16();

                mini = Math.Min(mini, locvi);
                maxi = Math.Max(maxi, locvi);
            }

            if (specCnt != 0)
            {
                si.Position = 16 * (tops + specOff);
                int vt0 = br.ReadInt32();
                int vt1 = br.ReadInt32();
                int vt2 = br.ReadInt32();
                int vt3 = br.ReadInt32();
                int vt4 = 0;
                if (specCnt >= 5)
                {
                    vt4 = br.ReadInt32();
                    br.ReadInt32();
                    br.ReadInt32();
                    br.ReadInt32();
                }

                MJ1[][] localalalni = new MJ1[vpu.VertexCount][];
                int xi = 0;
                for (xi = 0; xi < vt0; xi++)
                {
                    int ai = br.ReadInt32();
                    localalalni[xi] = (new MJ1[] { alni[ai] });
                }
                if (specCnt >= 2)
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
                if (specCnt >= 3)
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
                if (specCnt >= 4)
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
                if (specCnt >= 5)
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
                if (specCnt >= 6)
                {
                    throw new Exception("v28: " + specCnt);
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
