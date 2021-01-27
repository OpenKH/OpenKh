using Microsoft.Xna.Framework.Graphics;
using OpenKh.Common;
using OpenKh.Engine;
using OpenKh.Engine.MonoGame;
using OpenKh.Game.Entities;
using OpenKh.Game.Infrastructure;
using OpenKh.Kh2;
using OpenKh.Kh2.Extensions;
using OpenKh.Kh2.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OpenKh.Game.Field
{
    public class Kh2Map : IDisposable
    {
        private static readonly MeshGroup Empty = new MeshGroup
        {
            MeshDescriptors = new List<Engine.Parsers.MeshDescriptor>(0),
            Textures = new IKingdomTexture[0]
        };
        private readonly MeshGroup _mapMeshGroup;
        private readonly MeshGroup _skybox0MeshGroup;
        private readonly MeshGroup _skybox1MeshGroup;
        private readonly List<BobEntity> _bobEntities;
        private readonly List<MeshGroup> _bobModels;

        public Kh2Map(GraphicsDevice graphics, Kernel kernel, int world, int area) :
            this(graphics, kernel.DataContent, kernel.GetMapFileName(world, area))
        { }

        public Kh2Map(GraphicsDevice graphics, IDataContent content, string path)
        {
            var binarc = content.FileOpen(path).Using(Bar.Read);
            _skybox0MeshGroup = FromMdlx(graphics, binarc, "SK0") ?? Empty;
            _skybox1MeshGroup = FromMdlx(graphics, binarc, "SK1") ?? Empty;
            _mapMeshGroup = FromMdlx(graphics, binarc, "MAP") ?? Empty;

            _bobEntities = binarc.ForEntry("out", Bar.EntryType.BgObjPlacement, BobDescriptor.Read)?
                .Select(x => new BobEntity(x))?.ToList() ?? new List<BobEntity>();

            var bobModels = binarc.ForEntries("BOB", Bar.EntryType.Model, Mdlx.Read).ToList();
            var bobTextures = binarc.ForEntries("BOB", Bar.EntryType.ModelTexture, ModelTexture.Read).ToList();

            _bobModels = new List<MeshGroup>(bobModels.Count);
            for (var i = 0; i < bobModels.Count; i++)
            {
                _bobModels.Add(new MeshGroup
                {
                    MeshDescriptors = MeshLoader.FromKH2(bobModels[i]).MeshDescriptors,
                    Textures = bobTextures[i].LoadTextures(graphics).ToArray()
                });
            }
        }

        public void ForEveryStaticModel(Action<IMonoGameModel> action)
        {
            action(_skybox0MeshGroup);
            action(_skybox1MeshGroup);
            action(_mapMeshGroup);
        }

        public void ForEveryModel(Action<IEntity, IMonoGameModel> action)
        {
            foreach (var bob in _bobEntities)
                action(bob, _bobModels[bob.BobIndex]);
        }

        public void Dispose()
        {
            Action<MeshGroup> clearMeshGroup = (meshGroup) =>
            {
                foreach (var texture in meshGroup.Textures)
                    texture.Dispose();
            };

            clearMeshGroup(_mapMeshGroup);
            clearMeshGroup(_skybox0MeshGroup);
            clearMeshGroup(_skybox1MeshGroup);
            foreach (var bobModel in _bobModels)
                clearMeshGroup(bobModel);
        }

        private static MeshGroup FromMdlx(GraphicsDevice graphics, IEnumerable<Bar.Entry> entries, string name)
        {
            var model = entries.ForEntry(name, Bar.EntryType.Model, Mdlx.Read);
            if (model == null)
                return null;

            var textures = entries.ForEntry(name, Bar.EntryType.ModelTexture, ModelTexture.Read);
            if (model == null)
                return null;

            return new MeshGroup
            {
                MeshDescriptors = MeshLoader.FromKH2(model).MeshDescriptors,
                Textures = textures.LoadTextures(graphics).ToArray()
            };
        }
    }
}
