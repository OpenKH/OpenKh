using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using OpenKh.Engine.Parsers;
using OpenKh.Engine.Parsers.Kddf2;
using OpenKh.Engine.Parsers.Kddf2.Mset;
using OpenKh.Kh2;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace OpenKh.Engine.MonoGame
{
    public static class MeshLoader
    {
        public static MeshGroup FromKH2(GraphicsDevice graphics, Mdlx model, ModelTexture texture,
            IArchiveManager archiveManager = null, // legacy
            Func<Stream> mdlxLoader = null
        )
        {
            if (model == null || texture == null)
                return null;

            var meshGroup = FromKH2(model, archiveManager, mdlxLoader);
            meshGroup.Textures = LoadTextures(graphics, texture).ToArray();

            return meshGroup;
        }

        public static MeshGroup FromKH2(Mdlx model,
            IArchiveManager archiveManager = null, // legacy
            Func<Stream> mdlxLoader = null
        )
        {
            if (model == null)
                return null;

            var modelParsed = new MdlxParser(model);
            return modelParsed.MeshDescriptors != null ?
                LoadKH2New(modelParsed) :
                LoadKH2Legacy(modelParsed, archiveManager, mdlxLoader);
        }

        private static MeshGroup LoadKH2New(MdlxParser model)
        {
            return new MeshGroup
            {
                Segments = null,
                Parts = null,
                MeshDescriptors = model.MeshDescriptors?
                    .Select(x => new MeshDesc
                    {
                        Vertices = x.Vertices
                            .Select(v => new VertexPositionColorTexture(
                                new Vector3(v.X, v.Y, v.Z),
                                new Color((v.Color >> 16) & 0xff, (v.Color >> 8) & 0xff, v.Color & 0xff, (v.Color >> 24) & 0xff),
                                new Vector2(v.Tu, v.Tv)))
                            .ToArray(),
                        Indices = x.Indices,
                        TextureIndex = x.TextureIndex,
                        IsOpaque = x.IsOpaque
                    })
                    .ToList(),
                Textures = null
            };
        }

        private static MeshGroup LoadKH2Legacy(MdlxParser model,
            IArchiveManager archiveManager = null, // legacy
            Func<Stream> mdlxLoader = null
        )
        {
            MeshGroup.Segment[] segments = null;
            MeshGroup.Part[] parts = null;

            if (model.Model is MdlxAnimModel animModel)
            {
                var anbEntries = archiveManager.Get<List<Bar.Entry>>("A000") ?? archiveManager.Get<List<Bar.Entry>>("B000");
                if (anbEntries != null && mdlxLoader != null)
                {
                    var anbIndir = new AnbIndir(anbEntries);
                    using (var mdlxStream = mdlxLoader())
                    {
                        animModel.AnimMatricesProvider = anbIndir.GetAnimProvider(mdlxStream);
                    }
                }
            }
            model.Model.Update(0);

            segments = model.Model.Segments.Select(segment => new MeshGroup.Segment
            {
                Vertices = segment.Vertices.Select(vertex => new VertexPositionColorTexture
                {
                    Position = new Vector3(vertex.X, vertex.Y, vertex.Z),
                    TextureCoordinate = new Vector2(vertex.U, vertex.V),
                    Color = new Color((vertex.Color >> 16) & 0xff, (vertex.Color >> 8) & 0xff, vertex.Color & 0xff, (vertex.Color >> 24) & 0xff)
                }).ToArray()
            }).ToArray();

            parts = model.Model.Parts.Select(part => new MeshGroup.Part
            {
                Indices = part.Indices,
                SegmentId = part.SegmentIndex,
                TextureId = part.TextureIndex,
                IsOpaque = part.IsOpaque,
            }).ToArray();

            return new MeshGroup
            {
                Segments = segments,
                Parts = parts,
                Textures = null
            };
        }

        public static IEnumerable<KingdomTexture> LoadTextures(
            GraphicsDevice graphics, ModelTexture texture) => texture?.Images?
                .Select(texture => new KingdomTexture(texture, graphics)).ToArray() ??
                new KingdomTexture[0];
    }
}
