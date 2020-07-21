using ImGuiNET;
using OpenKh.Engine.Extensions;
using OpenKh.Engine.Renders;
using OpenKh.Tools.LayoutEditor.Interfaces;
using OpenKh.Tools.LayoutEditor.Models;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace OpenKh.Tools.LayoutEditor.Dialogs
{
    class SpriteEditDialog : IDisposable
    {
        private readonly List<SpriteModel> _spriteModels;
        private readonly ISpriteDrawing _spriteDrawing;
        private readonly ISpriteTexture _atlasTexture;
        private readonly ITextureBinder _textureBinder;
        private readonly IEditorSettings _settings;
        private readonly ISpriteTexture _cropAtlasTexture;
        private readonly IntPtr _cropAtlasTextureId;
        
        private int _selectedSpriteModel;
        private SpriteModel SpriteModel => _spriteModels[_selectedSpriteModel];

        public SpriteEditDialog(
            List<SpriteModel> spriteModels,
            int selectedSpriteModel,
            ISpriteDrawing spriteDrawing,
            ISpriteTexture atlasTexture,
            ITextureBinder textureBinder,
            IEditorSettings settings)
        {
            _spriteModels = spriteModels;
            _selectedSpriteModel = selectedSpriteModel;
            _spriteDrawing = spriteDrawing;
            _atlasTexture = atlasTexture;
            _textureBinder = textureBinder;
            _settings = settings;
            _settings.OnChangeBackground += (o, e) => DrawCropAtlasTexture();

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
            if (ImGui.InputInt("Selected sprite to edit", ref _selectedSpriteModel))
            {
                _selectedSpriteModel = Math.Min(Math.Max(_selectedSpriteModel, 0), _spriteModels.Count - 1);
                DrawCropAtlasTexture();
            }

            ImGui.Image(_cropAtlasTextureId, new Vector2(_atlasTexture.Width, _atlasTexture.Height));

            var source = new int[]
            {
                SpriteModel.Sprite.Left, SpriteModel.Sprite.Top,
                SpriteModel.Sprite.Right, SpriteModel.Sprite.Bottom
            };

            ImGui.Columns(4, "ltrb", false);
            bool sourceChanged = ImGui.DragInt("UA", ref source[0]); ImGui.NextColumn();
            sourceChanged |= ImGui.DragInt("VA", ref source[1]); ImGui.NextColumn();
            sourceChanged |= ImGui.DragInt("UB", ref source[2]); ImGui.NextColumn();
            sourceChanged |= ImGui.DragInt("VB", ref source[3]);
            if (sourceChanged)
            {
                SpriteModel.Sprite.Left = source[0];
                SpriteModel.Sprite.Top = source[1];
                SpriteModel.Sprite.Right = source[2];
                SpriteModel.Sprite.Bottom = source[3];
                SpriteModel.SizeChanged();
                DrawCropAtlasTexture();
            }
            ImGui.Columns(1);

            var colorTopLeft = Utilities.ConvertColor(SpriteModel.Sprite.ColorLeft);
            if (ImGui.ColorEdit4("Top left", ref colorTopLeft))
                SpriteModel.Sprite.ColorLeft = Utilities.ConvertColor(colorTopLeft);

            var colorTopRight = Utilities.ConvertColor(SpriteModel.Sprite.ColorTop);
            if (ImGui.ColorEdit4("Top right", ref colorTopRight))
                SpriteModel.Sprite.ColorTop = Utilities.ConvertColor(colorTopRight);

            var colorBottomLeft = Utilities.ConvertColor(SpriteModel.Sprite.ColorRight);
            if (ImGui.ColorEdit4("Bottom left", ref colorBottomLeft))
                SpriteModel.Sprite.ColorRight = Utilities.ConvertColor(colorBottomLeft);

            var colorBottomRight = Utilities.ConvertColor(SpriteModel.Sprite.ColorBottom);
            if (ImGui.ColorEdit4("Bottom right", ref colorBottomRight))
                SpriteModel.Sprite.ColorBottom = Utilities.ConvertColor(colorBottomRight);

            var uvAnim = new Vector2(SpriteModel.Sprite.UTranslation, SpriteModel.Sprite.VTranslation);
            if (ImGui.DragFloat2("UV animation", ref uvAnim, 0.0001f))
            {
                SpriteModel.Sprite.UTranslation = uvAnim.X;
                SpriteModel.Sprite.VTranslation = uvAnim.Y;
            }

            SpriteModel.Draw(0, 0);
            ImGui.Image(SpriteModel.TextureId, SuggestSpriteSize());
        }

        private void DrawCropAtlasTexture()
        {
            var sLeft = SpriteModel.Sprite.Left;
            var sTop = SpriteModel.Sprite.Top;
            var sRight = SpriteModel.Sprite.Right;
            var sBottom = SpriteModel.Sprite.Bottom;
            var cropColor = _settings.EditorBackground;
            cropColor.A = 0.75f;
            var invertedCropColor = new ColorF(
                1f - cropColor.R, 1f - cropColor.G,
                1f - cropColor.G, 1.0f);

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
            _spriteDrawing.Clear(_settings.EditorBackground);
            _spriteDrawing.AppendSprite(context);

            _spriteDrawing.FillRectangle(0, 0, _cropAtlasTexture.Width, sTop, cropColor);
            _spriteDrawing.FillRectangle(0, sTop, sLeft, sBottom - sTop, cropColor);
            _spriteDrawing.FillRectangle(sRight, sTop, _cropAtlasTexture.Width, sBottom - sTop, cropColor);
            _spriteDrawing.FillRectangle(0, sBottom, _cropAtlasTexture.Width, _cropAtlasTexture.Height, cropColor);
            _spriteDrawing.DrawRectangle(sLeft - 1, sTop - 1,
                sRight - sLeft + 2, sBottom - sTop + 2, invertedCropColor);

            _spriteDrawing.Flush();
            _spriteDrawing.DestinationTexture = null;
        }

        private Vector2 SuggestSpriteSize()
        {
            const int MaxZoomLevel = 8;
            var zoomLevelX = _atlasTexture.Width / SpriteModel.Width;
            var zoomLevelY = _atlasTexture.Height / SpriteModel.Height;
            var zoomLevel = Math.Max(1, Math.Min(MaxZoomLevel, Math.Min(zoomLevelX, zoomLevelY)));

            return new Vector2(SpriteModel.Width * zoomLevel, SpriteModel.Height * zoomLevel);
        }
    }
}
