using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using OpenKh.Engine.MonoGame;
using OpenKh.Engine.Parsers;
using OpenKh.Kh2;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OpenKh.Tools.Kh2MapStudio.Models
{
    class MeshGroupModel : IDisposable
    {
        private readonly GraphicsDevice _graphics;
        private KingdomTexture[] _kingdomTextures = new KingdomTexture[0];
        private readonly Mdlx _model;

        public MeshGroupModel(GraphicsDevice g, string name,
            Mdlx map, List<ModelTexture.Texture> texture)
        {
            _graphics = g;
            _model = map;
            Name = name;
            Texture = texture;
            IsVisible = true;
            InvalidateTextures();
            Invalidate();
        }

        public string Name { get; }
        public Mdlx.M4 Map => _model.MapModel;
        public List<ModelTexture.Texture> Texture { get; }
        public MeshGroup MeshGroup { get; private set; }
        public bool IsVisible { get; set; }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public void InvalidateTextures()
        {
            foreach (var texture in _kingdomTextures)
                texture?.Dispose();

            _kingdomTextures = Texture
                .Select(texture => new KingdomTexture(texture, _graphics))
                .ToArray() ?? new KingdomTexture[0];

            if (MeshGroup != null)
                MeshGroup.Textures = _kingdomTextures;
        }

        public void Invalidate()
        {
            MeshGroup = new MeshGroup
            {
                Segments = null,
                Parts = null,
                MeshDescriptors = new MdlxParser(_model).MeshDescriptors?
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
                Textures = _kingdomTextures
            };
        }
    }
}
