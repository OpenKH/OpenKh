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
    public class Kh2Map : IMap, IDisposable
    {
        private static readonly MeshGroup Empty = new MeshGroup
        {
            MeshDescriptors = new List<Engine.Parsers.MeshDescriptor>(0),
            Textures = new IKingdomTexture[0]
        };

        private readonly GraphicsDevice _graphics;
        private readonly MeshGroup _mapMeshGroup;
        private readonly MeshGroup _skybox0MeshGroup;
        private readonly MeshGroup _skybox1MeshGroup;
        private readonly List<BobEntity> _bobEntities;
        private readonly List<MeshGroup> _bobModels;

        public Coct CollisionOctalTree { get; }
        public Coct ColorOctalTree { get; }
        public Coct CameraOctalTree { get; }
        public Doct DrawOctalTree { get; }

        public Kh2Map(GraphicsDevice graphics, Kernel kernel, int world, int area) :
            this(graphics, kernel.DataContent, kernel.GetMapFileName(world, area))
        { }

        public Kh2Map(GraphicsDevice graphics, IDataContent content, string path)
        {
            _graphics = graphics;

            var binarc = content.FileOpen(path).Using(Bar.Read);
            _skybox0MeshGroup = FromMdlx(graphics, binarc, "SK0") ?? Empty;
            _skybox1MeshGroup = FromMdlx(graphics, binarc, "SK1") ?? Empty;
            _mapMeshGroup = FromMdlx(graphics, binarc, "MAP") ?? Empty;

            CollisionOctalTree = binarc.ForEntry(x => x.Type == Bar.EntryType.CollisionOctalTree, Coct.Read);
            ColorOctalTree = binarc.ForEntry(x => x.Type == Bar.EntryType.ColorOctalTree, Coct.Read);
            CameraOctalTree = binarc.ForEntry(x => x.Type == Bar.EntryType.CameraOctalTree, Coct.Read);
            DrawOctalTree = binarc.ForEntry(x => x.Type == Bar.EntryType.DrawOctalTree, Doct.Read);

            _bobEntities = binarc.ForEntry(x => x.Type == Bar.EntryType.BgObjPlacement, BobDescriptor.Read)?
                .Select(x => new BobEntity(x)).ToList() ?? new List<BobEntity>();

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

        public void Render(Camera camera, KingdomShader shader, EffectPass pass, bool passRenderOpaque)
        {
            shader.SetModelViewIdentity();
            pass.Apply();

            _graphics.RenderMeshNew(shader, pass, _skybox0MeshGroup, passRenderOpaque);
            _graphics.RenderMeshNew(shader, pass, _skybox1MeshGroup, passRenderOpaque);
            _graphics.RenderMeshNew(shader, pass, _mapMeshGroup, passRenderOpaque);

            foreach (var bob in _bobEntities)
            {
                shader.SetModelView(bob.GetMatrix());
                pass.Apply();

                _graphics.RenderMeshNew(shader, pass, _bobModels[bob.BobIndex], passRenderOpaque);
            }
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
