using OpenKh.Engine.Maths;
using OpenKh.Kh2;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;

namespace OpenKh.Engine.Parsers.Kddf2
{
    public class Kkdf2MdlxParser
    {
        public class LMap
        {
            public byte[] al = new byte[256];

            public LMap()
            {
                for (int x = 0; x < 256; x++)
                {
                    al[x] = (byte)Math.Min(255, 2 * x);
                }
            }
        }

        public class CI
        {
            public uint[] ali;
            public int texi, vifi;
        }

        public readonly List<CI> alci = new List<CI>();

        public readonly SortedDictionary<int, Model> dictModel = new SortedDictionary<int, Model>();
        public readonly LMap lm;

        public Kkdf2MdlxParser(List<Mdlx.SubModel> submodels)
        {
            const float scalf = 1f;

            foreach (var submodel in submodels)
            {
                var alaxb = submodel.Bones.ToArray();
                var matrices = new Matrix[alaxb.Length];
                {
                    var Va = new Vector3[matrices.Length];
                    var Qa = new Quaternion[matrices.Length];
                    for (int x = 0; x < matrices.Length; x++)
                    {
                        Quaternion Qo;
                        Vector3 Vo;
                        var axb = alaxb[x];
                        var parent = axb.Parent;
                        if (parent < 0)
                        {
                            Qo = Quaternion.Identity;
                            Vo = Vector3.Zero;
                        }
                        else
                        {
                            Qo = Qa[parent];
                            Vo = Va[parent];
                        }

                        var Vt = Vector3.TransformCoordinate(new Vector3(axb.TranslationX, axb.TranslationY, axb.TranslationZ), Matrix.RotationQuaternion(Qo));
                        Va[x] = Vo + Vt;

                        var Qt = Quaternion.Identity;
                        if (axb.RotationX != 0) Qt *= (Quaternion.RotationAxis(new Vector3(1, 0, 0), axb.RotationX));
                        if (axb.RotationY != 0) Qt *= (Quaternion.RotationAxis(new Vector3(0, 1, 0), axb.RotationY));
                        if (axb.RotationZ != 0) Qt *= (Quaternion.RotationAxis(new Vector3(0, 0, 1), axb.RotationZ));
                        Qa[x] = Qt * Qo;
                    }
                    for (int x = 0; x < matrices.Length; x++)
                    {
                        var M = Matrix.RotationQuaternion(Qa[x]);
                        M *= Matrix.Translation(Va[x]);
                        matrices[x] = M;
                    }
                }

                var albody1 = new List<Body1e>();
                var Mv = Matrix.Identity;
                foreach (Mdlx.DmaVif t13 in submodel.DmaChains.SelectMany(dmaChain => dmaChain.DmaVifs))
                {
                    var mem = new VU1Mem();
                    int tops = 0x40, top2 = 0x220;
                    new ParseVIF1(mem).Parse(new MemoryStream(t13.VifPacket, false), tops);
                    var b1 = SimaVU1e.Sima(mem, matrices, tops, top2, t13.TextureIndex, t13.Alaxi, Mv);
                    albody1.Add(b1);
                }

                if (true)
                {
                    FfMesh ffmesh = new FfMesh();
                    if (true)
                    {
                        int svi = 0;
                        int sti = 0;
                        Ff1[] alo1 = new Ff1[4];
                        int ai = 0;
                        int[] ord = new int[] { 1, 3, 2 };
                        foreach (Body1e b1 in albody1)
                        {
                            for (int x = 0; x < b1.alvi.Length; x++)
                            {
                                Ff1 o1 = new Ff1(svi + b1.alvi[x], sti + x);
                                alo1[ai] = o1;
                                ai = (ai + 1) & 3;
                                int fl = b1.alfl[x];
                                if (fl == 0x20 || fl == 0x00)
                                {
                                    Ff3 o3 = new Ff3(b1.t,
                                        alo1[(ai - ord[0]) & 3],
                                        alo1[(ai - ord[1]) & 3],
                                        alo1[(ai - ord[2]) & 3]
                                        );
                                    ffmesh.al3.Add(o3);
                                }
                                if (fl == 0x30 || fl == 0x00)
                                {
                                    Ff3 o3 = new Ff3(b1.t,
                                        alo1[(ai - ord[0]) & 3],
                                        alo1[(ai - ord[2]) & 3],
                                        alo1[(ai - ord[1]) & 3]
                                        );
                                    ffmesh.al3.Add(o3);
                                }
                            }
                            for (int x = 0; x < b1.alvertraw.Length; x++)
                            {
                                if (b1.alalni[x] == null)
                                {
                                    ffmesh.alpos.Add(Vector3.Zero);
                                    ffmesh.almtxuse.Add(new MJ1[0]);
                                    continue;
                                }
                                if (b1.alalni[x].Length == 1)
                                {
                                    MJ1 mj1 = b1.alalni[x][0];
                                    mj1.factor = 1.0f;
                                    Vector3 vpos = Vector3.TransformCoordinate(VCUt.V4To3(b1.alvertraw[mj1.vertexIndex]), matrices[mj1.matrixIndex]);
                                    ffmesh.alpos.Add(vpos);
                                }
                                else
                                {
                                    Vector3 vpos = Vector3.Zero;
                                    foreach (MJ1 mj1 in b1.alalni[x])
                                    {
                                        vpos += VCUt.V4To3(Vector4.Transform(b1.alvertraw[mj1.vertexIndex], matrices[mj1.matrixIndex]));
                                    }
                                    ffmesh.alpos.Add(vpos);
                                }
                                ffmesh.almtxuse.Add(b1.alalni[x]);
                            }
                            for (int x = 0; x < b1.aluv.Length; x++)
                            {
                                Vector2 vst = b1.aluv[x];
                                vst.Y = 1.0f - vst.Y; // !
                                ffmesh.alst.Add(vst);
                            }
                            svi += b1.alvertraw.Length;
                            sti += b1.aluv.Length;
                        }
                    }

                    {   // position xyz
                        int ct = ffmesh.al3.Count;
                        for (int t = 0; t < ct; t++)
                        {
                            Ff3 X3 = ffmesh.al3[t];
                            Model model;
                            if (dictModel.TryGetValue(X3.texi, out model) == false)
                            {
                                dictModel[X3.texi] = model = new Model();
                            }
                            for (int i = 0; i < X3.al1.Length; i++)
                            {
                                Ff1 X1 = X3.al1[i];
                                Vector3 v = (ffmesh.alpos[X1.vi] * scalf);
                                Vector2 txy = ffmesh.alst[X1.ti];
                                model.alv.Add(new CustomVertex.PositionColoredTextured(v, -1, txy.X, 1 - txy.Y));
                            }
                        }
                    }
                }
                break;
            }
        }

        public Kkdf2MdlxParser(Mdlx.M4 m4)
        {
            lm = new LMap();
            var texi = 0;
            int[] alrecent = new int[4];
            int recenti = 0;

            var model = new Model();
            var alv3 = model.alv;
            for (int tari = 0; tari < m4.VifPackets.Count; tari++)
            {
                var vli = m4.VifPackets[tari];
                byte[] vifpkt = vli.VifPacket;
                VU1Mem memo = new VU1Mem();
                ParseVIF1 pv1 = new ParseVIF1(memo);
                pv1.Parse(new MemoryStream(vifpkt, false), 0x00);
                foreach (byte[] ram in pv1.almsmem)
                {
                    CI ci = new CI();

                    MemoryStream si = new MemoryStream(ram, false);
                    BinaryReader br = new BinaryReader(si);

                    br.ReadInt32();
                    br.ReadInt32();
                    br.ReadInt32();
                    br.ReadInt32();
                    int v10 = br.ReadInt32(); // v10: cnt tex&vi&flg verts
                    int v14 = br.ReadInt32(); // v14: off tex&vi&flg verts
                    br.ReadInt32();
                    br.ReadInt32();
                    int v20 = br.ReadInt32(); // v20: cnt clr verts
                    int v24 = br.ReadInt32(); // v24: off clr verts
                    br.ReadInt32();
                    br.ReadInt32();
                    int v30 = br.ReadInt32(); // v30: cnt pos vert
                    int v34 = br.ReadInt32(); // v34: off pos vert

                    List<uint> altris = new List<uint>();
                    int bi = alv3.Count;
                    for (int i = 0; i < v10; i++)
                    {
                        si.Position = 16 * (v14 + i);
                        int tx = br.ReadInt16(); br.ReadInt16();
                        int ty = br.ReadInt16(); br.ReadInt16();
                        int vi = br.ReadInt16(); br.ReadInt16();
                        int fl = br.ReadInt16(); br.ReadInt16();
                        si.Position = 16 * (v34 + vi);
                        Vector3 v3;
                        v3.X = -br.ReadSingle();
                        v3.Y = +br.ReadSingle();
                        v3.Z = +br.ReadSingle();

                        si.Position = 16 * (v24 + i);
                        int fR = (byte)br.ReadUInt32();
                        int fG = (byte)br.ReadUInt32();
                        int fB = (byte)br.ReadUInt32();
                        int fA = (byte)br.ReadUInt32();

                        if (v24 == 0)
                        {
                            fR = 255;
                            fG = 255;
                            fB = 255;
                            fA = 255;
                        }

                        alrecent[recenti & 3] = bi + i;
                        recenti++;

                        //Debug.WriteLine("# " + fl.ToString("x2"));

                        if (fl == 0x00)
                        {
                        }
                        else if (fl == 0x10)
                        {
                        }
                        else if (fl == 0x20)
                        {
                            altris.Add(Convert.ToUInt32(alrecent[(recenti - 1) & 3]));
                            altris.Add(Convert.ToUInt32(alrecent[(recenti - 2) & 3]));
                            altris.Add(Convert.ToUInt32(alrecent[(recenti - 3) & 3]));
                        }
                        else if (fl == 0x30)
                        {
                            altris.Add(Convert.ToUInt32(alrecent[(recenti - 1) & 3]));
                            altris.Add(Convert.ToUInt32(alrecent[(recenti - 3) & 3]));
                            altris.Add(Convert.ToUInt32(alrecent[(recenti - 2) & 3]));
                        }

                        Color clr = Color.FromArgb(lm.al[fA], lm.al[fR], lm.al[fG], lm.al[fB]);

                        CustomVertex.PositionColoredTextured cv = new CustomVertex.PositionColoredTextured(
                            v3,
                            clr.ToArgb(),
                            +tx / 16.0f / 256.0f,
                            +ty / 16.0f / 256.0f
                            );
                        alv3.Add(cv);
                    }

                    ci.ali = altris.ToArray();
                    ci.texi = texi + vli.TextureId;
                    ci.vifi = tari;
                    alci.Add(ci);
                }
            }

            dictModel.Add(0, model);
        }
    }
}
