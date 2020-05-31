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
                Model = new MdlxAnimModel(mdlx);
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

        private static bool IsEntity(Mdlx mdlx) => mdlx.SubModels != null;

        private static bool IsMap(Mdlx mdlx) => mdlx.MapModel != null;

        public Model Model { get; }
    }
}
