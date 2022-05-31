using Microsoft.Xna.Framework.Graphics;
using OpenKh.Engine.MonoGame;
using OpenKh.Kh2;
using OpenKh.Bbs;
using OpenKh.Imaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace OpenKh.Tools.BbsMapStudio.Models
{
    class MeshGroupModel : IDisposable
    {
        private readonly GraphicsDevice _graphics;
        private Tim2KingdomTexture[] _kingdomTextures = new Tim2KingdomTexture[0];
        private readonly Pmo _model;
        public Vector3 Location;
        public Vector3 Rotation;
        public Vector3 Scale;
        public bool hasDifferentMatrix;

        public MeshGroupModel(GraphicsDevice g, string name,
            Pmo map, List<Tm2> texture, int index, Vector3 loc, Vector3 Rot, Vector3 Scl, bool diffMatrix)
        {
            _graphics = g;
            _model = map;
            Name = name;
            Texture = texture;
            Index = index;
            IsVisible = true;
            Location = loc;
            Rotation = Rot;
            Scale = Scl;
            hasDifferentMatrix = diffMatrix;
            InvalidateTextures();
            Invalidate();
        }

        public string Name { get; }
        public Pmo Map => _model;
        public List<Tm2> Texture { get; }
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
                .Select(texture => new Tim2KingdomTexture(texture, _graphics))
                .ToArray() ?? new Tim2KingdomTexture[0];

            if (MeshGroup != null)
                MeshGroup.Textures = _kingdomTextures;
        }

        public void Invalidate()
        {
            MeshGroup = new MeshGroup
            {
                MeshDescriptors = MeshLoader.FromBBS(_model).MeshDescriptors,
                Textures = _kingdomTextures
            };
        }
    }
}
