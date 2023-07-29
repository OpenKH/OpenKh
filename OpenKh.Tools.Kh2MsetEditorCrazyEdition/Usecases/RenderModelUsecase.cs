using Microsoft.Xna.Framework.Graphics;
using OpenKh.Common;
using OpenKh.Engine;
using OpenKh.Engine.MonoGame;
using OpenKh.Kh2;
using OpenKh.Tools.Kh2MsetEditorCrazyEdition.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using xna = Microsoft.Xna.Framework;

namespace OpenKh.Tools.Kh2MsetEditorCrazyEdition.Usecases
{
    public class RenderModelUsecase
    {
        private readonly ManageKingdomTextureUsecase _manageKingdomTextureUsecase;
        private readonly Camera _camera;
        private readonly KingdomShader _shader;
        private readonly GraphicsDevice _graphics;
        private readonly LoadedModel _loadedModel;
        private readonly Texture2D _whiteTexture;
        private readonly static BlendState DefaultBlendState = new BlendState()
        {
            ColorSourceBlend = Blend.SourceAlpha,
            AlphaSourceBlend = Blend.SourceAlpha,
            ColorDestinationBlend = Blend.InverseSourceAlpha,
            AlphaDestinationBlend = Blend.InverseSourceAlpha,
            ColorBlendFunction = BlendFunction.Add,
            AlphaBlendFunction = BlendFunction.Add,
            BlendFactor = xna.Color.White,
            MultiSampleMask = int.MaxValue,
            IndependentBlendEnable = false
        };

        public RenderModelUsecase(
            LoadedModel loadedModel,
            GraphicsDevice graphics,
            KingdomShader shader,
            Camera camera,
            GetWhiteTextureUsecase getWhiteTextureUsecase,
            ManageKingdomTextureUsecase manageKingdomTextureUsecase
        )
        {
            _manageKingdomTextureUsecase = manageKingdomTextureUsecase;
            _camera = camera;
            _shader = shader;
            _graphics = graphics;
            _loadedModel = loadedModel;
            _whiteTexture = getWhiteTextureUsecase();
        }

        public void Draw()
        {
            var viewport = _graphics.Viewport;
            _camera.AspectRatio = viewport.Width / (float)viewport.Height;

            _graphics.RasterizerState = new RasterizerState()
            {
                CullMode = CullMode.CullClockwiseFace
            };
            _graphics.DepthStencilState = new DepthStencilState();
            _graphics.BlendState = DefaultBlendState;

            var ones = _loadedModel.MdlxRenderableList.Where(it => it.IsVisible);

            _shader.Pass(pass =>
            {
                _shader.SetProjectionView(_camera.Projection);
                _shader.SetWorldView(_camera.World);
                _shader.SetModelViewIdentity();
                pass.Apply();

                foreach (var one in ones)
                {
                    Render(pass, one, true);
                }
                foreach (var one in ones)
                {
                    Render(pass, one, false);
                }

                _shader.SetRenderTexture(pass, _whiteTexture);
            });
        }

        private void Render(EffectPass pass, MdlxRenderSource mesh, bool passRenderOpaque)
        {
            float validFrameTime = (1 <= _loadedModel.FrameEnd)
                ? (float)MathEx.Modulus(_loadedModel.FrameTime, _loadedModel.FrameEnd)
                : _loadedModel.FrameTime;

            foreach (var meshDescriptor in mesh.GetMutatedMeshDescriptors(_loadedModel.PoseProvider?.Invoke(validFrameTime)))
            {
                if (meshDescriptor.IsOpaque != passRenderOpaque)
                {
                    continue;
                }
                if (meshDescriptor.Indices.Length == 0)
                {
                    continue;
                }

                var textureIndex = meshDescriptor.TextureIndex & 0xffff;
                if (textureIndex < mesh.Textures.Count)
                {
                    _shader.SetRenderTexture(
                        pass,
                        _manageKingdomTextureUsecase.CreateOrGet(mesh.Textures[textureIndex])
                    );
                }

                _graphics.DrawUserIndexedPrimitives(
                    PrimitiveType.TriangleList,
                    meshDescriptor.Vertices,
                    0,
                    meshDescriptor.Vertices.Length,
                    meshDescriptor.Indices,
                    0,
                    meshDescriptor.Indices.Length / 3,
                    MeshLoader.PositionColoredTexturedVertexDeclaration);
            }
        }
    }
}
