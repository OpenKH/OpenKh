using ImGuiNET;
using OpenKh.Engine.Extensions;
using OpenKh.Engine.Renders;
using OpenKh.Tools.LayoutEditor.Interfaces;
using OpenKh.Tools.LayoutEditor.Models;
using System;
using System.Numerics;

namespace OpenKh.Tools.LayoutEditor
{
    class SpriteEditDialog : IDisposable
    {
        private readonly SpriteModel _spriteModel;
        private readonly ISpriteDrawing _spriteDrawing;
        private readonly ISpriteTexture _atlasTexture;
        private readonly ITextureBinder _textureBinder;
        private readonly ISpriteTexture _cropAtlasTexture;
        private readonly IntPtr _cropAtlasTextureId;

        public SpriteEditDialog(
            SpriteModel spriteModel,
            ISpriteDrawing spriteDrawing,
            ISpriteTexture atlasTexture,
            ITextureBinder textureBinder)
        {
            _spriteModel = spriteModel;
            _spriteDrawing = spriteDrawing;
            _atlasTexture = atlasTexture;
            _textureBinder = textureBinder;

            _cropAtlasTexture = _spriteDrawing.CreateSpriteTexture(atlasTexture.Width, atlasTexture.Height);
            _cropAtlasTextureId = _textureBinder.BindTexture(_cropAtlasTexture);
            DrawCropAtlasTexture();
        }

        public void Dispose()
        {
            if (_cropAtlasTextureId != IntPtr.Zero)
                _textureBinder.UnbindTexture(_cropAtlasTextureId);
            _cropAtlasTexture?.Dispose();
        }

        public void Run()
        {
            ImGui.Image(_cropAtlasTextureId, new Vector2(_atlasTexture.Width, _atlasTexture.Height));

            var source = new int[]
            {
                _spriteModel.Sprite.Left, _spriteModel.Sprite.Top,
                _spriteModel.Sprite.Right, _spriteModel.Sprite.Bottom
            };
            if (ImGui.DragInt4("Source", ref source[0]))
            {
                _spriteModel.Sprite.Left = source[0];
                _spriteModel.Sprite.Top = source[1];
                _spriteModel.Sprite.Right = source[2];
                _spriteModel.Sprite.Bottom = source[3];
                _spriteModel.SizeChanged();
                DrawCropAtlasTexture();
            }

            var colorTopLeft = Utilities.ConvertColor(_spriteModel.Sprite.ColorLeft);
            if (ImGui.ColorEdit4("Top left", ref colorTopLeft))
                _spriteModel.Sprite.ColorLeft = Utilities.ConvertColor(colorTopLeft);

            var colorTopRight = Utilities.ConvertColor(_spriteModel.Sprite.ColorTop);
            if (ImGui.ColorEdit4("Top right", ref colorTopRight))
                _spriteModel.Sprite.ColorTop = Utilities.ConvertColor(colorTopRight);

            var colorBottomLeft = Utilities.ConvertColor(_spriteModel.Sprite.ColorRight);
            if (ImGui.ColorEdit4("Bottom left", ref colorBottomLeft))
                _spriteModel.Sprite.ColorRight = Utilities.ConvertColor(colorBottomLeft);

            var colorBottomRight = Utilities.ConvertColor(_spriteModel.Sprite.ColorBottom);
            if (ImGui.ColorEdit4("Bottom right", ref colorBottomRight))
                _spriteModel.Sprite.ColorBottom = Utilities.ConvertColor(colorBottomRight);

            var uvAnim = new Vector2(_spriteModel.Sprite.UTranslation, _spriteModel.Sprite.VTranslation);
            if (ImGui.DragFloat2("UV animation", ref uvAnim, 0.0001f))
            {
                _spriteModel.Sprite.UTranslation = uvAnim.X;
                _spriteModel.Sprite.VTranslation = uvAnim.Y;
            }

            var unknown00 = _spriteModel.Sprite.Unknown00;
            if (ImGui.DragInt("Unknown00", ref unknown00))
                _spriteModel.Sprite.Unknown00 = unknown00;

            _spriteModel.Draw(0, 0);
            ImGui.Image(_spriteModel.TextureId, SuggestSpriteSize());
        }

        private void DrawCropAtlasTexture()
        {
            var sLeft = _spriteModel.Sprite.Left;
            var sTop = _spriteModel.Sprite.Top;
            var sRight = _spriteModel.Sprite.Right;
            var sBottom = _spriteModel.Sprite.Bottom;
            var cropColor = new ColorF(1, 0, 1, 0.75f);

            var context = new SpriteDrawingContext()
                .SpriteTexture(_atlasTexture)
                .ColorDefault()
                .SourceLTRB(0, 0, _cropAtlasTexture.Width, _cropAtlasTexture.Height)
                .Position(0, 0)
                .DestinationSize(_cropAtlasTexture.Width, _cropAtlasTexture.Height);
            context.TextureWrapHorizontal(TextureWrapMode.Repeat, 0, _cropAtlasTexture.Width);
            context.TextureWrapVertical(TextureWrapMode.Repeat, 0, _cropAtlasTexture.Height);

            _spriteDrawing.DestinationTexture = _cropAtlasTexture;
            _spriteDrawing.SetViewport(0, _cropAtlasTexture.Width, 0, _cropAtlasTexture.Height);
            _spriteDrawing.Clear(new ColorF(1, 0, 1, 1));
            _spriteDrawing.AppendSprite(context);

            _spriteDrawing.FillRectangle(0, 0, _cropAtlasTexture.Width, sTop, cropColor);
            _spriteDrawing.FillRectangle(0, sTop, sLeft, sBottom - sTop, cropColor);
            _spriteDrawing.FillRectangle(sRight, sTop, _cropAtlasTexture.Width, sBottom - sTop, cropColor);
            _spriteDrawing.FillRectangle(0, sBottom, _cropAtlasTexture.Width, _cropAtlasTexture.Height, cropColor);

            _spriteDrawing.Flush();
            _spriteDrawing.DestinationTexture = null;
        }

        private Vector2 SuggestSpriteSize()
        {
            const int MaxZoomLevel = 8;
            var zoomLevelX = _atlasTexture.Width / _spriteModel.Width;
            var zoomLevelY = _atlasTexture.Height / _spriteModel.Height;
            var zoomLevel = Math.Max(1, Math.Min(MaxZoomLevel, Math.Min(zoomLevelX, zoomLevelY)));

            return new Vector2(_spriteModel.Width * zoomLevel, _spriteModel.Height * zoomLevel);
        }
    }
}
