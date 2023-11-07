using Microsoft.Xna.Framework.Graphics;
using OpenKh.Engine.MonoGame;
using OpenKh.Kh2;
using OpenKh.Tools.Kh2MsetMotionEditor.Helpers;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenKh.Tools.Kh2MsetMotionEditor.Usecases
{
    public class ManageKingdomTextureUsecase
    {
        private readonly GraphicsDevice _graphics;
        private readonly LoadedModel _loadedModel;

        public ManageKingdomTextureUsecase(
            LoadedModel loadedModel,
            GraphicsDevice graphics
        )
        {
            _graphics = graphics;
            _loadedModel = loadedModel;
        }

        public void ClearCache()
        {
            foreach (var it in _loadedModel.KingdomTextureCache)
            {
                it.Value.Dispose();
            }
            _loadedModel.KingdomTextureCache.Clear();
        }

        public IKingdomTexture CreateOrGet(ModelTexture.Texture texture)
        {
            _loadedModel.KingdomTextureCache.TryGetValue(texture, out KingdomTexture? kingdomTexture);

            if (kingdomTexture == null)
            {
                kingdomTexture = new KingdomTexture(
                    texture,
                    _graphics
                );

                _loadedModel.KingdomTextureCache[texture] = kingdomTexture;
            }

            return kingdomTexture;
        }
    }
}
