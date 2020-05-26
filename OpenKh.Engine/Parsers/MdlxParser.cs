using OpenKh.Engine.Maths;
using OpenKh.Engine.Parsers.Kddf2;
using OpenKh.Engine.Parsers.Kddf2.Mset;
using OpenKh.Engine.Parsers.Kddf2.Mset.EmuRunner;
using OpenKh.Kh2;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace OpenKh.Engine.Parsers
{
    public class MdlxParser
    {
        public MdlxParser(Mdlx mdlx)
        {
            if (IsEntity(mdlx))
            {
                var builtModel = FromEntity(mdlx);
                Model = new Model
                {
                    Segments = builtModel.textureIndexBasedModelDict.Values.Select(x => new Model.Segment
                    {
                        Vertices = x.Vertices.Select(vertex => new PositionColoredTextured
                        {
                            X = vertex.X,
                            Y = vertex.Y,
                            Z = vertex.Z,
                            U = vertex.Tu,
                            V = vertex.Tv,
                            Color = vertex.Color
                        }).ToArray()
                    }).ToArray(),
                    Parts = builtModel.parser.MeshDescriptors.Select(x => new Model.Part
                    {
                        Indices = x.Indices,
                        SegmentIndex = x.SegmentIndex,
                        TextureIndex = x.TextureIndex
                    }).ToArray()
                };
            }
            else if (IsMap(mdlx))
            {
                var myParser = new NewModelParser(mdlx);
                Model = new Model
                {
                    Segments = new Model.Segment[]
                    {
                        new Model.Segment
                        {
                            Vertices = myParser.Vertices.Select(vertex => new PositionColoredTextured
                            {
                                X = vertex.X,
                                Y = vertex.Y,
                                Z = vertex.Z,
                                U = vertex.Tu,
                                V = vertex.Tv,
                                Color = vertex.Color
                            }).ToArray()
                        }
                    },
                    Parts = myParser.MeshDescriptors.Select(x => new Model.Part
                    {
                        Indices = x.Indices,
                        SegmentIndex = x.SegmentIndex,
                        TextureIndex = x.TextureIndex
                    }).ToArray()
                };
            }
        }

        private static Kkdf2MdlxBuiltModel FromEntity(Mdlx mdlx)
        {
            Matrix[] matrixOut;
            {
                var mdlxFile = @"H:\KH2fm.yaz0r\obj\P_EX100.mdlx";
                var msetFile = @"H:\KH2fm.yaz0r\obj\P_EX100.mset";

                var mdlxStream = new MemoryStream(File.ReadAllBytes(mdlxFile), false);
                var msetStream = new MemoryStream(File.ReadAllBytes(msetFile), false);

                var msetBar = Bar.Read(msetStream);
                var anbBar = msetBar
                    .First(it => it.Type == Bar.EntryType.Bar);
                var animEntry = Bar.Read(anbBar.Stream)
                    .First(it => it.Type == Bar.EntryType.AnimationData);
                var animStream = animEntry.Stream;

                var anbAbsOff = (uint)(anbBar.Offset + animEntry.Offset);

                mdlxStream.Position = 0;
                msetStream.Position = 0;

                var anbParser = new AnimReader(animStream);
                var matrixOutStream = new MemoryStream();
                var emuRunner = new Mlink();
                emuRunner.Permit(
                    mdlxStream, anbParser.cntb1,
                    msetStream, anbParser.cntb2,
                    anbAbsOff, 0, matrixOutStream
                );

                BinaryReader br = new BinaryReader(matrixOutStream);
                matrixOutStream.Position = 0;
                matrixOut = new Matrix[anbParser.cntb1];
                for (int t = 0; t < anbParser.cntb1; t++)
                {
                    Matrix M1 = new Matrix();
                    M1.M11 = br.ReadSingle(); M1.M12 = br.ReadSingle(); M1.M13 = br.ReadSingle(); M1.M14 = br.ReadSingle();
                    M1.M21 = br.ReadSingle(); M1.M22 = br.ReadSingle(); M1.M23 = br.ReadSingle(); M1.M24 = br.ReadSingle();
                    M1.M31 = br.ReadSingle(); M1.M32 = br.ReadSingle(); M1.M33 = br.ReadSingle(); M1.M34 = br.ReadSingle();
                    M1.M41 = br.ReadSingle(); M1.M42 = br.ReadSingle(); M1.M43 = br.ReadSingle(); M1.M44 = br.ReadSingle();
                    matrixOut[t] = M1;
                }
            }

            var parser = new Kddf2.Kkdf2MdlxParser(mdlx.SubModels.First());
            var builtModel = parser
                .ProcessVerticesAndBuildModel(
                    matrixOut
                //MdlxMatrixUtil.BuildTPoseMatrices(mdlx.SubModels.First(), Matrix.Identity)
                );

            var ci = builtModel.textureIndexBasedModelDict.Values.Select((model, i) => new Kddf2.Kkdf2MdlxParser.CI
            {
                Indices = model.Vertices.Select((_, index) => index).ToArray(),
                TextureIndex = i,
                SegmentIndex = i
            });

            parser.MeshDescriptors.AddRange(ci);

            return builtModel;
        }

        private static bool IsEntity(Mdlx mdlx) => mdlx.SubModels != null;

        private static bool IsMap(Mdlx mdlx) => mdlx.MapModel != null;

        public Model Model { get; }
    }
}
