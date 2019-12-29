using OpenKh.Kh2;
using System.Linq;

namespace OpenKh.Engine.Parsers
{
    public class MdlxParser
    {
        public MdlxParser(Mdlx mdlx)
        {
            Kddf2.Kkdf2MdlxParser parser = null;

            if (IsEntity(mdlx))
                parser = FromEntity(mdlx);
            else if (IsMap(mdlx))
                parser = FromMap(mdlx);

            Model = new Model
            {
                Segments = parser.dictModel.Values.Select(x => new Model.Segment
                {
                    Vertices = x.alv.Select(vertex => new PositionColoredTextured
                    {
                        X = vertex.X,
                        Y = vertex.Y,
                        Z = vertex.Z,
                        U = vertex.Tu,
                        V = vertex.Tv,
                        Color = vertex.Color
                    }).ToArray()
                }).ToArray(),
                Parts = parser.alci.Select(x => new Model.Part
                { 
                    Indices = x.ali.Select(i => (int)i).ToArray(),
                    SegmentId = x.vifi,
                    TextureId = x.texi
                }).ToArray()
            };
        }

        private Kddf2.Kkdf2MdlxParser FromEntity(Mdlx mdlx)
        {
            var parser = new Kddf2.Kkdf2MdlxParser(mdlx.SubModels);
            var ci = parser.dictModel.Values.Select((model, i) => new Kddf2.Kkdf2MdlxParser.CI
            {
                ali = model.alv.Select((_, index) => (uint)index).ToArray(),
                texi = i,
                vifi = i
            });

            parser.alci.AddRange(ci);

            return parser;
        }

        private Kddf2.Kkdf2MdlxParser FromMap(Mdlx mdlx) => new Kddf2.Kkdf2MdlxParser(mdlx.MapModel);

        private static bool IsEntity(Mdlx mdlx) => mdlx.SubModels != null;

        private static bool IsMap(Mdlx mdlx) => mdlx.MapModel != null;

        public Model Model { get; }
    }
}
