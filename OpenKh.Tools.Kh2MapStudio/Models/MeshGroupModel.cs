using Microsoft.Xna.Framework.Graphics;
using OpenKh.Engine.MonoGame;
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
            Mdlx map, List<ModelTexture.Texture> texture, int index)
        {
            _graphics = g;
            _model = map;
            Name = name;
            Texture = texture;
            Index = index;
            IsVisible = true;
            InvalidateTextures();
            Invalidate();
        }

        public string Name { get; }
        public Mdlx.M4 Map => _model.MapModel;
        public List<ModelTexture.Texture> Texture { get; }
        public int Index { get; }
        public MeshGroup MeshGroup { get; private set; }
        public bool IsVisible { get; set; }

        public void Dispose()
        {
            foreach (var texture in _kingdomTextures)
                texture?.Dispose();
        }

        public void InvalidateTextures()
        {
            foreach (var texture in _kingdomTextures)
                texture?.Dispose();

            _kingdomTextures = Texture?
                .Select(texture => new KingdomTexture(texture, _graphics))
                .ToArray() ?? new KingdomTexture[0];

            if (MeshGroup != null)
                MeshGroup.Textures = _kingdomTextures;
        }

        public void Invalidate()
        {
            MeshGroup = new MeshGroup
            {
                MeshDescriptors = MeshLoader.FromKH2(_model).MeshDescriptors,
                Textures = _kingdomTextures
            };
        }
    }
}
