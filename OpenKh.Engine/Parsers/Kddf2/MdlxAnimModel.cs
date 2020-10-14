﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using OpenKh.Engine.Maths;
using OpenKh.Engine.Parsers.Kddf2.Mset;
using OpenKh.Engine.Parsers.Kddf2.Mset.EmuRunner;
using OpenKh.Engine.Parsers.Kddf2.Mset.Interfaces;
using OpenKh.Kh2;

namespace OpenKh.Engine.Parsers.Kddf2
{
    public class MdlxAnimModel : Model
    {
        private readonly Mdlx mdlx;

        private readonly Kkdf2MdlxParser parser;

        public IAnimMatricesProvider AnimMatricesProvider { get; set; }


        public MdlxAnimModel(Mdlx mdlx)
        {
            this.mdlx = mdlx;
            parser = new Kkdf2MdlxParser(mdlx.SubModels.First());
        }

        public void Update(double delta)
        {
            var builtModel = parser
                .ProcessVerticesAndBuildModel(
                    AnimMatricesProvider?.ProvideMatrices(delta)
                    ?? MdlxMatrixUtil.BuildTPoseMatrices(mdlx.SubModels.First(), Matrix4x4.Identity)
                );

            var ci = builtModel.textureIndexBasedModelDict.Values.Select((model, i) => new Kddf2.Kkdf2MdlxParser.CI
            {
                Indices = model.Vertices.Select((_, index) => index).ToArray(),
                TextureIndex = i,
                SegmentIndex = i
            });

            parser.MeshDescriptors.AddRange(ci);

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
            }).ToArray();
            Parts = builtModel.parser.MeshDescriptors.Select(x => new Model.Part
            {
                Indices = x.Indices,
                SegmentIndex = x.SegmentIndex,
                TextureIndex = x.TextureIndex
            }).ToArray();
        }
    }
}
