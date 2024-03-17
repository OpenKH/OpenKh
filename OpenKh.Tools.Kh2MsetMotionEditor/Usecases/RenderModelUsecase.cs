using ImGuiNET;
using Microsoft.Xna.Framework.Graphics;
using OpenKh.Common;
using OpenKh.Engine;
using OpenKh.Engine.MonoGame;
using OpenKh.Engine.Parsers;
using OpenKh.Kh2;
using OpenKh.Tools.Kh2MsetMotionEditor.Helpers;
using OpenKh.Engine.Monogame.Helpers;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using xna = Microsoft.Xna.Framework;

namespace OpenKh.Tools.Kh2MsetMotionEditor.Usecases
{
    public class RenderModelUsecase
    {
        private readonly GlobalInfo _globalInfo;
        private readonly ConvertVectorSpaceUsecase _convertVectorSpaceUsecase;
        private readonly PrintDebugInfo _printDebugInfo;
        private readonly ComputeSpriteIconUvUsecase _computeSpriteIconUvUsecase;
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
        private readonly SpriteBatch _spriteBatch;
        private readonly Texture2D _spriteIcons;

        public RenderModelUsecase(
            LoadedModel loadedModel,
            GraphicsDevice graphics,
            KingdomShader shader,
            Camera camera,
            CreateWhiteTextureUsecase getWhiteTextureUsecase,
            ManageKingdomTextureUsecase manageKingdomTextureUsecase,
            Settings settings,
            CreateSpriteIconsTextureUsecase createSpriteIconsTextureUsecase,
            ComputeSpriteIconUvUsecase computeSpriteIconUvUsecase,
            PrintDebugInfo printDebugInfo,
            ConvertVectorSpaceUsecase convertVectorSpaceUsecase,
            GlobalInfo globalInfo
        )
        {
            _globalInfo = globalInfo;
            _convertVectorSpaceUsecase = convertVectorSpaceUsecase;
            _printDebugInfo = printDebugInfo;
            _computeSpriteIconUvUsecase = computeSpriteIconUvUsecase;
            _settings = settings;
            _manageKingdomTextureUsecase = manageKingdomTextureUsecase;
            _camera = camera;
            _shader = shader;
            _graphics = graphics;
            _loadedModel = loadedModel;
            _whiteTexture = getWhiteTextureUsecase();
            _effectForBoneLines = new BasicEffect(_graphics);
            _effectForBoneLines.VertexColorEnabled = true;
            _spriteBatch = new SpriteBatch(_graphics);
            _spriteIcons = createSpriteIconsTextureUsecase();
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

            var fkView = _loadedModel.GetActiveFkBoneViews?.Invoke();
            int FindSpriteIconIndex(int index)
            {
                if (index == _loadedModel.SelectedJointIndex)
                {
                    return 255;
                }
                else
                {
                    return fkView?
                        .FirstOrDefault(it => it.I == index)?
                        .SpriteIcon ?? 0;
                }
            }

            var fkIk = _loadedModel.PoseProvider?.Invoke(_loadedModel.FrameTime);

            _shader.Pass(pass =>
            {
                _shader.SetProjectionView(_camera.Projection);
                _shader.SetWorldView(_camera.World);
                _shader.SetModelViewIdentity();
                pass.Apply();

                foreach (var one in ones)
                {
                    Render(pass, one, fkIk?.Fk, true);

                    if (fkIk != null && _loadedModel.MotionData?.IKHelpers is List<Motion.IKHelper> ikHelperList)
                    {
                        var viewFk = _settings.ViewFkBones;
                        var viewIk = _settings.ViewIkBones;

                        if (viewFk || viewIk)
                        {
                            var numFk = fkIk.Fk.Length;
                            var numIk = fkIk.Ik.Length;

                            RenderBones(
                                numFk + numIk,
                                index => false
                                    || (viewFk ? index < numFk : false)
                                    || (viewIk ? numFk <= index : false),
                                index => (index < numFk)
                                    ? xna.Color.Green
                                    : xna.Color.DarkMagenta,
                                index => (index < numFk)
                                    ? fkIk.Fk[index]
                                    : fkIk.Ik[index - numFk],
                                index => (index < numFk)
                                    ? _loadedModel.InternalFkBones[index].Parent
                                    : ikHelperList[index - numFk].ParentId,
                                FindSpriteIconIndex
                            );
                        }
                    }
                }
                foreach (var one in ones)
                {
                    Render(pass, one, fkIk?.Fk, false);
                }

                _shader.SetRenderTexture(pass, _whiteTexture);
            });
        }

        private void RenderBones(
            int numMatrices,
            Func<int, bool> isVisible,
            Func<int, xna.Color> getColor,
            Func<int, Matrix4x4> getMatrix,
            Func<int, int> getParent,
            Func<int, int> getSpriteIconIndex
        )
        {
            var points = new VertexPositionColor[2 * numMatrices];
            var pointIdx = 0;

            for (int x = 0; x < numMatrices; x++)
            {
                var parent = getParent(x);

                var from = getMatrix((parent < 0 || !isVisible(x)) ? x : parent).Translation;
                var to = getMatrix(x).Translation;

                var color = getColor(x);

                points[pointIdx++] = new VertexPositionColor(new xna.Vector3(from.X, from.Y, from.Z), color);
                points[pointIdx++] = new VertexPositionColor(new xna.Vector3(to.X, to.Y, to.Z), color);
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

            {
                _spriteBatch.Begin(blendState: BlendState.AlphaBlend, depthStencilState: DepthStencilState.None);

                var fromLocalSpaceToWindowsPixelSpace = _convertVectorSpaceUsecase.GetFromLocalSpaceToWindowsPixelSpace(
                    _graphics.Viewport.Bounds,
                    _camera.World * _camera.Projection
                );

                for (int x = 0; x < numMatrices; x++)
                {
                    if (!isVisible(x))
                    {
                        continue;
                    }

                    var spriteIconIndex = getSpriteIconIndex(x);
                    if (1 <= spriteIconIndex)
                    {
                        var sourceRect = _computeSpriteIconUvUsecase.ComputeDrawSourceRect(spriteIconIndex & 255, _spriteIcons.Width, _spriteIcons.Height);

                        var (position, scale, visible) = fromLocalSpaceToWindowsPixelSpace(getMatrix(x).Translation);

                        if (visible)
                        {
                            var length = 32 * 150 * scale;

                            _spriteBatch.Draw(
                                _spriteIcons,
                                new xna.Rectangle(
                                    (int)(position.X - length / 2),
                                    (int)(position.Y - length / 2),
                                    (int)length,
                                    (int)length
                                ),
                                sourceRect,
                                xna.Color.White
                            );
                        }
                    }
                }

                _spriteBatch.End();
            }

            {
                var writer = new StringWriter();

                foreach (var (matrix, index) in new Matrix4x4[] {
                    //_camera.Projection,
                    //_camera.World,
                    //_camera.Projection * _camera.World,
                    _camera.World * _camera.Projection,
                }
                    .SelectWithIndex()
                )
                {
                    //writer.WriteLine($"#{index}.xyz: {Vector3.Transform(Vector3.Zero, matrix)}");
                    //writer.WriteLine($"#{index}.xyz_: {Vector4.Transform(Vector3.Zero, matrix)}");
                    //writer.WriteLine($"#{index}.xyz0: {Vector4.Transform(Vector4.Zero, matrix)}");
                    //writer.WriteLine($"#{index}.xyz1: {Vector4.Transform(new Vector4(0, 0, 0, 1), matrix)}");
                    writer.WriteLine($"#{index}.xyz1: {WTo1(Vector4.Transform(new Vector4(0, 0, 0, 1), matrix))}");
                }

                _printDebugInfo.Printers["ProjectionWorldMatrix"] = () =>
                {
                    ImGui.Text(writer.ToString());
                };
            }

            _graphics.DepthStencilState = prevDepthStencilState;
            _graphics.BlendState = prevBlendState;
        }

        private Vector3 WTo1(Vector4 vector4)
        {
            return new Vector3(
                vector4.X / vector4.W,
                vector4.Y / vector4.W,
                vector4.Z / vector4.W
            );
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
