using OpenKh.Common;
using OpenKh.Engine.Maths;
using OpenKh.Kh2;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace OpenKh.Engine.Parsers.Kddf2
{
    public class Kkdf2MdlxParser
    {
        public class CI
        {
            public int[] Indices;
            public int TextureIndex, SegmentIndex;
        }

        public List<CI> MeshDescriptors { get; } = new List<CI>();

        public SortedDictionary<int, Model> Models { get; } = new SortedDictionary<int, Model>();

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
                    const int tops = 0x40, top2 = 0x220;

                    var unpacker = new VifUnpacker(t13.VifPacket)
                    {
                        Vif1_Tops = tops
                    };
                    unpacker.Run();

                    var b1 = SimaVU1e.Sima(unpacker.Memory, matrices, tops, top2, t13.TextureIndex, t13.Alaxi, Mv);
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
                            if (Models.TryGetValue(X3.texi, out model) == false)
                            {
                                Models[X3.texi] = model = new Model();
                            }
                            for (int i = 0; i < X3.al1.Length; i++)
                            {
                                Ff1 X1 = X3.al1[i];
                                Vector3 v = (ffmesh.alpos[X1.vi] * scalf);
                                Vector2 txy = ffmesh.alst[X1.ti];
                                model.Vertices.Add(new CustomVertex.PositionColoredTextured(v, -1, txy.X, 1 - txy.Y));
                            }
                        }
                    }
                }
                break;
            }
        }
    }
}
