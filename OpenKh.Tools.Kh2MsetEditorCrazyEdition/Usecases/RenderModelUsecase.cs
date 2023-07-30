using Microsoft.Xna.Framework.Graphics;
using OpenKh.Common;
using OpenKh.Engine;
using OpenKh.Engine.MonoGame;
using OpenKh.Engine.Parsers;
using OpenKh.Kh2;
using OpenKh.Tools.Kh2MsetEditorCrazyEdition.Helpers;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using xna = Microsoft.Xna.Framework;

namespace OpenKh.Tools.Kh2MsetEditorCrazyEdition.Usecases
{
    public class RenderModelUsecase
    {
        private readonly Settings _settings;
        private readonly ManageKingdomTextureUsecase _manageKingdomTextureUsecase;
        private readonly Camera _camera;
        private readonly KingdomShader _shader;
        private readonly GraphicsDevice _graphics;
        private readonly LoadedModel _loadedModel;
        private readonly Texture2D _whiteTexture;
        private readonly BasicEffect _effectForBoneLines;
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
            ManageKingdomTextureUsecase manageKingdomTextureUsecase,
            Settings settings
        )
        {
            _settings = settings;
            _manageKingdomTextureUsecase = manageKingdomTextureUsecase;
            _camera = camera;
            _shader = shader;
            _graphics = graphics;
            _loadedModel = loadedModel;
            _whiteTexture = getWhiteTextureUsecase();
            _effectForBoneLines = new BasicEffect(_graphics);
            _effectForBoneLines.VertexColorEnabled = true;
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

                float validFrameTime = (1 <= _loadedModel.FrameEnd)
                    ? (float)MathEx.Modulus(_loadedModel.FrameTime, _loadedModel.FrameEnd)
                    : _loadedModel.FrameTime;

                var matrices = _loadedModel.PoseProvider?.Invoke(validFrameTime);

                foreach (var one in ones)
                {
                    Render(pass, one, matrices, true);

                    if (_settings.ViewFkBones && matrices != null)
                    {
                        RenderFkBone(matrices, _loadedModel.InternalFkBones);
                    }
                }
                foreach (var one in ones)
                {
                    Render(pass, one, matrices, false);
                }

                _shader.SetRenderTexture(pass, _whiteTexture);
            });
        }

        private void RenderFkBone(Matrix4x4[] matrices, List<Mdlx.Bone> internalFkBones)
        {
            var points = new VertexPositionColor[2 * internalFkBones.Count];
            var pointIdx = 0;

            for (int x = 0; x < internalFkBones.Count; x++)
            {
                var one = internalFkBones[x];

                var from = matrices[(one.Parent < 0) ? one.Index : one.Parent].Translation;
                var to = matrices[one.Index].Translation;

                points[pointIdx++] = new VertexPositionColor(new xna.Vector3(from.X, from.Y, from.Z), xna.Color.Green);
                points[pointIdx++] = new VertexPositionColor(new xna.Vector3(to.X, to.Y, to.Z), xna.Color.Green);
            }

            _effectForBoneLines.Projection = _camera.Projection.ToXnaMatrix();
            _effectForBoneLines.World = _camera.World.ToXnaMatrix();

            var prevDepthStencilState = _graphics.DepthStencilState;
            _graphics.DepthStencilState = DepthStencilState.None;
            var prevBlendState = _graphics.BlendState;
            _graphics.BlendState = BlendState.Additive;

            foreach (var pass in _effectForBoneLines.CurrentTechnique.Passes)
            {
                pass.Apply();

                _graphics.DrawUserPrimitives(
                    PrimitiveType.LineList,
                    points.ToArray(),
                    0,
                    points.Length / 2,
                    VertexPositionColor.VertexDeclaration
                );
            }

            _graphics.DepthStencilState = prevDepthStencilState;
            _graphics.BlendState = prevBlendState;
        }

        private void Render(EffectPass pass, MdlxRenderSource mesh, Matrix4x4[]? matrices, bool passRenderOpaque)
        {
            foreach (var meshDescriptor in mesh.GetMutatedMeshDescriptors(matrices))
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
