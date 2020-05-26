using OpenKh.Engine.Maths;
using OpenKh.Engine.Parsers.Kddf2;
using OpenKh.Kh2;
using System.Linq;

namespace OpenKh.Engine.Parsers
{
    public class MdlxParser
    {
        public MdlxParser(Mdlx mdlx)
        {
            if (IsEntity(mdlx))
            {
                var parser = FromEntity(mdlx);
                Model = new Model
                {
                    Segments = parser.Models.Values.Select(x => new Model.Segment
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
                    Parts = parser.MeshDescriptors.Select(x => new Model.Part
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

        private static Kddf2.Kkdf2MdlxParser FromEntity(Mdlx mdlx)
        {
            var parser = new Kddf2.Kkdf2MdlxParser(mdlx.SubModels.First())
                .ProcessVerticesAndBuildModel(
                    MdlxMatrixUtil.BuildTPoseMatrices(mdlx.SubModels.First(), Matrix.Identity)
                );

            var ci = parser.Models.Values.Select((model, i) => new Kddf2.Kkdf2MdlxParser.CI
            {
                Indices = model.Vertices.Select((_, index) => index).ToArray(),
                TextureIndex = i,
                SegmentIndex = i
            });

            parser.MeshDescriptors.AddRange(ci);

            return parser;
        }

        private static bool IsEntity(Mdlx mdlx) => mdlx.SubModels != null;

        private static bool IsMap(Mdlx mdlx) => mdlx.MapModel != null;

        public Model Model { get; }
    }
}
